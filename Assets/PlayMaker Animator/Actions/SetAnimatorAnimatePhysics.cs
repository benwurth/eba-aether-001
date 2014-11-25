// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Animator")]
	[Tooltip("When turned on, animations will be executed in the physics loop. This is only useful in conjunction with kinematic rigidbodies.")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1060")]
	public class SetAnimatorAnimatePhysics: FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(Animator))]
		[Tooltip("The Target. An Animator component is required")]
		public FsmOwnerDefault gameObject;
		
		[Tooltip("If true, animations will be executed in the physics loop. This is only useful in conjunction with kinematic rigidbodies.")]
		public FsmBool animatePhysics;
		
		private Animator _animator;
		
		public override void Reset()
		{
			gameObject = null;
			animatePhysics= null;
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
			
			DoAnimatePhysics();
			
			Finish();
			
		}
	
		void DoAnimatePhysics()
		{		
			if (_animator==null)
			{
				return;
			}
			
			_animator.animatePhysics = animatePhysics.Value;
			
		}
		
	}
}