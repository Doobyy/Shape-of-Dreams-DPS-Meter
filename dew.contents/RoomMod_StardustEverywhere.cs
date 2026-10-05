using System.Linq;
using Mirror;
using UnityEngine;

public class RoomMod_StardustEverywhere : RoomModifierBase
{
	public int stardustCount = 100;

	public float targetedChance = 0.15f;

	public Vector2 targetedRange;

	public Vector2 nonTargetedRange;

	public Vector2 fallInterval;

	private float _nextFallTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer || !isNewInstance)
		{
			return;
		}
		for (int i = 0; i < stardustCount / 5; i++)
		{
			SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out var pos);
			CreateActor(pos, Quaternion.Euler(0f, Random.Range(0, 360), 0f), (Shrine_Stardust s) =>
			{
				s.amount = 5;
			});
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !(Time.time > _nextFallTime))
		{
			return;
		}
		_nextFallTime = Time.time + Random.Range(fallInterval.x, fallInterval.y);
		if (ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Normal && !ManagerBase<CameraManager>.instance.isPlayingCutscene && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && !SingletonDewNetworkBehaviour<Room>.instance.didClearRoom)
		{
			bool flag = Random.value < (float)DewPlayer.gamePlayers.Count((DewPlayer p) => !p.hero.IsNullInactiveDeadOrKnockedOut()) * targetedChance;
			Hero hero = Dew.SelectRandomAliveHero();
			Vector2 vector = (flag ? targetedRange : nonTargetedRange);
			Vector3 vector2 = hero.agentPosition + Random.onUnitSphere.Flattened() * Random.Range(vector.x, vector.y);
			vector2 = Dew.GetPositionOnGround(vector2);
			vector2 = Dew.GetValidAgentDestination_Closest(hero.agentPosition, vector2);
			CreateAbilityInstance<Ai_RoomMod_StardustEverywhere_Starfall>(vector2, null, default);
		}
	}

	private void MirrorProcessed()
	{
	}
}
