using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_D_APOnDodge : StarEffect
{
	public override Type heroType => typeof(Hero_Yubar);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		}
	}

	private void ClientHeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		if (obj.type == HeroSkillLocation.Movement)
		{
			if (victim.Status.TryGetStatusEffect<Se_Star_Yubar_D_APOnDodge_APBonus>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_Star_Yubar_D_APOnDodge_APBonus>(victim);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		}
	}

	private void MirrorProcessed()
	{
	}
}
