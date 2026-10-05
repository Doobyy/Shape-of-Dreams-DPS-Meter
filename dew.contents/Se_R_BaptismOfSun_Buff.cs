using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_R_BaptismOfSun_Buff : StatusEffect
{
	public float duration;

	public ScalingValue hasteAmount;

	public ScalingValue bonusDamage;

	public float procCoefficient;

	public float hitDelay;

	public GameObject fxHit;

	public int maxBuffCount = 6;

	[NonSerialized]
	public bool disableHaste;

	[NonSerialized]
	public int hitCountNonBoss;

	[NonSerialized]
	public int hitCountBoss;

	private float _baseDuration;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseDuration = duration;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		duration = _baseDuration;
		disableHaste = false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		List<Se_R_BaptismOfSun_Buff> list = DewPool.GetList(out ListReturnHandle<Se_R_BaptismOfSun_Buff> handle);
		foreach (StatusEffect statusEffect in victim.Status.statusEffects)
		{
			if (statusEffect is Se_R_BaptismOfSun_Buff item && !((UnityEngine.Object)(object)statusEffect == (UnityEngine.Object)(object)this))
			{
				list.Add(item);
			}
		}
		if (list.Count > maxBuffCount - 1)
		{
			list.Sort((Se_R_BaptismOfSun_Buff x, Se_R_BaptismOfSun_Buff y) => x.creationTime.CompareTo(y.creationTime));
			while (list.Count > maxBuffCount - 1)
			{
				list[0].Destroy();
				list.RemoveAt(0);
			}
		}
		handle.Return();
		SetTimer(duration);
		ShowOnScreenTimer();
		if (!disableHaste)
		{
			DoHaste(GetValue(hasteAmount));
		}
		DoAttackEmpower((EventInfoAttackEffect effect, int i) =>
		{
			if (!info.caster.IsNullInactiveDeadOrKnockedOut() && !effect.chain.DidReact(this))
			{
				((MonoBehaviour)(object)info.caster).StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				FxPlayNewNetworked(fxHit, effect.victim);
				if (hitDelay > 0.0001f)
				{
					yield return new WaitForSeconds(hitDelay);
				}
				Damage(bonusDamage, procCoefficient).ApplyStrength(effect.strength).SetElemental(ElementalType.Fire).SetOriginPosition(info.caster.position)
					.Dispatch(effect.victim, effect.chain.New(this));
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
