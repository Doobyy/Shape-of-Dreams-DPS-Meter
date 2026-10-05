using Mirror;
using UnityEngine;

public class Ai_MiniBoss_OrbSpitter_Orb : StandardProjectile
{
	public float startHeight;

	public DewCollider range;

	public ScalingValue damage;

	public GameObject telegraph;

	public GameObject hitEffect;

	public GameObject explodeEffect;

	public override bool reuseInRoom => true;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		SetCustomStartPosition(info.caster.position + Vector3.up * startHeight);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlayNew(telegraph, info.point, Quaternion.identity);
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		FxPlayNewNetworked(explodeEffect, position, Quaternion.identity);
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			Damage(damage).SetElemental(ElementalType.Dark).Dispatch(entity);
			FxPlayNewNetworked(hitEffect, entity);
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
