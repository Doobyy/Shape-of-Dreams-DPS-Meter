using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_SpawnTormentor_Attacker : AbilityInstance
{
	public float firstDelay;

	public float duration;

	public float radius;

	public float cooldownTime;

	public float atkUpDistance;

	public int beamCount;

	public GameObject fxInstance;

	public GameObject fxEnd;

	public GameObject fxCharged;

	public GameObject fxAtk;

	private float _currentTime;

	private bool _isCharged;

	private bool _enableCheckEnemy;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			_currentTime = 0f;
			_enableCheckEnemy = false;
			_isCharged = false;
			FxPlayNetworked(fxInstance, ((Component)(object)this).transform.position, Quaternion.identity);
			FxPlayNetworked(fxCharged, ((Component)(object)this).transform.position, Quaternion.identity);
			yield return new SI.WaitForSeconds(firstDelay);
			_enableCheckEnemy = true;
			yield return new SI.WaitForSeconds(duration - firstDelay);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxInstance);
			FxStopNetworked(fxCharged);
			FxPlayNewNetworked(fxEnd, ((Component)(object)this).transform.position, Quaternion.identity);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !_enableCheckEnemy || Time.time - _currentTime <= cooldownTime)
		{
			return;
		}
		if (!_isCharged)
		{
			FxPlayNetworked(fxCharged, ((Component)(object)this).transform.position, Quaternion.identity);
			_isCharged = true;
		}
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, ((Component)(object)this).transform.position, radius, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = false,
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		if (list.Count < 1)
		{
			handle.Return();
			return;
		}
		Entity entity = null;
		foreach (Entity item in list)
		{
			if (!(item is Monster))
			{
				entity = item;
				break;
			}
		}
		handle.Return();
		if ((Object)(object)entity == null)
		{
			return;
		}
		FxStopNetworked(fxCharged);
		FxPlayNetworked(fxAtk, ((Component)(object)this).transform.position + Vector3.up * (atkUpDistance + 0.85f), Quaternion.identity);
		_isCharged = false;
		_currentTime = Time.time;
		Vector3 normalized = (AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), entity, 1f) - ((Component)(object)this).transform.position).normalized;
		float num = Vector3.SignedAngle(info.forward, normalized, Vector3.up);
		for (int i = 0; i < beamCount; i++)
		{
			CreateAbilityInstance(((Component)(object)this).transform.position + Vector3.up * atkUpDistance, null, new CastInfo(info.caster, num + 15f * ((float)i - (float)(beamCount - 1) / 2f)), (Ai_Mon_Special_BossErebos_SpawnTormentor_Attacker_Instance b) =>
			{
				b.DestroyOnDestroy(this);
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
