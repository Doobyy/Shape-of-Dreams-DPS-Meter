using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(Gem_L_ChaosApple))]
public class ACH_CHAOTIC_GOOD : DewAchievementItem
{
	private const int RequiredLegendaryChaos = 5;

	[AchPersistentVar]
	private int _currentActivationCount;

	public override int GetMaxProgress()
	{
		return 5;
	}

	public override int GetCurrentProgress()
	{
		return _currentActivationCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAdd);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnActorAdd);
		}
	}

	private void OnActorAdd(Actor obj)
	{
		Shrine_Chaos chaos = obj as Shrine_Chaos;
		if (chaos == null)
		{
			return;
		}
		chaos.ClientEvent_OnSuccessfulUse += (Action<Entity>)((Entity user) =>
		{
			if (chaos.rarity == Rarity.Legendary && !((UnityEngine.Object)(object)user == null) && !((UnityEngine.Object)(object)user.owner != (UnityEngine.Object)(object)DewPlayer.local))
			{
				_currentActivationCount++;
				if (_currentActivationCount >= 5)
				{
					Complete();
				}
			}
		});
	}
}
