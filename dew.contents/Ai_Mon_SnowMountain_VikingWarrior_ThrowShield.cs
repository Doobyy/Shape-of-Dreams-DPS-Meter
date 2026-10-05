using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_VikingWarrior_ThrowShield : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__14 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Mon_SnowMountain_VikingWarrior_ThrowShield _003C_003E4__this;

		private BoxTelegraphController _003Cline_003E5__2;

		private float _003Ctime_003E5__3;

		private Vector3 _003Cpoint_003E5__4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003COnCreateSequenced_003Ed__14(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			Ai_Mon_SnowMountain_VikingWarrior_ThrowShield ai_Mon_SnowMountain_VikingWarrior_ThrowShield = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)ai_Mon_SnowMountain_VikingWarrior_ThrowShield).isServer)
				{
					return false;
				}
				ai_Mon_SnowMountain_VikingWarrior_ThrowShield.DestroyOnDeath(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.caster);
				_003Cline_003E5__2 = ai_Mon_SnowMountain_VikingWarrior_ThrowShield.lineObject.GetComponentInChildren<BoxTelegraphController>();
				ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.caster.Animation.PlayAbilityAnimation(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.castAnim);
				ai_Mon_SnowMountain_VikingWarrior_ThrowShield.FxPlayNetworked(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.lineObject, ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.caster);
				ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.caster.Control.StartDaze(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.castDuration + ai_Mon_SnowMountain_VikingWarrior_ThrowShield.atkDelay + ai_Mon_SnowMountain_VikingWarrior_ThrowShield.dashDuration);
				ai_Mon_SnowMountain_VikingWarrior_ThrowShield.FxPlayNetworked(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.fxTelegraph, ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.target);
				_003Ctime_003E5__3 = Time.time;
				goto IL_01d4;
			case 1:
			{
				_003C_003E1__state = -1;
				ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.caster.Control.RotateTowards(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.target, immediately: true);
				float height = Vector3.Distance(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.target.position, ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.caster.position);
				_003Cline_003E5__2.height = height;
				Vector3 normalized = (ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.target.position - ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.caster.position).normalized;
				_003Cline_003E5__2.transform.forward = normalized;
				Vector3 position = (ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.caster.position + ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.target.position) / 2f;
				_003Cline_003E5__2.transform.position = position;
				goto IL_01d4;
			}
			case 2:
				_003C_003E1__state = -1;
				ai_Mon_SnowMountain_VikingWarrior_ThrowShield.FxPlayNetworked(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.fxDashTelegraph, _003Cpoint_003E5__4, null);
				ai_Mon_SnowMountain_VikingWarrior_ThrowShield.CreateAbilityInstance<Ai_Mon_SnowMountain_VikingWarrior_Shield>(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.caster.position, null, new CastInfo(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.caster, _003Cpoint_003E5__4));
				ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.caster.Control.StartDaze(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.postDelay);
				_003C_003E2__current = new SI.WaitForSeconds(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.postDelay);
				_003C_003E1__state = 3;
				return true;
			case 3:
				{
					_003C_003E1__state = -1;
					ai_Mon_SnowMountain_VikingWarrior_ThrowShield.FxStopNetworked(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.fxDashTelegraph);
					ai_Mon_SnowMountain_VikingWarrior_ThrowShield.Destroy();
					return false;
				}
				IL_01d4:
				if (Time.time - _003Ctime_003E5__3 < ai_Mon_SnowMountain_VikingWarrior_ThrowShield.castDuration)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				_003Cpoint_003E5__4 = ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.target.agentPosition;
				_003Cpoint_003E5__4 = Dew.GetPositionOnGround(_003Cpoint_003E5__4);
				ai_Mon_SnowMountain_VikingWarrior_ThrowShield.FxStopNetworked(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.startEffect);
				ai_Mon_SnowMountain_VikingWarrior_ThrowShield.info.caster.Animation.PlayAbilityAnimation(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.atkAnim);
				_003C_003E2__current = new SI.WaitForSeconds(ai_Mon_SnowMountain_VikingWarrior_ThrowShield.atkDelay);
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	public DewCollider range;

	public ScalingValue dmgFactor;

	public GameObject lineObject;

	public DewAnimationClip castAnim;

	public DewAnimationClip atkAnim;

	public GameObject fxTelegraph;

	public GameObject fxDashTelegraph;

	public GameObject fxFinishAtk;

	public GameObject fxHit;

	public DewEase ease;

	public float atkDelay;

	public float dashDuration;

	public float castDuration;

	public float postDelay;

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__14))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	[ClientRpc]
	private void OnFinishDisplacementRoutine()
	{
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_OnFinishDisplacementRoutine()
	{
	}

	protected static void InvokeUserCode_OnFinishDisplacementRoutine(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
	}

	static Ai_Mon_SnowMountain_VikingWarrior_ThrowShield()
	{
	}
}
