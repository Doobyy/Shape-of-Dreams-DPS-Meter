using System.Collections;
using UnityEngine;

[AchUnlockOnComplete(typeof(Gem_R_Blood))]
public class ACH_KYAAAAAA : DewAchievementItem
{
	private const float RequiredDamageRatio = 0.7f;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnTakeDamage((EventInfoDamage dmg) =>
		{
			if (!(dmg.negatedAmountByShield > 1f) && !(dmg.damage.amount < 0.7f * hero.maxHealth))
			{
				AchStartCoroutine(Routine());
			}
		});
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(1.5f);
			if (!hero.IsNullInactiveDeadOrKnockedOut())
			{
				Complete();
			}
		}
	}
}
