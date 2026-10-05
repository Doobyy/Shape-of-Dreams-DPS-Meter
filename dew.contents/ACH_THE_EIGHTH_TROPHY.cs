using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[AchUnlockOnComplete(typeof(St_E_CrimsonLance))]
public class ACH_THE_EIGHTH_TROPHY : DewAchievementItem
{
	private const int RequiredCount = 8;

	[AchPersistentVar]
	private int _progress;

	private List<NetworkBehaviour> _counted = new List<NetworkBehaviour>();

	public override int GetCurrentProgress()
	{
		return _progress;
	}

	public override int GetMaxProgress()
	{
		return 8;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		foreach (KeyValuePair<int, AbilityTrigger> ability in hero.Ability.abilities)
		{
			if (ability.Value is SkillTrigger { rarity: Rarity.Unique } skillTrigger)
			{
				_counted.Add((NetworkBehaviour)(object)skillTrigger);
			}
		}
		foreach (KeyValuePair<GemLocation, Gem> gem in hero.Skill.gems)
		{
			if (gem.Value.rarity == Rarity.Unique)
			{
				_counted.Add((NetworkBehaviour)(object)gem.Value);
			}
		}
		hero.Skill.ClientHeroEvent_OnSkillPickup += new Action<SkillTrigger>(ClientHeroEventOnSkillPickup);
		hero.Skill.ClientHeroEvent_OnGemPickup += new Action<Gem>(ClientHeroEventOnGemPickup);
		NetworkedManagerBase<ClientEventManager>.instance.OnDismantled += new Action<Hero, NetworkBehaviour>(OnDismantled);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if ((UnityEngine.Object)(object)hero != null)
		{
			hero.Skill.ClientHeroEvent_OnSkillPickup += new Action<SkillTrigger>(ClientHeroEventOnSkillPickup);
			hero.Skill.ClientHeroEvent_OnGemPickup += new Action<Gem>(ClientHeroEventOnGemPickup);
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnDismantled += new Action<Hero, NetworkBehaviour>(OnDismantled);
		}
	}

	private void OnDismantled(Hero arg1, NetworkBehaviour arg2)
	{
		if (!((UnityEngine.Object)(object)arg1 != (UnityEngine.Object)(object)hero))
		{
			Check(arg2);
		}
	}

	private void ClientHeroEventOnGemPickup(Gem obj)
	{
		Check((NetworkBehaviour)(object)obj);
	}

	private void ClientHeroEventOnSkillPickup(SkillTrigger obj)
	{
		Check((NetworkBehaviour)(object)obj);
	}

	private void Check(NetworkBehaviour obj)
	{
		if (!_counted.Contains(obj) && (obj is SkillTrigger { rarity: Rarity.Unique } || obj is Gem { rarity: Rarity.Unique }))
		{
			_progress++;
			_counted.Add(obj);
			if (_progress >= 8)
			{
				Complete();
			}
		}
	}
}
