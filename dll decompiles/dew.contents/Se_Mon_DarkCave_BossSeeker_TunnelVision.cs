using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Se_Mon_DarkCave_BossSeeker_TunnelVision : StatusEffect
{
	[Serializable]
	public struct Pattern
	{
		public OffsetAndAngle[] items;

		public float interval;
	}

	[Serializable]
	public struct OffsetAndAngle
	{
		public Vector3 offset;

		public float angle;
	}

	public GameObject mainEffectOnAltSkill;

	public int baseCount = 3;

	public int perPlayerCount = 1;

	public float initDelay;

	public float endDelay;

	public float postDaze;

	public float secondAtkDistance;

	[Space(15f)]
	public int maxFakeCount;

	public float fakeInitChance;

	public GameObject fxFake;

	public DewAnimationClip fakeClip;

	public Vector2 fakeDisappearDelay;

	public Vector2 fakeAppearDelay;

	public Vector2 disappearDelay = new Vector2(0.7f, 1.25f);

	public Vector2 appearDelay = new Vector2(0.35f, 2.5f);

	private int _poolIndex;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		_poolIndex = ((Monster)info.caster).currentPoolIndex;
		if (_poolIndex == 1)
		{
			startEffectVictim = mainEffectOnAltSkill;
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DoUnstoppable();
		Disappear();
		BossMonster.RevealStealthedBeforeSpecialAttack();
		bool enableFake = NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier() < 0.5f;
		victim.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			CreateStatusEffect(gamePlayer.hero, (Se_Mon_DarkCave_BossSeeker_TunnelVision_PlayerLight b) =>
			{
				if (_poolIndex == 1)
				{
					b.isAltSkillEffect = true;
				}
			});
		}
		yield return new SI.WaitForSeconds(initDelay);
		int num = DewPlayer.gamePlayers.Count((DewPlayer p) => !p.hero.IsNullInactiveDeadOrKnockedOut());
		int count = baseCount + perPlayerCount * num;
		int i;
		for (i = 0; i < count; i++)
		{
			Entity target = GetTarget();
			yield return new SI.WaitForSeconds(0.15f);
			Appear();
			_ = Quaternion.identity;
			if (enableFake && UnityEngine.Random.value < fakeInitChance)
			{
				int fakeCount = UnityEngine.Random.Range(0, maxFakeCount + 1);
				for (int j = 0; j < fakeCount; j++)
				{
					if (j >= 1)
					{
						target = GetTarget();
					}
					victim.Control.RotateTowards(target.GetAIAgentPosition(victim), immediately: false);
					FxPlayNewNetworked(fxFake, info.caster);
					info.caster.Animation.PlayAbilityAnimation(fakeClip);
					yield return new SI.WaitForSeconds(UnityEngine.Random.Range(fakeDisappearDelay.x, fakeDisappearDelay.y));
					Disappear();
					yield return new SI.WaitForSeconds(UnityEngine.Random.Range(fakeAppearDelay.x, fakeAppearDelay.y));
					Appear();
				}
			}
			float num2 = 0f;
			Vector3 vector = target.GetAIAgentPosition(victim) - victim.agentPosition;
			Quaternion value = Quaternion.LookRotation(vector.normalized);
			victim.Control.RotateTowards(target.GetAIAgentPosition(victim), immediately: false);
			switch (_poolIndex)
			{
			case 0:
				CreateAbilityInstance<Ai_Mon_DarkCave_BossSeeker_TunnelVision_Claw>(victim.position, value, new CastInfo(victim, value.eulerAngles.y));
				break;
			case 1:
			{
				num2 = 1.5f;
				Vector3 point = victim.position + vector.normalized * secondAtkDistance;
				CreateAbilityInstance(point, value, new CastInfo(victim, point), (Ai_Mon_DarkCave_BossSeeker_TunnelVision_SecondPoolAtk b) =>
				{
					b.atkIndex = i;
				});
				break;
			}
			}
			if (i >= count - 1)
			{
				break;
			}
			yield return new SI.WaitForSeconds(UnityEngine.Random.Range(disappearDelay.x, disappearDelay.y) + num2);
			Disappear();
			yield return new SI.WaitForSeconds(UnityEngine.Random.Range(appearDelay.x, appearDelay.y));
		}
		yield return new SI.WaitForSeconds(endDelay);
		Vector3 center = ((SingletonBehaviour<DarkCave_BossRoomCenter>.instance == null) ? victim.position : SingletonBehaviour<DarkCave_BossRoomCenter>.instance.transform.position);
		CreateStatusEffect(victim, (Se_Mon_DarkCave_BossSeeker_Blink blink) =>
		{
			blink.customDestination = center;
		});
		Destroy();
		Entity GetTarget()
		{
			DewPlayer dewPlayer = Dew.SelectBestWithScore((IList<DewPlayer>)DewPlayer.gamePlayers, (Func<DewPlayer, int, float>)((DewPlayer p, int _) =>
			{
				if (p.hero.IsNullInactiveDeadOrKnockedOut())
				{
					return float.NegativeInfinity;
				}
				return p.hero.Status.isUndetectableByNonAllies ? (-100f) : (0f - p.hero.Status.normalizedHealth);
			}), 0.15f, (DewRandom)null);
			if (dewPlayer.hero.IsNullInactiveDeadOrKnockedOut())
			{
				return null;
			}
			Hero hero = dewPlayer.hero;
			float y = UnityEngine.Random.Range(120f, 240f);
			Vector3 end = AbilityTrigger.PredictPoint_Simple(victim, UnityEngine.Random.value, hero, 1f) + Quaternion.Euler(0f, y, 0f) * ((Component)(object)hero).transform.forward * 3f;
			end = Dew.GetValidAgentDestination_LinearSweep(hero.GetAIAgentPosition(victim), end);
			Teleport(victim, end);
			return hero;
		}
	}

	protected override void OnDestroyActor()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
			Appear();
			victim.Control.StartDaze(postDaze);
		}
	}

	private void Disappear()
	{
		CreateStatusEffect<Se_Mon_DarkCave_BossSeeker_TunnelVision_Disappear>(victim);
	}

	private void Appear()
	{
		if (victim.Status.TryGetStatusEffect<Se_Mon_DarkCave_BossSeeker_TunnelVision_Disappear>(out var effect))
		{
			effect.Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
