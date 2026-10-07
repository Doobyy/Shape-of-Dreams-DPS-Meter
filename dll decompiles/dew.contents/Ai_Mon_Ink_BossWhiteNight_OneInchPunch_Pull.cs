using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_OneInchPunch_Pull : AbilityInstance
{
	public DewCollider range;

	public DewCollider rageRange;

	public ScalingValue dmgFactor;

	public GameObject fxHit;

	public GameObject fxPull;

	public DewEase pullEase;

	public float pullSpeed;

	public float forwardDistance;

	public float totalDuration;

	public float atkDelay;

	public int waveCount;

	public int atkCountPerWave;

	public float distanceStep;

	public float angleStep;

	public float atkInterval;

	public GameObject fxRageTelegraph;

	public GameObject fxRageAddEffect;

	private bool _isRage;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_isRage = ((Mon_Ink_BossWhiteNight)info.caster)._isRage;
		if (_isRage)
		{
			FxPlayNetworked(fxRageAddEffect, info.caster.agentPosition, rotation);
		}
		Vector3 vector = info.caster.agentPosition + ((Component)(object)info.caster).transform.forward * forwardDistance;
		bool isExistPulled = false;
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		foreach (Entity item in entities)
		{
			if (!item.IsNullInactiveDeadOrKnockedOut())
			{
				Vector3 end = vector + UnityEngine.Random.insideUnitCircle.ToXZ().normalized * UnityEngine.Random.Range(0f, 1f);
				end = Dew.GetValidAgentDestination_LinearSweep(item.agentPosition, end);
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.caster.agentPosition).Dispatch(item);
				FxPlayNewNetworked(fxHit, item);
				if (!item.Status.hasCrowdControlImmunity)
				{
					float num = (item.agentPosition - end).magnitude / pullSpeed;
					float duration = totalDuration - num;
					FxPlayNewNetworked(fxPull, item);
					CreateBasicEffect(item, new StunEffect(), duration);
					item.Control.StartDisplacement(new DispByDestination
					{
						affectedByMovementSpeed = false,
						canGoOverTerrain = false,
						destination = end,
						ease = pullEase,
						isCanceledByCC = false,
						isFriendly = false,
						duration = num
					});
					isExistPulled = true;
				}
			}
		}
		if (_isRage)
		{
			ListReturnHandle<Entity> handle2;
			foreach (Entity entity in rageRange.GetEntities(out handle2, tvDefaultHarmfulEffectTargets))
			{
				if (!entities.Contains(entity))
				{
					Vector3 end2 = vector + UnityEngine.Random.insideUnitCircle.ToXZ().normalized * UnityEngine.Random.Range(0f, 1f);
					end2 = Dew.GetValidAgentDestination_LinearSweep(entity.agentPosition, end2);
					CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.caster.agentPosition).Dispatch(entity);
					FxPlayNewNetworked(fxHit, entity);
					if (!entity.Status.hasCrowdControlImmunity)
					{
						float num2 = (entity.agentPosition - end2).magnitude / pullSpeed;
						float duration2 = totalDuration - num2;
						FxPlayNewNetworked(fxPull, entity);
						CreateBasicEffect(entity, new StunEffect(), duration2);
						entity.Control.StartDisplacement(new DispByDestination
						{
							affectedByMovementSpeed = false,
							canGoOverTerrain = false,
							destination = end2,
							ease = pullEase,
							isCanceledByCC = false,
							isFriendly = false,
							duration = num2
						});
						isExistPulled = true;
					}
				}
			}
			handle2.Return();
		}
		handle.Return();
		yield return new SI.WaitForSeconds(atkDelay);
		if (isExistPulled)
		{
			CreateAbilityInstance<Ai_Ai_Mon_Ink_BossWhiteNight_OneInchPunch_Atk>(info.caster.agentPosition, Quaternion.AngleAxis(info.angle, Vector3.up), new CastInfo(info.caster, info.angle));
		}
		Destroy();
	}

	private void SpawnRageInstance()
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			float randomAngle = UnityEngine.Random.Range(0f, 360f);
			for (int wave = 0; wave < waveCount; wave++)
			{
				for (int i = 0; i < atkCountPerWave; i++)
				{
					float f = (randomAngle + 360f / (float)atkCountPerWave * (float)i + angleStep * (float)wave) * ((float)Math.PI / 180f);
					float num = distanceStep * (float)(wave + 1);
					float x = Mathf.Cos(f) * num;
					float z = Mathf.Sin(f) * num;
					Vector3 point = info.caster.agentPosition + ((Component)(object)info.caster).transform.forward * forwardDistance + new Vector3(x, 0f, z);
					FxPlayNewNetworked(fxRageTelegraph, point, Quaternion.identity);
					CreateAbilityInstance(point, null, new CastInfo(info.caster, point), (Ai_Mon_Ink_BossWhiteNight_RageInstance b) =>
					{
						b.startDelay = 0.55f;
						b.fxTelegraph = null;
					});
				}
				yield return new WaitForSeconds(atkInterval);
				if (!isActive)
				{
					break;
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
