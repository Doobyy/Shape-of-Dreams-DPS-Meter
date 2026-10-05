using UnityEngine;

public class At_Mon_Ink_BossWhiteNight_OneInchPunch : AbilityTrigger
{
	public GameObject fxRageTelegraph;

	public override void OnCastStart(int configIndex, CastInfo info)
	{
		base.OnCastStart(configIndex, info);
		if (info.caster is Mon_Ink_BossWhiteNight { _isRage: not false })
		{
			FxPlayNetworked(fxRageTelegraph, info.caster.agentPosition, Quaternion.AngleAxis(info.angle, Vector3.up));
		}
	}

	private void MirrorProcessed()
	{
	}
}
