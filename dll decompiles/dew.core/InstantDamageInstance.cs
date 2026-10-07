using System.Collections;
using Mirror;
using UnityEngine;

public class InstantDamageInstance : DamageInstance
{
	public float damageDelay = 0.05f;

	public GameObject mainEffectAfterDelay;

	protected override IEnumerator OnCreateSequenced()
	{
		float num = ((affectedByAttackSpeed && !info.caster.IsNullInactiveDeadOrKnockedOut()) ? (damageDelay / info.caster.Status.attackSpeedMultiplier) : damageDelay);
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		if (CheckShouldBeDestroyed())
		{
			Destroy();
			yield break;
		}
		if (num > 0f)
		{
			yield return new SI.WaitForSeconds(num);
		}
		if (CheckShouldBeDestroyed())
		{
			Destroy();
			yield break;
		}
		if (mainEffectAfterDelay != null)
		{
			FxPlayNetworked(mainEffectAfterDelay, info.caster);
		}
		DoCollisionChecks();
		OnAfterDelay();
		if (destroyWhenDone)
		{
			Destroy();
		}
	}

	protected virtual void OnAfterDelay()
	{
	}

	private void MirrorProcessed()
	{
	}
}
