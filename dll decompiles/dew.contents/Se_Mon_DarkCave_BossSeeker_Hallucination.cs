using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class Se_Mon_DarkCave_BossSeeker_Hallucination : StatusEffect
{
	public float armorBonusAmount;

	public float disappearDuration;

	public int clonesCount;

	public float radiusFromCenter;

	public GameObject fxAppearReal;

	public GameObject fxAppearClone;

	public DewAnimationClip animAppear;

	public float cloneDestroyDamageThreshold = 0.25f;

	public float realDamageThreshold = 0.15f;

	public float dispDuration;

	public float dispDurationDiff;

	public float dispAwayDistance;

	public DewEase dispEase;

	public DewAnimationClip animStagger;

	public GameObject fxStagger;

	public float staggerDaze;

	private bool _isSpinning;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DoUnstoppable();
		if ((Object)(object)info.caster == (Object)(object)victim && NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier() < 0.5f)
		{
			DoArmorBoost(armorBonusAmount);
		}
		victim.Control.StartDaze(disappearDuration + dispDuration);
		Se_Mon_DarkCave_BossSeeker_Hallucination_Disappear disappear = CreateStatusEffect<Se_Mon_DarkCave_BossSeeker_Hallucination_Disappear>(victim);
		yield return new SI.WaitForSeconds(disappearDuration - 0.25f);
		Vector3 center = ((SingletonBehaviour<DarkCave_BossRoomCenter>.instance == null) ? victim.position : SingletonBehaviour<DarkCave_BossRoomCenter>.instance.transform.position);
		float angle = Random.Range(0f, 360f);
		Teleport(victim, center + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radiusFromCenter);
		victim.Control.RotateTowards(center, immediately: true, 1f);
		yield return new SI.WaitForSeconds(0.25f);
		int realIndex = Random.Range(0, clonesCount + 1);
		Entity[] clones = new Entity[clonesCount];
		for (int i = 0; i < clonesCount + 1; i++)
		{
			if (i == realIndex)
			{
				disappear.Destroy();
				FxPlayNetworked(fxAppearReal, victim);
				victim.Control.StartDaze(dispDuration + 1f);
				victim.Animation.PlayAbilityAnimation(animAppear);
				victim.Control.StartDisplacement(new DispByDestination
				{
					affectedByMovementSpeed = false,
					canGoOverTerrain = true,
					destination = Dew.GetValidAgentPosition(victim.position + (victim.position - center).Flattened().normalized * dispAwayDistance),
					duration = dispDuration,
					ease = dispEase,
					isCanceledByCC = false,
					isFriendly = true,
					rotateForward = false
				});
				yield return new SI.WaitForSeconds(0.15f);
				continue;
			}
			int num = i;
			if (i > realIndex)
			{
				num--;
			}
			angle += 360f / (float)(clonesCount + 1);
			Vector3 vector = center + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radiusFromCenter;
			Mon_DarkCave_SeekerHallucination mon_DarkCave_SeekerHallucination = Dew.SpawnEntity<Mon_DarkCave_SeekerHallucination>(vector, Quaternion.LookRotation(center - vector).Flattened(), this, DewPlayer.creep, info.caster.level);
			mon_DarkCave_SeekerHallucination.Status.SetHealth(victim.normalizedHealth * mon_DarkCave_SeekerHallucination.maxHealth);
			mon_DarkCave_SeekerHallucination.destroyHpThreshold = mon_DarkCave_SeekerHallucination.normalizedHealth - cloneDestroyDamageThreshold;
			mon_DarkCave_SeekerHallucination.Control.StartDaze(dispDuration + 1f);
			mon_DarkCave_SeekerHallucination.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = true,
				destination = Dew.GetValidAgentPosition(mon_DarkCave_SeekerHallucination.position + (mon_DarkCave_SeekerHallucination.position - center).Flattened().normalized * dispAwayDistance),
				duration = dispDuration,
				ease = dispEase,
				isCanceledByCC = false,
				isFriendly = true,
				rotateForward = false
			});
			FxPlayNewNetworked(fxAppearClone, mon_DarkCave_SeekerHallucination);
			mon_DarkCave_SeekerHallucination.Animation.PlayAbilityAnimation(animAppear);
			clones[num] = mon_DarkCave_SeekerHallucination;
			yield return new SI.WaitForSeconds(0.15f);
		}
		float startRealHealth = victim.normalizedHealth;
		bool shouldStagger = false;
		while (!clones.All((Entity c) => c.IsNullOrInactive()))
		{
			if (startRealHealth - victim.normalizedHealth > realDamageThreshold)
			{
				shouldStagger = true;
				break;
			}
			yield return new SI.WaitForSeconds(0.25f);
		}
		Entity[] array = clones;
		foreach (Entity entity in array)
		{
			if (!entity.IsNullInactiveDeadOrKnockedOut())
			{
				entity.Kill();
			}
		}
		victim.Control.CancelOngoingChannels();
		victim.Control.Stop();
		if (shouldStagger)
		{
			victim.Animation.PlayAbilityAnimation(animStagger);
			victim.Control.StartDaze(staggerDaze);
			FxPlayNetworked(fxStagger, victim);
			yield return new SI.WaitForSeconds(staggerDaze);
		}
		CreateStatusEffect(victim, (Se_Mon_DarkCave_BossSeeker_Blink blink) =>
		{
			blink.customDestination = center;
		});
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
