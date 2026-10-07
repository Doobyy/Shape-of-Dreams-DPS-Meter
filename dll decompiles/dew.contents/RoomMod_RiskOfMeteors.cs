using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class RoomMod_RiskOfMeteors : RoomModifierBase
{
	public float startDelay = 3f;

	public float targetedMeteorChancePerPlayer = 0.125f;

	public float waveInterval = 5f;

	public float wavePerShotInterval = 0.2f;

	public float meteorCountInWavePerArea = 0.001f;

	public int maxMeteorCountInWave = 10;

	public float spreadOutWaveDuration = 5f;

	private int _meteorCount;

	private float _nextWaveTime = float.PositiveInfinity;

	private float _nextRegularTime = float.PositiveInfinity;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
			{
				_nextWaveTime = Time.time + startDelay + waveInterval * Random.value;
				_nextRegularTime = Time.time + startDelay + waveInterval * Random.value;
			});
			_meteorCount = Mathf.Min(Mathf.RoundToInt(SingletonDewNetworkBehaviour<Room>.instance.map.mapData.area * meteorCountInWavePerArea), maxMeteorCountInWave);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || SingletonDewNetworkBehaviour<Room>.instance.didClearRoom || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			return;
		}
		if (Time.time >= _nextRegularTime)
		{
			_nextRegularTime = Time.time + spreadOutWaveDuration / (float)_meteorCount * Random.Range(0.1f, 4f);
			int num = DewPlayer.gamePlayers.Count((DewPlayer d) => !d.hero.IsNullInactiveDeadOrKnockedOut());
			SpawnMeteor(targetedMeteorChancePerPlayer * (float)num);
		}
		if (Time.time >= _nextWaveTime)
		{
			_nextWaveTime = Time.time + waveInterval;
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			if (!SingletonDewNetworkBehaviour<Room>.instance.didClearRoom)
			{
				float interval = wavePerShotInterval * Random.Range(0.5f, 3f);
				int aliveCount = DewPlayer.gamePlayers.Count((DewPlayer d) => !d.hero.IsNullInactiveDeadOrKnockedOut());
				for (int j = 0; j < _meteorCount; j++)
				{
					if (NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
					{
						break;
					}
					if ((Object)(object)this == null)
					{
						break;
					}
					SpawnMeteor(targetedMeteorChancePerPlayer * (float)aliveCount);
					yield return new WaitForSeconds(interval);
				}
			}
		}
	}

	private void SpawnMeteor(float targetedMeteorChance)
	{
		Hero hero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
		Vector3 vector;
		if (Random.value < targetedMeteorChance)
		{
			vector = AbilityTrigger.PredictPoint_Simple(null, Random.Range(0.25f, 1f), hero, 0.5f) + Random.insideUnitSphere.Flattened() * 1.5f;
		}
		else
		{
			vector = hero.position + Random.insideUnitSphere.Flattened() * 15f;
			vector = Dew.GetValidAgentDestination_Closest(hero.agentPosition, vector);
		}
		vector = Dew.GetPositionOnGround(vector);
		CreateAbilityInstance<Ai_RoomMod_RiskOfMeteors_Meteor>(vector, Quaternion.identity, default);
	}

	private void MirrorProcessed()
	{
	}
}
