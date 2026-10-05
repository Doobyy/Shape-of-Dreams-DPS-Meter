using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossLightElemental_Summon : AbilityInstance
{
	public int summonBonusLevel = 3;

	public bool excludeDeadPlayers;

	public float selfShieldHpMultiplier;

	public float selfShieldDuration = 3f;

	public float summonShieldHpMultiplier;

	public float summonShieldDuration = 3f;

	public GameObject spawnEffectOnSelf;

	public GameObject spawnEffectOnGround;

	public GameObject spawnEffectOnEntity;

	public Entity[] entries;

	public Vector2 spawnDistanceRange;

	public float intervalBetweenSpawn;

	public float postDelayAfterSpawn;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		int num = 0;
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			if (!((Object)(object)gamePlayer.hero == null) && (!excludeDeadPlayers || !gamePlayer.hero.isKnockedOut))
			{
				num++;
			}
		}
		if (num < 0)
		{
			num = 0;
		}
		int totalSpawnCount = entries.Length + num;
		float duration = (float)totalSpawnCount * intervalBetweenSpawn + postDelayAfterSpawn;
		info.caster.Control.StartDaze(duration);
		for (int i = 0; i < totalSpawnCount; i++)
		{
			Entity entity = entries[i % entries.Length];
			Hero hero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
			if ((Object)(object)hero == null)
			{
				hero = DewPlayer.gamePlayers[0].hero;
			}
			Vector3 aIAgentPosition = hero.GetAIAgentPosition(info.caster);
			Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(aIAgentPosition, aIAgentPosition + Random.Range(spawnDistanceRange.x, spawnDistanceRange.y) * Random.onUnitSphere.Flattened().normalized);
			Entity entity2 = Dew.SpawnEntity(entity, validAgentDestination_LinearSweep, Quaternion.LookRotation(hero.GetAIPosition(info.caster) - validAgentDestination_LinearSweep), null, info.caster.owner, NetworkedManagerBase<GameManager>.instance.ambientLevel + summonBonusLevel);
			FxPlayNewNetworked(spawnEffectOnSelf, info.caster);
			FxPlayNewNetworked(spawnEffectOnGround, validAgentDestination_LinearSweep, Quaternion.identity);
			FxPlayNewNetworked(spawnEffectOnEntity, entity2);
			GiveShield(entity2, entity2.maxHealth * summonShieldHpMultiplier, summonShieldDuration);
			GiveShield(info.caster, selfShieldHpMultiplier * info.caster.maxHealth, selfShieldDuration);
			yield return new SI.WaitForSeconds(intervalBetweenSpawn);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
