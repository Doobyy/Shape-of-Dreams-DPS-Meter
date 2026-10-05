using System.Collections;
using Mirror;
using UnityEngine;

public class RoomMod_InkStrikeWarning : RoomModifierBase
{
	public Vector2 initDelay;

	public Vector2 interval;

	public float ramdomMagnitude;

	public float baseChance;

	public AnimationCurve chanceMultiplierByArea;

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (SingletonDewNetworkBehaviour<Room>.instance.didClearRoom)
		{
			DestroyIfActive();
			return;
		}
		GameManager.CallOnReady(() =>
		{
			((MonoBehaviour)(object)this).StartCoroutine(ArtilleryRoutine());
		});
		SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(DestroyIfActive);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
			{
				SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.RemoveListener(DestroyIfActive);
			}
			((MonoBehaviour)(object)this).StopAllCoroutines();
		}
	}

	private IEnumerator ArtilleryRoutine()
	{
		float spawnChance = baseChance * chanceMultiplierByArea.Evaluate(SingletonDewNetworkBehaviour<Room>.instance.map.mapData.area);
		if (isNewInstance)
		{
			yield return new WaitForSeconds(Random.Range(initDelay.x, initDelay.y));
		}
		while (true)
		{
			Hero hero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
			if (hero.IsNullInactiveDeadOrKnockedOut())
			{
				break;
			}
			if (Random.value < spawnChance)
			{
				Vector3 end = hero.agentPosition + Random.insideUnitSphere.Flattened() * ramdomMagnitude;
				end = Dew.GetValidAgentDestination_Closest(hero.agentPosition, end);
				CreateAbilityInstance<Ai_RoomMod_InkStrikeWarning_Artillery>(end, null, default);
			}
			yield return new WaitForSeconds(Random.Range(interval.x, interval.y));
		}
	}

	private void MirrorProcessed()
	{
	}
}
