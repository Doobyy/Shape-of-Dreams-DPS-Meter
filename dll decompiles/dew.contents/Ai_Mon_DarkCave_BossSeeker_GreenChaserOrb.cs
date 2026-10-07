using Mirror;
using UnityEngine;

public class Ai_Mon_DarkCave_BossSeeker_GreenChaserOrb : StandardProjectile
{
	public float maxDuration = 4f;

	public Transform projEffectTransform;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		float num = Time.time - creationTime;
		float num2 = num / maxDuration;
		projEffectTransform.localPosition = Vector3.up * (Mathf.Sin(num * 7f + 1f) * (1f - num2) + 1f + 2f * (1f - num2));
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Time.time - creationTime > maxDuration)
		{
			CreateAbilityInstance(position, null, new CastInfo(info.caster), (Ai_Mon_DarkCave_BossSeeker_GreenChaserOrb_Explode orb) =>
			{
				orb.strengthMultiplier = GetStrength();
			});
			Destroy();
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		CreateAbilityInstance(position, null, new CastInfo(info.caster), (Ai_Mon_DarkCave_BossSeeker_GreenChaserOrb_Explode orb) =>
		{
			orb.strengthMultiplier = GetStrength();
		});
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		CreateAbilityInstance(position, null, new CastInfo(info.caster), (Ai_Mon_DarkCave_BossSeeker_GreenChaserOrb_Explode orb) =>
		{
			orb.strengthMultiplier = GetStrength();
		});
		Destroy();
	}

	private float GetStrength()
	{
		return Mathf.Clamp01((Time.time - creationTime) / maxDuration) * 0.5f + 0.5f;
	}

	private void MirrorProcessed()
	{
	}
}
