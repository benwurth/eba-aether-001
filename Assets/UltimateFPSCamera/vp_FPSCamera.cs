/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSCamera.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	a first person camera class with weapon rendering and animation
//					features. animates the camera transform using springs, bob and
//					perlin noise, in response to user input
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Camera))]

public class vp_FPSCamera : vp_Component
{

	// character controller of the parent gameobject
	protected CharacterController Controller = null;

	// camera input
	public Vector2 MouseSensitivity = new Vector2(5.0f, 5.0f);
	public int MouseSmoothSteps = 10;				// allowed range: 1-10
	public float MouseSmoothWeight = 0.5f;			// allowed range: 0.0f - 1.0f
	public bool MouseAcceleration = false;
	public float MouseAccelerationThreshold = 0.4f;
	protected Vector2 m_MouseMove = Vector2.zero;		// distance moved since last frame
	protected List<Vector2> m_MouseSmoothBuffer = new List<Vector2>();

	// camera rendering
	public float RenderingFieldOfView = 60.0f;
	public float RenderingZoomDamping = 0.5f;
	protected float m_FinalZoomTime = 0.0f;

	// camera position
	public Vector3 PositionOffset = new Vector3(0.0f, 1.75f, 0.1f);
	public float PositionGroundLimit = -1.5f;
	public float PositionSpringStiffness = 0.01f;
	public float PositionSpringDamping = 0.25f;
	public float PositionSpring2Stiffness = 0.95f;
	public float PositionSpring2Damping = 0.25f;
	public float PositionKneeling = 0.025f;
	protected vp_Spring m_PositionSpring = null;		// spring for external forces (falling impact, bob, earthquakes)
	protected vp_Spring m_PositionSpring2 = null;		// 2nd spring for external forces (typically with stiffer spring settings)

	// camera rotation
	public Vector2 RotationPitchLimit = new Vector2(-90.0f, 90.0f);
	public Vector2 RotationYawLimit = new Vector2(-360.0f, 360.0f);
	protected float m_Pitch = 0.0f;
	protected float m_Yaw = 0.0f;
	public float RotationSpringStiffness = 0.01f;
	public float RotationSpringDamping = 0.25f;
	public float RotationKneeling = 0.025f;
	public float RotationStrafeRoll = 0.01f;
	protected vp_Spring m_RotationSpring = null;
	protected Vector2 m_InitialRotation = Vector2.zero;	// angle of camera at moment of startup

	// camera shake
	public float ShakeSpeed = 0.0f;
	public Vector3 ShakeAmplitude = new Vector3(10, 10, 0);
	protected Vector3 m_Shake = Vector3.zero;

	// camera bob
	public Vector4 BobRate = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);			// TIP: use x for a mech / dino like walk cycle. y should be (x * 2) for a nice classic curve of motion. typical defaults for y are 0.9 (rate) and 0.1 (amp)
	public Vector4 BobAmplitude = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);		// TIP: make x & y negative to invert the curve
	public float BobInputVelocityScale = 1.0f;								
	public float BobMaxInputVelocity = 1000;								// TIP: calibrate using 'Debug.Log(Controller.velocity.sqrMagnitude);'

	// camera bob step variables
	public delegate void BobStepDelegate();
	public BobStepDelegate BobStepCallback;
	public float BobStepThreshold = 10.0f;
	protected float m_LastUpBob = 0.0f;
	protected bool m_BobWasElevating = false;

	// velocities
	protected float m_HighestFallSpeed = 0.0f;
	protected bool m_WasGroundedLastFrame = false;

	// earthquake
	protected float m_EarthQuakeTime = 0.0f;
	protected Vector2 m_EarthQuakeMagnitude = Vector2.zero;
	protected float m_EarthQuakeWeaponShakeFactor = 1.0f;
	protected float m_EarthQuakeCameraRollFactor = 0.0f;

	// weapon camera
	protected GameObject m_WeaponCamera = null;

	// list of weapon objects
	protected List<GameObject> m_Weapons = new List<GameObject>();
	protected int m_CurrentWeaponID = 0;
	protected vp_FPSWeapon m_CurrentWeapon = null;
	protected vp_FPSShooter m_CurrentShooter = null;

	// IComparer to sort the weapons alphabetically. this is used to
	// provide some control over weapon order in the Unity Hierarchy
	public class WeaponComparer : IComparer
	{
		int IComparer.Compare(System.Object x, System.Object y)
		{
			return ((new CaseInsensitiveComparer()).Compare(((GameObject)x).name, ((GameObject)y).name));
		}
	}

	// weapon switching
	protected vp_Timer m_SwitchWeaponTimer = null;
	protected vp_Timer m_ShowWeaponTimer = null;
	public delegate void WeaponSwitchDelegate();

	// sound
	protected AudioListener m_AudioListener = null;


	//////////////////////////////////////////////////////////
	// properties
	//////////////////////////////////////////////////////////
	public int CurrentWeaponID { get { return (int)m_CurrentWeaponID; } }
	public vp_FPSWeapon CurrentWeapon { get { return m_CurrentWeapon; } }
	public vp_FPSShooter CurrentShooter { get { return m_CurrentShooter; } }
	public List<GameObject> Weapons { get { return m_Weapons; } }
	public AudioListener AudioListener { get { return m_AudioListener; } }
	public Vector3 PositionSpring{get{return m_PositionSpring.State;}}
	public Vector2 Angle
	{
		get	{	return new Vector2(m_Yaw, m_Pitch);	}
		set
		{
			if (value.y > 90)
				value.y -= 360;
			m_InitialRotation = Vector3.zero;	// discard initial editor rotation
			m_Yaw = value.x;
			m_Pitch = -value.y;
		}
	}


	///////////////////////////////////////////////////////////
	// in 'Awake' we do things that need to be run once at the
	// very beginning. NOTE: as of Unity 4, gameobject hierarchy
	// can not be altered in 'Awake'
	///////////////////////////////////////////////////////////
	protected new void Awake()
	{

		base.Awake();

		Controller = transform.root.GetComponent<CharacterController>();

		// detect angle of camera at moment of startup. this will be added to all mouse
		// input and is needed to retain initial rotation set by user in the editor.
		m_InitialRotation = new Vector2(transform.eulerAngles.y, transform.eulerAngles.x);

		// set parent gameobject layer to 'Player', so camera can exclude it
		transform.parent.gameObject.layer = vp_Layer.Player;
		foreach (Transform b in transform.parent.transform)
		{
			b.gameObject.layer = vp_Layer.Player;
		}

		// main camera initialization
		// render everything except body and weapon
		camera.cullingMask &= ~((1 << vp_Layer.Player) | (1 << vp_Layer.Weapon));
		camera.depth = 0;

		// weapon camera initialization
		// find a regular Unity Camera component existing in a child
		// gameobject to the FPSCamera's gameobject. if we don't find
		// a weapon cam, that's OK (some games don't have weapons)
		Camera weaponCam = null;
		foreach (Transform t in transform)
		{
			weaponCam = (Camera)t.GetComponent(typeof(Camera));
			if (weaponCam != null)
			{
				weaponCam.transform.localPosition = Vector3.zero;
				weaponCam.transform.localEulerAngles = Vector3.zero;
				weaponCam.clearFlags = CameraClearFlags.Depth;
				weaponCam.cullingMask = (1 << vp_Layer.Weapon);	// only render the weapon
				weaponCam.depth = 1;
				weaponCam.farClipPlane = 100;
				weaponCam.nearClipPlane = 0.01f;
				weaponCam.fov = 60;
				m_WeaponCamera = weaponCam.gameObject;
				break;
			}
		}

		// add the gameobjects of any weapon components to the weapon list,
		// and make them inactive
		Component[] weaponComponents;
		weaponComponents = GetComponentsInChildren<vp_FPSWeapon>();
		foreach (vp_FPSWeapon comp in weaponComponents)
		{
			m_Weapons.Insert(m_Weapons.Count, comp.gameObject);
		}

		// sort the weapons alphabetically. this allows the user to
		// order them by putting e.g. a number at the beginning of
		// their names in the hierarchy.
		IComparer comparer = new WeaponComparer();
		m_Weapons.Sort(comparer.Compare);

		// create springs for camera motion

		// primary position spring
		// this is used for all sorts of positional force acting on the camera
		m_PositionSpring = new vp_Spring(gameObject.transform, vp_Spring.TransformType.Position);
		m_PositionSpring.MinVelocity = 0.00001f;
		m_PositionSpring.RestState = PositionOffset;

		// secondary position spring
		// this is mainly intended for positional force from recoil, stomping and explosions
		m_PositionSpring2 = new vp_Spring(gameObject.transform, vp_Spring.TransformType.PositionAdditive);
		m_PositionSpring2.MinVelocity = 0.00001f;

		// rotation spring
		// this is used for all sorts of angular force acting on the camera
		m_RotationSpring = new vp_Spring(gameObject.transform, vp_Spring.TransformType.RotationAdditive);
		m_RotationSpring.MinVelocity = 0.00001f;

		m_AudioListener = (AudioListener)gameObject.AddComponent("AudioListener");

	}


	///////////////////////////////////////////////////////////
	// in 'Start' we do things that need to be run once at the
	// beginning, but potentially depend on all other scripts
	// first having run their 'Awake' calls.
	// NOTE: don't do anything here that depends on activity
	// in other 'Start' calls
	///////////////////////////////////////////////////////////
	protected new void Start()
	{

		base.Start();

		Refresh();

		// snap the camera to its start values when first activated
		SnapSprings();
		SnapZoom();

	}

	
	///////////////////////////////////////////////////////////
	// in 'Init' we do things that must be run once at the
	// beginning, but only after all other components have
	// run their 'Start' calls. this method is called once
	// by the vp_Component base class in its first 'Update'
	///////////////////////////////////////////////////////////
	protected override void Init()
	{

		base.Init();

		// initially unwield all weapons. this will initialize
		// weapon sorting and prevent all weapons from being
		// enabled at once on startup
		if(m_Weapons.Count > 0)
			SetWeapon(0);

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	protected new void Update()
	{

		base.Update();
		
		UpdateMouseLook();
		
	}


	///////////////////////////////////////////////////////////
	// actual rotation of the player model and camera is performed in
	// LateUpdate, since by then all game logic should be finished
	///////////////////////////////////////////////////////////
	protected void LateUpdate()
	{

		// rotate the parent gameobject (i.e. player model)
		// NOTE: that this rotation does not pitch the player model, it only uses yaw
		Quaternion xQuaternion = Quaternion.AngleAxis(m_Yaw + m_InitialRotation.x, Vector3.up);
		Quaternion yQuaternion = Quaternion.AngleAxis(0, Vector3.left);
		gameObject.transform.parent.transform.rotation = xQuaternion * yQuaternion;

		// pitch and yaw the camera
		yQuaternion = Quaternion.AngleAxis(m_Pitch - m_InitialRotation.y, Vector3.left);
		gameObject.camera.transform.rotation = xQuaternion * yQuaternion;

		// roll the camera
		gameObject.camera.transform.localEulerAngles += new Vector3(0, 0, m_RotationSpring.State.z);
		
	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	protected void FixedUpdate()
	{

		// zoom happens here for framerate independence
		if (m_FinalZoomTime > Time.time)
			UpdateZoom();

		// update fall impact, swaying, bobbing and earthquakes
		UpdateForces();

		m_PositionSpring.FixedUpdate();
		m_PositionSpring2.FixedUpdate();
		m_RotationSpring.FixedUpdate();
		
		// update camera and weapon shakes
		UpdateShakes();

		// prevent camera from going through objects
		DoCameraCollision();

	}


	///////////////////////////////////////////////////////////
	// calls various force handling methods once per FixedUpdate
	///////////////////////////////////////////////////////////
	protected void UpdateForces()
	{

		// handle falling impact
		DetectFallingImpact();

		// handle sway
		DoSwaying(Controller.velocity);

		// handle bob
		if (Controller.isGrounded)
			DoBob(Controller.velocity.sqrMagnitude);
		else
			DoBob(0);
		
		// handle earthquakes
		UpdateEarthQuake();
		
	}


	///////////////////////////////////////////////////////////
	// camera has its own falling impact logic to make it
	// independent of any particular controller logic
	///////////////////////////////////////////////////////////
	protected void DetectFallingImpact()
	{

		// store highest velocity
		if (Controller.velocity.y < m_HighestFallSpeed)
			m_HighestFallSpeed = Controller.velocity.y;

		// detect falling impact
		if (Controller.isGrounded && !m_WasGroundedLastFrame)
		{
			ApplyFallImpact(Mathf.Abs(m_HighestFallSpeed * (Time.smoothDeltaTime * 60.0f)));	// use smooth delta to avoid sending an fps spike as an impact
			m_HighestFallSpeed = 0.0f;
		}
		m_WasGroundedLastFrame = Controller.isGrounded;
	
	}


	///////////////////////////////////////////////////////////
	// prevents the camera from intersecting other objects by
	// raycasting from the controller to the camera and stopping
	// the camera on the first object hit
	///////////////////////////////////////////////////////////
	protected void DoCameraCollision()
	{

		// start position is the center of the character controller
		// and height of the camera PositionOffset. this will detect
		// objects between the camera and controller even if the
		// camera PositionOffset is far from the controller
		Vector3 startPos = Controller.transform.TransformPoint(0, PositionOffset.y, 0);

		// end position is the current camera position plus we'll move it
		// back the distance of our Controller.radius in order to reduce
		// camera clipping issues very close to walls
		// TIP: for solving such issues, you can also try reducing the
		// main camera's near clipping plane 
		Vector3 endPos = transform.position + (transform.position - startPos).normalized * Controller.radius;

		RaycastHit hit;
		if (Physics.Linecast(startPos, endPos, out hit,
			~((1 << vp_Layer.Player) | (1 << vp_Layer.Debris)))) // ignore the player and shell cases
		{
			if (!hit.collider.isTrigger)
				transform.position = hit.point - (hit.point - startPos).normalized * Controller.radius;
		}
		// uncomment this to draw a camera intersection debug line in the scene view
		//Debug.DrawLine(startPos, endPos, (hit.collider == null) ? Color.yellow : Color.red);

		// also, prevent the camera from ever going below the player's
		// feet (not even when up in the air)
		if (transform.localPosition.y < PositionGroundLimit)
			transform.localPosition = new Vector3(transform.localPosition.x,
											PositionGroundLimit, transform.localPosition.z);

	}


	///////////////////////////////////////////////////////////
	// applies various forces to the camera and weapon springs
	// in response to falling impact
	///////////////////////////////////////////////////////////
	public void ApplyFallImpact(float impact)
	{

		float posImpact = impact * PositionKneeling;
		float rotImpact = impact * RotationKneeling;

		// smooth step the impacts to make the springs react more subtly
		// from short falls, and more aggressively from longer falls

		posImpact = Mathf.SmoothStep(0, 1, posImpact);
		posImpact = Mathf.Clamp01(posImpact);

		rotImpact = Mathf.SmoothStep(0, 1, rotImpact);
		rotImpact = Mathf.SmoothStep(0, 1, rotImpact);
		rotImpact = Mathf.Clamp01(rotImpact);
		
		// apply impact to camera position spring
		if (m_PositionSpring != null)
			m_PositionSpring.AddForce(new Vector3(0, -posImpact, 0));

		// apply impact to camera rotation spring
		if (m_RotationSpring != null)
		{
			float roll = Random.value > 0.5f ? (rotImpact * 2) : -(rotImpact * 2);
			m_RotationSpring.AddForce(new Vector3(0, 0, roll));
		}

		// apply falling impact on the current weapon, if present
		if (m_CurrentWeapon != null)
			m_CurrentWeapon.ApplyFallImpact(posImpact);

	}


	///////////////////////////////////////////////////////////
	// pushes the camera position spring along the 'force' vector
	// for one frame. For external use.
	///////////////////////////////////////////////////////////
	public void AddForce(Vector3 force)
	{
		m_PositionSpring.AddForce(force);
	}

	public void AddForce(float x, float y, float z)
	{
		AddForce(new Vector3(x, y, z));
	}


	///////////////////////////////////////////////////////////
	// pushes the 2nd camera position spring along the 'force'
	// vector for one frame. for external use.
	///////////////////////////////////////////////////////////
	public void AddForce2(Vector3 force)
	{
		m_PositionSpring2.AddForce(force);
	}

	public void AddForce2(float x, float y, float z)
	{
		AddForce2(new Vector3(x, y, z));
	}


	///////////////////////////////////////////////////////////
	// twists the camera around its z vector for one frame
	///////////////////////////////////////////////////////////
	public void AddRollForce(float force)
	{
		m_RotationSpring.AddForce(new Vector3(0, 0, force));
	}


	///////////////////////////////////////////////////////////
	// rotates the camera for one frame
	///////////////////////////////////////////////////////////
	public void AddRotationForce(Vector3 force)
	{
		m_RotationSpring.AddForce(force);
	}


	public void AddRotationForce(float x, float y, float z)
	{
		AddRotationForce(new Vector3(x, y, z));
	}


	///////////////////////////////////////////////////////////
	// mouse look implementation with smooth filtering
	///////////////////////////////////////////////////////////
	protected void UpdateMouseLook()
	{

		// --- fetch mouse input ---

		// NOTE: if you want to hook the camera to another input source,
		// this is the place

		m_MouseMove.x = Input.GetAxisRaw("Mouse X");
		m_MouseMove.y = Input.GetAxisRaw("Mouse Y");

		// --- mouse smoothing ---

		// NOTE: if you experience mouse lagging in fullscreen, try disabling VSync

		// make sure the defined smoothing vars are within range
		MouseSmoothSteps = Mathf.Clamp(MouseSmoothSteps, 1, 20);
		MouseSmoothWeight = Mathf.Clamp01(MouseSmoothWeight);

		// keep mousebuffer at a maximum of (MouseSmoothSteps + 1) values
		while (m_MouseSmoothBuffer.Count > MouseSmoothSteps)
			m_MouseSmoothBuffer.RemoveAt(0);

		// add current input to mouse input buffer
		m_MouseSmoothBuffer.Add(m_MouseMove);

		// calculate mouse smoothing
		float weight = 1;
		Vector2 average = Vector2.zero;
		float averageTotal = 0.0f;
		for (int i = m_MouseSmoothBuffer.Count - 1; i > 0; i--)
		{
			average += m_MouseSmoothBuffer[i] * weight;
			averageTotal += (1.0f * weight);
			weight *= (MouseSmoothWeight / Delta);
		}

		// store the averaged input value
		averageTotal = Mathf.Max(1, averageTotal);
		Vector2 input = (average / averageTotal);

		// --- mouse acceleration ---

		float mouseAcceleration = 0.0f;

		float accX = Mathf.Abs(input.x);
		float accY = Mathf.Abs(input.y);

		if (MouseAcceleration)
		{
			mouseAcceleration = Mathf.Sqrt((accX * accX) + (accY * accY)) / Delta;
			mouseAcceleration = (mouseAcceleration <= MouseAccelerationThreshold) ? 0.0f : mouseAcceleration;
		}

		// --- update camera ---

		// modify pitch and yaw with input, sensitivity and acceleration
		m_Yaw += input.x * (MouseSensitivity.x + mouseAcceleration);
		m_Pitch += input.y * (MouseSensitivity.y + mouseAcceleration);

		// clamp angles
		m_Yaw = m_Yaw < -360F ? m_Yaw += 360F : m_Yaw;
		m_Yaw = m_Yaw > 360F ? m_Yaw -= 360F : m_Yaw;
		m_Yaw = Mathf.Clamp(m_Yaw, RotationYawLimit.x, RotationYawLimit.y);
		m_Pitch = m_Pitch < -360F ? m_Pitch += 360F : m_Pitch;
		m_Pitch = m_Pitch > 360F ? m_Pitch -= 360F : m_Pitch;
		m_Pitch = Mathf.Clamp(m_Pitch, RotationPitchLimit.x, RotationPitchLimit.y);

	}


	///////////////////////////////////////////////////////////
	// interpolates to the target FOV value
	///////////////////////////////////////////////////////////
	protected void UpdateZoom()
	{

		RenderingZoomDamping = Mathf.Max(RenderingZoomDamping, 0.01f);
		float zoom = 1 - ((m_FinalZoomTime - Time.time) / RenderingZoomDamping);
		gameObject.camera.fov = Mathf.SmoothStep(gameObject.camera.fov, RenderingFieldOfView, zoom);

	}


	///////////////////////////////////////////////////////////
	// interpolates to the target FOV using 'RenderingZoomDamping'
	// as interval
	///////////////////////////////////////////////////////////
	public void Zoom()
	{
		m_FinalZoomTime = Time.time + RenderingZoomDamping;
	}


	///////////////////////////////////////////////////////////
	// instantly sets camera to the target FOV
	///////////////////////////////////////////////////////////
	public void SnapZoom()
	{

		gameObject.camera.fov = RenderingFieldOfView;

	}

	
	///////////////////////////////////////////////////////////
	// updates the procedural shaking of the camera.
	// NOTE: x and y shakes are applied to the actual controls of the
	// character model. if you increase the shakes, you will essentially
	// get a drunken / sick / drugged movement experience.
	// this can also be used for i.e. sniper breathing since it will
	// affect aiming.
	///////////////////////////////////////////////////////////
	protected void UpdateShakes()
	{

		// apply camera shakes
		if (ShakeSpeed != 0.0f)
		{
			m_Yaw -= m_Shake.x;			// subtract shake from last frame or camera will drift
			m_Pitch -= m_Shake.y;
			m_Shake = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(ShakeSpeed), ShakeAmplitude);
			m_Yaw += m_Shake.x;			// apply new shake
			m_Pitch += m_Shake.y;
			m_RotationSpring.AddForce(0, 0, m_Shake.z);
		}
	
	}


	///////////////////////////////////////////////////////////
	// speed should be the magnitude speed of the character
	// controller. if controller has no ground contact, '0.0f'
	// should be passed and the bob will fade to a halt.
	///////////////////////////////////////////////////////////
	protected void DoBob(float speed)
	{

		if (BobAmplitude == Vector4.zero || BobRate == Vector4.zero)
			return;

		// scale and limit input velocity
		speed = Mathf.Min(speed * BobInputVelocityScale, BobMaxInputVelocity);

		// reduce number of decimals to avoid floating point imprecision bugs
		speed = Mathf.Round(speed * 1000.0f) / 1000.0f;
		
		float upAmp = (speed * (BobAmplitude.y * -0.0001f));
		float upBob = (Mathf.Cos(Time.time * (BobRate.y * 10.0f))) * upAmp;

		float sideAmp = (speed * (BobAmplitude.x * 0.0001f));
		float sideBob = (Mathf.Cos(Time.time * (BobRate.x * 10.0f))) * sideAmp;

		float forwAmp = (speed * (BobAmplitude.z * 0.0001f));
		float forwBob = (Mathf.Cos(Time.time * (BobRate.z * 10.0f))) * forwAmp;

		float rollAmp = (speed * (BobAmplitude.w * 0.0001f));
		float rollBob = (Mathf.Cos(Time.time * (BobRate.w * 10.0f))) * rollAmp;


		m_PositionSpring.AddForce(new Vector3(sideBob, upBob, forwBob));
		m_RotationSpring.AddForce(new Vector3(0, 0, rollBob));

		DetectBobStep(speed, upBob);

	}


	///////////////////////////////////////////////////////////
	// the bob step callback is triggered when the vertical
	// camera bob reaches its bottom value (provided that the
	// speed is higher than the bob step threshold). this can
	// can be used for various footstep sounds and behaviors.
	///////////////////////////////////////////////////////////
	protected void DetectBobStep(float speed, float upBob)
	{

		if (BobStepCallback == null)
			return;

		if (speed < BobStepThreshold)
			return;

		bool elevating = (m_LastUpBob < upBob) ? true : false;
		m_LastUpBob = upBob;

		if (elevating && !m_BobWasElevating)
			BobStepCallback();

		m_BobWasElevating = elevating;

	}


	///////////////////////////////////////////////////////////
	// applies swaying forces to the camera in response to user
	// input and character controller motion.
	///////////////////////////////////////////////////////////
	protected void DoSwaying(Vector3 velocity)
	{

		Vector3 localVelocity = transform.InverseTransformDirection(velocity / 60);
		AddRollForce(localVelocity.x * RotationStrafeRoll);

	}


	///////////////////////////////////////////////////////////
	// NOTE: this should not be the logic governing a world-wide
	// quake. other objects may also need updating e.g. on the
	// server. rather, when your main game logic has determined
	// the amplitude and length of an earthquake, feed it to this
	// method to update this particular client.
	///////////////////////////////////////////////////////////
	protected void UpdateEarthQuake()
	{

		if (m_EarthQuakeTime <= 0.0f)
			return;

		m_EarthQuakeTime -= 0.0166f * Delta;	 // reduce earthquake time by 1 every second

		if (!Controller.isGrounded)
			return;

		// the horisontal move is a perlin noise value between 0 and
		// 'm_EarthQuakeMagnitude' (depending on 'm_EarthQuakeTime').
		// horizMove will ease out during the last second.
		Vector3 horizMove = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(1), new Vector3(m_EarthQuakeMagnitude.x,
													0, m_EarthQuakeMagnitude.x)) * Mathf.Min(m_EarthQuakeTime, 1.0f);

		// the vertical move is half the earthquake magnitude and
		// has a 30% chance of occuring each frame. when it does,
		// it alternates between positive and negative. this produces
		// sharp shakes with nice spring smoothness inbetween.
		// vertMove will ease out during the last second.
		float vertMove = 0;
		if (UnityEngine.Random.value < 0.3f)
		{
			vertMove = UnityEngine.Random.Range(0, (m_EarthQuakeMagnitude.y * 0.35f)) * Mathf.Min(m_EarthQuakeTime, 1.0f);
			if (m_PositionSpring.State.y >= m_PositionSpring.RestState.y)
				vertMove = -vertMove;
		}

		// apply horisontal move to the camera spring.
		// NOTE: this will only shake the camera, though it will give
		// the appearance of pushing the player around.

		// TIP: for a more physical feel, apply the horisontal move to the
		// the controller's horisontal plane instead of the camera spring.
		// note however that this will have gameplay implications since
		// the character will now move in world space.
		// example: YourFPSController.AddForce(horizMove);
		m_PositionSpring.AddForce(horizMove);

		// apply earthquake roll force on the camera rotation spring
		m_RotationSpring.AddForce(new Vector3(0, 0, -horizMove.x * 2) * m_EarthQuakeCameraRollFactor);

		// apply earthquake force on the current weapon, if present
		if (m_CurrentWeapon != null)
			m_CurrentWeapon.AddForce(new Vector3(0, 0, -horizMove.z * 0.015f) * m_EarthQuakeWeaponShakeFactor,
					new Vector3(vertMove * 2, -horizMove.x, horizMove.x * 2) * m_EarthQuakeWeaponShakeFactor);

		// vertical move is always applied to the camera spring. this
		// feels cool though it impairs aiming.
		// then again ... it's a friggin' earthquake! :)
		m_PositionSpring.AddForce(new Vector3(0, vertMove, 0));

	}


	///////////////////////////////////////////////////////////
	// helper method to start an earthquake. shakes the camera
	// pos using <magnitude> strength for <duration> seconds.
	// will ease out in the last second.
	///////////////////////////////////////////////////////////
	public void DoEarthQuake(float x, float y, float duration,
							float weaponShakeFactor = 1.0f, float cameraRollFactor = 0.0f)
	{

		m_EarthQuakeMagnitude = new Vector2(x, y);
		m_EarthQuakeTime = duration;
		m_EarthQuakeWeaponShakeFactor = weaponShakeFactor;
		m_EarthQuakeCameraRollFactor = cameraRollFactor;

	}


	///////////////////////////////////////////////////////////
	// stops any ongoing earthquake
	///////////////////////////////////////////////////////////
	public void StopEarthQuake()
	{
		m_EarthQuakeTime = 0.0f;
	}


	///////////////////////////////////////////////////////////
	// helper method to produce an explosion impact. 'force'
	// values larger than 1.0 are not recommended.
	///////////////////////////////////////////////////////////
	public void DoBomb(float force)
	{

		DoBomb((new Vector3(1.0f, -4.0f, 1.0f) * force),
			1,
			3.5f,
			new Vector2(force * 0.5f, force * 0.5f),
			1,
			0.25f,
			0.5f,
			Vector3.zero,
			(new Vector3(-0.3f, 0.1f, 0.5f) * force)	
			);

	}


	///////////////////////////////////////////////////////////
	// performs a highly customizable explosion impact on the
	// camera and current weapon
	///////////////////////////////////////////////////////////
	public void DoBomb(Vector3 positionForce, float minRollForce, float maxRollForce,
		Vector2 earthQuakeForce, float earthQuakeTime, float earthQuakeWeaponShakeFactor,
		float earthQuakeCameraRollFactor, Vector3 weaponPositionForce,
		Vector3 weaponRotationForce)
	{

		AddForce2(positionForce);

		float roll = Random.Range(minRollForce, maxRollForce);
		if (Random.value > 0.5f)
			roll = -roll;
		AddRollForce(roll);

		DoEarthQuake(earthQuakeForce.x, earthQuakeForce.y, earthQuakeTime,
			earthQuakeWeaponShakeFactor, earthQuakeCameraRollFactor);

		CurrentWeapon.AddForce(weaponPositionForce, weaponRotationForce);

	}


	///////////////////////////////////////////////////////////
	// helper method to make the ground shake as if a large
	// dinosaur or mech is approaching. great for bosses!
	///////////////////////////////////////////////////////////
	public void DoStomp(float force)
	{

		AddForce2(new Vector3(0.0f, -1.0f, 0.0f) * force);
		CurrentWeapon.AddForce(Vector3.zero, new Vector3(-0.25f, 0.0f, 0.0f) * force);

	}


	///////////////////////////////////////////////////////////
	// this method is called to reset various camera settings,
	// typically after creating or loading a camera
	///////////////////////////////////////////////////////////
	public override void Refresh()
	{

		if (!Application.isPlaying)
			return;

		m_PositionSpring.Stiffness =
			new Vector3(PositionSpringStiffness, PositionSpringStiffness, PositionSpringStiffness);
		m_PositionSpring.Damping = Vector3.one -
			new Vector3(PositionSpringDamping, PositionSpringDamping, PositionSpringDamping);

		m_PositionSpring2.Stiffness =
			new Vector3(PositionSpring2Stiffness, PositionSpring2Stiffness, PositionSpring2Stiffness);
		m_PositionSpring2.Damping = Vector3.one -
			new Vector3(PositionSpring2Damping, PositionSpring2Damping, PositionSpring2Damping);

		m_RotationSpring.Stiffness =
			new Vector3(RotationSpringStiffness, RotationSpringStiffness, RotationSpringStiffness);
		m_RotationSpring.Damping = Vector3.one -
			new Vector3(RotationSpringDamping, RotationSpringDamping, RotationSpringDamping);


		m_PositionSpring.MinState.y = PositionGroundLimit;
		m_PositionSpring.RestState = PositionOffset;

		m_PositionSpring2.MinState.y = (-PositionOffset.y) + PositionGroundLimit;
		// we don't force a position offset for position spring 2

		Zoom();

		if (CurrentWeapon != null)
			CurrentWeapon.Refresh();
		if (CurrentShooter != null)
			CurrentShooter.Refresh();

	}

	
	///////////////////////////////////////////////////////////
	// [OBSOLETE] (please use the 'angle' property instead)
	// sets the pitch and yaw angles of the FPSCamera and thereby
	// the character controller, e.g. for when spawning or
	// teleporting a player to a new location
	///////////////////////////////////////////////////////////
	public void SetAngle(float yaw, float pitch)
	{

		Angle = new Vector2(yaw, pitch);

	}
	

	///////////////////////////////////////////////////////////
	// resets all the springs to their default positions, i.e.
	// for when loading a new camera or switching a weapon
	///////////////////////////////////////////////////////////
	public void SnapSprings()
	{

		if (m_PositionSpring != null)
		{
			m_PositionSpring.RestState = PositionOffset;
			m_PositionSpring.State = PositionOffset;
			m_PositionSpring.Stop();
		}

		if (m_RotationSpring != null)
		{
			m_RotationSpring.RestState = Vector3.zero;
			m_RotationSpring.State = Vector3.zero;
			m_RotationSpring.Stop();
		}

	}


	///////////////////////////////////////////////////////////
	// stops all the springs
	///////////////////////////////////////////////////////////
	public void StopSprings()
	{

		if (m_PositionSpring != null)
			m_PositionSpring.Stop();

		if (m_PositionSpring2 != null)
			m_PositionSpring2.Stop();

		if (m_RotationSpring != null)
			m_RotationSpring.Stop();

	}


	///////////////////////////////////////////////////////////
	// moves the current weapon model to its exit offset, changes
	// the weapon model and moves the new weapon into view.
	// an optional callback may be provided to execute a method
	// as the new weapon becomes active
	///////////////////////////////////////////////////////////
	public void SwitchWeapon(int weapon, WeaponSwitchDelegate callback = null)
	{
		
		// prevent firing while putting the current weapon away
		if (CurrentShooter != null)
			CurrentShooter.PreventFiring(0.5f);

		if (CurrentWeapon != null)
		{
			CurrentWeapon.StateManager.Reset();

			// rotate and move the current weapon out of view
			CurrentWeapon.PositionOffset = CurrentWeapon.PositionExitOffset;
			CurrentWeapon.RotationOffset = CurrentWeapon.RotationExitOffset;
			CurrentWeapon.Refresh();

			// play unwield sound
			if (CurrentWeapon.audio != null)
			{
				if (CurrentWeapon.SoundUnWield != null)
				{
					CurrentWeapon.audio.pitch = 1;
					CurrentWeapon.audio.PlayOneShot(CurrentWeapon.SoundUnWield);
				}
			}

		}

		// cancel any already ongoing weapon switching activity
		CancelWeaponSwitch();
		
		// create a new event to switch the weapon once model has had
		// time enough to rotate out of view. 0.15 seconds should do.
		m_SwitchWeaponTimer = vp_Timer.In(0.15f, delegate()
		{

			// switch weapon
			SetWeapon(weapon);
		
			// prevent firing while taking out the new weapon
			if (CurrentShooter != null)
				CurrentShooter.PreventFiring(0.5f);

			// force the 'rotated down' angle and position onto the new
			// weapon when it spawns, or it will pop into view
			if (CurrentWeapon != null)
			{
				CurrentWeapon.RotationOffset = CurrentWeapon.RotationExitOffset;
				CurrentWeapon.PositionOffset = CurrentWeapon.PositionExitOffset;
				CurrentWeapon.SnapSprings();
				CurrentWeapon.SnapPivot();
				CurrentWeapon.SnapZoom();
				CurrentWeapon.Refresh();
			}

			// create an event to show the new weapon in 0.15 seconds
			m_ShowWeaponTimer = vp_Timer.In(0.15f, delegate()
			{
				if (CurrentWeapon != null)
				{

					// we do this by smoothly restoring the loaded weapon's
					// desired position and angle
					CurrentWeapon.PositionOffset = CurrentWeapon.DefaultPosition;
					CurrentWeapon.RotationOffset = CurrentWeapon.DefaultRotation;
					CurrentWeapon.Refresh();

					// play wield sound
					if (CurrentWeapon.audio != null)
					{
						if (CurrentWeapon.SoundWield != null)
						{
							CurrentWeapon.audio.pitch = 1;
							if (CurrentWeapon.audio.gameObject.active)
								CurrentWeapon.audio.PlayOneShot(CurrentWeapon.SoundWield);
						}
					}
				}

				// execute user defined callback, if provided
				if (callback != null)
					callback();

			});
		});
		
	}


	///////////////////////////////////////////////////////////
	// cancels any already ongoing weapon switching activity
	///////////////////////////////////////////////////////////
	public void CancelWeaponSwitch()
	{

		if (m_SwitchWeaponTimer != null)
			m_SwitchWeaponTimer.Cancel();
		if (m_ShowWeaponTimer != null)
			m_ShowWeaponTimer.Cancel();

	}


	///////////////////////////////////////////////////////////
	// any child objects to the FPSCamera component are treated
	// as weapons. these are put in the 'm_Weapons' list in
	// alphabetical order. this method will first disable the
	// currently activated weapon, then activate the one with
	// index 'i'. if 'i' is zero, no child will be activated.
	///////////////////////////////////////////////////////////
	public void SetWeapon(int i)
	{
		
		if (m_Weapons.Count < 1)
		{
			Debug.LogError("Error: Tried to set weapon with an empty weapon list.");
			return;
		}

		if (i < 0 || i > m_Weapons.Count)
		{
			Debug.LogError("Error: Weapon list does not have a weapon with index: " + i);
			return;
		}

		// clear current weapon, set all child objects inactive
		// and clear the shooter
		m_CurrentWeapon = null;
		foreach (Transform t in transform)
		{
			if(t.gameObject != m_WeaponCamera)
				DeactivateWhenSilent(t);
		}
		m_CurrentShooter = null;

		// activate the new weapon
		m_CurrentWeaponID = i;
		if (m_CurrentWeaponID > 0)
		{

			// set parent transform of new weapon active
			m_Weapons[m_CurrentWeaponID - 1].transform.parent.gameObject.SetActiveRecursively(true);

			// store the components of the new weapon and shooter
			m_CurrentWeapon = (vp_FPSWeapon)m_Weapons[m_CurrentWeaponID - 1].GetComponentInChildren(typeof(vp_FPSWeapon));
			m_CurrentShooter = (vp_FPSShooter)m_Weapons[m_CurrentWeaponID - 1].GetComponentInChildren(typeof(vp_FPSShooter));

			// activate the new shooter and camera, and make sure the weapon
			// renderer is enabled
			if (m_CurrentWeapon != null)
			{

				if (m_CurrentShooter != null)
				{
					if (m_CurrentShooter.MuzzleFlash != null)
						m_CurrentShooter.MuzzleFlash.SetActiveRecursively(true);
				}
				if (m_CurrentWeapon.gameObject.renderer != null)
					m_CurrentWeapon.gameObject.renderer.enabled = true;

				CurrentWeapon.PositionOffset = CurrentWeapon.DefaultPosition;
				CurrentWeapon.RotationOffset = CurrentWeapon.DefaultRotation;
	
				m_CurrentWeapon.SetPivotVisible(false);
	
			}

		}
		
		Refresh();

	}


	///////////////////////////////////////////////////////////
	// the purpose of this method is to allow disabling of a
	// weapon without cutting off its currently playing fire
	// sound. if a sound is playing on its audio source the
	// weapon will be made invisible and deactivated properly
	// only after the sound has stopped playing. if the weapon
	// isn't playing any sound it will be instantly deactivated
	///////////////////////////////////////////////////////////
	protected void DeactivateWhenSilent(Transform t)
	{

		// only perform the audio check on currently active transforms
		// to avoid scanning for child components in too large a tree.
		if (t.gameObject.active)
		{
			foreach (AudioSource a in t.GetComponentsInChildren<AudioSource>())
			{
				if (a.isPlaying && !a.loop)		// looping sounds will be cut off
				{

					// first make sure that the weapon we are about to hide is
					// not the current weapon. if so, the player has switched
					// away from and back to the previous weapon quickly and
					// we must abort this delayed deactivation
					if ((CurrentWeapon != null) && (CurrentWeapon.transform.parent.gameObject == t.gameObject))
					{
						foreach (Renderer r in t.GetComponentsInChildren<Renderer>())
						{
							r.enabled = true;
						} return;
					}
					// temporarily make the weapon invisible by disabling the renderer
					foreach (Renderer r in t.GetComponentsInChildren<Renderer>())
					{
						r.enabled = false;
					}
					// we can't disable the gameobject properly yet because an audio-
					// source is still playing, so retry in 0.1 seconds
					vp_Timer.In(0.1f, delegate()
					{
						DeactivateWhenSilent(t);
					});
					return;

				}
			}
		}

		// if we reach this far there are no audio sources playing on
		// the transform, so deactivate the gameobject properly after
		// making sure the renderer is enabled
		foreach (Renderer r in t.GetComponentsInChildren<Renderer>())
		{
			r.enabled = true;
		}

		t.gameObject.SetActiveRecursively(false);

	}

	
	///////////////////////////////////////////////////////////
	// sets layer of the weapon model, for controlling which
	// camera the weapon is rendered by
	///////////////////////////////////////////////////////////
	public void SetWeaponLayer(int layer)
	{

		if (m_CurrentWeaponID < 1 || m_CurrentWeaponID > m_Weapons.Count)
			return;

		vp_Layer.Set(m_Weapons[m_CurrentWeaponID - 1], layer, true);

	}


}

	