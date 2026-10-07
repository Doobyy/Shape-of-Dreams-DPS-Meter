using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_DarkCave_BossSeeker_PurpleOrb : AbilityInstance
{
	public float startDelay;

	public DewCollider range;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public GameObject fxExplosion;

	public GameObject fxTelegraph;

	public GameObject fxHit;

	[Space(15f)]
	public float firstAtkDelay;

	public float duration;

	public float radius;

	public float targetChance;

	public float atkPerInterval;

	public Vector2 countPerAtk;

	public Vector2 atkWaveInterval;

	public GameObject fxLoop;

	public GameObject fxEnd;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		FxPlayNetworked(fxTelegraph, info.point, null);
		yield return new SI.WaitForSeconds(startDelay);
		FxPlayNetworked(fxExplosion, info.point, null);
		range.transform.position = info.point;
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
		{
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.point).Dispatch(entity);
			knockback.ApplyWithOrigin(info.point, entity);
			FxPlayNewNetworked(fxHit, entity);
		}
		handle.Return();
		FxPlayNetworked(fxLoop, info.point, null);
		yield return new SI.WaitForSeconds(firstAtkDelay);
		float interval;
		for (float timer = 0f; timer < duration; timer += interval)
		{
			float atkCount = Random.Range(countPerAtk.x, countPerAtk.y + 1f);
			for (int i = 0; (float)i < atkCount; i++)
			{
				Vector3 point = info.point + Random.insideUnitCircle.ToXZ() * radius;
				ListReturnHandle<Entity> handle2;
				foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle2, info.point, radius, tvDefaultHarmfulEffectTargets))
				{
					if (!item.IsNullInactiveDeadOrKnockedOut() && !(Random.value > targetChance))
					{
						Vector3 vector = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), item, 0.5f);
						Vector3 vector2 = info.point - vector;
						point = ((!(vector2.magnitude > radius)) ? vector : (info.point + vector2.normalized * radius));
					}
				}
				handle2.Return();
				point += Random.insideUnitCircle.ToXZ() * 2f;
				CreateAbilityInstance<Ai_Mon_DarkCave_BossSeeker_PurpleOrb_Instance>(point, null, new CastInfo(info.caster, point));
				yield return new SI.WaitForSeconds(atkPerInterval);
				timer += atkPerInterval;
			}
			interval = Random.Range(atkWaveInterval.x, atkWaveInterval.y);
			yield return new SI.WaitForSeconds(interval);
		}
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxLoop);
			FxPlayNewNetworked(fxEnd, info.point, null);
		}
	}

	private void MirrorProcessed()
	{
	}
}
