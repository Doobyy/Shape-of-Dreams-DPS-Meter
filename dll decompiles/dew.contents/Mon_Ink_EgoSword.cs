using System;
using System.Collections;
using System.Runtime.InteropServices;
using DG.Tweening;
using Mirror;
using UnityEngine;

public class Mon_Ink_EgoSword : Monster
{
	public float amplitude = 0.5f;

	public float frequency = 1f;

	public float rotationSpeed = 30f;

	[NonSerialized]
	public GameObject model;

	public Vector2 runDirRefreshTime;

	public Vector2 runDuration;

	public float maxDistance;

	private float _currentRunDuration;

	private float _runStartTime;

	private float _lastRunDirectionTime;

	private Vector3 _dir;

	private float _rotSpeed;

	private bool _isRunning;

	private EntityTransformModifier _entTransform;

	private float _xAngle;

	[SyncVar]
	private bool _isAttacking;

	private float _baseFrequency;

	public bool Network_isAttacking
	{
		get
		{
			return _isAttacking;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isAttacking, 256uL, (Action<bool, bool>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseFrequency = frequency;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			Network_isAttacking = false;
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		frequency = _baseFrequency * UnityEngine.Random.Range(1.2f, 0.8f);
		_xAngle = 30f * Mathf.Sin(Time.time * 1f);
		if (((NetworkBehaviour)this).isServer)
		{
			EntityEvent_OnAttackFiredBeforePrepare += new Action<EventInfoAttackFired>(OnAttackFiredBeforePrepare);
		}
	}

	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		model = Visual.model.GetCustomMapping<GameObject>("model");
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return;
		}
		if (!AI.Helper_CanBeCast<At_Mon_Ink_EgoSword_Melee>() && !_isRunning)
		{
			StartRunning();
		}
		else if (Time.time - _runStartTime < _currentRunDuration)
		{
			if ((UnityEngine.Object)(object)context.targetEnemy == null || !context.targetEnemy.isActive || Vector2.Distance(context.targetEnemy.GetAIPosition(this).ToXY(), position.ToXY()) > maxDistance)
			{
				StopRunning();
			}
			else if (Time.time - _lastRunDirectionTime > UnityEngine.Random.Range(runDirRefreshTime.x, runDirRefreshTime.y))
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

	private void OnAttackFiredBeforePrepare(EventInfoAttackFired obj)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			Network_isAttacking = true;
			yield return new WaitForSeconds(1f);
			Network_isAttacking = false;
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		float y = amplitude * Mathf.Sin(Time.time * frequency);
		if (!_isAttacking)
		{
			_xAngle = 30f * Mathf.Sin(Time.time * 1f);
			model.transform.Rotate(_xAngle, 0f, 0f);
		}
		else
		{
			ShortcutExtensions.DOLocalRotate(model.transform, Vector3.zero, 0.5f, (RotateMode)0);
		}
		model.transform.position += new Vector3(0f, y, 0f);
	}

	public void StartRunning()
	{
		_isRunning = true;
		_runStartTime = Time.time;
		_currentRunDuration = UnityEngine.Random.Range(runDuration.x, runDuration.y);
	}

	public void StopRunning()
	{
		_isRunning = false;
		_runStartTime = float.NegativeInfinity;
		if ((UnityEngine.Object)(object)AI.context.targetEnemy != null)
		{
			Control.Attack(AI.context.targetEnemy, doChase: true);
		}
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

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, _isAttacking);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isAttacking);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isAttacking, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isAttacking, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
