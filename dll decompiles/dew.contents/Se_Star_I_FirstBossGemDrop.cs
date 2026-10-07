using System;
using Mirror;
using UnityEngine;

public class Se_Star_I_FirstBossGemDrop : StarEffect
{
	public StarScalingValue gemSpawnCount;

	public int spawnGemQuality;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(OnKillOrAssist);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(OnKillOrAssist);
		}
	}

	private void OnKillOrAssist(EventInfoKill infoKill)
	{
		if (infoKill.victim is BossMonster bossMonster)
		{
			for (int i = 0; i < GetValueInt(gemSpawnCount); i++)
			{
				NetworkedManagerBase<LootManager>.instance.SelectGemAndQuality(Rarity.Common, out var gem, out var _);
				Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(bossMonster.position);
				Dew.CreateGem(gem, goodRewardPosition, spawnGemQuality, player);
			}
			hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(OnKillOrAssist);
			DestroyIfActive();
		}
	}

	private void MirrorProcessed()
	{
	}
}
