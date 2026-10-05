using System;
using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class RoomMod_Hunted : RoomModifierBase
{
	[Header("General")]
	public Formula addedMirageChance;

	public Formula addedHunterChance;

	public Formula spawnedPopMultiplier;

	public Formula hunterDamageAmp;

	public Formula hunterHealthAmp;

	public Formula hunterSizeMultiplier;

	public Formula smartMonsterRatio;

	[Header("Artillery")]
	public bool enableArtillery = true;

	public bool endWhenRiftOpen;

	public Vector2 initDelay;

	public Formula intervalMinByHuntLevel;

	public Formula intervalMaxByHuntLevel;

	public Formula intervalMultiplierByNormalizedHealth;

	public float bigArtilleryRandomMag;

	public Formula bigArtilleryChanceByHuntLevel;

	public AnimationCurve bigArtilleryChanceMultiplierByArea;

	public Vector2Int smallArtilleryCount;

	public Formula smallArtilleryCountMultiplierByHuntLevel;

	public Vector2 smallArtilleryInterval;

	public float smallArtilleryRandomMag;

	public AnimationCurve smallArtilleryCountMultiplierByArea;

	public Formula artilleryStrengthByPlayers;

	public Formula artilleryStrengthByHuntLevel;

	public Formula artillerySpeedMultiplierByHuntLevel;

	private float _addedMirageChance;

	private float _addedHunterChance;

	private float _spawnedPopMultiplier;

	public override void OnStartServer()
	{
		base.OnStartServer();
		_addedMirageChance = addedMirageChance.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
		_addedHunterChance = addedHunterChance.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
		_spawnedPopMultiplier = spawnedPopMultiplier.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
		if ((UnityEngine.Object)(object)GameMod_MirageSkin.instance != null && GameMod_MirageSkin.instance.GetCurrentBaseMirageChance() > 0.001f)
		{
			SingletonDewNetworkBehaviour<Room>.instance.monsters.addedMirageChance += _addedMirageChance;
		}
		SingletonDewNetworkBehaviour<Room>.instance.monsters.addedHunterChance += _addedHunterChance;
		SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier *= _spawnedPopMultiplier;
		SingletonDewNetworkBehaviour<Room>.instance.monsters.onAfterSpawn += new Action<Entity>(OnAfterSpawn);
		if (enableArtillery)
		{
			GameManager.CallOnReady(() =>
			{
				((MonoBehaviour)(object)this).StartCoroutine(SmallArtilleryRoutine());
				((MonoBehaviour)(object)this).StartCoroutine(BigArtilleryRoutine());
			});
		}
	}

	private void OnAfterSpawn(Entity obj)
	{
		if (obj.Status.HasStatusEffect<Se_HunterBuff>())
		{
			ApplyHunterStatBonusAndAIPrediction(obj, NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
		}
	}

	public void ApplyHunterStatBonusAndAIPrediction(Entity entity, int level)
	{
		float num = hunterHealthAmp.Evaluate(level);
		float num2 = hunterDamageAmp.Evaluate(level);
		float num3 = hunterSizeMultiplier.Evaluate(level);
		entity.Status.AddStatBonus(new StatBonus
		{
			maxHealthPercentage = num * 100f,
			abilityPowerPercentage = num2 * 100f,
			attackDamagePercentage = num2 * 100f
		});
		entity.Visual.GetNewTransformModifier().scaleMultiplier = Vector3.one * num3;
		entity.Control.outerRadius *= num3;
		if (UnityEngine.Random.value < smartMonsterRatio.Evaluate(level))
		{
			entity.AI.predictionStrengthOverride = () => UnityEngine.Random.value;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		((MonoBehaviour)(object)this).StopAllCoroutines();
		if (!((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance == null))
		{
			if ((UnityEngine.Object)(object)GameMod_MirageSkin.instance != null && GameMod_MirageSkin.instance.GetCurrentBaseMirageChance() > 0.001f)
			{
				SingletonDewNetworkBehaviour<Room>.instance.monsters.addedMirageChance -= _addedMirageChance;
			}
			SingletonDewNetworkBehaviour<Room>.instance.monsters.addedHunterChance -= _addedHunterChance;
			SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier /= _spawnedPopMultiplier;
			SingletonDewNetworkBehaviour<Room>.instance.monsters.onAfterSpawn -= new Action<Entity>(OnAfterSpawn);
		}
	}

	private IEnumerator SmallArtilleryRoutine()
	{
		int currentHuntLevel = NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel;
		float intervalMin = intervalMinByHuntLevel.Evaluate(currentHuntLevel);
		float intervalMax = intervalMaxByHuntLevel.Evaluate(currentHuntLevel);
		float countMultiplier = smallArtilleryCountMultiplierByHuntLevel.Evaluate(currentHuntLevel) * smallArtilleryCountMultiplierByArea.Evaluate(SingletonDewNetworkBehaviour<Room>.instance.map.mapData.area);
		float speed = artillerySpeedMultiplierByHuntLevel.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
		yield return new WaitForSeconds(UnityEngine.Random.Range(initDelay.x, initDelay.y));
		while (true)
		{
			Hero hero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
			if ((UnityEngine.Object)(object)hero == null)
			{
				break;
			}
			int count = DewMath.RandomRoundToInt((float)UnityEngine.Random.Range(smallArtilleryCount.x, smallArtilleryCount.y) * countMultiplier);
			float strength = artilleryStrengthByPlayers.Evaluate(DewPlayer.gamePlayers.Count((DewPlayer p) => !p.hero.IsNullInactiveDeadOrKnockedOut())) * artilleryStrengthByHuntLevel.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
			for (int i = 0; i < count; i++)
			{
				if (endWhenRiftOpen && Rift.instance.isOpen && !Rift.instance.isLocked)
				{
					yield break;
				}
				Vector3 end = AbilityTrigger.PredictPoint_Simple(null, UnityEngine.Random.value, hero, 1f) + UnityEngine.Random.insideUnitSphere.Flattened() * smallArtilleryRandomMag;
				end = Dew.GetValidAgentDestination_Closest(hero.agentPosition, end);
				CreateAbilityInstance(end, null, default, (Ai_HunterArtillery_Small ai) =>
				{
					ai.NetworkstrengthMultiplier = strength;
					ai.NetworkspeedMultiplier = speed;
				});
				yield return new WaitForSeconds(UnityEngine.Random.Range(smallArtilleryInterval.x, smallArtilleryInterval.y));
			}
			float num = intervalMultiplierByNormalizedHealth.Evaluate(hero.normalizedHealth);
			yield return new WaitForSeconds(UnityEngine.Random.Range(intervalMin, intervalMax) * num);
		}
	}

	private IEnumerator BigArtilleryRoutine()
	{
		int currentHuntLevel = NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel;
		float intervalMin = intervalMinByHuntLevel.Evaluate(currentHuntLevel);
		float intervalMax = intervalMaxByHuntLevel.Evaluate(currentHuntLevel);
		float bigChance = bigArtilleryChanceByHuntLevel.Evaluate(currentHuntLevel) * bigArtilleryChanceMultiplierByArea.Evaluate(SingletonDewNetworkBehaviour<Room>.instance.map.mapData.area);
		float speed = artillerySpeedMultiplierByHuntLevel.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
		yield return new WaitForSeconds(UnityEngine.Random.Range(initDelay.x, initDelay.y));
		while (true)
		{
			Hero hero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
			if ((UnityEngine.Object)(object)hero == null || (endWhenRiftOpen && Rift.instance.isOpen && !Rift.instance.isLocked))
			{
				break;
			}
			if (UnityEngine.Random.value < bigChance)
			{
				Vector3 end = hero.agentPosition + UnityEngine.Random.insideUnitSphere.Flattened() * bigArtilleryRandomMag;
				end = Dew.GetValidAgentDestination_Closest(hero.agentPosition, end);
				float strength = artilleryStrengthByPlayers.Evaluate(DewPlayer.gamePlayers.Count((DewPlayer p) => !p.hero.IsNullInactiveDeadOrKnockedOut())) * artilleryStrengthByHuntLevel.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
				CreateAbilityInstance(end, null, default, (Ai_HunterArtillery_Big ai) =>
				{
					ai.NetworkstrengthMultiplier = strength;
					ai.NetworkspeedMultiplier = speed;
				});
			}
			float num = intervalMultiplierByNormalizedHealth.Evaluate(hero.normalizedHealth);
			yield return new WaitForSeconds(UnityEngine.Random.Range(intervalMin, intervalMax) * num);
		}
	}

	private void MirrorProcessed()
	{
	}
}
