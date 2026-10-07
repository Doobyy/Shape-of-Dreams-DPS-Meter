using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_E_SliceThroat : AbilityInstance
{
	public GameObject fxSlice;

	public DewCollider range;

	public DewCollider outerRange;

	public ChannelData channel;

	public ScalingValue damage;

	public ScalingValue outerDamage;

	public ScalingValue outerHealRatioPerHit;

	public ScalingValue cooldownReductionRatioPerHit;

	public float outerAttackEffect = 0.5f;

	public GameObject fxOuterHit;

	public GameObject fxHit;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			channel.Get().AddOnCancel(DestroyIfActive).AddOnComplete(Finish)
				.Dispatch(info.caster);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		position = info.caster.agentPosition;
	}

	private void Finish()
	{
		FxPlayNetworked(fxSlice, info.caster);
		List<Entity> entities = outerRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		List<Entity> entities2 = range.GetEntities(out var handle2, tvDefaultHarmfulEffectTargets);
		float num = info.caster.Status.missingHealth * GetValue(outerHealRatioPerHit);
		for (int i = 0; i < entities.Count; i++)
		{
			Damage(outerDamage).SetOriginPosition(info.caster.position).SetAttr(DamageAttribute.IsCrit).DoAttackEffect(AttackEffectType.Others, outerAttackEffect)
				.SetElemental(ElementalType.Dark)
				.Dispatch(entities[i]);
			FxPlayNewNetworked(fxOuterHit, entities[i]);
		}
		if (entities.Count > 0 && (Object)(object)firstTrigger != null)
		{
			ApplyCooldownReductionByRatio(firstTrigger, GetValue(cooldownReductionRatioPerHit) * (float)entities.Count);
		}
		Heal((float)entities.Count * num).Dispatch(info.caster);
		for (int j = 0; j < entities2.Count; j++)
		{
			if (!entities.Contains(entities2[j]))
			{
				Damage(damage).SetOriginPosition(info.caster.position).SetElemental(ElementalType.Dark).Dispatch(entities2[j]);
				FxPlayNewNetworked(fxHit, entities2[j]);
			}
		}
		handle.Return();
		handle2.Return();
		DestroyIfActive();
	}

	private void MirrorProcessed()
	{
	}
}
