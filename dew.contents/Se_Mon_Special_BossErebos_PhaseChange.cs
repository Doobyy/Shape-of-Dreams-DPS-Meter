using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Se_Mon_Special_BossErebos_PhaseChange : StatusEffect
{
	public bool doInvulnerable = true;

	public bool doUncollidable = true;

	public bool doUntargetable = true;

	public bool doInvisible = true;

	public bool enableDestroyOtherInstance = true;

	public DewAnimationClip startClip;

	public DewAnimationClip loopClip;

	[Space(15f)]
	public float spawnGazeDelay;

	public float mapRadius = 22f;

	public float secondGazeDistance = 17f;

	public float gazeSpawnInterval;

	public float gazeDuration;

	[Space(15f)]
	public float spawnSeedDelay;

	public int spawnSeedCount;

	public float seedDazeDuration;

	public float seedSpawnRadius;

	public float seedSpawnInterval;

	[Space(15f)]
	public float spawnWhiteholeDelay;

	public float whiteholeTelegraphDelay;

	public float whiteholeDuration;

	public GameObject fxWhiteholeTelegraph;

	public AnimationCurve repelStrengthByDist;

	public AnimationCurve repelStrengthMulOverLifetime;

	[Space(15f)]
	public float postDelay;

	public DewAnimationClip staggerEnd;

	public GameObject fxStaggerEnd;

	private bool _isAllSpawendDead;

	private Channel _channel;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		if (enableDestroyOtherInstance)
		{
			Actor[] array = NetworkedManagerBase<ActorManager>.instance.allActors.ToArray();
			foreach (Actor actor in array)
			{
				if (actor is AbilityInstance && actor.IsDescendantOf(victim) && (Object)(object)actor != (Object)(object)this)
				{
					actor.Destroy();
				}
			}
		}
		if (victim.Status.TryGetStatusEffect<Se_Elm_Fire>(out var effect))
		{
			effect.Destroy();
		}
		if (victim.Status.TryGetStatusEffect<Se_Mon_Ink_BossDeathInterrupt>(out var effect2))
		{
			effect2.Destroy();
		}
		List<MirageSkinEffect> mirageSkins = DewResources.FindAllByType<MirageSkinEffect>(default(ResourceLoadSettings)).ToList();
		Vector3 center = SingletonBehaviour<Erebos_BossRoomCenter>.instance.transform.position;
		if (doInvulnerable)
		{
			DoInvulnerable();
		}
		if (doUntargetable)
		{
			DoUntargetable();
		}
		if (doInvisible)
		{
			DoInvisible(ignoreReveal: true);
		}
		if (doUncollidable)
		{
			DoUncollidable();
		}
		info.caster.Animation.PlayAbilityAnimation(startClip);
		info.caster.Control.Stop();
		info.caster.Control.CancelOngoingChannels();
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = float.PositiveInfinity,
			isAttack = false,
			uncancellableTime = 1f
		});
		yield return new SI.WaitForSeconds(2.5f);
		info.caster.Visual.DisableRenderers();
		yield return new SI.WaitForSeconds(spawnGazeDelay);
		Teleport(info.caster, center);
		FxPlayNetworked(fxWhiteholeTelegraph, center, Quaternion.identity);
		Vector3[] directions = new Vector3[4]
		{
			Vector3.right,
			Vector3.forward,
			Vector3.left,
			Vector3.back
		};
		for (int j = 0; j < directions.Length; j++)
		{
			Vector3 vector = center + directions[j] * mapRadius;
			Vector3 vector2 = center + directions[(j + 1) % directions.Length] * mapRadius;
			float angle = Vector3.SignedAngle(info.forward, vector2 - vector, Vector3.up);
			CreateAbilityInstance(vector, null, new CastInfo(info.caster, angle), (Ai_Mon_Special_BossErebos_Gaze_Instance b) =>
			{
				b.duration = gazeDuration;
			});
			yield return new SI.WaitForSeconds(gazeSpawnInterval);
			if (j % 2 == 0)
			{
				angle = Vector3.SignedAngle(info.forward, Vector3.forward, Vector3.up);
				vector = center + directions[j] * secondGazeDistance;
			}
			else
			{
				angle = Vector3.SignedAngle(info.forward, Vector3.right, Vector3.up);
				vector = center + directions[j] * secondGazeDistance;
			}
			CreateAbilityInstance(vector, null, new CastInfo(info.caster, angle), (Ai_Mon_Special_BossErebos_Gaze_Instance b) =>
			{
				b.duration = gazeDuration;
			});
			yield return new SI.WaitForSeconds(gazeSpawnInterval);
		}
		yield return new SI.WaitForSeconds(spawnWhiteholeDelay);
		float whiteholeEndTime = Time.time + whiteholeDuration;
		CreateAbilityInstance(center, null, new CastInfo(info.caster, center), (Ai_Mon_Special_BossErebos_SpawnBlackhole_Instance b) =>
		{
			b.attractStrengthByDist = repelStrengthByDist;
			b.attractStrengthMulOverLifetime = repelStrengthMulOverLifetime;
			b.blackholeDuration = whiteholeDuration;
			b.isWhitehole = true;
		});
		yield return new SI.WaitForSeconds(spawnSeedDelay);
		float entActiveTime = Time.time + seedDazeDuration;
		for (int j = 0; j < spawnSeedCount; j++)
		{
			Vector3 end = center + Quaternion.AngleAxis(360f / (float)spawnSeedCount * (float)j, Vector3.up) * Vector3.forward * seedSpawnRadius;
			end = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, end);
			Mon_Sky_StarSeed mon_Sky_StarSeed = Dew.SpawnEntity(end, Quaternion.identity, info.caster, DewPlayer.creep, info.caster.level, (Mon_Sky_StarSeed b) =>
			{
				b.Control.StartDaze(entActiveTime - Time.time);
			});
			int index = j % mirageSkins.Count;
			CreateStatusEffect(mirageSkins[index], mon_Sky_StarSeed);
			yield return new SI.WaitForSeconds(seedSpawnInterval);
		}
		yield return new SI.WaitForCondition(() => whiteholeEndTime - Time.time < 0.0001f);
		info.caster.Visual.EnableRenderers();
		info.caster.Animation.PlayAbilityAnimation(staggerEnd);
		FxPlayNetworked(fxStaggerEnd, info.caster);
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (_channel != null && _channel.isAlive)
			{
				_channel.Cancel();
				_channel = null;
			}
			FxStopNetworked(fxWhiteholeTelegraph);
		}
	}

	private void MirrorProcessed()
	{
	}
}
