using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Holy_RainFire : AbilityInstance
{
	public float dispDuration = 1.5f;

	public float afterDispDelay = 0.5f;

	public GameObject fxPerWave;

	public GameObject fxOncePerMeteorCreate;

	public Vector2 waveIntervalStartToEnd = new Vector2(1.4f, 0.6f);

	public int waveCount = 8;

	public int angleCountPerWave = 8;

	public int meteorCountPerAngle = 6;

	public Vector2 meteorSizeNearToFar;

	public float perMeteorDistance = 2.5f;

	public float postLastWaveDuration = 1f;

	public float afterDazeDuration = 1f;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
			CreateBasicEffect(info.caster, new InvulnerableEffect(), 3600f).DestroyOnDestroy(this);
			Vector3 center = SingletonBehaviour<Room_BossArena>.instance.center;
			info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: false, 1f);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				destination = center,
				duration = dispDuration,
				ease = DewEase.EaseInOutQuad,
				canGoOverTerrain = true,
				rotateForward = false,
				isFriendly = true
			});
			yield return new SI.WaitForSeconds(dispDuration + afterDispDelay);
			for (int waveIndex = 0; waveIndex < waveCount; waveIndex++)
			{
				int waveIndex_ = waveIndex;
				FxPlayNewNetworked(fxPerWave, info.caster);
				float angleOffset;
				if (waveIndex >= 3)
				{
					angleOffset = Random.Range(0f, 360f);
				}
				else
				{
					angleOffset = ((waveIndex % 2 == 0) ? 0f : 22.5f);
				}
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
				float seconds = waveIntervalStartToEnd.Lerp((float)waveIndex / (float)(waveCount - 1));
				yield return new SI.WaitForSeconds(seconds);
				IEnumerator Routine()
				{
					for (int meteorIndex = 0; meteorIndex < meteorCountPerAngle; meteorIndex++)
					{
						meteorSizeNearToFar.Lerp((float)meteorIndex / (float)meteorCountPerAngle);
						for (int i = 0; i < angleCountPerWave; i++)
						{
							float y = (float)i * 360f / (float)angleCountPerWave + angleOffset;
							Vector3 vector = info.caster.agentPosition + Quaternion.Euler(0f, y, 0f) * Vector3.forward * (perMeteorDistance * 0.7f + (float)meteorIndex * perMeteorDistance);
							vector = Dew.GetPositionOnGround(vector);
							if ((int)Dew.GetNavMeshPathStatus(info.caster.agentPosition, vector) == 0)
							{
								CreateAbilityInstance(vector, null, new CastInfo(info.caster), (Ai_Mon_Special_BossPolaris_Holy_RainFire_Damage ai) =>
								{
									if (waveIndex_ % 3 == 0)
									{
										ai.NetworkangularSpeed = 50f;
									}
									else if (waveIndex_ % 3 == 1)
									{
										ai.NetworkangularSpeed = -50f;
									}
									else
									{
										ai.NetworkangularSpeed = Random.Range(-30f, 30f);
									}
								});
							}
						}
						FxPlayNewNetworked(fxOncePerMeteorCreate, info.caster);
						yield return new WaitForSeconds(0.05f);
					}
				}
			}
			yield return new SI.WaitForSeconds(postLastWaveDuration);
			info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
			Destroy();
			info.caster.Control.StartDaze(afterDazeDuration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
