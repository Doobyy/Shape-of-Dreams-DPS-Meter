using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_Purgatory_Instance : AbilityInstance
{
	public ScalingValue dmgFactor;

	public GameObject fxExplosion;

	public GameObject fxHit;

	public float range;

	public float knockupStrength;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		FxPlayNetworked(fxExplosion, info.point, Quaternion.identity);
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.point, range, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < list.Count; i++)
		{
			Entity e = list[i];
			e.Visual.KnockUp(knockupStrength, isFriendly: false);
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetDirection(Vector3.up).SetOriginPosition(info.point).Dispatch(e);
			FxPlayNewNetworked(fxHit, e);
			CreateAbilityInstance(info.point, null, new CastInfo(info.caster, info.caster), (Ai_Mon_Special_BossMaw_Purgatory_Heal b) =>
			{
				b.SetCustomStartPosition(e.Visual.GetCenterPosition());
			});
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
