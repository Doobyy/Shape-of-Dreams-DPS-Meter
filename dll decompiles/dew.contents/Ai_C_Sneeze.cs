using Mirror;
using UnityEngine;

public class Ai_C_Sneeze : AbilityInstance
{
	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((NetworkedManagerBase<ZoneManager>.instance.currentZone == null || NetworkedManagerBase<ZoneManager>.instance.currentZone.name != "Zone_Sky") && Random.value > ((info.caster is Hero_Yubar) ? 1f : 0.01f))
		{
			Destroy();
			return;
		}
		int num = Random.Range(1, 5);
		if (Random.value < 0.2f)
		{
			num *= 2;
		}
		if (Random.value < 0.1f)
		{
			num *= 2;
		}
		for (int i = 0; i < num; i++)
		{
			CreateAbilityInstance(position, null, new CastInfo(info.caster, info.angle + 15f * ((float)i - (float)(num - 1) / 2f)), (Ai_Mon_Sky_BigBaam_BeamAtk ai) =>
			{
				ai.endDaze = 0f;
				ai.beamDuration *= 0.8f;
			});
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
