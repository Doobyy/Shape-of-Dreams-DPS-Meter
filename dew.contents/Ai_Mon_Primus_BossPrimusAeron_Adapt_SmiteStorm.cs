using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Adapt_SmiteStorm : AbilityInstance
{
	public GameObject fxWave;

	public int waveCount = 4;

	public int stormCount = 10;

	public float postDelay = 1f;

	public float postDazeDuration = 1f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		CreateAbilityInstance(position, null, new CastInfo(info.caster), (Ai_Mon_Primus_BossPrimusAeron_Dash ai) =>
		{
			ai.customPoint = SingletonBehaviour<Room_BossArena>.instance.center;
		});
		info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
		DestroyOnCondition(() => ((Mon_Primus_BossPrimusAeron)info.caster).phase != Mon_Primus_BossPrimusAeron.PhaseType.Adapt);
		yield return new SI.WaitForSeconds(1.5f);
		for (int w = 0; w < waveCount; w++)
		{
			FxPlayNewNetworked(fxWave, info.caster);
			List<float> angles = new List<float>();
			Dew.SelectRandomAliveHero();
			angles.Add((w < 4) ? ((float)w * 45f) : Random.Range(0f, 360f));
			info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: false);
			angles.Add(angles[0] + 90f);
			angles.Add(angles[0] + 180f);
			angles.Add(angles[0] + 270f);
			for (int i = 0; i < stormCount; i++)
			{
				bool didDoSound = false;
				for (int num = 0; num < angles.Count; num++)
				{
					Vector3 vector = info.caster.agentPosition + Quaternion.Euler(0f, angles[num], 0f) * Vector3.forward * (3.5f + (float)i * 3.5f);
					vector = Dew.GetPositionOnGround(vector);
					if ((int)Dew.GetNavMeshPathStatus(info.caster.agentPosition, vector) == 0)
					{
						CreateAbilityInstance(vector, null, new CastInfo(info.caster), (Ai_Mon_Primus_BossPrimusAeron_Adapt_SmiteStorm_Smite ai) =>
						{
							ai.NetworkdisableSound = didDoSound;
							ai.NetworksizeMultiplier = 0.8f + (float)i * 0.25f;
							didDoSound = true;
						});
					}
				}
				yield return new SI.WaitForSeconds(0.1f);
			}
			yield return new SI.WaitForSeconds(0.1f);
		}
		yield return new SI.WaitForSeconds(postDelay);
		info.caster.Control.StartDaze(postDazeDuration);
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

	private void MirrorProcessed()
	{
	}
}
