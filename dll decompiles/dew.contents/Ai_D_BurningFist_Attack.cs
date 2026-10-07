using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_D_BurningFist_Attack : MeleeAttackInstance
{
	public float dashDuration;

	public float dashDistance;

	public Knockback Knockback;

	public DewEase dashEase;

	public ScalingValue adBonusDmg;

	private Vector3 _direction;

	private Vector3 _startPosition;

	private bool _enableSpawnInstance;

	private float _baseDashDistance;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseDashDistance = dashDistance;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		dashDistance = _baseDashDistance;
		_enableSpawnInstance = false;
	}

	protected override void OnCreate()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		_startPosition = info.caster.agentPosition;
		_direction = (info.point - info.caster.agentPosition).normalized;
		Vector3 agentPosition = info.caster.agentPosition;
		if ((Object)(object)info.target != null)
		{
			Vector3 vector = info.target.agentPosition - info.caster.agentPosition;
			float magnitude = vector.magnitude;
			_direction = vector.normalized;
			if (magnitude < dashDistance)
			{
				dashDistance = magnitude;
			}
		}
		agentPosition = info.caster.agentPosition + _direction * dashDistance;
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = (Channel.BlockedAction.Move | Channel.BlockedAction.Ability | Channel.BlockedAction.Attack),
			duration = dashDuration,
			isAttack = true
		});
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = true,
			canGoOverTerrain = false,
			destination = agentPosition,
			ease = dashEase,
			duration = dashDuration,
			isCanceledByCC = false,
			isDodging = false,
			isFriendly = true,
			onCancel = DestroyIfActive,
			onFinish = OnDashFinished
		});
	}

	[Server]
	private void OnDashFinished()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Ai_D_BurningFist_Attack::OnDashFinished()' called when server was not active");
			return;
		}
		RpcPlayAttackVisual();
		base.OnCreate();
	}

	[ClientRpc]
	private void RpcPlayAttackVisual()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_D_BurningFist_Attack::RpcPlayAttackVisual()", 1859116228, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	public override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		dmg.SetElemental(ElementalType.Fire);
		dmg.AddFlatAmount(GetValue(adBonusDmg));
	}

	public override void OnHit(Entity entity, bool isMain)
	{
		if (((NetworkBehaviour)this).isServer && entity.Status.HasElemental(ElementalType.Fire))
		{
			_enableSpawnInstance = true;
		}
		base.OnHit(entity, isMain);
		if (((NetworkBehaviour)this).isServer)
		{
			Knockback.ApplyWithDirection(_direction, entity);
			FxPlayNewNetworked(isMain ? fxHitMain : fxHitSub, entity);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _enableSpawnInstance)
		{
			CreateAbilityInstance<Ai_D_BurningFist_Attack_Instance>(_startPosition, Quaternion.LookRotation(_direction), new CastInfo(info.caster, CastInfo.GetAngle(_direction)));
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayAttackVisual()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			base.OnCreate();
		}
	}

	protected static void InvokeUserCode_RpcPlayAttackVisual(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayAttackVisual called on server.");
		}
		else
		{
			((Ai_D_BurningFist_Attack)(object)obj).UserCode_RpcPlayAttackVisual();
		}
	}

	static Ai_D_BurningFist_Attack()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_D_BurningFist_Attack), "System.Void Ai_D_BurningFist_Attack::RpcPlayAttackVisual()", (RemoteCallDelegate)InvokeUserCode_RpcPlayAttackVisual);
	}
}
