using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Ai_L_SoulKiller : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__14 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_L_SoulKiller _003C_003E4__this;

		private Vector3 _003Cpoint_003E5__2;

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
			Ai_L_SoulKiller ai_L_SoulKiller = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)ai_L_SoulKiller).isServer)
				{
					return false;
				}
				ai_L_SoulKiller.DestroyOnDeath(ai_L_SoulKiller.info.caster);
				_003Cpoint_003E5__2 = ai_L_SoulKiller.info.caster.agentPosition;
				ai_L_SoulKiller.FxPlayNetworked(ai_L_SoulKiller.fxMain, _003Cpoint_003E5__2 + Vector3.up * 0.9f, null);
				_003C_003E2__current = new SI.WaitForSeconds(ai_L_SoulKiller.startDelay);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				ai_L_SoulKiller.FxPlayNewNetworked(ai_L_SoulKiller.fxExplosionPrepare, _003Cpoint_003E5__2 + Vector3.up * 0.2f, null);
				_003C_003E2__current = new SI.WaitForSeconds(ai_L_SoulKiller.interval);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				ai_L_SoulKiller.ExplodeSoulsRoutine();
				_003C_003E2__current = new SI.WaitForSeconds(ai_L_SoulKiller.explosionDelay);
				_003C_003E1__state = 3;
				return true;
			case 3:
			{
				_003C_003E1__state = -1;
				ai_L_SoulKiller.FxPlayNewNetworked(ai_L_SoulKiller.fxExlposion, _003Cpoint_003E5__2, null);
				ai_L_SoulKiller.range.transform.position = _003Cpoint_003E5__2;
				ListReturnHandle<Entity> handle;
				foreach (Entity entity in ai_L_SoulKiller.range.GetEntities(out handle, ai_L_SoulKiller.tvDefaultHarmfulEffectTargets))
				{
					if (!entity.IsNullInactiveDeadOrKnockedOut())
					{
						ai_L_SoulKiller.FxPlayNewNetworked(ai_L_SoulKiller.fxHit, entity);
						ai_L_SoulKiller.knockback.ApplyWithOrigin(_003Cpoint_003E5__2, entity);
						ai_L_SoulKiller.CreateDamage(DamageData.SourceType.Magic, ai_L_SoulKiller.dmgFactor).SetOriginPosition(_003Cpoint_003E5__2).Dispatch(entity);
					}
				}
				handle.Return();
				ai_L_SoulKiller.Destroy();
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

	public DewCollider range;

	public Knockback knockback;

	public GameObject fxMain;

	public GameObject fxExplosionPrepare;

	public GameObject fxExlposion;

	public GameObject fxHit;

	public float startDelay;

	public float interval;

	public float explosionDelay;

	internal Ai_L_SoulKiller_Soul[] souls;

	internal ScalingValue dmgFactor;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	protected override void OnDisable()
	{
	}

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__14))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void ExplodeSoulsRoutine()
	{
	}

	private void MirrorProcessed()
	{
	}
}
