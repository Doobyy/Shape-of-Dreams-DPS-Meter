using Mirror;
using UnityEngine;

public class St_R_AnnoyingBanner : SkillTrigger
{
	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		AbilityInstance result = base.OnCastComplete(configIndex, info);
		if (!((NetworkBehaviour)this).isServer)
		{
			return result;
		}
		SpawnSummon<Sum_R_AnnoyingBanner>(info.point, Quaternion.identity);
		return result;
	}

	private void MirrorProcessed()
	{
	}
}
