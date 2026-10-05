using System;
using Mirror;
using UnityEngine;

[AchUnlockOnComplete(typeof(Gem_E_Crimson))]
public class ACH_IN_CARNAGE_I_BLOOM : DewAchievementItem
{
	private const int RequiredUpgradeCount = 20;

	[SaveVar(SaveVarFlags.Default)]
	private int _currentUpgradeCount;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ClientEventManager>.instance.OnItemUpgraded += new Action<Hero, NetworkBehaviour>(OnItemUpgraded);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnItemUpgraded -= new Action<Hero, NetworkBehaviour>(OnItemUpgraded);
		}
	}

	private void OnItemUpgraded(Hero arg1, NetworkBehaviour arg2)
	{
		if (!((UnityEngine.Object)(object)arg1 != (UnityEngine.Object)(object)hero) && arg2 is SkillTrigger skillTrigger && (skillTrigger.tags & DescriptionTags.Dark) != 0)
		{
			_currentUpgradeCount++;
			if (_currentUpgradeCount >= 20)
			{
				Complete();
			}
		}
	}
}
