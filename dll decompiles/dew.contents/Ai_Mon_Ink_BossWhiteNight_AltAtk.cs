using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_AltAtk : StandardProjectile
{
	public ScalingValue dmgFactor;

	public Knockback knockback;

	public GameObject fxMain;

	public override bool reuseInRoom => true;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		SetCustomStartPosition(info.caster.Visual.GetBonePosition((HumanBodyBones)18));
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNewNetworked(fxMain, info.caster);
		}
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(((Component)(object)this).transform.position).SetDirection(info.forward).SetElemental(ElementalType.Light)
			.Dispatch(hit.entity);
		knockback.ApplyWithDirection(info.forward, hit.entity);
	}

	private void MirrorProcessed()
	{
	}
}
