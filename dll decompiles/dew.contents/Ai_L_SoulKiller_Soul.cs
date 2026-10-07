using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Ai_L_SoulKiller_Soul : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__25 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_L_SoulKiller_Soul _003C_003E4__this;

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
		public _003COnCreateSequenced_003Ed__25(int _003C_003E1__state)
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
			Ai_L_SoulKiller_Soul ai_L_SoulKiller_Soul = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)ai_L_SoulKiller_Soul).isServer)
				{
					return false;
				}
				ai_L_SoulKiller_Soul.owner = ai_L_SoulKiller_Soul.info.caster.owner;
				if ((ai_L_SoulKiller_Soul.info.target is Monster monster && (monster.type == Monster.MonsterType.Boss || monster.type == Monster.MonsterType.MiniBoss)) || ai_L_SoulKiller_Soul.info.target.GetRelation(ai_L_SoulKiller_Soul.info.caster) == EntityRelation.Ally)
				{
					ai_L_SoulKiller_Soul._fxSoul = ai_L_SoulKiller_Soul.fxLargeSoul;
					ai_L_SoulKiller_Soul._fxExlposion = ai_L_SoulKiller_Soul.fxLargeExplosion;
					ai_L_SoulKiller_Soul._damage = ai_L_SoulKiller_Soul.GetValue(ai_L_SoulKiller_Soul.dmgFactor) * ai_L_SoulKiller_Soul.dmgLargeMultiplier;
					ai_L_SoulKiller_Soul.radius *= ai_L_SoulKiller_Soul.radiusMultiplier;
				}
				else
				{
					ai_L_SoulKiller_Soul._fxSoul = ai_L_SoulKiller_Soul.fxSmallSoul;
					ai_L_SoulKiller_Soul._fxExlposion = ai_L_SoulKiller_Soul.fxSmallExplosion;
					ai_L_SoulKiller_Soul.knockback.duration *= 1.5f;
					ai_L_SoulKiller_Soul.knockback.distance *= 0.5f;
					ai_L_SoulKiller_Soul._damage = ai_L_SoulKiller_Soul.GetValue(ai_L_SoulKiller_Soul.dmgFactor) * ai_L_SoulKiller_Soul.dmgSmallMultiplier;
				}
				ai_L_SoulKiller_Soul.FxPlayNetworked(ai_L_SoulKiller_Soul._fxSoul);
				_003C_003E2__current = new SI.WaitForSeconds(ai_L_SoulKiller_Soul.duration);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				if (ai_L_SoulKiller_Soul.isDestroyed)
				{
					return false;
				}
				ai_L_SoulKiller_Soul.Destroy();
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

	public float duration;

	public float radius;

	public float dmgLargeMultiplier;

	public float dmgSmallMultiplier;

	public float radiusMultiplier;

	public float procCoefficient;

	public GameObject fxSmallSoul;

	public GameObject fxLargeSoul;

	public GameObject fxSmallExplosion;

	public GameObject fxLargeExplosion;

	public GameObject fxHit;

	public Knockback knockback;

	public AbilityTargetValidator validator;

	[NonSerialized]
	public DewPlayer owner;

	internal ScalingValue dmgFactor;

	private GameObject _fxSoul;

	private GameObject _fxExlposion;

	private float _damage;

	private float _baseRadius;

	private float _baseKnockbackDuration;

	private float _baseKnockbackDistance;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	protected override void Awake()
	{
	}

	protected override void OnDisable()
	{
	}

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__25))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	public void ExplodeSoul(float delay)
	{
	}

	protected override void OnDestroyActor()
	{
	}

	private void MirrorProcessed()
	{
	}
}
