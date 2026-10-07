using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_RageInstance_RockSpawner : AbilityInstance
{
	public int spawnCount;

	public float castDuration;

	public Vector2 spawnInterval;

	public float spawnRadius;

	public float randomDirChance;

	public Vector2 randomStartDelay;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		yield return new SI.WaitForSeconds(Random.Range(randomStartDelay.x, randomStartDelay.y));
		Hero target = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
		if (target.IsNullInactiveDeadOrKnockedOut())
		{
			Destroy();
			yield break;
		}
		BossMonster.RevealStealthedBeforeSpecialAttack();
		for (int i = 0; i < spawnCount; i++)
		{
			if (target.IsNullInactiveDeadOrKnockedOut())
			{
				target = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
				if (target.IsNullInactiveDeadOrKnockedOut())
				{
					break;
				}
			}
			Vector3 startPos = target.GetAIPosition(info.caster) + Random.insideUnitCircle.ToXZ() * Random.Range(5f, spawnRadius);
			float y = ((Random.value <= randomDirChance) ? ((float)Random.Range(0, 360)) : AbilityTrigger.PredictAngle_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), target, startPos, castDuration));
			CreateAbilityInstance(startPos, Quaternion.Euler(0f, y, 0f), new CastInfo(info.caster), (Ai_Mon_Ink_BossWhiteNight_RageInstance_Rock b) =>
			{
				b.castDuration = castDuration;
			});
			yield return new SI.WaitForSeconds(Random.Range(spawnInterval.x, spawnInterval.y));
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
