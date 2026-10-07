using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_SpawnBlackhole : AbilityInstance
{
	public Vector2 blackholeSpawnRange;

	public float castDelay;

	public float postDelay;

	public GameObject fxTelegraph;

	public DewAnimationClip castClip;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			Vector3 vector = SingletonBehaviour<Erebos_BossRoomCenter>.instance.transform.position;
			Vector3 vector2 = vector + Random.insideUnitCircle.ToXZ() * Random.Range(blackholeSpawnRange.x, blackholeSpawnRange.y);
			vector2 = Dew.GetPositionOnGround(vector2);
			Vector3 targetPoint = Dew.GetValidAgentDestination_LinearSweep(vector, vector2);
			info.caster.Control.StartChannel(new Channel
			{
				blockedActions = Channel.BlockedAction.Everything,
				duration = castDelay + postDelay,
				isAttack = false,
				onCancel = DestroyIfActive,
				uncancellableTime = 0.5f
			});
			CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
			info.caster.Control.RotateTowards(targetPoint, immediately: false, castDelay);
			FxPlayNetworked(fxTelegraph, targetPoint, Quaternion.identity);
			yield return new SI.WaitForSeconds(castDelay);
			info.caster.Animation.PlayAbilityAnimation(castClip);
			CreateAbilityInstance<Ai_Mon_Special_BossErebos_SpawnBlackhole_Instance>(targetPoint, null, new CastInfo(info.caster, targetPoint));
			yield return new SI.WaitForSeconds(postDelay);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
