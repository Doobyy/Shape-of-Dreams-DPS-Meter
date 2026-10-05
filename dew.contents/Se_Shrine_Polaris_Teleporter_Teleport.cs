using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Shrine_Polaris_Teleporter_Teleport : StatusEffect
{
	public float delay;

	public float duration;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(victim);
		DoUncollidable();
		DoUntargetable();
		victim.Control.CancelOngoingChannels();
		victim.Control.CancelOngoingDisplacement();
		victim.Control.StartDaze(duration);
		if (victim is Hero hero)
		{
			foreach (Summon summon in hero.summons)
			{
				Summon sum = summon;
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
				IEnumerator Routine()
				{
					yield return new WaitForSeconds(Random.Range(0.1f, 0.35f));
					if (!sum.IsNullInactiveDeadOrKnockedOut() && !sum.Status.HasStatusEffect<Se_Shrine_Polaris_Teleporter_Teleport>())
					{
						sum.CreateStatusEffect<Se_Shrine_Polaris_Teleporter_Teleport>(sum, new CastInfo(sum, info.point));
					}
				}
			}
		}
		yield return new SI.WaitForSeconds(delay);
		Vector3 positionOnGround = Dew.GetPositionOnGround(info.point);
		Teleport(victim, positionOnGround);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
