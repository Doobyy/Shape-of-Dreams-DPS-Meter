using System;
using UnityEngine;

public class Ai_Mon_Despair_WretchedArtillery_BarrageAtk_Missile : StandardProjectile
{
	public float startHeight;

	public DewCollider range;

	public ScalingValue damage;

	public GameObject telegraph;

	public GameObject hitEffect;

	public GameObject explodeEffect;

	public bool spawnAoE = true;

	[NonSerialized]
	public bool isFirstMissile;

	public override bool reuseInRoom => true;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		SetCustomStartPosition(info.caster.position + Vector3.up * startHeight);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(telegraph, info.point, Quaternion.identity);
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		Vector3 positionOnGround = Dew.GetPositionOnGround(position);
		FxPlayNewNetworked(explodeEffect, positionOnGround, Quaternion.identity);
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			Damage(damage).Dispatch(entity);
			FxPlayNewNetworked(hitEffect, entity);
		}
		if (spawnAoE)
		{
			CreateAbilityInstance(positionOnGround, null, new CastInfo(info.caster), (Ai_Mon_Despair_WretchedArtillery_BarrageAtk_AoE a) =>
			{
				a.playSizzleSound = isFirstMissile;
			});
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
