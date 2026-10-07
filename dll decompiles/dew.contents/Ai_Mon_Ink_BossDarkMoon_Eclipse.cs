using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_Eclipse : AbilityInstance
{
	public float centerDistance;

	public float startDelay;

	public GameObject fxEntityTransform;

	public AnimationClip animHallucinationIdle;

	public float ascendDuration;

	public DewAnimationClip ascendClip;

	public GameObject fxAscend;

	public float teleportDuration;

	public float descendDuration;

	public DewAnimationClip descendClip;

	public DewAnimationClip loopClip;

	public GameObject fxDescend;

	public GameObject fxLoop;

	public int waveCount;

	public int spawnCount;

	public Vector2 spawnRange;

	public float targetingChance;

	public float targetPosDeviation;

	public GameObject fxSpawn;

	public float atkDelay;

	public float waveInterval;

	public GameObject fxEachWaveTelegraph;

	public GameObject fxAtk;

	public DewAnimationClip atkClip;

	public GameObject fxEnd;

	public DewAnimationClip endClip;

	public float landingDuration;

	public float postDelay;

	private Vector3 _teleportPos;

	private Channel _channel;

	private bool _didDisableRenderers;

	private List<Mon_Ink_BossDarkMoonHallucination> _hallucinations;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		BossMonster.RevealStealthedBeforeSpecialAttack();
		if (!info.caster.Status.HasStatusEffect<Se_Mon_Ink_BossDarkMoon_Eclipse>())
		{
			CreateStatusEffect<Se_Mon_Ink_BossDarkMoon_Eclipse>(info.caster, new CastInfo(info.caster));
		}
		_hallucinations = new List<Mon_Ink_BossDarkMoonHallucination>();
		((MonoBehaviour)(object)this).StartCoroutine(SpawnHallucinations());
		Vector3 vector = SingletonBehaviour<Ink_BossTeleportPosition>.instance.transform.position;
		vector = Dew.GetPositionOnGround(vector);
		_teleportPos = vector + SingletonBehaviour<Ink_BossTeleportPosition>.instance.transform.forward * centerDistance;
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = float.PositiveInfinity,
			isAttack = false,
			onCancel = DestroyIfActive,
			uncancellableTime = startDelay + ascendDuration + teleportDuration + descendDuration
		});
		yield return new SI.WaitForSeconds(startDelay);
		FxPlayNetworked(fxAscend, info.caster);
		info.caster.Animation.PlayAbilityAnimation(ascendClip);
		yield return new SI.WaitForSeconds(ascendDuration);
		info.caster.Visual.HideGroundMarker();
		info.caster.Visual.DisableRenderers();
		_didDisableRenderers = true;
		Teleport(info.caster, _teleportPos);
		info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: true);
		yield return new SI.WaitForSeconds(teleportDuration);
		FxStopNetworked(fxAscend);
		FxPlayNetworked(fxDescend, info.caster);
		FxPlayNetworked(fxEntityTransform, info.caster);
		info.caster.Visual.EnableRenderers();
		_didDisableRenderers = false;
		info.caster.Animation.PlayAbilityAnimation(descendClip);
		yield return new SI.WaitForSeconds(descendDuration);
		FxPlayNetworked(fxLoop, info.caster);
		info.caster.Animation.PlayAbilityAnimation(loopClip);
		RoomSection section = SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection();
		for (int i = 0; i < waveCount; i++)
		{
			float atkTime = Time.time + atkDelay;
			List<Vector3> positions = new List<Vector3>();
			GetSpawnPositions(positions);
			FxPlayNetworked(fxEachWaveTelegraph);
			for (int j = 0; j < positions.Count; j++)
			{
				Vector3 vector2 = positions[j];
				Hero target = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
				Vector3 vector3 = AbilityTrigger.PredictPoint_Simple(info.caster, Random.Range(0, 2), target, atkDelay) + Random.insideUnitCircle.ToXZ() * Random.Range(0f, targetPosDeviation);
				vector3 = ((Random.value <= targetingChance) ? vector3 : section.GetAnyRandomNode());
				vector3 = Dew.GetPositionOnGround(vector3);
				if (j >= _hallucinations.Count)
				{
					continue;
				}
				Mon_Ink_BossDarkMoonHallucination mon_Ink_BossDarkMoonHallucination = _hallucinations[j];
				if (!mon_Ink_BossDarkMoonHallucination.IsNullOrInactive())
				{
					Quaternion value = Quaternion.LookRotation((vector3 - vector2).normalized);
					Teleport(mon_Ink_BossDarkMoonHallucination, vector2);
					mon_Ink_BossDarkMoonHallucination.Control.Rotate(value, immediately: true);
					FxPlayNewNetworked(fxSpawn, mon_Ink_BossDarkMoonHallucination);
					CreateAbilityInstance(vector2, value, new CastInfo(mon_Ink_BossDarkMoonHallucination, CastInfo.GetAngle(value)), (Ai_Mon_Ink_BossDarkMoon_Eclipse_Instance b) =>
					{
						b._atkTime = atkTime;
					});
					yield return new SI.WaitForSeconds(0.05f);
				}
			}
			yield return new SI.WaitForCondition(() => atkTime - Time.time < 0.001f);
			FxPlayNetworked(fxAtk, info.caster);
			info.caster.Animation.PlayAbilityAnimation(atkClip);
			yield return new SI.WaitForSeconds(waveInterval);
		}
		Vector3 vector4 = _teleportPos + ((Component)(object)info.caster).transform.forward * 5f;
		vector4 = Dew.GetPositionOnGround(vector4);
		FxStopNetworked(fxLoop);
		FxStopNetworked(fxEntityTransform);
		FxPlayNetworked(fxEnd, info.caster);
		info.caster.Animation.PlayAbilityAnimation(endClip);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = false,
			canGoOverTerrain = true,
			duration = landingDuration,
			destination = vector4,
			ease = DewEase.EaseInOutQuad,
			isCanceledByCC = false,
			isFriendly = true
		});
		yield return new SI.WaitForSeconds(landingDuration);
		Destroy();
	}

	private IEnumerator SpawnHallucinations()
	{
		for (int i = 0; i < spawnCount; i++)
		{
			Mon_Ink_BossDarkMoonHallucination mon_Ink_BossDarkMoonHallucination = SpawnEntity(Vector3.zero, null, DewPlayer.creep, info.caster.level, (Mon_Ink_BossDarkMoonHallucination b) =>
			{
				b.Network_isSpecialAtk = true;
				b.Visual.DisableRenderers();
			});
			mon_Ink_BossDarkMoonHallucination.Animation.ReplaceAnimation(EntityAnimation.ReplaceableAnimationType.Idle, animHallucinationIdle);
			_hallucinations.Add(mon_Ink_BossDarkMoonHallucination);
			yield return new SI.WaitForSeconds(0.2f);
		}
	}

	private void GetSpawnPositions(List<Vector3> positions)
	{
		positions.Clear();
		int num = Random.Range(0, 360);
		int num2 = 360 / spawnCount;
		Vector3 positionOnGround = Dew.GetPositionOnGround(SingletonBehaviour<Ink_BossRoomCenter>.instance.transform.position);
		for (int i = 0; i < spawnCount; i++)
		{
			float y = (float)(num + num2 * i) * Random.Range(0.75f, 1f);
			Vector3 vector = Quaternion.Euler(0f, y, 0f) * ((Component)(object)info.caster).transform.forward;
			Vector3 item = positionOnGround + vector * Random.Range(spawnRange.x, spawnRange.y);
			positions.Add(item);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_didDisableRenderers = false;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		FxStopNetworked(fxLoop);
		FxStopNetworked(fxEntityTransform);
		if (_didDisableRenderers)
		{
			_didDisableRenderers = false;
			if (!info.caster.IsNullOrInactive())
			{
				info.caster.Visual.EnableRenderers();
			}
		}
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		info.caster.Control.StartDaze(postDelay);
		IEnumerator Routine()
		{
			if (_channel != null && _channel.isAlive)
			{
				_channel.Cancel();
				_channel = null;
			}
			info.caster.Visual.ShowGroundMarker();
			if (info.caster.Status.TryGetStatusEffect<Se_Mon_Ink_BossDarkMoon_Eclipse>(out var effect))
			{
				effect.Destroy();
			}
			foreach (Mon_Ink_BossDarkMoonHallucination hallucination in _hallucinations)
			{
				if (!hallucination.IsNullOrInactive())
				{
					hallucination.Destroy();
					yield return new SI.WaitForSeconds(0.2f);
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
