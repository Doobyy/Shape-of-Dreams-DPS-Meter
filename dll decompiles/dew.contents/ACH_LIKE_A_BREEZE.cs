using UnityEngine;

[AchUnlockOnComplete(typeof(St_QR_InfernalTales))]
public class ACH_LIKE_A_BREEZE : DewAchievementItem
{
	private float _lastDamageTime = float.NegativeInfinity;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (((object)DewPlayer.local.hero).GetType() != typeof(Hero_Bismuth))
		{
			return;
		}
		AchOnKillOrAssist((EventInfoKill kill) =>
		{
			if (kill.victim is BossMonster && kill.victim.creationTime > _lastDamageTime)
			{
				Complete();
			}
		});
		AchOnTakeDamage((EventInfoDamage _) =>
		{
			_lastDamageTime = Time.time;
		});
	}
}
