using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_LivingShards_Atk : AbilityInstance
{
	public float castDelay;

	public ScalingValue dmgFactor;

	public GameObject telegraph;

	public GameObject fxAtk;

	public GameObject fxHit;

	public DewCollider range;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		((Component)(object)this).transform.position = info.point;
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(telegraph, info.caster);
			DestroyOnDeath(info.caster);
			yield return new SI.WaitForSeconds(castDelay);
			FxStopNetworked(telegraph);
			FxPlayNetworked(fxAtk, info.point, null);
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetElemental(ElementalType.Cold).SetOriginPosition(((Component)(object)this).transform.position).Dispatch(entity);
				FxPlayNetworked(fxHit, entity);
			}
			handle.Return();
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(telegraph);
		}
	}

	private void MirrorProcessed()
	{
	}
}
