using System;
using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Mon_DarkCave_NightOlm : Monster
{
	public GameObject fxCamouflageOn;

	public GameObject fxCamouflageOff;

	public float forceAtkChance;

	public float desiredDistance;

	public float runDirRefreshTime = 1f;

	public Vector2 runDuration;

	public float maxDistance;

	public float baseOpacity;

	public float camouflageCooldown;

	private EntityColorModifier _entModifier;

	private float _camouflageOffTime;

	private bool _isCamouflageOn;

	private float _currentRunDuration;

	private float _runStartTime;

	private float _lastRunDirectionTime;

	private Vector3 _dir;

	private float _rotSpeed;

	private bool _isRunning;

	private float _baseWalkAnimSpeed;

	private float _baseWalkSpeed;

	private Entity _target;

	protected override void OnCreate()
	{
		base.OnCreate();
		_entModifier = Visual.GetNewColorModifier();
		_entModifier.opacity = baseOpacity;
		if (((NetworkBehaviour)this).isServer)
		{
			_isCamouflageOn = false;
			FxPlayNetworked(fxCamouflageOn, this);
			_baseWalkSpeed = Control.baseAgentSpeed;
			Ability.attackAbility.TriggerEvent_OnCastStart += new Action<EventInfoCast>(OnAttackStart);
			EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		_baseWalkAnimSpeed = Animation.model.walkAnimationSpeed;
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return;
		}
		_target = context.targetEnemy;
		if ((Vector3.Distance(context.targetEnemy.agentPosition, agentPosition) < desiredDistance || UnityEngine.Random.Range(0f, 1f) > forceAtkChance) && !AI.Helper_CanBeCast<At_Mon_DarkCave_NightOlm_Atk>() && !_isRunning)
		{
			StartRunning();
		}
		else if (Time.time - _runStartTime < _currentRunDuration)
		{
			if ((UnityEngine.Object)(object)context.targetEnemy == null || !context.targetEnemy.isActive || Vector2.Distance(context.targetEnemy.position.ToXY(), position.ToXY()) > maxDistance)
			{
				StopRunning();
			}
			else if (Time.time - _lastRunDirectionTime > runDirRefreshTime)
			{
				_lastRunDirectionTime = Time.time;
				_dir = GetRunFromTargetDestination(context.targetEnemy);
				Control.MoveToDestination(_dir, immediately: false);
			}
		}
		else
		{
			_isRunning = false;
			AI.Helper_ChaseTarget();
		}
	}

	public void StartRunning()
	{
		_isRunning = true;
		_runStartTime = Time.time;
		_currentRunDuration = UnityEngine.Random.Range(runDuration.x, runDuration.y);
		Animation.model.walkAnimationSpeed = _baseWalkAnimSpeed;
		Control.baseAgentSpeed = _baseWalkSpeed;
	}

	public void StopRunning()
	{
		_isRunning = false;
		_runStartTime = float.NegativeInfinity;
		if ((UnityEngine.Object)(object)AI.context.targetEnemy != null)
		{
			Control.Attack(AI.context.targetEnemy, doChase: true);
		}
		Animation.model.walkAnimationSpeed = 0.75f;
		Control.baseAgentSpeed = 2.5f;
	}

	private Vector3 GetRunFromTargetDestination(Entity target)
	{
		if ((UnityEngine.Object)(object)target == null)
		{
			target = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
		}
		float num = 5f;
		Vector3 normalized = (agentPosition - target.GetAIAgentPosition(this)).normalized;
		Vector3 vector = Quaternion.AngleAxis(UnityEngine.Random.Range(-45f, 45f), Vector3.up) * normalized;
		Vector3 vector2 = agentPosition + vector * num;
		Vector3 validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(agentPosition, vector2);
		if (Vector3.SqrMagnitude(vector2) > Vector3.SqrMagnitude(validAgentDestination_LinearSweep))
		{
			validAgentDestination_LinearSweep = Dew.GetValidAgentDestination_LinearSweep(agentPosition, Vector3.Reflect(-validAgentDestination_LinearSweep, Vector3.forward));
		}
		return validAgentDestination_LinearSweep;
	}

	private void OnAttackStart(EventInfoCast obj)
	{
		_camouflageOffTime = Time.time + 3f;
		if (_isCamouflageOn)
		{
			_isCamouflageOn = false;
			FxStopNetworked(fxCamouflageOn);
			FxPlayNetworked(fxCamouflageOff, this);
			ChangeEntityOpacityToOne(2f);
			Visual.ShowGroundMarker();
		}
	}

	private void OnTakeDamage(EventInfoDamage obj)
	{
		_camouflageOffTime = Time.time;
		if (_isCamouflageOn)
		{
			_isCamouflageOn = false;
			FxStopNetworked(fxCamouflageOn);
			FxPlayNetworked(fxCamouflageOff, this);
			ChangeEntityOpacityToOne(1f);
			Visual.ShowGroundMarker();
		}
	}

	[ClientRpc]
	private void ChangeEntityOpacityToOne(float speed)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteFloat((NetworkWriter)(object)val, speed);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Mon_DarkCave_NightOlm::ChangeEntityOpacityToOne(System.Single)", -1652092454, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void ChangeEntityOpacityToBase()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Mon_DarkCave_NightOlm::ChangeEntityOpacityToBase()", -131027128, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !(Time.time - _camouflageOffTime <= camouflageCooldown) && !_isCamouflageOn)
		{
			FxStopNetworked(fxCamouflageOff);
			FxPlayNetworked(fxCamouflageOn, this);
			_isCamouflageOn = true;
			ChangeEntityOpacityToBase();
			Visual.HideGroundMarker();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_entModifier != null)
		{
			_entModifier.Stop();
			_entModifier = null;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxCamouflageOn);
			FxStopNetworked(fxCamouflageOff);
			EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(OnTakeDamage);
			Ability.attackAbility.TriggerEvent_OnCastStart -= new Action<EventInfoCast>(OnAttackStart);
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_ChangeEntityOpacityToOne__Single(float speed)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			while (_entModifier != null && _entModifier.opacity < 1f)
			{
				_entModifier.opacity += Time.deltaTime * speed;
				yield return null;
			}
		}
	}

	protected static void InvokeUserCode_ChangeEntityOpacityToOne__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC ChangeEntityOpacityToOne called on server.");
		}
		else
		{
			((Mon_DarkCave_NightOlm)(object)obj).UserCode_ChangeEntityOpacityToOne__Single(NetworkReaderExtensions.ReadFloat(reader));
		}
	}

	protected void UserCode_ChangeEntityOpacityToBase()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			while (_entModifier != null && _entModifier.opacity > baseOpacity)
			{
				_entModifier.opacity -= Time.deltaTime;
				yield return null;
			}
		}
	}

	protected static void InvokeUserCode_ChangeEntityOpacityToBase(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC ChangeEntityOpacityToBase called on server.");
		}
		else
		{
			((Mon_DarkCave_NightOlm)(object)obj).UserCode_ChangeEntityOpacityToBase();
		}
	}

	static Mon_DarkCave_NightOlm()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Mon_DarkCave_NightOlm), "System.Void Mon_DarkCave_NightOlm::ChangeEntityOpacityToOne(System.Single)", (RemoteCallDelegate)InvokeUserCode_ChangeEntityOpacityToOne__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(Mon_DarkCave_NightOlm), "System.Void Mon_DarkCave_NightOlm::ChangeEntityOpacityToBase()", (RemoteCallDelegate)InvokeUserCode_ChangeEntityOpacityToBase);
	}
}
