using System.Collections.Generic;
using UnityEngine;

[AchUnlockOnComplete(typeof(St_E_MassCleanse))]
public class ACH_TRUE_SUPPORT : DewAchievementItem
{
	private const float TimeWindow = 1f;

	private const float StartHealthThreshold = 0.3f;

	private List<Entity> _keysToRemove = new List<Entity>();

	private Dictionary<Entity, float> _healStartTimes = new Dictionary<Entity, float>();

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchSetInterval(() =>
		{
			PurgeList();
			foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
			{
				if (allHero.normalizedHealth < 0.3f)
				{
					_healStartTimes[allHero] = Time.time;
				}
			}
		}, 0.1f);
		AchOnDoHeal((EventInfoHeal heal) =>
		{
			if (!((Object)(object)heal.actor == null) && heal.target is Hero)
			{
				PurgeList();
				if ((heal.target.currentHealth - heal.amount) / heal.target.maxHealth < 0.3f)
				{
					_healStartTimes[heal.target] = Time.time;
				}
				if (heal.target.normalizedHealth > 0.999f && _healStartTimes.TryGetValue(heal.target, out var value) && Time.time - value < 1f)
				{
					_healStartTimes.Remove(heal.target);
					Complete();
				}
			}
		});
		void PurgeList()
		{
			foreach (KeyValuePair<Entity, float> healStartTime in _healStartTimes)
			{
				if (healStartTime.Key.IsNullOrInactive() || Time.time - healStartTime.Value > 1f)
				{
					_keysToRemove.Add(healStartTime.Key);
				}
			}
			foreach (Entity item in _keysToRemove)
			{
				_healStartTimes.Remove(item);
			}
			_keysToRemove.Clear();
		}
	}
}
