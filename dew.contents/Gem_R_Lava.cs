using System;
using System.Collections.Generic;
using System.Linq;

public class Gem_R_Lava : Gem
{
	public List<Ai_R_Lava_LavaField> activatedLavas = new List<Ai_R_Lava_LavaField>();

	public int maxLavaCount = 5;

	protected override void OnCastCompleteBeforePrepare(EventInfoCast info)
	{
		base.OnCastCompleteBeforePrepare(info);
		if (!IsReady())
		{
			return;
		}
		bool didActivate = false;
		info.instance.ActorEvent_OnDealDamage += (Action<EventInfoDamage>)((EventInfoDamage dealData) =>
		{
			if (!didActivate && dealData.damage.elemental == ElementalType.Fire && owner.CheckEnemyOrNeutral(dealData.victim))
			{
				if (activatedLavas.Count >= maxLavaCount)
				{
					activatedLavas.First().Destroy();
				}
				Ai_R_Lava_LavaField newLava = CreateAbilityInstanceWithSource<Ai_R_Lava_LavaField>(dealData.actor, dealData.victim.position, null, new CastInfo(owner));
				activatedLavas.Add(newLava);
				didActivate = true;
				newLava.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
				{
					activatedLavas.Remove(newLava);
				});
			}
		});
		NotifyUse();
		StartCooldown();
	}

	private void MirrorProcessed()
	{
	}
}
