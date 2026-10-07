using Mirror;
using UnityEngine;

public class Se_Star_I_StartSkill : StarEffect
{
	public StarScalingValue startSkillLevel;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		GameManager.CallOnReady(() =>
		{
			if (isActive)
			{
				Rarity value = NetworkedManagerBase<LootManager>.instance.SelectSkillRarity();
				NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(value, out var trigger, out var _);
				Vector3 pivot = hero.agentPosition + (((Component)(object)Rift.instance).transform.position - hero.agentPosition).normalized * 2.5f;
				pivot = Dew.GetGoodRewardPosition(pivot, 1f);
				Dew.CreateSkillTrigger(trigger, pivot, GetValueInt(startSkillLevel), player);
				Destroy();
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
