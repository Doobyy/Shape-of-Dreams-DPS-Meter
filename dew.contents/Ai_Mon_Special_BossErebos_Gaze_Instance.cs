using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_Gaze_Instance : AbilityInstance
{
	public float castDelay;

	public float duration;

	public float distance;

	public float width;

	public float damageInterval;

	public Knockback Knockback;

	public ScalingValue tickDamage;

	public ScalingValue initDamage;

	public GameObject fxInit;

	public GameObject fxInstance;

	public GameObject fxTelegraph;

	public GameObject fxHit;

	internal bool isRoomModInstance;

	internal float tickDmgHealthRatio;

	internal float initDmgHealthRatio;

	private bool _isCastStart;

	private float _currentTime;

	private float _baseDuration;

	private bool _cachedBase;

	protected override void Awake()
	{
		base.Awake();
		if (!_cachedBase)
		{
			_baseDuration = duration;
			_cachedBase = true;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		isRoomModInstance = false;
		tickDmgHealthRatio = 0f;
		initDmgHealthRatio = 0f;
		_isCastStart = false;
		if (_cachedBase)
		{
			duration = _baseDuration;
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		if (!isRoomModInstance)
		{
			DestroyOnDeath(info.caster);
		}
		FxPlayNetworked(fxTelegraph, ((Component)(object)this).transform.position, Quaternion.AngleAxis(info.angle, Vector3.up));
		yield return new SI.WaitForSeconds(castDelay);
		Knockback knockback = new Knockback
		{
			distance = 3.5f,
			duration = Knockback.duration
		};
		FxPlayNetworked(fxInit, ((Component)(object)this).transform.position, Quaternion.AngleAxis(info.angle, Vector3.up));
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.SphereCastAllEntities(out handle, ((Component)(object)this).transform.position + distance * 0.5f * -info.forward, width, info.forward, distance, tvDefaultHarmfulEffectTargets))
		{
			FxPlayNewNetworked(fxHit, item);
			float amount = GetValue(initDamage);
			if (isRoomModInstance)
			{
				amount = (item.maxHealth + item.Status.currentShield) * initDmgHealthRatio;
			}
			CreateDamage(DamageData.SourceType.Default, amount).SetOriginPosition(((Component)(object)this).transform.position + distance * 0.5f * -info.forward).SetDirection(info.forward).SetElemental(ElementalType.Light)
				.SetAttr(DamageAttribute.DamageOverTime)
				.Dispatch(item);
			knockback.ApplyWithDirection((item.agentPosition - ((Component)(object)this).transform.position).normalized, item);
		}
		handle.Return();
		_isCastStart = true;
		FxPlayNetworked(fxInstance, ((Component)(object)this).transform.position, Quaternion.AngleAxis(info.angle, Vector3.up));
		yield return new SI.WaitForSeconds(duration);
		_isCastStart = false;
		FxStopNetworked(fxInstance);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxInstance);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !_isCastStart || Time.time - _currentTime <= damageInterval)
		{
			return;
		}
		_currentTime = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.SphereCastAllEntities(out handle, ((Component)(object)this).transform.position + distance * 0.5f * -info.forward, width, info.forward, distance))
		{
			if (item is Hero || item is Summon)
			{
				FxPlayNewNetworked(fxHit, item);
				float amount = GetValue(tickDamage);
				if (isRoomModInstance)
				{
					amount = (item.maxHealth + item.Status.currentShield) * tickDmgHealthRatio;
				}
				CreateDamage(DamageData.SourceType.Default, amount).SetOriginPosition(((Component)(object)this).transform.position + distance * 0.5f * -info.forward).SetDirection(info.forward).SetElemental(ElementalType.Light)
					.SetAttr(DamageAttribute.DamageOverTime)
					.Dispatch(item);
				Knockback.ApplyWithDirection(info.forward, item);
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
