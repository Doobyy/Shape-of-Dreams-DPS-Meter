using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_PillarOfFlame_Projectile : StandardProjectile
{
	public float duration;

	public GameObject fxTelegraph;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Vector3 bonePosition = info.caster.Visual.GetBonePosition((HumanBodyBones)18);
		SetCustomStartPosition(bonePosition);
		initialSpeed = (info.point - Dew.GetPositionOnGround(bonePosition)).magnitude / duration;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxApplySpeedMultiplierNetworked(fxTelegraph, 1f / duration);
			FxPlayNewNetworked(fxTelegraph, info.point, Quaternion.identity);
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		CreateAbilityInstance<Ai_Mon_Special_BossMaw_PillarOfFlame_Instance>(info.point, null, new CastInfo(info.caster, info.point));
	}

	private void MirrorProcessed()
	{
	}
}
