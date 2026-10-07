using UnityEngine;

public class At_Mon_DarkCave_BossSeeker_GreenChaserOrb : AbilityTrigger
{
	public bool spawnForEveryone;

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		if (spawnForEveryone)
		{
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut() && !((Object)(object)gamePlayer.hero == (Object)(object)info.target))
				{
					base.OnCastComplete(configIndex, new CastInfo(owner, gamePlayer.hero));
				}
			}
		}
		return base.OnCastComplete(configIndex, info);
	}

	private void MirrorProcessed()
	{
	}
}
