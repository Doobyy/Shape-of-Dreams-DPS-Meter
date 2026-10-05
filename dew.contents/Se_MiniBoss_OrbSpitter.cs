using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_MiniBoss_OrbSpitter : MiniBossEffect
{
	public Vector2 range;

	public Vector2 landTime;

	public GameObject fxShootCaster;

	public float startDelay;

	public float periodicShotInterval;

	public int perCastOrbs;

	public float perCastOrbInterval;

	public float targetedShotChance;

	public float targetedShotRandomMag;

	private float _lastShootTime;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnCastComplete += new Action<EventInfoCast>(EntityEventOnCastComplete);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnCastComplete -= new Action<EventInfoCast>(EntityEventOnCastComplete);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_lastShootTime = 0f;
	}

	private void EntityEventOnCastComplete(EventInfoCast obj)
	{
		((MonoBehaviour)(object)this).StopAllCoroutines();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			for (int i = 0; i < perCastOrbs; i++)
			{
				ShootOrb();
				yield return new WaitForSeconds(perCastOrbInterval);
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !(Time.time - creationTime < startDelay) && Time.time - _lastShootTime > periodicShotInterval)
		{
			ShootOrb();
		}
	}

	[Server]
	public void ShootOrb()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_MiniBoss_OrbSpitter::ShootOrb()' called when server was not active");
		}
		else
		{
			if (victim.Visual.isRendererOff)
			{
				return;
			}
			FxPlayNewNetworked(fxShootCaster, victim);
			_lastShootTime = Time.time;
			Vector3 pos = victim.position + UnityEngine.Random.insideUnitCircle.ToXZ() * UnityEngine.Random.Range(range.x, range.y);
			float landT = UnityEngine.Random.Range(landTime.x, landTime.y);
			if (UnityEngine.Random.value < targetedShotChance)
			{
				List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, victim.position, range.y, tvDefaultHarmfulEffectTargets);
				if (list.Count > 0)
				{
					Entity target = list[UnityEngine.Random.Range(0, list.Count)];
					pos = AbilityTrigger.PredictPoint_Simple(victim, UnityEngine.Random.value, target, landT) + UnityEngine.Random.insideUnitCircle.ToXZ() * targetedShotRandomMag;
				}
				handle.Return();
			}
			Vector3 vector = (pos - victim.position).Flattened();
			if (vector.sqrMagnitude < range.x * range.x)
			{
				vector = vector.normalized * range.x;
				pos = victim.position + vector;
			}
			pos = Dew.GetPositionOnGround(pos);
			CreateAbilityInstance(info.caster.position, null, new CastInfo(info.caster, pos), (Ai_MiniBoss_OrbSpitter_Orb a) =>
			{
				a.initialSpeed = Vector3.Distance(info.caster.position, pos) / landT;
				a.targetSpeed = a.initialSpeed;
				a.acceleration = 0f;
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
