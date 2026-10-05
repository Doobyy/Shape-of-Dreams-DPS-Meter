using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Mon_Primus_BossPrimusAeron_Force_GoldRain : StatusEffect
{
	public float targetedChance = 0.15f;

	public float targetedRandomRad = 5f;

	public Vector2 rainInterval;

	public float initialDisappearTime = 2f;

	public int waveCount = 5;

	public float disappearTime = 1.5f;

	public float afterDashTime = 0.25f;

	public float postDelay = 0.75f;

	private float _nextRainTime;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
		DoUnstoppable();
		BossMonster.RevealStealthedBeforeSpecialAttack();
		DestroyOnCondition(() => ((Mon_Primus_BossPrimusAeron)victim).phase != Mon_Primus_BossPrimusAeron.PhaseType.Force);
		Se_Mon_Primus_BossPrimusAeron_Force_GoldRain_Disappear disappear = CreateStatusEffect<Se_Mon_Primus_BossPrimusAeron_Force_GoldRain_Disappear>(victim);
		yield return new SI.WaitForSeconds(initialDisappearTime);
		float atkDelay = DewResources.GetByType<Ai_Mon_Primus_BossPrimusAeron_Force_GoldRain_Attack>(default(ResourceLoadSettings)).damageDelay;
		int totalWaveCount = waveCount + DewPlayer.gamePlayers.Count;
		for (int i = 0; i < totalWaveCount; i++)
		{
			Hero target = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
			Vector3 normalized = (SingletonBehaviour<Room_BossArena>.instance.center - target.GetAIAgentPosition(victim)).normalized;
			Vector3 startPos = SingletonBehaviour<Room_BossArena>.instance.center + Quaternion.Euler(0f, Random.Range(-45f, 45f), 0f) * normalized * 6f;
			Teleport(victim, startPos);
			yield return null;
			disappear.Destroy();
			victim.Control.StartDaze(atkDelay + afterDashTime);
			float angle = AbilityTrigger.PredictAngle_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), target, startPos, atkDelay);
			victim.Control.Rotate(angle, immediately: true);
			CreateAbilityInstance<Ai_Mon_Primus_BossPrimusAeron_Force_GoldRain_Attack>(victim.agentPosition, null, new CastInfo(victim, angle));
			yield return new SI.WaitForSeconds(atkDelay + afterDashTime);
			if (i != totalWaveCount - 1)
			{
				disappear = CreateStatusEffect<Se_Mon_Primus_BossPrimusAeron_Force_GoldRain_Disappear>(victim);
				yield return new SI.WaitForSeconds(disappearTime);
			}
		}
		yield return new SI.WaitForSeconds(postDelay);
		At_Mon_Primus_BossPrimusAeron_Force_JumpAttack ability = victim.Ability.GetAbility<At_Mon_Primus_BossPrimusAeron_Force_JumpAttack>();
		ResetCooldown(ability);
		victim.Control.Cast(ability, new CastInfo(victim, SingletonBehaviour<Room_BossArena>.instance.center));
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)info.caster != null)
		{
			info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !(Time.time < _nextRainTime))
		{
			_nextRainTime = Time.time + Random.Range(rainInterval.x, rainInterval.y);
			Vector3 vector;
			if (Random.value < targetedChance * (float)Dew.GetAliveHeroCount())
			{
				Vector3 aIAgentPosition = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true).GetAIAgentPosition(victim);
				vector = aIAgentPosition + Random.insideUnitCircle.ToXZ() * targetedRandomRad;
				vector = Dew.GetPositionOnGround(vector);
				vector = Dew.GetValidAgentDestination_LinearSweep(aIAgentPosition, vector);
			}
			else
			{
				vector = SingletonBehaviour<Room_BossArena>.instance.GetRandomPathablePosition();
			}
			CreateAbilityInstance<Ai_Mon_Primus_BossPrimusAeron_Force_GoldRain_RainInstance>(vector, null, new CastInfo(info.caster));
		}
	}

	private void MirrorProcessed()
	{
	}
}
