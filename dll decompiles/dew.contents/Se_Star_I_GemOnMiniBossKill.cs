using System;
using Mirror;
using UnityEngine;

public class Se_Star_I_GemOnMiniBossKill : StarEffect
{
	public StarScalingValue chance;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		if (!(UnityEngine.Random.value > GetValue(chance)) && obj.victim is Monster { type: Monster.MonsterType.MiniBoss } && !hero.IsNullInactiveDeadOrKnockedOut())
		{
			NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(null, out var gem, out var quality);
			Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(obj.victim.agentPosition);
			Dew.CreateGem(gem, goodRewardPosition, quality, player);
		}
	}

	private void MirrorProcessed()
	{
	}
}
