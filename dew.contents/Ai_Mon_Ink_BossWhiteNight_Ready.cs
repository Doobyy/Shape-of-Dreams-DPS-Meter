using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_Ready : AbilityInstance
{
	public float atkDelay;

	public float dashSpeed;

	public float dashMinDistance;

	public float dashLengthOffset;

	public float postDelay;

	public DewEase dashEase;

	public DewAnimationClip atkClip;

	public GameObject fxTelegraph;

	public GameObject fxDash;

	[Space(15f)]
	public float rageInstanceStartDelay;

	public float rageInstanceCount;

	public float rageInstanceInterval;

	public float rageInstancePerDistance;

	public float rageInstanceMultiplier;

	public ScalingValue rageInstanceDmgFactor;

	public GameObject fxRageTelegraph;

	private float _atkSpeed;

	private bool _isRage;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			_isRage = ((Mon_Ink_BossWhiteNight)info.caster)._isRage;
			FxPlayNetworked(fxDash, info.caster);
			Vector3 vector = info.target.GetAIAgentPosition(info.caster) - info.caster.agentPosition;
			vector = vector.normalized * (vector.magnitude + dashLengthOffset);
			if (vector.magnitude < dashMinDistance)
			{
				vector = vector.normalized * dashMinDistance;
			}
			Vector3 validAgentDestination_Closest = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, info.caster.agentPosition + vector);
			float num = (info.caster.agentPosition - validAgentDestination_Closest).magnitude / dashSpeed;
			if (num < 0.1f)
			{
				num = 0.1f;
			}
			info.caster.Control.StartDaze(num + 0.1f);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = true,
				destination = validAgentDestination_Closest,
				isCanceledByCC = true,
				duration = num,
				ease = dashEase,
				isFriendly = true,
				rotateForward = true,
				rotateSmoothly = false,
				onCancel = () =>
				{
					FxStopNetworked(fxDash);
					DestroyIfActive();
				},
				onFinish = () =>
				{
					((MonoBehaviour)(object)this).StartCoroutine(Routine());
				}
			});
		}
		IEnumerator Routine()
		{
			FxStopNetworked(fxDash);
			Vector3 pos = info.caster.position;
			float speed = info.caster.Status.attackSpeedMultiplier;
			Quaternion rot = Quaternion.Euler(0f, AbilityTrigger.PredictAngle_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), info.target, pos, atkDelay / speed), 0f);
			info.caster.Control.Rotate(rot, immediately: true);
			yield return null;
			if (_isRage)
			{
				FxPlayNetworked(fxRageTelegraph, info.caster.position, rot);
			}
			else
			{
				FxPlayNetworked(fxTelegraph, info.caster, info.caster.position, rot);
			}
			info.caster.Control.StartChannel(new Channel
			{
				duration = Mathf.Max(atkDelay / speed, 0.45f),
				blockedActions = Channel.BlockedAction.Everything,
				onCancel = () =>
				{
					FxStopNetworked(fxTelegraph);
					firstTrigger.SetCooldownTime(0, 1f);
					DestroyIfActive();
				},
				onComplete = () =>
				{
					if (_isRage)
					{
						SpawnRageInstance(pos, rot);
					}
					FxStopNetworked(fxTelegraph);
					info.caster.Animation.PlayAbilityAnimation(atkClip, speed);
					CreateAbilityInstance<Ai_Mon_Ink_BossWhiteNight_AtkInstance>(pos, rot, new CastInfo(info.caster, CastInfo.GetAngle(((Component)(object)info.caster).transform.forward)));
					info.caster.Control.StartDaze(postDelay);
					if (!_isRage)
					{
						DestroyIfActive();
					}
				}
			}.AddValidation(new AbilitySelfValidator()));
		}
	}

	private void SpawnRageInstance(Vector3 basePos, Quaternion rot)
	{
		StartSequence(Routine());
		IEnumerator Routine()
		{
			yield return new SI.WaitForSeconds(rageInstanceStartDelay);
			Vector3 dir = rot * Vector3.forward;
			basePos += dir * rageInstancePerDistance;
			for (int i = 0; (float)i < rageInstanceCount; i++)
			{
				Vector3 point = basePos + dir * (rageInstancePerDistance * (float)i);
				CreateAbilityInstance(point, null, new CastInfo(info.caster, point), (Ai_Mon_Ink_BossWhiteNight_RageInstance b) =>
				{
					b.dmgFactor = rageInstanceDmgFactor;
					b.startDelay = 0.05f;
					b.fxTelegraph = null;
				});
				yield return new SI.WaitForSeconds(rageInstanceInterval);
			}
			DestroyIfActive();
		}
	}

	private void MirrorProcessed()
	{
	}
}
