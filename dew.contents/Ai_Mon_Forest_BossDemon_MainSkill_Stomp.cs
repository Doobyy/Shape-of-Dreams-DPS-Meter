using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Forest_BossDemon_MainSkill_Stomp : AbilityInstance
{
	public int waves;

	public float treeInterval;

	public int perWaveTreeCount;

	public float waveInterval;

	public float knockupAmount;

	public GameObject hitEffect;

	public DewCollider range;

	public ScalingValue damage;

	public Knockback knockback;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			DefaultDamage(damage).SetOriginPosition(((Component)(object)this).transform.position).Dispatch(entity);
			knockback.ApplyWithOrigin(((Component)(object)this).transform.position, entity);
			entity.Visual.KnockUp(knockupAmount, isFriendly: false);
			FxPlayNewNetworked(hitEffect, entity);
		}
		handle.Return();
		float treeDelay = DewResources.GetByType<Ai_Mon_Forest_BossDemon_MainSkill_Stomp_Tree>(default(ResourceLoadSettings)).damageDelay;
		RoomSection section = info.caster.section;
		if (section == null)
		{
			section = SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection();
		}
		if (section == null)
		{
			Destroy();
			yield break;
		}
		for (int j = 0; j < waves; j++)
		{
			for (int k = 0; k < perWaveTreeCount; k++)
			{
				Vector3 vector;
				if (k < DewPlayer.gamePlayers.Count && !DewPlayer.gamePlayers[k].hero.isKnockedOut)
				{
					vector = DewPlayer.gamePlayers[k].hero.GetAIPosition(info.caster);
				}
				else
				{
					vector = ((k < perWaveTreeCount - DewPlayer.gamePlayers.Count || DewPlayer.gamePlayers[k - perWaveTreeCount + DewPlayer.gamePlayers.Count].hero.isKnockedOut) ? (section.GetAnyRandomNode() + Random.insideUnitSphere.Flattened() * 1.5f) : AbilityTrigger.PredictPoint_Simple(info.caster, Random.Range(0.7f, 1f), DewPlayer.gamePlayers[k - perWaveTreeCount + DewPlayer.gamePlayers.Count].hero, treeDelay));
				}
				vector = Dew.GetPositionOnGround(vector);
				CreateAbilityInstance<Ai_Mon_Forest_BossDemon_MainSkill_Stomp_Tree>(vector, null, new CastInfo(info.caster, vector));
				yield return new SI.WaitForSeconds(treeInterval);
			}
			yield return new SI.WaitForSeconds(waveInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
