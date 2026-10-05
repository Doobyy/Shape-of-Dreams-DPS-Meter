using UnityEngine;

public class Rev_Special_T1Emote : DewSpecialReverieItem
{
	[AchPersistentVar]
	private int _killCount;

	public override int grantedStardust => 0;

	public override bool excludeFromPool => true;

	public override string[] grantedItems => new string[2] { "Emote_ThirdParty_T1RedBadge", "Emote_ThirdParty_T1AtiThumbsUp" };

	public override int GetCurrentProgress()
	{
		return _killCount;
	}

	public override int GetMaxProgress()
	{
		return 3;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist((EventInfoKill k) =>
		{
			if (k.victim is BossMonster bossMonster && ((Object)(object)bossMonster).name.Contains("Forest_BossDemon"))
			{
				_killCount++;
				if (_killCount >= GetMaxProgress())
				{
					Complete();
				}
			}
		});
	}
}
