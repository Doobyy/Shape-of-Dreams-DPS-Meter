using System.Collections;
using UnityEngine;

public class Gem_R_Stillness : Gem
{
	public float delay;

	protected override void OnCastCompleteBeforePrepare(EventInfoCast info)
	{
		base.OnCastCompleteBeforePrepare(info);
		if (IsReady())
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			info.instance.LockDestroy();
			if (!isValid)
			{
				info.instance.UnlockDestroy();
			}
			else
			{
				NotifyUse();
				StartCooldown();
				yield return new WaitForSeconds(delay);
				info.instance.UnlockDestroy();
				if (isValid)
				{
					CreateAbilityInstanceWithSource<Ai_Gem_R_Stillness>(info.instance, owner.position, Quaternion.identity, new CastInfo(owner));
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
