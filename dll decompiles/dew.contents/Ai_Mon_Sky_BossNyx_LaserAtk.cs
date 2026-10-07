using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_LaserAtk : AbilityInstance
{
	public float spawnInterval;

	public float spawnMaxRadius;

	public float postDelay;

	public float speedMult;

	public Vector2 movementMaxRadius;

	private int _spawnCount;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_spawnCount = ((At_Mon_Sky_BossNyx_LaserAtk)firstTrigger).spawnCount;
		float attackSpeedMultiplier = info.caster.Status.attackSpeedMultiplier;
		float scaledSpawnInterval = spawnInterval / attackSpeedMultiplier;
		float scaledPostDelay = postDelay / attackSpeedMultiplier;
		float num = (float)_spawnCount * scaledSpawnInterval + scaledPostDelay;
		float delta = 0f;
		float interval = num * 0.33333f;
		int lastStep = -1;
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack),
			duration = num,
			isAttack = true,
			onCancel = () =>
			{
				firstTrigger.SetCooldownTime(0, 1f);
				DestroyIfActive();
			},
			onComplete = DestroyIfActive,
			onTick = (float dt) =>
			{
				delta += dt;
				int num2 = Mathf.FloorToInt(delta / interval);
				if (num2 > lastStep)
				{
					lastStep = num2;
					Vector3 end = info.caster.agentPosition + Random.insideUnitCircle.ToXZ().normalized * Random.Range(movementMaxRadius.x, movementMaxRadius.y);
					end = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, end);
					info.caster.Control.MoveToDestination(end, immediately: false, speedMult);
				}
			}
		});
		float delay = DewResources.GetByType<Ai_Mon_Sky_BossNyx_LaserAtk_Instance>(default(ResourceLoadSettings)).damageDelay;
		Vector3 basePosistion = info.caster.agentPosition + ((Component)(object)info.caster).transform.forward * firstTrigger.configs[0].effectiveRange;
		for (int i = 0; i < _spawnCount; i++)
		{
			Entity entity = info.target;
			if (entity.IsNullInactiveDeadOrKnockedOut())
			{
				entity = Dew.GetClosestAliveHero(basePosistion, fallbackToDead: true, info.caster);
			}
			Quaternion quaternion = Quaternion.LookRotation(entity.GetAIAgentPosition(info.caster) - info.caster.agentPosition).Flattened();
			Vector3 vector;
			if (Random.value < 0.35f && !entity.IsNullInactiveDeadOrKnockedOut())
			{
				vector = entity.GetAIAgentPosition(info.caster) + Random.insideUnitCircle.ToXZ().normalized * 1.5f;
			}
			else if (Random.value < 0.25f && !entity.IsNullInactiveDeadOrKnockedOut())
			{
				vector = AbilityTrigger.PredictPoint_Simple(info.caster, Random.Range(0.7f, 1f), entity, delay);
				vector += Random.insideUnitCircle.ToXZ().normalized * 1.5f;
			}
			else
			{
				vector = basePosistion + Random.insideUnitCircle.ToXZ().normalized * Random.Range(0f, spawnMaxRadius);
			}
			vector = Dew.GetPositionOnGround(vector);
			CreateAbilityInstance<Ai_Mon_Sky_BossNyx_LaserAtk_Instance>(vector, quaternion * Quaternion.Euler(Random.Range(-20f, 20f), Random.Range(-20f, 20f), Random.Range(-20f, 20f)), new CastInfo(info.caster, vector));
			yield return new SI.WaitForSeconds(scaledSpawnInterval);
		}
		yield return new SI.WaitForSeconds(scaledPostDelay);
		DestroyIfActive();
	}

	private void MirrorProcessed()
	{
	}
}
