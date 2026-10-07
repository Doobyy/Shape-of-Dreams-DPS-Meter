using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_Tiger_ShadowWalk : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__8 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Mon_Ink_Tiger_ShadowWalk _003C_003E4__this;

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
		public _003COnCreateSequenced_003Ed__8(int _003C_003E1__state)
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
			Ai_Mon_Ink_Tiger_ShadowWalk ai_Mon_Ink_Tiger_ShadowWalk = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)ai_Mon_Ink_Tiger_ShadowWalk).isServer)
				{
					return false;
				}
				ai_Mon_Ink_Tiger_ShadowWalk.DestroyOnDeath(ai_Mon_Ink_Tiger_ShadowWalk.info.caster);
				ai_Mon_Ink_Tiger_ShadowWalk.info.caster.Control.StartDaze(ai_Mon_Ink_Tiger_ShadowWalk.castDuration + ai_Mon_Ink_Tiger_ShadowWalk.shadowWalkduration);
				ai_Mon_Ink_Tiger_ShadowWalk.FxPlayNetworked(ai_Mon_Ink_Tiger_ShadowWalk.castEffect, ai_Mon_Ink_Tiger_ShadowWalk.info.caster);
				_003C_003E2__current = new SI.WaitForSeconds(ai_Mon_Ink_Tiger_ShadowWalk.castDuration);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				ai_Mon_Ink_Tiger_ShadowWalk.FxStopNetworked(ai_Mon_Ink_Tiger_ShadowWalk.castEffect);
				ai_Mon_Ink_Tiger_ShadowWalk.info.caster.Visual.DisableRenderers();
				ai_Mon_Ink_Tiger_ShadowWalk.CreateBasicEffect(ai_Mon_Ink_Tiger_ShadowWalk.info.caster, new UntargetableEffect(), ai_Mon_Ink_Tiger_ShadowWalk.shadowWalkduration);
				ai_Mon_Ink_Tiger_ShadowWalk._dest = ai_Mon_Ink_Tiger_ShadowWalk.info.caster.agentPosition + UnityEngine.Random.insideUnitCircle.ToXZ() * UnityEngine.Random.Range(ai_Mon_Ink_Tiger_ShadowWalk.range.x, ai_Mon_Ink_Tiger_ShadowWalk.range.y);
				if (!ai_Mon_Ink_Tiger_ShadowWalk.info.target.IsNullInactiveDeadOrKnockedOut() && UnityEngine.Random.value > 0.4f)
				{
					ai_Mon_Ink_Tiger_ShadowWalk._dest = ai_Mon_Ink_Tiger_ShadowWalk.info.target.agentPosition + (ai_Mon_Ink_Tiger_ShadowWalk.info.target.agentPosition - ai_Mon_Ink_Tiger_ShadowWalk.info.caster.agentPosition).normalized * ai_Mon_Ink_Tiger_ShadowWalk.backDis;
				}
				ai_Mon_Ink_Tiger_ShadowWalk._dest = Dew.GetPositionOnGround(ai_Mon_Ink_Tiger_ShadowWalk._dest);
				ai_Mon_Ink_Tiger_ShadowWalk._dest = Dew.GetValidAgentDestination_Closest(ai_Mon_Ink_Tiger_ShadowWalk.info.caster.agentPosition, ai_Mon_Ink_Tiger_ShadowWalk._dest);
				ai_Mon_Ink_Tiger_ShadowWalk.Teleport(ai_Mon_Ink_Tiger_ShadowWalk.info.caster, ai_Mon_Ink_Tiger_ShadowWalk._dest);
				ai_Mon_Ink_Tiger_ShadowWalk.info.caster.Control.RotateTowards(ai_Mon_Ink_Tiger_ShadowWalk.info.target, immediately: true, 0.5f);
				ai_Mon_Ink_Tiger_ShadowWalk.info.caster.AI.Aggro(ai_Mon_Ink_Tiger_ShadowWalk.info.target);
				_003C_003E2__current = new SI.WaitForSeconds(ai_Mon_Ink_Tiger_ShadowWalk.shadowWalkduration);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				ai_Mon_Ink_Tiger_ShadowWalk.FxPlayNetworked(ai_Mon_Ink_Tiger_ShadowWalk.shadowWalkEffect, ai_Mon_Ink_Tiger_ShadowWalk.info.caster);
				ai_Mon_Ink_Tiger_ShadowWalk.info.caster.Visual.EnableRenderers();
				_003C_003E2__current = new SI.WaitForSeconds(ai_Mon_Ink_Tiger_ShadowWalk.postDelay);
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				ai_Mon_Ink_Tiger_ShadowWalk.Destroy();
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

	public GameObject castEffect;

	public float castDuration;

	public Vector2 range;

	public float backDis;

	public float shadowWalkduration;

	public GameObject shadowWalkEffect;

	public float postDelay;

	private Vector3 _dest;

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__8))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
