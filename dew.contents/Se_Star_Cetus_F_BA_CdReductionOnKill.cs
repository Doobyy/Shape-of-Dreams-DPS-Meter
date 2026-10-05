using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_F_BA_CdReductionOnKill : StarEffect
{
	public float trackingGracePeriod = 3f;

	public float cdReductionAmount = 1f;

	private static readonly List<Entity> _expired = new List<Entity>(16);

	private SkillTrigger _skill;

	private readonly Dictionary<Entity, float> _hitTimes = new Dictionary<Entity, float>();

	private readonly List<Ai_R_BackOff_Damage> _subscribedAis = new List<Ai_R_BackOff_Damage>();

	private Action<EventInfoKill> _cachedOnVictimDeath;

	public override Type heroType => typeof(Hero_Cetus);

	public override Type skillType => typeof(St_R_BackOff);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoSkill((SkillTrigger s) =>
		{
			_skill = s;
			s.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			return () =>
			{
				s.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
				_skill = null;
			};
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		foreach (Ai_R_BackOff_Damage subscribedAi in _subscribedAis)
		{
			if ((bool)(UnityEngine.Object)(object)subscribedAi)
			{
				subscribedAi.onHit -= new Action<Entity>(OnSkillHit);
			}
		}
		_subscribedAis.Clear();
		foreach (KeyValuePair<Entity, float> hitTime in _hitTimes)
		{
			if ((UnityEngine.Object)(object)hitTime.Key != null)
			{
				hitTime.Key.EntityEvent_OnDeath -= _cachedOnVictimDeath;
			}
		}
		_hitTimes.Clear();
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_R_BackOff_Damage ai_R_BackOff_Damage)
		{
			ai_R_BackOff_Damage.onHit += new Action<Entity>(OnSkillHit);
			_subscribedAis.Add(ai_R_BackOff_Damage);
		}
	}

	private void OnSkillHit(Entity entity)
	{
		if (!((UnityEngine.Object)(object)entity == null) && !entity.Status.isDead)
		{
			if (!_hitTimes.ContainsKey(entity))
			{
				entity.EntityEvent_OnDeath += new Action<EventInfoKill>(OnVictimDeath);
			}
			_hitTimes[entity] = Time.time;
			CullExpired();
		}
	}

	private void OnVictimDeath(EventInfoKill obj)
	{
		if (_hitTimes.TryGetValue(obj.victim, out var value))
		{
			_hitTimes.Remove(obj.victim);
			obj.victim.EntityEvent_OnDeath -= _cachedOnVictimDeath;
			if (!(Time.time - value > trackingGracePeriod) && !_skill.IsNullOrInactive())
			{
				ApplyCooldownReduction(_skill, cdReductionAmount);
			}
		}
	}

	private void CullExpired()
	{
		foreach (KeyValuePair<Entity, float> hitTime in _hitTimes)
		{
			if ((UnityEngine.Object)(object)hitTime.Key == null || Time.time - hitTime.Value > trackingGracePeriod)
			{
				_expired.Add(hitTime.Key);
			}
		}
		foreach (Entity item in _expired)
		{
			_hitTimes.Remove(item);
			if ((UnityEngine.Object)(object)item != null)
			{
				item.EntityEvent_OnDeath -= _cachedOnVictimDeath;
			}
		}
		_expired.Clear();
	}

	private void MirrorProcessed()
	{
	}
}
