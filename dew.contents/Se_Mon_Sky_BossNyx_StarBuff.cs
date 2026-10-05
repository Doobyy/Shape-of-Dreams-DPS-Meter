using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Mon_Sky_BossNyx_StarBuff : StatusEffect
{
	public float startDelay;

	public float postDelay;

	public float shieldMaxHealthRatio;

	public float speedAmount;

	public AnimationClip walkAnimation;

	public Vector2 rainInterval;

	public float rainRandomMagnitude;

	public float rainTargetedChance;

	public float rainMaxRange;

	public int lineAtkWaveCount;

	public int linAtkCountPerWave;

	public float lineAtkWaveInterval;

	public float lineAtkInterval;

	public float lineAtkPositionOffset;

	public float lineAtkMaxRange;

	private AnimationClip _originWalkAnimation;

	private float _totalWalkDuration;

	private float _nextRainTime;

	private Channel _channel;

	protected override IEnumerator OnCreateSequenced()
	{
		_originWalkAnimation = victim.Animation.model.runForwardClip;
		victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForward, walkAnimation);
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(victim);
		BossMonster.RevealStealthedBeforeSpecialAttack();
		DoShield(shieldMaxHealthRatio * victim.maxHealth);
		DoSpeed(speedAmount);
		DoUnstoppable();
		_totalWalkDuration = startDelay + (float)lineAtkWaveCount * (lineAtkWaveInterval + (float)lineAtkWaveCount * lineAtkInterval) + 0.5f;
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = (Channel.BlockedAction.Ability | Channel.BlockedAction.Attack),
			duration = float.PositiveInfinity,
			onCancel = DestroyIfActive
		});
		yield return new SI.WaitForSeconds(0.5f);
		for (int waveIndex = 0; waveIndex < lineAtkWaveCount; waveIndex++)
		{
			Hero target = Dew.GetClosestAliveHero(victim.agentPosition, fallbackToDead: true, victim);
			for (int lineIndex = 0; lineIndex < linAtkCountPerWave; lineIndex++)
			{
				Vector3 end = target.GetAIAgentPosition(victim);
				switch (Random.Range(0, 3))
				{
				case 1:
					end = AbilityTrigger.PredictPoint_Simple(victim, Random.Range(0.7f, 1f), target, 1.5f);
					break;
				case 2:
					end = victim.agentPosition + Random.insideUnitCircle.ToXZ().normalized * Random.Range(5f, lineAtkMaxRange);
					break;
				}
				end = Dew.GetValidAgentDestination_Closest(victim.agentPosition, end);
				float y = Random.Range(0f, 360f);
				Quaternion quaternion = Quaternion.Euler(0f, y, 0f);
				end -= quaternion * Vector3.forward * lineAtkPositionOffset;
				CreateAbilityInstance(end, quaternion, new CastInfo(victim), (Ai_Mon_Sky_BossNyx_LineAtk b) =>
				{
					b.disableAnimations = true;
				});
				yield return new SI.WaitForSeconds(lineAtkInterval);
			}
			yield return new SI.WaitForSeconds(lineAtkWaveInterval);
		}
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || Time.time - creationTime < startDelay)
		{
			return;
		}
		if (Time.time - creationTime > _totalWalkDuration)
		{
			victim.Control.Stop();
			return;
		}
		Hero closestAliveHero = Dew.GetClosestAliveHero(victim.agentPosition, fallbackToDead: true, victim);
		if ((Object)(object)closestAliveHero == null)
		{
			return;
		}
		victim.Control.MoveToDestination(Dew.GetValidAgentDestination_LinearSweep(victim.agentPosition, closestAliveHero.GetAIAgentPosition(victim)), immediately: true);
		if (!(Time.time < _nextRainTime))
		{
			_nextRainTime = Time.time + Random.Range(rainInterval.x, rainInterval.y);
			Vector3 vector;
			if (Random.value < rainTargetedChance * (float)Dew.GetAliveHeroCount())
			{
				Vector3 aIAgentPosition = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true).GetAIAgentPosition(victim);
				vector = aIAgentPosition + Random.insideUnitCircle.ToXZ() * rainRandomMagnitude;
				vector = Dew.GetPositionOnGround(vector);
				vector = Dew.GetValidAgentDestination_LinearSweep(aIAgentPosition, vector);
			}
			else
			{
				vector = victim.agentPosition + Random.insideUnitCircle.ToXZ() * rainMaxRange;
			}
			Quaternion quaternion = Quaternion.LookRotation(vector - victim.agentPosition).Flattened();
			CreateAbilityInstance<Ai_Mon_Sky_BossNyx_LaserAtk_Instance>(vector, quaternion * Quaternion.Euler(Random.Range(-20f, 20f), Random.Range(-20f, 20f), Random.Range(-20f, 20f)), new CastInfo(info.caster));
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		victim.Animation.ReplaceAnimationLocal(EntityAnimation.ReplaceableAnimationType.RunForward, _originWalkAnimation);
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
