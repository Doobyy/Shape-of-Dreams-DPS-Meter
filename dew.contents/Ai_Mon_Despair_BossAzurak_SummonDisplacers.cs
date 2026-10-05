using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_SummonDisplacers : AbilityInstance
{
	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		yield return new SI.WaitForSeconds(0.1f);
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, position, 20f);
		for (int i = 0; i < list.Count; i++)
		{
			Entity entity = list[i];
			if (!((Object)(object)entity == (Object)(object)info.caster))
			{
				CreateBasicEffect(entity, new StunEffect(), 1.7f);
			}
		}
		handle.Return();
		yield return new SI.WaitForSeconds(1.4f);
		int count = Mathf.RoundToInt(3f + NetworkedManagerBase<GameManager>.instance.GetMultiplayerDifficultyFactor(reduceWhenDead: true));
		for (int j = 0; j < count; j++)
		{
			Vector3 aIAgentPosition = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true).GetAIAgentPosition(info.caster);
			Vector3 vector = aIAgentPosition + Random.onUnitSphere.Flattened() * 5f;
			vector = Dew.GetPositionOnGround(vector);
			vector = Dew.GetValidAgentDestination_Closest(aIAgentPosition, vector);
			Dew.SpawnEntity(vector, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), null, info.caster.owner, info.caster.level, (Mon_Despair_Displacer b) =>
			{
				b.disableLoot = true;
			});
			yield return new SI.WaitForSeconds(Random.Range(0.2f, 0.4f));
		}
		yield return new SI.WaitForSeconds(7.5f);
		for (int j = 0; j < count; j++)
		{
			Vector3 aIAgentPosition2 = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true).GetAIAgentPosition(info.caster);
			Vector3 vector2 = aIAgentPosition2 + Random.onUnitSphere.Flattened() * 5f;
			vector2 = Dew.GetPositionOnGround(vector2);
			vector2 = Dew.GetValidAgentDestination_Closest(aIAgentPosition2, vector2);
			Dew.SpawnEntity(vector2, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), null, info.caster.owner, info.caster.level, (Mon_Despair_Displacer b) =>
			{
				b.disableLoot = true;
			});
			yield return new SI.WaitForSeconds(Random.Range(0.2f, 0.4f));
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
