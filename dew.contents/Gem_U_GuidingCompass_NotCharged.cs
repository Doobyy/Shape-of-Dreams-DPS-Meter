using System;
using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class Gem_U_GuidingCompass_NotCharged : Gem
{
	public GameObject fxSpreadCurses;

	public GameObject fxCompleted;

	public float activateCurseDelay = 0.65f;

	public int addedPerGuidanceBreak = 50;

	public int addedPerMiniBoss = 50;

	public int addedPerHeroicBoss = 100;

	public int completeThreshold = 500;

	[SaveVar(SaveVarFlags.Default)]
	private bool _didActivateCurses;

	public override bool isDroppedOnOwnerDisconnect => true;

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			Se_Curse_GuidingCompass_CripplingAnxiety.LiftAllIfCompassMissingDelayed();
		}
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
			if (!_didActivateCurses)
			{
				_didActivateCurses = true;
				FxPlayNetworked(fxSpreadCurses, newOwner);
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(activateCurseDelay);
			foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
			{
				if (!allHero.Status.HasStatusEffect<Se_Curse_GuidingCompass_CripplingAnxiety>())
				{
					allHero.CreateStatusEffect(allHero, new CastInfo(allHero), (Se_Curse_GuidingCompass_CripplingAnxiety ai) =>
					{
						ai.currentStrength = HatredStrengthType.Powerful;
						ai.progressType = QuestProgressType.Travel;
						ai.requiredAmount = 99;
					});
				}
			}
		}
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		Entity victim = obj.victim;
		Monster m = victim as Monster;
		if (m != null && m.IsAnyBoss())
		{
			CreateAbilityInstance(obj.victim.agentPosition, null, new CastInfo(obj.victim, owner), (Ai_Gem_U_GuidingCompass_PowerGainProjectile ai) =>
			{
				ai.qualityIncrease = ((m is BossMonster) ? addedPerHeroicBoss : addedPerMiniBoss);
				ai.targetGem = this;
			});
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldOwner != null)
		{
			oldOwner.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	protected override void OnQualityChange(int oldQuality, int newQuality)
	{
		base.OnQualityChange(oldQuality, newQuality);
		if (((NetworkBehaviour)this).isServer && !owner.IsNullOrInactive() && newQuality >= completeThreshold)
		{
			GemLocation loc = location;
			Hero hero = owner;
			Destroy();
			Gem_U_GuidingCompass_Charged gem = Dew.CreateGem<Gem_U_GuidingCompass_Charged>(hero.position, newQuality);
			if (!hero.Skill.gems.Values.Any((Gem g) => g is Gem_U_GuidingCompass_Charged))
			{
				hero.Skill.EquipGem(loc, gem);
			}
			hero.Skill.RequestOnlyGemNotification(gem);
			FxPlayNetworked(fxCompleted, hero);
		}
	}

	private void MirrorProcessed()
	{
	}
}
