using System;
using UnityEngine;

[AchUnlockOnComplete(typeof(Gem_R_Rejuvenation))]
public class ACH_LOYAL_CUSTOMER : DewAchievementItem
{
	private const int RequiredCount = 15;

	[SaveVar(SaveVarFlags.Default)]
	private int _currentCount;

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
		if (!(obj is Shrine_Guidance shrine_Guidance))
		{
			return;
		}
		shrine_Guidance.ClientEvent_OnSuccessfulUse += (Action<Entity>)((Entity user) =>
		{
			if (!((UnityEngine.Object)(object)user == null) && !((UnityEngine.Object)(object)user.owner != (UnityEngine.Object)(object)DewPlayer.local))
			{
				_currentCount++;
				if (_currentCount >= 15)
				{
					Complete();
				}
			}
		});
	}
}
