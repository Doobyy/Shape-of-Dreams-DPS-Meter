using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class At_Atk_YubarStardust : AttackTrigger
{
	public DewBeamRenderer beamRenderer;

	[SyncVar(hook = "OnTargetChanged")]
	private Entity _target;

	private Vector3 _targetPos;

	protected NetworkBehaviourSyncVar ____targetNetId;

	public Action<Entity, Entity> _Mirror_SyncVarHookDelegate__target;

	public Entity Network_target
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Entity>(____targetNetId, ref _target);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Entity>(value, ref _target, 1024uL, _Mirror_SyncVarHookDelegate__target, ref ____targetNetId);
		}
	}

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			Hero hero = newOwner as Hero;
			if (!((UnityEngine.Object)(object)hero == null))
			{
				hero.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(HeroEventOnSkillUse);
			}
		}
	}

	[Server]
	public void ClearTarget()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void At_Atk_YubarStardust::ClearTarget()' called when server was not active");
		}
		else
		{
			Network_target = null;
		}
	}

	private void HeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		Network_target = null;
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			Hero hero = owner as Hero;
			if (!((UnityEngine.Object)(object)hero == null))
			{
				hero.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(HeroEventOnSkillUse);
			}
		}
	}

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		AbilityInstance result = base.OnCastComplete(configIndex, info);
		if ((UnityEngine.Object)(object)info.target != null)
		{
			Network_target = info.target;
		}
		return result;
	}

	private void OnTargetChanged(Entity oldv, Entity newv)
	{
		if ((UnityEngine.Object)(object)newv == null || !newv.isActive)
		{
			beamRenderer.enabled = false;
			return;
		}
		beamRenderer.SetPoints(owner.Visual.GetBonePosition((HumanBodyBones)18), Network_target.Visual.GetCenterPosition());
		beamRenderer.enabled = true;
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
	}

	private void LateUpdate()
	{
		if (isActive && !((UnityEngine.Object)(object)Network_target == null))
		{
			beamRenderer.SetPoints(owner.Visual.GetBonePosition((HumanBodyBones)18), Network_target.Visual.GetCenterPosition());
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (beamRenderer != null)
		{
			beamRenderer.enabled = false;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && beamRenderer != null)
		{
			beamRenderer.enabled = false;
		}
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)Network_target != null)
		{
			AbilityTrigger attackAbility = owner.Ability.attackAbility;
			if ((UnityEngine.Object)(object)owner.Control.attackTarget != (UnityEngine.Object)(object)Network_target || !attackAbility.IsTargetInRange(Network_target) || !attackAbility.currentConfig.selfValidator.Evaluate(owner) || !attackAbility.currentConfig.targetValidator.Evaluate(owner, Network_target) || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
			{
				Network_target = null;
			}
		}
	}

	public At_Atk_YubarStardust()
	{
		_Mirror_SyncVarHookDelegate__target = OnTargetChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_target);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Network_target);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Entity>(ref _target, _Mirror_SyncVarHookDelegate__target, reader, ref ____targetNetId);
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Entity>(ref _target, _Mirror_SyncVarHookDelegate__target, reader, ref ____targetNetId);
		}
	}
}
