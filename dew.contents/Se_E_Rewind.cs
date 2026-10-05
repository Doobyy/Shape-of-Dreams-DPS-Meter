using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_E_Rewind : StatusEffect
{
	public float duration = 8f;

	public ScalingValue reduceAmount;

	public GameObject useEffect;

	[NonSerialized]
	public SkillTrigger targetSkill;

	private Hero _casterHero;

	private AbilityInstance _trackedInstance;

	private Action<EventInfoDamage> _onTrackedInstanceDealDamage;

	[SyncVar]
	internal bool _didConsume;

	public override bool reuseInRoom => true;

	public bool Network_didConsume
	{
		get
		{
			return _didConsume;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _didConsume, 4096uL, (Action<bool, bool>)null);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Network_didConsume = false;
		_casterHero = null;
		_trackedInstance = null;
		_onTrackedInstanceDealDamage = null;
		targetSkill = null;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (targetSkill is St_E_Rewind)
		{
			Destroy();
			return;
		}
		SetTimer(duration);
		ShowOnScreenTimer();
		_casterHero = (Hero)info.caster;
		_casterHero.HeroEvent_OnAbilityInstanceBeforePrepareFromSkill += new Action<EventInfoSkillAbilityInstance>(AttachHandlerToNewInstance);
		if (!((UnityEngine.Object)(object)targetSkill == null))
		{
			ApplyCooldownReduction(targetSkill, GetValue(reduceAmount));
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)_casterHero != null)
			{
				_casterHero.HeroEvent_OnAbilityInstanceBeforePrepareFromSkill -= new Action<EventInfoSkillAbilityInstance>(AttachHandlerToNewInstance);
			}
			if ((UnityEngine.Object)(object)_trackedInstance != null && _onTrackedInstanceDealDamage != null)
			{
				_trackedInstance.ActorEvent_OnDealDamage -= _onTrackedInstanceDealDamage;
			}
		}
	}

	private void AttachHandlerToNewInstance(EventInfoSkillAbilityInstance info)
	{
		if ((info.type != HeroSkillLocation.Q && info.type != HeroSkillLocation.W && info.type != HeroSkillLocation.E && info.type != HeroSkillLocation.R) || info.skill is St_E_Rewind)
		{
			return;
		}
		_casterHero.HeroEvent_OnAbilityInstanceBeforePrepareFromSkill -= new Action<EventInfoSkillAbilityInstance>(AttachHandlerToNewInstance);
		if ((UnityEngine.Object)(object)this == null || !isActive)
		{
			return;
		}
		List<Entity> damaged = new List<Entity>();
		_trackedInstance = info.instance;
		_onTrackedInstanceDealDamage = (EventInfoDamage damage) =>
		{
			if (!damage.chain.DidReact(this) && !damaged.Contains(damage.victim) && victim.CheckEnemyOrNeutral(damage.victim))
			{
				damaged.Add(damage.victim);
				CreateStatusEffect(damage.victim, (Se_E_Rewind_Damage c) =>
				{
					c.chain = damage.chain.New(this);
				});
			}
		};
		_trackedInstance.ActorEvent_OnDealDamage += _onTrackedInstanceDealDamage;
		FxPlayNetworked(useEffect, base.info.caster);
		DestroyOnDestroy(info.instance);
		StopTimer();
		HideOnScreenTimer();
		Network_didConsume = true;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, _didConsume);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _didConsume);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _didConsume, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _didConsume, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
