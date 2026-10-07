using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_BossAzurak_StompBlock : AbilityInstance
{
	public GameObject fxBlockHit;

	public float blockHitEffectInterval = 0.15f;

	public GameObject fxPrepare;

	public float prepareDuration;

	public GameObject fxBlocking;

	public DewCollider stompRange;

	public ScalingValue stompDamage;

	public GameObject fxStompHit;

	public int waveCount = 3;

	public float waveDuration = 3f;

	public Vector2 shootInterval;

	public Vector2 shootRange;

	public Vector2 landTime;

	public float targetedChance = 0.25f;

	private bool _isBlocking;

	private float _lastBlockEffectTime;

	private float _nextShootTime;

	private Channel _channel;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		info.caster.takenDamageProcessor.Add(Processor);
		_channel = info.caster.Control.StartChannel(new Channel
		{
			duration = float.PositiveInfinity,
			blockedActions = Channel.BlockedAction.Everything
		});
		for (int i = 0; i < waveCount; i++)
		{
			GiveShield(info.caster, info.caster.Status.maxHealth * 0.1f, waveDuration);
			Vector3 aIAgentPosition = Dew.GetClosestAliveHero(info.caster.position, fallbackToDead: true, info.caster).GetAIAgentPosition(info.caster);
			FxPlayNetworked(fxPrepare, info.caster);
			info.caster.Control.RotateTowards(aIAgentPosition, immediately: false);
			yield return new SI.WaitForSeconds(prepareDuration);
			FxStopNetworked(fxPrepare);
			FxPlayNetworked(fxBlocking, info.caster);
			List<Entity> entities = stompRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int j = 0; j < entities.Count; j++)
			{
				Entity entity = entities[j];
				Damage(stompDamage).SetOriginPosition(info.caster.position).SetDirection(Vector3.up).Dispatch(entity);
				entity.Visual.KnockUp(1.5f, isFriendly: false);
				FxPlayNewNetworked(fxStompHit, entity);
			}
			handle.Return();
			_isBlocking = true;
			yield return new SI.WaitForSeconds(waveDuration);
			_isBlocking = false;
			FxStopNetworked(fxBlocking);
			yield return new SI.WaitForSeconds(0.5f);
		}
		Destroy();
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		position = info.caster.agentPosition;
		rotation = info.caster.Control.desiredRotation;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !_isBlocking || !(Time.time > _nextShootTime))
		{
			return;
		}
		_nextShootTime = Time.time + Random.Range(shootInterval.x, shootInterval.y);
		Vector3 pos = info.caster.position + Random.insideUnitCircle.ToXZ() * Random.Range(shootRange.x, shootRange.y);
		float landT = Random.Range(landTime.x, landTime.y);
		if (Random.value < targetedChance)
		{
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.position, shootRange.y, tvDefaultHarmfulEffectTargets);
			if (list.Count > 0)
			{
				Entity target = list[Random.Range(0, list.Count)];
				pos = AbilityTrigger.PredictPoint_Simple(info.caster, Random.value, target, landT) + Random.insideUnitCircle.ToXZ() * 4f;
			}
			handle.Return();
		}
		Vector3 vector = (pos - info.caster.position).Flattened();
		if (vector.sqrMagnitude < shootRange.x * shootRange.x)
		{
			vector = vector.normalized * shootRange.x;
			pos = info.caster.position + vector;
		}
		pos = Dew.GetPositionOnGround(pos);
		CreateAbilityInstance(info.caster.position, null, new CastInfo(info.caster, pos), (Ai_Mon_Despair_BossAzurak_StompBlock_Artillery a) =>
		{
			a.initialSpeed = Vector3.Distance(info.caster.position, pos) / landT;
			a.targetSpeed = a.initialSpeed;
			a.acceleration = 0f;
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.takenDamageProcessor.Remove(Processor);
			FxStopNetworked(fxBlocking);
			if (_channel != null && _channel.isAlive)
			{
				_channel.Cancel();
				_channel = null;
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_isBlocking = false;
		_lastBlockEffectTime = 0f;
		_nextShootTime = 0f;
		_channel = null;
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (!_isBlocking)
		{
			return;
		}
		Entity entity = actor.firstEntity;
		if (!((Object)(object)entity == null) && info.caster.CheckEnemyOrNeutral(entity) && !(Vector2.Angle(((Component)(object)info.caster).transform.forward.ToXY(), (entity.agentPosition - info.caster.agentPosition).ToXY()) > 90f))
		{
			data.BlockWithImmunity();
			if (Time.time - _lastBlockEffectTime > blockHitEffectInterval)
			{
				_lastBlockEffectTime = Time.time;
				FxPlayNewNetworked(fxBlockHit, info.caster);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
