using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_C_IceBlock : StatusEffect
{
	public GameObject fxStart;

	public GameObject fxEnd;

	public ScalingValue dmgFactor;

	public ScalingValue shieldFactor;

	public ScalingValue perHealFactor;

	public Knockback knockback;

	public DewCollider range;

	public float duration;

	public GameObject hitEffect;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		List<Se_C_IceBlock> list = DewPool.GetList(out ListReturnHandle<Se_C_IceBlock> handle);
		foreach (StatusEffect statusEffect in victim.Status.statusEffects)
		{
			if (statusEffect is Se_C_IceBlock item && !((Object)(object)statusEffect == (Object)(object)this))
			{
				list.Add(item);
			}
		}
		list.Sort((Se_C_IceBlock x, Se_C_IceBlock y) => x.creationTime.CompareTo(y.creationTime));
		int num = list.Count - 4;
		for (int num2 = 0; num2 < num; num2++)
		{
			list[num2].DestroyIfActive();
		}
		handle.Return();
		ShowOnScreenTimer();
		if ((Object)(object)info.target != (Object)(object)info.caster)
		{
			CreateAbilityInstance<Ai_C_IceBlock_Projectile>(info.caster.position, null, info);
		}
		FxPlayNetworked(fxStart, info.target);
		DoShield(GetValue(shieldFactor), (EventInfoDamageNegatedByShield obj) =>
		{
			if (obj.shield.amount < 0.001f)
			{
				DestroyIfActive();
			}
		});
		SetTimer(duration);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		FxStopNetworked(fxStart);
		if ((Object)(object)info.target == null)
		{
			return;
		}
		FxPlayNetworked(fxEnd, info.target);
		range.transform.position = info.target.position;
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		foreach (Entity item in entities)
		{
			FxPlayNewNetworked(hitEffect, item);
			Damage(dmgFactor).SetElemental(ElementalType.Cold).Dispatch(item);
			knockback.ApplyWithOrigin(info.target.position, item);
		}
		int len = entities.Count;
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		handle.Return();
		IEnumerator Routine()
		{
			yield return null;
			Heal(GetValue(perHealFactor) * (float)len).Dispatch(info.target);
		}
	}

	private void MirrorProcessed()
	{
	}
}
