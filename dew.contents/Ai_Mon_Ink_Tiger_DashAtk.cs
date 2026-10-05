using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_Tiger_DashAtk : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__14 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Mon_Ink_Tiger_DashAtk _003C_003E4__this;

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
			Ai_Mon_Ink_Tiger_DashAtk CS_0024_003C_003E8__locals29 = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)CS_0024_003C_003E8__locals29).isServer)
				{
					return false;
				}
				CS_0024_003C_003E8__locals29.DestroyOnDeath(CS_0024_003C_003E8__locals29.info.caster);
				CS_0024_003C_003E8__locals29.FxPlayNetworked(CS_0024_003C_003E8__locals29.flyEffect, CS_0024_003C_003E8__locals29.info.caster);
				Vector3 end = CS_0024_003C_003E8__locals29.info.caster.agentPosition + CS_0024_003C_003E8__locals29.info.forward * CS_0024_003C_003E8__locals29.dashDis;
				end = Dew.GetValidAgentDestination_LinearSweep(CS_0024_003C_003E8__locals29.info.caster.agentPosition, end);
				CS_0024_003C_003E8__locals29.info.caster.Control.StartDaze(CS_0024_003C_003E8__locals29.dashDuration);
				CS_0024_003C_003E8__locals29.info.caster.Control.StartDisplacement(new DispByDestination
				{
					affectedByMovementSpeed = true,
					canGoOverTerrain = false,
					destination = end,
					duration = CS_0024_003C_003E8__locals29.dashDuration,
					ease = CS_0024_003C_003E8__locals29.ease,
					isCanceledByCC = false,
					isFriendly = true,
					rotateForward = true,
					onFinish = () =>
					{
					}
				});
				_003C_003E2__current = new SI.WaitForSeconds(CS_0024_003C_003E8__locals29.dashDuration);
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
			{
				_003C_003E1__state = -1;
				CS_0024_003C_003E8__locals29.range.transform.position = CS_0024_003C_003E8__locals29.info.caster.agentPosition;
				List<Entity> entities = CS_0024_003C_003E8__locals29.range.GetEntities(out var handle, CS_0024_003C_003E8__locals29.hittable, CS_0024_003C_003E8__locals29.info.caster);
				for (int i = 0; i < entities.Count; i++)
				{
					Entity entity = entities[i];
					CS_0024_003C_003E8__locals29.FxPlayNewNetworked(CS_0024_003C_003E8__locals29.hitEffect, entity);
					CS_0024_003C_003E8__locals29.CreateBasicEffect(entity, new SlowEffect
					{
						strength = CS_0024_003C_003E8__locals29.slowStrength
					}, CS_0024_003C_003E8__locals29.slowDuration);
					CS_0024_003C_003E8__locals29.CreateDamage(DamageData.SourceType.Default, CS_0024_003C_003E8__locals29.dmgFactor).Dispatch(entity);
				}
				handle.Return();
				CS_0024_003C_003E8__locals29.Destroy();
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

	public float dashDis;

	public float dashDuration;

	public float landAfterDelay;

	public DewEase ease;

	public GameObject landEffect;

	public GameObject flyEffect;

	public DewCollider range;

	public AbilityTargetValidator hittable;

	public ScalingValue dmgFactor;

	public GameObject hitEffect;

	public float slowDuration;

	public float slowStrength;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__14))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
