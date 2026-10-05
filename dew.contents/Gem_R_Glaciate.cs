using System.Collections;
using UnityEngine;

public class Gem_R_Glaciate : Gem
{
	public float delay;

	public float waitForDashMaxTime;

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			info.instance.LockDestroy();
			yield return new WaitForSeconds(delay);
			if (!isValid)
			{
				info.instance.UnlockDestroy();
			}
			else
			{
				float startTime = Time.time;
				while ((Object)(object)owner != null && owner.Control.isDashing && Time.time - startTime < waitForDashMaxTime)
				{
					yield return null;
				}
				info.instance.UnlockDestroy();
				if (isValid)
				{
					CreateAbilityInstanceWithSource<Ai_Gem_R_Glaciate>(info.instance, owner.position, Quaternion.identity, new CastInfo(owner));
					NotifyUse();
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
