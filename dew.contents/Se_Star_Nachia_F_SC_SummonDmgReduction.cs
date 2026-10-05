using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_F_SC_SummonDmgReduction : StarEffect
{
	public int requiredCount = 4;

	public float abilityPowerPercentage = 15f;

	public float armorAmount = 40f;

	public GameObject fxApply;

	public GameObject fxApplySummon;

	private StatBonus _heroBonus;

	private Dictionary<Sum_Q_SylvanCall_LeafHound, StatBonus> _houndBonuses = new Dictionary<Sum_Q_SylvanCall_LeafHound, StatBonus>();

	private List<Sum_Q_SylvanCall_LeafHound> _current = new List<Sum_Q_SylvanCall_LeafHound>();

	public override Type heroType => typeof(Hero_Nachia);

	public override Type skillType => typeof(St_Q_SylvanCall);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		_heroBonus = DoStatBonus();
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnAdd);
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorRemove += new Action<Actor>(OnRemove);
		foreach (Summon summon in hero.summons)
		{
			if (summon is Sum_Q_SylvanCall_LeafHound item && !_current.Contains(item))
			{
				_current.Add(item);
			}
		}
		Apply();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		FxStopNetworked(fxApply);
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnAdd);
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorRemove -= new Action<Actor>(OnRemove);
		}
		foreach (KeyValuePair<Sum_Q_SylvanCall_LeafHound, StatBonus> houndBonuse in _houndBonuses)
		{
			if (!houndBonuse.Key.IsNullOrInactive())
			{
				houndBonuse.Key.Status.RemoveStatBonus(houndBonuse.Value);
			}
		}
		_houndBonuses.Clear();
	}

	private void OnAdd(Actor obj)
	{
		if (obj is Sum_Q_SylvanCall_LeafHound sum_Q_SylvanCall_LeafHound && !((UnityEngine.Object)(object)sum_Q_SylvanCall_LeafHound.info.caster != (UnityEngine.Object)(object)hero))
		{
			if (!_current.Contains(sum_Q_SylvanCall_LeafHound))
			{
				_current.Add(sum_Q_SylvanCall_LeafHound);
			}
			Apply();
		}
	}

	private void OnRemove(Actor obj)
	{
		if (obj is Sum_Q_SylvanCall_LeafHound sum_Q_SylvanCall_LeafHound && !((UnityEngine.Object)(object)sum_Q_SylvanCall_LeafHound.info.caster != (UnityEngine.Object)(object)hero))
		{
			_current.Remove(sum_Q_SylvanCall_LeafHound);
			_houndBonuses.Remove(sum_Q_SylvanCall_LeafHound);
			Apply();
		}
	}

	private void Apply()
	{
		bool flag = _current.Count >= requiredCount;
		_heroBonus.abilityPowerPercentage = (flag ? abilityPowerPercentage : 0f);
		if (flag)
		{
			FxPlayNetworked(fxApply, victim);
			{
				foreach (Sum_Q_SylvanCall_LeafHound item in _current)
				{
					if (!_houndBonuses.ContainsKey(item) && !item.IsNullOrInactive())
					{
						_houndBonuses[item] = item.Status.AddStatBonus(new StatBonus
						{
							armorFlat = armorAmount
						});
						FxPlayNewNetworked(fxApplySummon, item);
					}
				}
				return;
			}
		}
		FxStopNetworked(fxApply);
		foreach (KeyValuePair<Sum_Q_SylvanCall_LeafHound, StatBonus> houndBonuse in _houndBonuses)
		{
			if (!houndBonuse.Key.IsNullOrInactive())
			{
				houndBonuse.Key.Status.RemoveStatBonus(houndBonuse.Value);
			}
		}
		_houndBonuses.Clear();
	}

	private void MirrorProcessed()
	{
	}
}
