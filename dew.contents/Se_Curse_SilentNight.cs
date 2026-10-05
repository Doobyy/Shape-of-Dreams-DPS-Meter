using System;
using Mirror;
using UnityEngine;

public class Se_Curse_SilentNight : CurseStatusEffect
{
	public float[] lockDuration;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			((Hero)victim).ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		}
	}

	private void ClientHeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		if (obj.type != HeroSkillLocation.Movement)
		{
			if (victim.Status.TryGetStatusEffect<Se_Curse_SilentNight_Locked>(out var effect))
			{
				effect.ResetTimer();
				return;
			}
			Se_Curse_SilentNight_Locked se_Curse_SilentNight_Locked = CreateStatusEffect<Se_Curse_SilentNight_Locked>(victim);
			se_Curse_SilentNight_Locked.SetTimer(GetValue(lockDuration));
			se_Curse_SilentNight_Locked.ShowOnScreenTimer("Se_Curse_SilentNight");
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			((Hero)victim).ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		}
	}

	private void MirrorProcessed()
	{
	}
}
