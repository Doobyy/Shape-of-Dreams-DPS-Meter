using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_PullAtk : AbilityInstance
{
	public float delay;

	public float awayDis;

	public float pullSpeed;

	public float totalDuration;

	public float afterAtkDelay;

	public float postDelay;

	public ScalingValue dmgFactor;

	public DewEase ease;

	public GameObject rangeObject;

	public DewCollider[] ranges;

	public GameObject fxSuccess;

	public GameObject fxPull;

	public GameObject fxTelegraph;

	public GameObject fxAfterAtkTelegraph;

	public GameObject fxHit;

	public DewAnimationClip clipAtkFailed;

	public DewAnimationClip clipAtkSuccess;

	private const float MinPullDuration = 0.1f;

	private Channel _channel;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		if ((Object)(object)info.target == null)
		{
			DestroyIfActive();
			yield break;
		}
		DestroyOnDeath(info.caster);
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = float.PositiveInfinity,
			onCancel = DestroyIfActive
		});
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		bool hasPulled = false;
		Vector3 vector = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), info.target, delay);
		Vector3 vector2 = ((Component)(object)info.caster).transform.InverseTransformPoint(vector);
		float num = Mathf.Abs(vector2.x);
		float num2 = Mathf.Abs(vector2.z);
		float y = ((num > num2) ? 90f : 0f);
		Quaternion rot = info.caster.rotation * Quaternion.Euler(0f, y, 0f);
		rangeObject.transform.rotation = rot;
		rangeObject.transform.position = info.caster.agentPosition;
		ranges[0].GetEntities(out var handle);
		ranges[1].GetEntities(out handle);
		FxPlayNetworked(fxTelegraph, info.caster.agentPosition, rot);
		yield return new SI.WaitForSeconds(delay);
		FxPlayNetworked(fxPull, info.caster.agentPosition + Vector3.up * 1f, rot);
		List<Entity> entities = ranges[0].GetEntities(out var handle2, tvDefaultHarmfulEffectTargets);
		foreach (Entity item in entities)
		{
			HitAndPull(item);
		}
		ListReturnHandle<Entity> handle3;
		foreach (Entity entity in ranges[1].GetEntities(out handle3, tvDefaultHarmfulEffectTargets))
		{
			if (!entities.Contains(entity))
			{
				HitAndPull(entity);
			}
		}
		handle2.Return();
		handle3.Return();
		if (!hasPulled)
		{
			info.caster.Animation.PlayAbilityAnimation(clipAtkFailed);
			yield return new SI.WaitForSeconds(postDelay);
			Destroy();
		}
		else
		{
			CreateAbilityInstance<Ai_Mon_Sky_BossNyx_PullAtk_AfterAtk>(info.caster.agentPosition, rot, new CastInfo(info.caster));
			yield return new SI.WaitForSeconds(totalDuration + afterAtkDelay + 0.75f);
			Destroy();
		}
		void HitAndPull(Entity e)
		{
			FxPlayNewNetworked(fxHit, e);
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.caster.agentPosition).Dispatch(e);
			if (!e.Status.hasCrowdControlImmunity)
			{
				hasPulled = true;
				Vector3 normalized = (info.caster.agentPosition - e.agentPosition).normalized;
				Vector3 end = info.caster.agentPosition - awayDis * normalized;
				end = Dew.GetValidAgentDestination_Closest(e.agentPosition, end);
				float num3 = Vector3.Distance(e.agentPosition, end) / pullSpeed;
				float duration = Mathf.Max(totalDuration - num3, 0.1f);
				CreateBasicEffect(e, new StunEffect(), duration, "nyxpull_stun");
				e.Control.StartDisplacement(new DispByDestination
				{
					affectedByMovementSpeed = false,
					canGoOverTerrain = true,
					destination = end,
					ease = ease,
					duration = duration,
					isCanceledByCC = false,
					isFriendly = false
				});
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _channel != null)
		{
			_channel.Cancel();
			_channel = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
