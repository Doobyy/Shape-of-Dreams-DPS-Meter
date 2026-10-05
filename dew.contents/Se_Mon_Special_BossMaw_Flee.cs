using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Se_Mon_Special_BossMaw_Flee : StatusEffect
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__7 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Se_Mon_Special_BossMaw_Flee _003C_003E4__this;

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
		public _003COnCreateSequenced_003Ed__7(int _003C_003E1__state)
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
			Se_Mon_Special_BossMaw_Flee se_Mon_Special_BossMaw_Flee = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)se_Mon_Special_BossMaw_Flee).isServer)
				{
					return false;
				}
				se_Mon_Special_BossMaw_Flee._ge = Dew.FindActorOfType<Ge_CallOfTheRavenous>();
				se_Mon_Special_BossMaw_Flee.DoInvulnerable();
				if (se_Mon_Special_BossMaw_Flee.info.caster.Status.TryGetStatusEffect<Se_Mon_Special_BossMaw_ShieldConversion>(out var effect))
				{
					effect.Destroy();
				}
				se_Mon_Special_BossMaw_Flee.info.caster.Visual.HideGroundMarker();
				se_Mon_Special_BossMaw_Flee.info.caster.Animation.StopAbilityAnimation();
				se_Mon_Special_BossMaw_Flee.info.caster.Control.Stop();
				se_Mon_Special_BossMaw_Flee.info.caster.Control.CancelOngoingChannels();
				se_Mon_Special_BossMaw_Flee.info.caster.Control.ClearActionQueue();
				se_Mon_Special_BossMaw_Flee.info.caster.Visual.LoadModelDefault();
				se_Mon_Special_BossMaw_Flee.info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
				se_Mon_Special_BossMaw_Flee.info.caster.Animation.PlayAbilityAnimation(se_Mon_Special_BossMaw_Flee.staggerClip);
				_003C_003E2__current = new SI.WaitForSeconds(se_Mon_Special_BossMaw_Flee.staggerDuration);
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
				_003C_003E1__state = -1;
				se_Mon_Special_BossMaw_Flee.FxPlayNetworked(se_Mon_Special_BossMaw_Flee.fxFlee, se_Mon_Special_BossMaw_Flee.info.caster);
				_003C_003E2__current = new SI.WaitForSeconds(se_Mon_Special_BossMaw_Flee.fleeDuration);
				_003C_003E1__state = 2;
				return true;
			case 2:
			{
				_003C_003E1__state = -1;
				Vector3 positionOnGround = Dew.GetPositionOnGround(se_Mon_Special_BossMaw_Flee.info.caster.agentPosition);
				se_Mon_Special_BossMaw_Flee.FxStopNetworked(se_Mon_Special_BossMaw_Flee.fxFlee);
				se_Mon_Special_BossMaw_Flee.FxPlayNetworked(se_Mon_Special_BossMaw_Flee.fxEnd, se_Mon_Special_BossMaw_Flee.info.caster);
				Dew.CreateActor<Shrine_CallOfTheRavenous>(positionOnGround, null);
				se_Mon_Special_BossMaw_Flee.info.caster.Visual.DisableRenderers();
				se_Mon_Special_BossMaw_Flee.info.caster.Destroy();
				_003C_003E2__current = new SI.WaitForSeconds(1f);
				_003C_003E1__state = 3;
				return true;
			}
			case 3:
				_003C_003E1__state = -1;
				se_Mon_Special_BossMaw_Flee.Destroy();
				return false;
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

	public float staggerDuration;

	public float fleeDuration;

	public DewAnimationClip staggerClip;

	public DewAnimationClip fleeClip;

	public GameObject fxFlee;

	public GameObject fxEnd;

	private Ge_CallOfTheRavenous _ge;

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__7))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	protected override void OnDestroyActor()
	{
	}

	private void MirrorProcessed()
	{
	}
}
