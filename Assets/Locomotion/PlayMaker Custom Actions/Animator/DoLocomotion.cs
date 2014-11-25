// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Animator")]
	[Tooltip("Bridge for the Locomotion system.")]
	//[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1066")]
	public class DoLocomotion : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(Animator))]
		[Tooltip("The target. An Animator component is required")]
		public FsmOwnerDefault gameObject;
		
		[Tooltip("The speed value")]
		public FsmFloat speed;
		
		[Tooltip("Optional: The angle value")]
		public FsmFloat angle;
		
		[Tooltip("Repeat every frame. Useful when changing over time.")]
		public bool everyFrame;
		
		private Animator _animator;	
		protected Locomotion _locomotion;
		
		public override void Reset()
		{
			gameObject = null;
			speed = null;
			angle = null;
			everyFrame = true;
		}
		
		public override void OnEnter()
		{
			// get the animator component
			var go = Fsm.GetOwnerDefaultTarget(gameObject);
			
			if (go==null)
			{
				Finish();
				return;
			}
			
			_animator = go.GetComponent<Animator>();
			
			if (_animator==null)
			{
				Finish();
				return;
			}
			
			_locomotion = new Locomotion(_animator);
			
			SetLocomotion();
			
			if (!everyFrame) 
			{
				Finish();
			}
		}

		
		public override void OnUpdate() 
		{
			SetLocomotion();
		}
		
		public void SetLocomotion()
		{
			_locomotion.Do(speed.Value, angle.Value);
		}
		
	}
}