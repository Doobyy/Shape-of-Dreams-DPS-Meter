using System.Collections;
using UnityEngine;

public class Ai_L_SpectreBullet : StandardProjectile
{
	public ScalingValue damage;

	public ScalingValue dreamDustAmount;

	public float gracePeriod = 0.5f;

	public GameObject fxDropReward;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		Damage(damage).DoAttackEffect(AttackEffectType.Others).Dispatch(hit.entity);
		Destroy();
		LockDestroy();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(gracePeriod);
			UnlockDestroy();
			if (hit.entity.IsNullOrInactive())
			{
				Vector3 vector = (((Object)(object)hit.entity != null) ? hit.entity.position : position);
				vector = Dew.GetPositionOnGround(vector);
				NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, Mathf.RoundToInt(GetValue(dreamDustAmount)), vector, info.caster as Hero);
				FxPlayNewNetworked(fxDropReward, vector, null);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
