using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_BossSkoll_AuraBladeRain : AbilityInstance
{
	[Serializable]
	public class PhaseData
	{
		public float phaseInterval;

		public int bladeCount;

		public Vector2 bladeInterval;
	}

	public float startDelay;

	public float postDelay;

	public float ascendTime;

	public GameObject fxAscend;

	public DewAnimationClip ascendClip;

	public float descendDelay;

	public float descendTime;

	public GameObject fxDescend;

	public DewAnimationClip descendClip;

	public DewAnimationClip landClip;

	public float followStartRandomMag;

	public float followDuration;

	public float followSpeed;

	public GameObject fxFollowTelegraph;

	public GameObject fxLastBlade;

	[SyncVar]
	private Vector3 _followPos;

	public List<PhaseData> phases;

	public override bool reuseInRoom => true;

	public Vector3 Network_followPos
	{
		get
		{
			return _followPos;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector3>(value, ref _followPos, 64uL, (Action<Vector3, Vector3>)null);
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		BossMonster.RevealStealthedBeforeSpecialAttack();
		CreateStatusEffect<Se_Mon_SnowMountain_BossSkoll_DeathFromAbove_Invulnerable>(info.caster).DestroyOnDestroy(this);
		info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
		info.caster.Animation.PlayAbilityAnimation(ascendClip);
		FxPlayNetworked(fxAscend, info.caster);
		yield return new SI.WaitForSeconds(ascendTime);
		info.caster.Visual.DisableRenderers();
		info.caster.Visual.HideGroundMarker();
		yield return new SI.WaitForSeconds(startDelay);
		Hero targetHero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
		for (int phaseIndex = 0; phaseIndex < phases.Count; phaseIndex++)
		{
			PhaseData phase = phases[phaseIndex];
			bool isLastPhase = phaseIndex == phases.Count - 1;
			if (isLastPhase)
			{
				FxPlayNetworked(fxLastBlade);
			}
			yield return new SI.WaitForSeconds(phase.phaseInterval);
			RoomSection section = info.caster.section;
			if (section == null)
			{
				section = SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection();
			}
			if (section == null)
			{
				yield break;
			}
			int bladeCount = phase.bladeCount;
			int patternIndex = phaseIndex % 4;
			if (targetHero.IsNullInactiveDeadOrKnockedOut() || targetHero.Status.isUndetectableByNonAllies)
			{
				targetHero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
			}
			Vector3 targetPos = (isLastPhase ? default(Vector3) : targetHero.GetAIAgentPosition(info.caster));
			int j;
			for (j = 0; j < bladeCount; j++)
			{
				float t = (float)j / (float)bladeCount;
				float bladeInterval = Mathf.Lerp(phase.bladeInterval.x, phase.bladeInterval.y, t);
				Vector3 pos = default;
				Quaternion rot = default;
				if (isLastPhase)
				{
					GetPseudoRandomPos(out pos, out rot);
				}
				else
				{
					float y = 0f;
					float num = 4f;
					switch (patternIndex)
					{
					case 0:
						y = 0f;
						pos = targetPos + Quaternion.Euler(0f, y, 0f) * Vector3.forward * (num * ((float)bladeCount * 0.5f - (float)j));
						break;
					case 1:
						y = 90f;
						pos = targetPos + Quaternion.Euler(0f, y, 0f) * Vector3.forward * (num * ((float)bladeCount * 0.5f - (float)j));
						break;
					case 2:
						y = ((UnityEngine.Random.value < 0.5f) ? (-45f) : 45f);
						pos = targetPos + Quaternion.Euler(0f, y, 0f) * Vector3.forward * (num * ((float)bladeCount * 0.5f - (float)j));
						break;
					case 3:
						y = (float)j * 360f / (float)bladeCount;
						pos = targetPos + Quaternion.Euler(0f, y, 0f) * Vector3.forward * 3f;
						break;
					}
					rot = Quaternion.Euler(0f, y, 0f);
				}
				CreateAbilityInstance(pos, rot, new CastInfo(info.caster, pos), (Ai_Mon_SnowMountain_BossSkoll_AuraBladeRain_Instance b) =>
				{
					b.isLastPhase = isLastPhase;
				});
				yield return new SI.WaitForSeconds(bladeInterval);
				void GetPseudoRandomPos(out Vector3 reference, out Quaternion reference2)
				{
					if (j < DewPlayer.gamePlayers.Count && (UnityEngine.Object)(object)DewPlayer.gamePlayers[j].hero != null && !DewPlayer.gamePlayers[j].hero.isKnockedOut)
					{
						reference = DewPlayer.gamePlayers[j].hero.GetAIPosition(info.caster);
					}
					else if (j >= bladeCount - DewPlayer.gamePlayers.Count && (UnityEngine.Object)(object)DewPlayer.gamePlayers[j - bladeCount + DewPlayer.gamePlayers.Count].hero != null && !DewPlayer.gamePlayers[j - bladeCount + DewPlayer.gamePlayers.Count].hero.isKnockedOut)
					{
						reference = AbilityTrigger.PredictPoint_Simple(info.caster, UnityEngine.Random.Range(0.7f, 1f), DewPlayer.gamePlayers[j - bladeCount + DewPlayer.gamePlayers.Count].hero, bladeInterval);
					}
					else
					{
						reference = section.GetAnyRandomNode() + UnityEngine.Random.insideUnitSphere.Flattened() * 1.5f;
					}
					reference = Dew.GetPositionOnGround(reference);
					reference2 = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
				}
			}
		}
		yield return new SI.WaitForSeconds(descendDelay);
		FxStopNetworked(fxLastBlade);
		Hero hero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
		Network_followPos = Dew.GetPositionOnGround(hero.GetAIAgentPosition(info.caster) + UnityEngine.Random.onUnitSphere * followStartRandomMag);
		Network_followPos = Dew.GetValidAgentPosition(_followPos);
		FxPlayNetworked(fxFollowTelegraph, info.caster);
		for (float t2 = 0f; t2 < followDuration; t2 += LogicUpdateManager.logicDeltaTime)
		{
			hero = Dew.GetClosestAliveHero(_followPos, fallbackToDead: true, info.caster);
			Network_followPos = Dew.GetPositionOnGround(Vector3.MoveTowards(_followPos, hero.GetAIAgentPosition(info.caster), followSpeed * LogicUpdateManager.logicDeltaTime));
			Teleport(info.caster, _followPos);
			yield return null;
		}
		info.caster.Control.Teleport(_followPos);
		info.caster.Control.RotateTowards(Dew.GetClosestAliveHero(_followPos, fallbackToDead: true, info.caster), immediately: true);
		info.caster.Animation.PlayAbilityAnimation(descendClip);
		info.caster.Visual.EnableRenderers();
		info.caster.Visual.ShowGroundMarker();
		FxPlayNetworked(fxDescend, info.caster);
		FxStopNetworked(fxAscend);
		yield return new SI.WaitForSeconds(descendTime);
		info.caster.Animation.PlayAbilityAnimation(landClip);
		CreateAbilityInstance<Ai_Mon_SnowMountain_BossSkoll_DeathFromAbove_Land>(info.caster.agentPosition, null, new CastInfo(info.caster));
		info.caster.Control.StartDaze(postDelay);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxAscend);
			if (!info.caster.IsNullOrInactive())
			{
				info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
			}
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		fxFollowTelegraph.transform.position = _followPos;
	}

	private bool CheckValidPositionOnGround(ref Vector3 pos, Quaternion rot, float raycastLength)
	{
		float num = 0f;
		float y = pos.y;
		while (num < raycastLength)
		{
			Vector3 vector = pos + rot * new Vector3(num, 0f, 0f);
			num++;
			if (!(Mathf.Abs(y - Dew.GetPositionOnGround(vector).y) <= 2f))
			{
				pos += rot * new Vector3(num - raycastLength, 0f, 0f);
				break;
			}
		}
		num = 0f;
		while (num > 0f - raycastLength)
		{
			Vector3 vector2 = pos + rot * new Vector3(num, 0f, 0f);
			num--;
			if (!(Mathf.Abs(y - Dew.GetPositionOnGround(vector2).y) <= 2f))
			{
				return false;
			}
		}
		return true;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteVector3(writer, _followPos);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteVector3(writer, _followPos);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _followPos, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _followPos, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
		}
	}
}
