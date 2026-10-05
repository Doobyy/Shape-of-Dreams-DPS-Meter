using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_DarkCave_BossSeeker_Atk : AbilityInstance
{
	public GameObject fxShootEffect;

	public Vector2 waveCountRange;

	public float waveInterval;

	public int perWaveSpawnCount = 3;

	public int perWaveSpawnCountHallucinating = 1;

	public float perShotInterval;

	public float randomMagnitude = 3.5f;

	public DewAnimationClip animEnd;

	public float postDaze;

	private int _waveCount;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		_waveCount = Mathf.RoundToInt(Random.Range(waveCountRange.x, waveCountRange.y));
		int num = perWaveSpawnCount;
		if (info.caster.Status.normalizedHealth < 0.5f)
		{
			num++;
		}
		bool flag = info.caster is Mon_DarkCave_SeekerHallucination || info.caster.Status.HasStatusEffect<Se_Mon_DarkCave_BossSeeker_Hallucination>();
		Ai_Mon_DarkCave_BossSeeker_Atk_Projectile aiPrefab = DewResources.GetByType<Ai_Mon_DarkCave_BossSeeker_Atk_Projectile>(default(ResourceLoadSettings));
		if (info.caster.Status.normalizedHealth <= 0.5f)
		{
			num = Mathf.FloorToInt((float)num * 1.5f);
		}
		int currentPerWaveCount = (flag ? perWaveSpawnCountHallucinating : num);
		float duration = (float)_waveCount * waveInterval + (float)(_waveCount * currentPerWaveCount) * perShotInterval + postDaze;
		CreateBasicEffect(info.caster, new UnstoppableEffect(), duration);
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.EverythingCancelable,
			duration = duration,
			onCancel = Destroy,
			onComplete = Destroy,
			uncancellableTime = 1f
		});
		int[] array = new int[_waveCount];
		float num2 = randomMagnitude / (float)_waveCount;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Mathf.RoundToInt((float)i * num2);
		}
		for (int j = 0; j < _waveCount; j++)
		{
			if (info.target.IsNullInactiveDeadOrKnockedOut())
			{
				break;
			}
			FxPlayNetworked(fxShootEffect, info.caster);
			for (int k = 0; k < currentPerWaveCount; k++)
			{
				if (info.target.IsNullInactiveDeadOrKnockedOut())
				{
					break;
				}
				int num3 = 1;
				Vector3 vector = AbilityTrigger.PredictPoint_SpeedAcceleration(info.caster, num3, info.target, info.caster.position, 0.25f, aiPrefab.frontDist, aiPrefab.initialSpeed, aiPrefab.targetSpeed, aiPrefab.acceleration);
				vector = Dew.GetValidAgentDestination_Closest(info.target.GetAIAgentPosition(info.caster), Dew.GetPositionOnGround(vector + Random.insideUnitSphere * randomMagnitude));
				CreateAbilityInstance<Ai_Mon_DarkCave_BossSeeker_Atk_Projectile>(position, Quaternion.identity, new CastInfo(info.caster, vector));
				yield return new SI.WaitForSeconds(perShotInterval);
			}
			if (j != _waveCount - 1)
			{
				yield return new SI.WaitForSeconds(waveInterval);
			}
		}
		info.caster.Animation.PlayAbilityAnimation(animEnd);
	}

	private void MirrorProcessed()
	{
	}
}
