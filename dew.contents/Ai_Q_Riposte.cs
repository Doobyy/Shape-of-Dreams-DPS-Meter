using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Ai_Q_Riposte : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__12 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Q_Riposte _003C_003E4__this;

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
		public _003COnCreateSequenced_003Ed__12(int _003C_003E1__state)
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
			Ai_Q_Riposte ai_Q_Riposte = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				ai_Q_Riposte.FxPlay(ai_Q_Riposte.mainEffect);
				if (!((NetworkBehaviour)ai_Q_Riposte).isServer)
				{
					return false;
				}
				_003C_003E2__current = new SI.WaitForSeconds(ai_Q_Riposte.damageDelay);
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				List<Entity> entities = ai_Q_Riposte.range.GetEntities(out var handle, ai_Q_Riposte.tvDefaultHarmfulEffectTargets);
				if (entities.Count > 0 && ai_Q_Riposte.fastFeetChargeCount > 0)
				{
					St_M_FastFeet ability = ai_Q_Riposte.info.caster.Ability.GetAbility<St_M_FastFeet>();
					if ((UnityEngine.Object)(object)ability != null && ability.currentConfigCurrentCharge < ability.configs[0].maxCharges)
					{
						ability.SetCharge(0, ability.currentConfigCurrentCharge + 1);
					}
				}
				bool flag = false;
				if (ai_Q_Riposte.canCrit)
				{
					for (int i = 0; i < entities.Count; i++)
					{
						if (entities[i].Control.isDashing || entities[i].Control.ongoingChannels.Count > 0)
						{
							flag = true;
							break;
						}
					}
				}
				for (int j = 0; j < entities.Count; j++)
				{
					Entity entity = entities[j];
					DamageData damageData = ai_Q_Riposte.Damage(ai_Q_Riposte.damage).SetOriginPosition(ai_Q_Riposte.info.caster.position);
					if (flag)
					{
						damageData.ApplyAmplification(ai_Q_Riposte.critDamageAmp);
						damageData.SetAttr(DamageAttribute.IsCrit);
						entity.Stagger(entity.position - ai_Q_Riposte.info.caster.position);
						ai_Q_Riposte.FxPlayNewNetworked(ai_Q_Riposte.attackHitCritEffect, entity);
						ai_Q_Riposte.CreateBasicEffect(entity, new StunEffect(), ai_Q_Riposte.stunDuration, "riposte_crit_stun");
					}
					else
					{
						ai_Q_Riposte.FxPlayNewNetworked(ai_Q_Riposte.attackHitEffect, entity);
					}
					damageData.DoAttackEffect(AttackEffectType.Others);
					damageData.Dispatch(entity);
				}
				handle.Return();
				ai_Q_Riposte.ResetCooldown(ai_Q_Riposte.info.caster.Ability.attackAbility);
				ai_Q_Riposte.Destroy();
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

	public GameObject mainEffect;

	public float damageDelay;

	public GameObject attackHitEffect;

	public bool canCrit;

	public GameObject attackHitCritEffect;

	public ScalingValue damage;

	public float critDamageAmp;

	public float stunDuration;

	public int fastFeetChargeCount;

	public DewCollider range;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__12))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
