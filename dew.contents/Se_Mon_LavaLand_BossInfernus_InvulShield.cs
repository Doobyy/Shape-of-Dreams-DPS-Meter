using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Mon_LavaLand_BossInfernus_InvulShield : StatusEffect
{
	private readonly List<(Mon_LavaLand_InfernusPillar pillar, Action<Actor> handler)> _pillarSubscriptions = new List<(Mon_LavaLand_InfernusPillar, Action<Actor>)>();

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoInvulnerable();
		DoUnstoppable();
		if (SingletonBehaviour<LavaLand_InfernusPillarPosition>.instance == null)
		{
			Destroy();
			return;
		}
		LavaLand_InfernusPillarPosition instance = SingletonBehaviour<LavaLand_InfernusPillarPosition>.instance;
		int num = Mathf.Clamp(2 + Dew.GetAliveHeroCount(), 3, 5);
		if (NetworkedManagerBase<GameManager>.instance.difficulty.specialSkillChanceMultiplier < 0.9f)
		{
			num--;
		}
		RefValue<int> remainingPillars = new RefValue<int>(num);
		for (int i = 0; i < num; i++)
		{
			Dew.SpawnEntity(Dew.GetValidAgentPosition(Dew.GetPositionOnGround(instance.GetRandomPosition())), Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), this, DewPlayer.creep, info.caster.level, (Mon_LavaLand_InfernusPillar pillar) =>
			{
				Action<Actor> action = (Actor _) =>
				{
					if (isActive)
					{
						remainingPillars.value--;
						if (remainingPillars.value == 0)
						{
							DestroyIfActive();
						}
					}
				};
				pillar.ClientActorEvent_OnDestroyed += action;
				_pillarSubscriptions.Add((pillar, action));
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		foreach (var (mon_LavaLand_InfernusPillar, action) in _pillarSubscriptions)
		{
			if ((UnityEngine.Object)(object)mon_LavaLand_InfernusPillar != null)
			{
				mon_LavaLand_InfernusPillar.ClientActorEvent_OnDestroyed -= action;
			}
		}
		_pillarSubscriptions.Clear();
	}

	private void MirrorProcessed()
	{
	}
}
