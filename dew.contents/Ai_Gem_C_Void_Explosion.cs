using System;
using Mirror;
using UnityEngine;

public class Ai_Gem_C_Void_Explosion : AbilityInstance
{
	public DewCollider range;

	public ScalingValue damage;

	public GameObject mainEffect;

	public GameObject hitEffect;

	[NonSerialized]
	public float strength;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(mainEffect);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			Damage(damage).ApplyStrength(strength).SetElemental(ElementalType.Dark).Dispatch(entity, chain);
			FxPlayNewNetworked(hitEffect, entity);
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
