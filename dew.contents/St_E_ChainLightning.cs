using UnityEngine;

public class St_E_ChainLightning : SkillTrigger
{
	protected override Vector3 GetInstanceSpawnPosition(int configIndex, CastInfo info)
	{
		return info.caster.Visual.GetCenterPosition();
	}

	private void MirrorProcessed()
	{
	}
}
