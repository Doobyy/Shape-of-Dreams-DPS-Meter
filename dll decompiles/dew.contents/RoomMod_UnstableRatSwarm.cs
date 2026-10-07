using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomMod_UnstableRatSwarm : RoomModifierBase, IPrewarmRoomContributor
{
	public int perSpawnCount;

	public float initDelay;

	public float spawnRadius;

	public float perSpawnInterval;

	public Vector2 loopInterval;

	public int maxRatCount;

	public GameObject fxStart;

	public void ContributeMonsterPrewarm(Dictionary<Monster, int> counts)
	{
		Mon_Despair_UnstableRat byType = DewResources.GetByType<Mon_Despair_UnstableRat>(default(ResourceLoadSettings));
		if (!((Object)(object)byType == null))
		{
			counts.TryGetValue(byType, out var value);
			counts[byType] = value + maxRatCount * 3;
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (!isNewInstance)
		{
			return;
		}
		GameManager.CallOnReady(() =>
		{
			if ((Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
			{
				SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(((MonoBehaviour)(object)this).StopAllCoroutines);
			}
			((MonoBehaviour)(object)this).StartCoroutine(SpawnRatSwarmRoutine());
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			((MonoBehaviour)(object)this).StopAllCoroutines();
		}
	}

	private IEnumerator SpawnRatSwarmRoutine()
	{
		FxPlayNetworked(fxStart);
		yield return new WaitForSeconds(initDelay);
		int heroCount = 0;
		List<DewPlayer> gamePlayers = DewPlayer.gamePlayers;
		for (int i = 0; i < gamePlayers.Count; i++)
		{
			if (!gamePlayers[i].hero.IsNullInactiveDeadOrKnockedOut())
			{
				heroCount++;
			}
		}
		while (true)
		{
			int num = 0;
			foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
			{
				if (!allEntity.IsNullInactiveDeadOrKnockedOut() && allEntity is Mon_Despair_UnstableRat)
				{
					num++;
				}
			}
			if (num >= maxRatCount)
			{
				yield return new WaitForSeconds(5f);
				continue;
			}
			int count = perSpawnCount + Mathf.FloorToInt((float)heroCount * 0.5f);
			for (int j = 0; j < count; j++)
			{
				Hero hero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
				Vector3 normalized = Random.insideUnitCircle.ToXZ().normalized;
				Vector3 vector = hero.agentPosition + normalized * Random.Range(0f, spawnRadius);
				vector = Dew.GetPositionOnGround(vector);
				CreateAbilityInstance<Ai_UnstableRatSwarm_Projectile>(vector, null, default);
				yield return new WaitForSeconds(perSpawnInterval);
			}
			yield return new WaitForSeconds(Random.Range(loopInterval.x, loopInterval.y));
		}
	}

	private void MirrorProcessed()
	{
	}
}
