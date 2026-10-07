using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_ScatterShot_Spawner : AbilityInstance
{
	public float maxAngle;

	public float frontDistance;

	public int shotCount;

	public float longShotChance;

	public float castDuration;

	public float longShotDuration;

	public float postDelay;

	public float shortShotCooldownTime;

	public GameObject fxShortTelegraph;

	public GameObject fxShortShotCastStart;

	public GameObject fxShortShotCastEnd;

	public GameObject fxLongTelegraph;

	public GameObject fxLongShotCastStart;

	public GameObject fxLongShotCastEnd;

	public DewAnimationClip clip;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DestroyOnDeath(info.caster);
		float duration = castDuration;
		bool isLongShot = Random.value < longShotChance;
		if (isLongShot)
		{
			maxAngle *= 1.35f;
			duration = longShotDuration;
			FxPlayNetworked(fxLongShotCastStart, info.caster);
		}
		else
		{
			FxPlayNetworked(fxShortShotCastStart, info.caster);
		}
		float angleLBound = 0f - maxAngle;
		float angleRBound = maxAngle;
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = duration,
			onCancel = () =>
			{
				info.caster.Control.StartDaze(postDelay);
				DestroyIfActive();
			},
			onComplete = () =>
			{
				info.caster.Animation.PlayAbilityAnimation(clip);
				for (int i = 0; i < shotCount; i++)
				{
					float num3 = Mathf.Lerp(angleLBound, angleRBound, (float)i / (float)(shotCount - 1));
					if (isLongShot)
					{
						FxPlayNetworked(fxLongShotCastEnd, info.caster);
						CreateAbilityInstance<Ai_Mon_Special_BossMaw_ScatterShot_LongProjectile>(info.caster.Visual.GetCenterPosition() + info.forward * frontDistance, Quaternion.identity, new CastInfo(info.caster, info.angle + num3));
					}
					else
					{
						FxPlayNetworked(fxShortShotCastEnd, info.caster);
						firstTrigger.SetCooldownTime(0, shortShotCooldownTime);
						CreateAbilityInstance<Ai_Mon_Special_BossMaw_ScatterShot_ShortProjectile>(info.caster.Visual.GetCenterPosition() + info.forward * frontDistance, Quaternion.identity, new CastInfo(info.caster, info.angle + num3));
					}
				}
				info.caster.Control.StartDaze(postDelay);
				DestroyIfActive();
			}
		});
		for (int num = 0; num < shotCount; num++)
		{
			float num2 = Mathf.Lerp(angleLBound, angleRBound, (float)num / (float)(shotCount - 1));
			GameObject effect = (isLongShot ? fxLongTelegraph : fxShortTelegraph);
			FxPlayNewNetworked(effect, info.caster.agentPosition + info.forward * frontDistance, Quaternion.AngleAxis(info.angle + num2, Vector3.up));
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxShortShotCastStart);
			FxStopNetworked(fxLongShotCastStart);
		}
	}

	private void MirrorProcessed()
	{
	}
}
