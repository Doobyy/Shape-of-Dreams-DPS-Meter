using Mirror;
using UnityEngine;

public class Se_AutoDetectPresence : StatusEffect
{
	public float castDetectChancePerSecond = 0.5f;

	public float castThresholdScore = 2.5f;

	public float scoreDecaySpeed = 2f;

	public float castCooldownTime = 5f;

	private float _accumulatedTime;

	private float _lastDetectPresenceTime;

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || Time.time - _lastDetectPresenceTime < castCooldownTime)
		{
			return;
		}
		bool flag = false;
		if (victim.AI.context.targetEnemy.IsNullInactiveDeadOrKnockedOut())
		{
			foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
			{
				if (!allHero.isKnockedOut && allHero.Status.isUndetectableByNonAllies)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			_accumulatedTime = Mathf.MoveTowards(_accumulatedTime, castThresholdScore * 2f, dt);
		}
		else
		{
			_accumulatedTime = Mathf.MoveTowards(_accumulatedTime, 0f, dt * scoreDecaySpeed);
		}
		if (!(_accumulatedTime < castThresholdScore) && !(Random.value > castDetectChancePerSecond * dt) && !victim.Visual.isRendererOff)
		{
			CreateAbilityInstance<Ai_AutoDetectPresence_DetectWave>(victim.agentPosition, null, new CastInfo(victim));
			_accumulatedTime = 0f;
			_lastDetectPresenceTime = Time.time;
		}
	}

	private void MirrorProcessed()
	{
	}
}
