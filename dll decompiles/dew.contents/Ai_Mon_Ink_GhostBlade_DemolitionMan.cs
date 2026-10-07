using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_GhostBlade_DemolitionMan : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__9 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Mon_Ink_GhostBlade_DemolitionMan _003C_003E4__this;

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
		public _003COnCreateSequenced_003Ed__9(int _003C_003E1__state)
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
			Ai_Mon_Ink_GhostBlade_DemolitionMan CS_0024_003C_003E8__locals13 = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)CS_0024_003C_003E8__locals13).isServer)
				{
					return false;
				}
				CS_0024_003C_003E8__locals13.DestroyOnDeath(CS_0024_003C_003E8__locals13.info.caster);
				_003C_003E2__current = new SI.WaitForSeconds(CS_0024_003C_003E8__locals13.delay);
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(CS_0024_003C_003E8__locals13.info.caster.agentPosition, CS_0024_003C_003E8__locals13.info.caster.agentPosition + CS_0024_003C_003E8__locals13.info.forward * CS_0024_003C_003E8__locals13.dashDistance);
				CS_0024_003C_003E8__locals13.info.caster.Control.StartDaze(CS_0024_003C_003E8__locals13.dashDuration);
				CS_0024_003C_003E8__locals13.info.caster.Control.StartDisplacement(new DispByDestination
				{
					affectedByMovementSpeed = false,
					canGoOverTerrain = false,
					destination = validAgentDestination_LinearSweep,
					duration = CS_0024_003C_003E8__locals13.dashDuration,
					ease = CS_0024_003C_003E8__locals13.ease,
					isCanceledByCC = false,
					isFriendly = true,
					rotateForward = true,
					onFinish = () =>
					{
					},
					onCancel = () =>
					{
					}
				});
				return false;
			}
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

	public GameObject hitEffect;

	public float delay;

	public float dashDistance;

	public float dashDuration;

	public DewEase ease;

	public DewCollider range;

	public ScalingValue dmgFactor;

	public AbilityTargetValidator hittable;

	public Knockback knockback;

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__9))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
