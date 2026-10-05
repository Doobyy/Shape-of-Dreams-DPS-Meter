using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Monster_PizzaLightning_Spawner : AbilityInstance
{
	public float initDelay = 1f;

	public int waveCount = 7;

	public int maxSkippedWaveByDifficulty = 3;

	public Vector2 afterShootDelayRange = new Vector2(0.7f, 0f);

	public float angleDiffPerInstance = 50f;

	public GameObject fxPrepare;

	public GameObject fxBoom;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		bool isFirstTime = false;
		if (!info.caster.Status.HasStatusEffect<Se_Mon_Special_BossPolaris_Monster_PizzaLightning_Rage>())
		{
			isFirstTime = true;
			CreateStatusEffect<Se_Mon_Special_BossPolaris_Monster_PizzaLightning_Rage>(info.caster);
		}
		Ai_Mon_Special_BossPolaris_Monster_PizzaLightning_Instance atkPrefab = DewResources.GetByType<Ai_Mon_Special_BossPolaris_Monster_PizzaLightning_Instance>(ResourceLoadSettings.Light);
		info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
		CreateBasicEffect(info.caster, new InvulnerableEffect(), 3600f).DestroyOnDestroy(this);
		BossMonster.RevealStealthedBeforeSpecialAttack();
		yield return new SI.WaitForSeconds(initDelay);
		float dispDestAngle = Random.Range(0f, 360f);
		int skippedWaves = DewMath.RandomRoundToInt((float)maxSkippedWaveByDifficulty * (1f - NetworkedManagerBase<GameManager>.instance.difficulty.specialSkillChanceMultiplier));
		for (int waveIndex = 0; waveIndex < waveCount - skippedWaves; waveIndex++)
		{
			Vector3 pos = info.caster.agentPosition;
			dispDestAngle += Random.Range(110f, 180f) * (float)((!(Random.value < 0.5f)) ? 1 : (-1));
			pos = SingletonBehaviour<Room_BossArena>.instance.center + Quaternion.Euler(0f, dispDestAngle, 0f) * Vector3.forward * (SingletonBehaviour<Room_BossArena>.instance.radius - 8f);
			CreateAbilityInstance<Ai_Mon_Special_BossPolaris_Monster_Dash>(info.caster.agentPosition, null, new CastInfo(info.caster, pos));
			yield return null;
			yield return new SI.WaitForCondition(() => !info.caster.Control.isDisplacing);
			Hero closestAliveHero = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
			float shootMainAngle = Quaternion.LookRotation(SingletonBehaviour<Room_BossArena>.instance.center - info.caster.agentPosition).eulerAngles.y;
			if ((bool)(Object)(object)closestAliveHero)
			{
				float num = AbilityTrigger.PredictAngle_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), closestAliveHero, info.caster.agentPosition, atkPrefab.damageDelay);
				shootMainAngle = num;
			}
			int count = Mathf.Clamp(1 + waveIndex, 1, 5);
			yield return ShootRoutine(count, shootMainAngle);
			if (Random.value < 0.5f * NetworkedManagerBase<GameManager>.instance.difficulty.specialSkillChanceMultiplier)
			{
				yield return ShootRoutine(count + 1, shootMainAngle);
			}
			IEnumerator ShootRoutine(int pizzaCount, float angle)
			{
				FxPlayNetworked(fxPrepare, info.caster);
				info.caster.Control.Rotate(angle, immediately: false, 1f);
				for (int i = 0; i < pizzaCount; i++)
				{
					float y = angle - angleDiffPerInstance * 0.5f * (float)(pizzaCount - 1) + angleDiffPerInstance * (float)i;
					CreateAbilityInstance<Ai_Mon_Special_BossPolaris_Monster_PizzaLightning_Instance>(info.caster.agentPosition, Quaternion.Euler(0f, y, 0f), new CastInfo(info.caster, pos));
				}
				yield return new SI.WaitForSeconds(atkPrefab.damageDelay);
				FxStopNetworked(fxPrepare);
				FxPlayNewNetworked(fxBoom, info.caster);
				yield return new SI.WaitForSeconds(afterShootDelayRange.Lerp((float)waveIndex / (float)(waveCount - 3)));
			}
		}
		yield return new SI.WaitForSeconds(0.5f);
		CreateAbilityInstance<Ai_Mon_Special_BossPolaris_Monster_Dash>(info.caster.agentPosition, null, new CastInfo(info.caster, SingletonBehaviour<Room_BossArena>.instance.center));
		if (isFirstTime)
		{
			CreateBasicEffect(info.caster, new ArmorBoostEffect
			{
				strength = 50f
			}, 48f);
			CreateBasicEffect(info.caster, new ArmorBoostEffect
			{
				strength = 50f
			}, 24f);
			CreateBasicEffect(info.caster, new ArmorBoostEffect
			{
				strength = 50f
			}, 12f);
			CreateBasicEffect(info.caster, new ArmorBoostEffect
			{
				strength = 50f
			}, 6f);
		}
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
		}
	}

	private void MirrorProcessed()
	{
	}
}
