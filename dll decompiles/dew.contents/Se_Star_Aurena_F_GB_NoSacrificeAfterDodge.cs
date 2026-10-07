using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_F_GB_NoSacrificeAfterDodge : StarEffect
{
	public float freeDuration = 3f;

	[SyncVar]
	private float _lastDodgeTime = float.NegativeInfinity;

	private OnScreenTimerHandle _handle;

	public override Type heroType => typeof(Hero_Aurena);

	public override Type skillType => typeof(St_Q_GoldenBurst);

	public float Network_lastDodgeTime
	{
		get
		{
			return _lastDodgeTime;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _lastDodgeTime, 8192uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
			hero.HeroEvent_OnAbilityInstanceCreatedFromSkill += new Action<EventInfoSkillAbilityInstance>(HeroEventOnAbilityInstanceCreatedFromSkill);
		}
	}

	private void HeroEventOnAbilityInstanceCreatedFromSkill(EventInfoSkillAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_GoldenBurst)
		{
			Network_lastDodgeTime = float.NegativeInfinity;
			UpdateFreeCastStatus();
		}
	}

	private void UpdateFreeCastStatus()
	{
		bool networkcastForFree = Time.time - _lastDodgeTime < freeDuration;
		if (skill is St_Q_GoldenBurst st_Q_GoldenBurst)
		{
			st_Q_GoldenBurst.NetworkcastForFree = networkcastForFree;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			UpdateFreeCastStatus();
		}
		if ((UnityEngine.Object)(object)hero == null || !((NetworkBehaviour)hero).isOwned)
		{
			return;
		}
		bool flag = Time.time - _lastDodgeTime < freeDuration;
		if (flag && _handle == null)
		{
			_handle = ShowOnScreenTimerLocally(new OnScreenTimerHandle
			{
				fillAmountGetter = () => 1f - (Time.time - _lastDodgeTime) / freeDuration
			});
		}
		else if (!flag && _handle != null)
		{
			HideOnScreenTimerLocally(_handle);
			_handle = null;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_handle != null)
		{
			HideOnScreenTimerLocally(_handle);
			_handle = null;
		}
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
			hero.HeroEvent_OnAbilityInstanceCreatedFromSkill -= new Action<EventInfoSkillAbilityInstance>(HeroEventOnAbilityInstanceCreatedFromSkill);
		}
	}

	private void ClientHeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		if (hero.isInCombat && obj.type == HeroSkillLocation.Movement)
		{
			Network_lastDodgeTime = Time.time;
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _lastDodgeTime);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _lastDodgeTime);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _lastDodgeTime, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _lastDodgeTime, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
