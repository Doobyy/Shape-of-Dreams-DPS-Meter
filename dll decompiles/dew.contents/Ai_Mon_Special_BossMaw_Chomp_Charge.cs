using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(GenericTransformSync))]
public class Ai_Mon_Special_BossMaw_Chomp_Charge : AbilityInstance
{
	public float speed;

	public float maxRange;

	public float duration;

	public float delay;

	public float knockbackRange;

	public Knockback knockback;

	public GameObject fxTelegraph;

	private float _elapsedTime;

	private bool _isPosFixed;

	private Channel _channel;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			_channel = info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = float.PositiveInfinity,
				onCancel = DestroyIfActive
			});
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.agentPosition, knockbackRange, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < list.Count; i++)
			{
				Entity to = list[i];
				knockback.ApplyWithOrigin(info.caster.agentPosition, to);
			}
			handle.Return();
			yield return new SI.WaitForSeconds(duration);
			_isPosFixed = true;
			FxPlayNewNetworked(fxTelegraph, position, Quaternion.LookRotation(((Component)(object)info.caster).transform.forward));
			yield return new SI.WaitForSeconds(delay);
			CreateAbilityInstance<Ai_Mon_Special_BossMaw_Chomp_DashAtk>(info.caster.agentPosition, null, new CastInfo(info.caster, position));
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _channel != null && _channel.isAlive)
		{
			_channel.Cancel();
			_channel = null;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || _isPosFixed)
		{
			return;
		}
		Entity entity = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.agentPosition, maxRange, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		if (list.Count > 0)
		{
			entity = list[0];
		}
		if ((Object)(object)entity == null)
		{
			handle.Return();
			return;
		}
		_elapsedTime += dt;
		float t = Mathf.Clamp01(_elapsedTime / duration);
		float num = Mathf.Lerp(speed, speed * 0.75f, t);
		Vector3 vector = Vector3.MoveTowards(position, entity.GetAIAgentPosition(info.caster), num * dt);
		vector = Dew.GetPositionOnGround(vector);
		vector = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, vector);
		Vector3 vector2 = vector - info.caster.agentPosition;
		if (vector2.sqrMagnitude > maxRange * maxRange)
		{
			vector = info.caster.agentPosition + vector2.normalized * maxRange;
			vector = Dew.GetPositionOnGround(vector);
		}
		info.caster.Control.RotateTowards(vector, immediately: false);
		position = vector;
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
