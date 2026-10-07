using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_D_CanineLover : StarEffect
{
	public StarScalingValue hasteAmount;

	public float requiredDistance = 9f;

	public float damageReduction = 0.3f;

	private readonly List<Entity> _subscribedCanines = new List<Entity>();

	private Action<EventInfoKill> _onCanineDeath;

	public override Type heroType => typeof(Hero_Nachia);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		hero.dealtDamageProcessor.Add(ReduceDamageAgainstCanines);
		NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += new Action<Entity>(OnEntityAdd);
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			OnEntityAdd(allEntity);
		}
	}

	private void OnEntityAdd(Entity obj)
	{
		if (IsCanine(obj))
		{
			obj.EntityEvent_OnDeath += new Action<EventInfoKill>(EntityEventOnDeath);
			_subscribedCanines.Add(obj);
		}
	}

	private void EntityEventOnDeath(EventInfoKill obj)
	{
		if (this.IsNullOrInactive() || hero.IsNullOrInactive() || Vector2.Distance(hero.agentPosition.ToXY(), obj.victim.agentPosition.ToXY()) > requiredDistance)
		{
			return;
		}
		if (hero.Status.TryGetStatusEffect<Se_Star_Nachia_D_CanineLover_Anger>(out var effect))
		{
			effect.ResetTimer();
			return;
		}
		CreateStatusEffect(hero, (Se_Star_Nachia_D_CanineLover_Anger se) =>
		{
			se.amount = GetValue(hasteAmount);
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)hero != null)
		{
			hero.dealtDamageProcessor.Remove(ReduceDamageAgainstCanines);
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd -= new Action<Entity>(OnEntityAdd);
		}
		foreach (Entity subscribedCanine in _subscribedCanines)
		{
			if ((UnityEngine.Object)(object)subscribedCanine != null)
			{
				subscribedCanine.EntityEvent_OnDeath -= _onCanineDeath;
			}
		}
		_subscribedCanines.Clear();
	}

	private void ReduceDamageAgainstCanines(ref DamageData data, Actor actor, Entity target)
	{
		if (target is Monster && IsCanine(target) && !data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyReduction(damageReduction);
		}
	}

	private bool IsCanine(Entity e)
	{
		if (!(e is Mon_Forest_Hound) && !(e is Mon_LavaLand_SuperheatedWolf) && !(e is Mon_LavaLand_Magmadon) && !(e is Sum_Q_MoonlightPact_Fenrir))
		{
			return e is Sum_Q_SylvanCall_LeafHound;
		}
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
