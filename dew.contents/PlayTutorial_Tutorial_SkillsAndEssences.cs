using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class PlayTutorial_Tutorial_SkillsAndEssences : PlayTutorial_Tutorial
{
	public Mon_Polaris polaris;

	public Transform polarisIntroPos;

	public Room_Barrier barrier;

	public Transform polarisSkillPos;

	public Shrine_Memory skillShrine;

	public Transform skillShrineSpawnPos;

	public Transform[] testSkillMonsterPoses;

	public UI_ShowAndHideObject allSkillsTutorial;

	public Transform polarisEssencePos;

	public Shrine_Concept essenceShrine;

	public Transform essenceShrineSpawnPos;

	public Transform[] testEssenceMonsterPoses;

	public UI_ShowAndHideObject gemDetailedTutorial;

	public UI_ShowAndHideObject gemSimpleTutorial;

	public Transform[] createEssencePoses;

	public Transform treantPos;

	public UI_ShowAndHideObject essenceCombinationTutorial;

	public Transform skillEditPivot;

	public UI_ShowAndHideObject editSkillDetailedTutorial;

	public UI_ShowAndHideObject editSkillKeyTutorial;

	public UI_ShowAndHideObject statDetailedTutorial;

	public UI_ShowAndHideObject dismantleTutorial;

	public Transform floatingSkillTransform;

	protected override IEnumerator OnStart()
	{
		barrier.Open();
		skillShrine.isLocked = true;
		essenceShrine.isLocked = true;
		PlayTutorialManager.instance.LockEditSkill();
		PlayTutorialManager.instance.LockDismantle();
		polaris.Status.SetHealth(polaris.maxHealth);
		h.Status.SetHealth(h.maxHealth * 0.5f);
		polaris.Control.Teleport(Dew.GetValidAgentPosition(Dew.GetPositionOnGround(polarisIntroPos.position)));
		polaris.owner = DewPlayer.local;
		Se_Mon_Polaris_Disappear eff = polaris.CreateStatusEffect(polaris, new CastInfo(polaris), (Se_Mon_Polaris_Disappear disappear) =>
		{
			disappear.startEffect = null;
			disappear.startEffectVictim = null;
			disappear.startEffectNoStop = null;
		});
		yield return WaitForPlayerPrescenceRoutine();
		eff.Destroy();
		polaris.Control.RotateTowards(h, immediately: true);
		ManagerBase<ControlManager>.instance.DisableCharacterControls();
		yield return new WaitForSeconds(1.5f);
		yield return SayRoutine("PlayTutorial.Introduction");
		yield return new WaitForSeconds(0.5f);
		ManagerBase<ControlManager>.instance.EnableCharacterControls();
		yield return BlinkPolarisRoutine(polarisSkillPos.position);
		yield return WaitForPlayerPrescenceRoutine();
		barrier.Close();
		yield return SayRoutine("PlayTutorial.Remnants");
		skillShrine.skillOverride = DewResources.GetByType<St_R_PillarOfFlame>(default(ResourceLoadSettings));
		skillShrine.positionOverride = skillShrineSpawnPos.position;
		skillShrine.isLocked = false;
		ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetLocalizedText("PlayTutorial_Remnants_Activate").FollowWorldTarget(() => skillShrine.position + Vector3.up)
			.SetDuration(float.PositiveInfinity)
			.SetBoxPlacement(BoxPlacementMode.Below)
			.SetDestroyCondition(() => !skillShrine.isAvailable);
		yield return new WaitWhile(() => skillShrine.isAvailable);
		yield return PauseRoutine(1.5f);
		yield return SayRoutine("PlayTutorial.EquipSkill");
		St_R_PillarOfFlame skill = Dew.FindActorOfType<St_R_PillarOfFlame>();
		skill.configs[0].cooldownTime = 3.5f;
		ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetLocalizedText("PlayTutorial_Remnants_Equip").FollowWorldTarget(() => skill.worldModel.iconQuad.transform.position)
			.SetDuration(float.PositiveInfinity)
			.SetBoxPlacement(BoxPlacementMode.Below)
			.SetDestroyCondition(() => (UnityEngine.Object)(object)skill.owner != null);
		yield return new WaitWhile(() => (UnityEngine.Object)(object)skill.owner == null);
		yield return PauseRoutine(2f);
		yield return SayRoutine("PlayTutorial.TrySkill");
		allSkillsTutorial.Show();
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = GetAveragePosition(testSkillMonsterPoses);
		Transform[] array = testSkillMonsterPoses;
		foreach (Transform transform in array)
		{
			Mon_Forest_SpiderSpitter mon_Forest_SpiderSpitter = SpawnHallucination<Mon_Forest_SpiderSpitter>(transform.position);
			mon_Forest_SpiderSpitter.Status.AddStatBonus(new StatBonus
			{
				maxHealthPercentage = 35f
			});
			mon_Forest_SpiderSpitter.takenDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
			{
				if ((UnityEngine.Object)(object)actor.FindFirstOfType<St_R_PillarOfFlame>() == null)
				{
					data.ApplyRawMultiplier(0f);
				}
			});
			yield return new WaitForSeconds(0.15f);
		}
		ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetLocalizedText("PlayTutorial_Remnants_KillWithSkill").FollowScreenPos(() =>
		{
			h.Skill.TryGetSkillLocation(skill, out var type2);
			return PlayTutorialManager.instance.GetSkillArrowPivot(type2).position;
		})
			.SetDestroyCondition(() => spawnedEntities.All((Entity e) => e.IsNullInactiveDeadOrKnockedOut()))
			.SetDuration(float.PositiveInfinity);
		Coroutine highlightRoutine = StartSkillHighlightCoroutine(() =>
		{
			h.Skill.TryGetSkillLocation(skill, out var type2);
			return type2;
		});
		yield return WaitForSpawnedEntityDeathRoutine();
		StopCoroutine(highlightRoutine);
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = null;
		yield return new WaitForSeconds(1.5f);
		allSkillsTutorial.Hide();
		ManagerBase<ControlManager>.instance.DisableCharacterControls();
		yield return BlinkPolarisRoutine(polarisEssencePos.position);
		h.Control.MoveToDestination(polarisEssencePos.position, immediately: true);
		yield return WaitForPlayerPrescenceRoutine();
		h.Control.Stop();
		ManagerBase<ControlManager>.instance.EnableCharacterControls();
		yield return SayRoutine("PlayTutorial.MoreRemnants");
		essenceShrine.gemOverride = DewResources.GetByType<Gem_C_Charcoal>(default(ResourceLoadSettings));
		essenceShrine.positionOverride = essenceShrineSpawnPos.position;
		essenceShrine.isLocked = false;
		ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetLocalizedText("PlayTutorial_Remnants_Activate").FollowWorldTarget(() => essenceShrine.position + Vector3.up)
			.SetBoxPlacement(BoxPlacementMode.Below)
			.SetDuration(float.PositiveInfinity)
			.SetDestroyCondition(() => !essenceShrine.isAvailable);
		yield return new WaitWhile(() => essenceShrine.isAvailable);
		yield return PauseRoutine(1.5f);
		if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			yield return SayRoutine("PlayTutorial.EquipEssence");
			ManagerBase<ControlManager>.instance.DisableCharacterControls();
			gemDetailedTutorial.Show();
			yield return new WaitWhile(() => gemDetailedTutorial.gameObject.activeSelf);
			ManagerBase<ControlManager>.instance.EnableCharacterControls();
		}
		gemSimpleTutorial.Show();
		ManagerBase<ControlManager>.instance.gemLocationConstraint = HeroSkillLocation.W;
		Gem_C_Charcoal gem = Dew.FindActorOfType<Gem_C_Charcoal>();
		ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetLocalizedText("PlayTutorial_Remnants_Equip").FollowWorldTarget(() => gem.worldModel.iconQuad.transform.position)
			.SetBoxPlacement(BoxPlacementMode.Below)
			.SetDuration(float.PositiveInfinity)
			.SetDestroyCondition(() => (UnityEngine.Object)(object)gem.handOwner != null || (UnityEngine.Object)(object)gem.owner != null);
		yield return new WaitWhile(() => (UnityEngine.Object)(object)gem.handOwner == null);
		highlightRoutine = StartSkillHighlightCoroutine(() =>
		{
			h.Skill.TryGetSkillLocation(skill, out var type2);
			return type2;
		});
		ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetLocalizedText("PlayTutorial_Essence_EquipOnNewSkill").FollowScreenPos(() =>
		{
			h.Skill.TryGetSkillLocation(skill, out var type2);
			return PlayTutorialManager.instance.GetSkillArrowPivot(type2).position;
		})
			.SetDestroyCondition(() => (UnityEngine.Object)(object)gem.owner != null)
			.SetDuration(float.PositiveInfinity);
		yield return new WaitWhile(() => (UnityEngine.Object)(object)gem.owner == null);
		StopCoroutine(highlightRoutine);
		yield return PauseRoutine(1.5f);
		gemSimpleTutorial.Hide();
		array = testEssenceMonsterPoses;
		foreach (Transform transform2 in array)
		{
			Mon_Forest_SpiderSpitter mon_Forest_SpiderSpitter2 = SpawnHallucination<Mon_Forest_SpiderSpitter>(transform2.position);
			mon_Forest_SpiderSpitter2.Status.AddStatBonus(new StatBonus
			{
				maxHealthPercentage = 35f
			});
			mon_Forest_SpiderSpitter2.takenDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
			{
				if ((UnityEngine.Object)(object)actor.FindFirstOfType<St_R_PillarOfFlame>() == null)
				{
					data.ApplyRawMultiplier(0f);
				}
			});
			yield return new WaitForSeconds(0.15f);
		}
		yield return SayRoutine("PlayTutorial.TryEmpoweredSkill");
		allSkillsTutorial.Show();
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = GetAveragePosition(testEssenceMonsterPoses);
		highlightRoutine = StartSkillHighlightCoroutine(() =>
		{
			h.Skill.TryGetSkillLocation(skill, out var type2);
			return type2;
		});
		yield return WaitForSpawnedEntityDeathRoutine();
		StopCoroutine(highlightRoutine);
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = null;
		allSkillsTutorial.Hide();
		yield return PauseRoutine(1.5f);
		yield return SayRoutine("PlayTutorial.UseSynergyToYourAdvantage");
		Gem_C_Charcoal gemToMerge = Dew.CreateGem<Gem_C_Charcoal>(createEssencePoses[0].position, 100);
		yield return new WaitForSeconds(2f);
		ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetLocalizedText("PlayTutorial_Remnants_Equip").FollowWorldTarget(() => gemToMerge.position)
			.SetDestroyCondition(() => (UnityEngine.Object)(object)gemToMerge.handOwner != null || (UnityEngine.Object)(object)gemToMerge.owner != null || gemToMerge.IsNullOrInactive())
			.SetDuration(float.PositiveInfinity);
		yield return new WaitUntil(() => gemToMerge.IsNullOrInactive());
		highlightRoutine = StartSkillHighlightCoroutine(() =>
		{
			h.Skill.TryGetSkillLocation(skill, out var type2);
			return type2;
		});
		ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetLocalizedText("PlayTutorial_Essence_MergeEssence").FollowScreenPos(() =>
		{
			h.Skill.TryGetSkillLocation(skill, out var type2);
			return PlayTutorialManager.instance.GetSkillArrowPivot(type2).position;
		})
			.SetDuration(7f);
		yield return PauseRoutine(1.5f);
		yield return SayRoutine("PlayTutorial.DuplicateEssenceMerge");
		StopCoroutine(highlightRoutine);
		yield return SayRoutine("PlayTutorial.MemoryAndEssence");
		ManagerBase<ControlManager>.instance.DisableCharacterControls();
		Gem_C_Sharp moreGems0 = Dew.CreateGem<Gem_C_Sharp>(createEssencePoses[0].position, 220);
		yield return new WaitForSeconds(0.15f);
		Gem_E_Clemency moreGems1 = Dew.CreateGem<Gem_E_Clemency>(createEssencePoses[1].position, 220);
		yield return new WaitForSeconds(0.35f);
		ManagerBase<ControlManager>.instance.EnableCharacterControls();
		yield return SayRoutine("PlayTutorial.UseTwoEssences");
		ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetLocalizedText("PlayTutorial_Remnants_Equip").FollowWorldTarget(() => (createEssencePoses[0].position + createEssencePoses[1].position) / 2f)
			.SetDestroyCondition(() => ((UnityEngine.Object)(object)moreGems0.owner != null || (UnityEngine.Object)(object)moreGems0.handOwner != null) && ((UnityEngine.Object)(object)moreGems1.owner != null || (UnityEngine.Object)(object)moreGems1.handOwner != null))
			.SetBoxPlacement(BoxPlacementMode.Below)
			.SetDuration(float.PositiveInfinity);
		yield return new WaitWhile(() => (UnityEngine.Object)(object)moreGems0.handOwner == null && (UnityEngine.Object)(object)moreGems1.handOwner == null);
		highlightRoutine = StartSkillHighlightCoroutine(() =>
		{
			h.Skill.TryGetSkillLocation(skill, out var type2);
			return type2;
		});
		ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetLocalizedText("PlayTutorial_Essence_EquipTwoEssencesOnNewSkill").FollowScreenPos(() =>
		{
			h.Skill.TryGetSkillLocation(skill, out var type2);
			return PlayTutorialManager.instance.GetSkillArrowPivot(type2).position;
		})
			.SetDestroyCondition(() => (UnityEngine.Object)(object)moreGems0.owner != null && (UnityEngine.Object)(object)moreGems1.owner != null)
			.SetDuration(float.PositiveInfinity);
		yield return new WaitWhile(() => (UnityEngine.Object)(object)moreGems0.owner == null || (UnityEngine.Object)(object)moreGems1.owner == null);
		StopCoroutine(highlightRoutine);
		yield return PauseRoutine(2f);
		Mon_Forest_Treant treant = SpawnHallucination<Mon_Forest_Treant>(treantPos.position);
		treant.Visual.GetNewTransformModifier().scaleMultiplier = Vector3.one * 1.2f;
		treant.Status.AddStatBonus(new StatBonus
		{
			maxHealthPercentage = 100f
		});
		treant.takenDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
		{
			if ((UnityEngine.Object)(object)actor.FindFirstOfType<St_R_PillarOfFlame>() == null)
			{
				data.ApplyRawMultiplier(0.7f);
			}
		});
		yield return SayRoutine("PlayTutorial.TryEmpoweredSkill2");
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = treantPos.position;
		ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetLocalizedText("PlayTutorial_KillTheMonster").FollowWorldTarget(() => treant.Visual.GetCenterPosition())
			.SetDestroyCondition(() => treant.IsNullInactiveDeadOrKnockedOut())
			.SetDuration(float.PositiveInfinity);
		yield return new WaitWhile(() => !treant.IsNullInactiveDeadOrKnockedOut());
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = null;
		yield return new WaitForSeconds(2f);
		yield return SayRoutine("PlayTutorial.Synergies");
		ManagerBase<ControlManager>.instance.DisableCharacterControls();
		essenceCombinationTutorial.Show();
		yield return new WaitWhile(() => essenceCombinationTutorial.gameObject.activeSelf);
		ManagerBase<ControlManager>.instance.EnableCharacterControls();
		yield return new WaitForSeconds(1f);
		yield return SayRoutine("PlayTutorial.EditSkill");
		editSkillDetailedTutorial.Show();
		yield return new WaitWhile(() => editSkillDetailedTutorial.gameObject.activeSelf);
		editSkillKeyTutorial.Show();
		PlayTutorialManager.instance.UnlockEditSkill();
		ManagerBase<ControlManager>.instance.gemLocationConstraint = null;
		ManagerBase<ControlManager>.instance.dropConstraint = (UnityEngine.Object _) => false;
		RefValue<int> moveCount = new RefValue<int>(0);
		h.Skill.ClientHeroEvent_OnSkillSwap += (Action<HeroSkillLocation, HeroSkillLocation>)((HeroSkillLocation _, HeroSkillLocation _) =>
		{
			moveCount.value++;
		});
		h.Skill.ClientHeroEvent_OnGemSwap += (Action<GemLocation, GemLocation>)((GemLocation _, GemLocation _) =>
		{
			moveCount.value++;
		});
		h.Skill.ClientHeroEvent_OnSkillDrop += (Action<SkillTrigger>)((SkillTrigger _) =>
		{
			moveCount.value++;
		});
		h.Skill.ClientHeroEvent_OnGemDrop += (Action<Gem>)((Gem _) =>
		{
			moveCount.value++;
		});
		while (moveCount.value < 1)
		{
			InputMode currentMode = DewInput.currentMode;
			if (ManagerBase<EditSkillManager>.instance.mode == EditSkillManager.ModeType.None)
			{
				TutorialArrow arr = ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetRawText(string.Format(DewLocalization.GetUIValue((DewInput.currentMode == InputMode.KeyboardAndMouse) ? "PlayTutorial_Dismantle_HoldToEnterEditMemory" : "PlayTutorial_Dismantle_HoldToEnterEditMemory_Gamepad"), DewInput.GetReadableTextForCurrentMode(DewSave.profileMain.controls.editSkillHold)), processBacktickExpressions: true).FollowScreenPos(() => skillEditPivot.position)
					.SetDestroyCondition(() => ManagerBase<EditSkillManager>.instance.mode != EditSkillManager.ModeType.None || DewInput.currentMode != currentMode)
					.SetDuration(float.PositiveInfinity);
				yield return new WaitWhile(() => arr != null);
				continue;
			}
			string text = string.Format(DewLocalization.GetUIValue("PlayTutorial_EditSkill_MoveAroundSkillsOrEssences"), 1);
			TutorialArrow arr2 = ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetRawText(text, processBacktickExpressions: true).FollowScreenPos(() => skillEditPivot.position)
				.SetDestroyCondition(() => moveCount.value >= 1 || ManagerBase<EditSkillManager>.instance.mode == EditSkillManager.ModeType.None || DewInput.currentMode != currentMode)
				.SetDuration(float.PositiveInfinity);
			yield return new WaitWhile(() => arr2 != null);
		}
		editSkillKeyTutorial.Hide();
		string text2 = ((DewInput.currentMode == InputMode.KeyboardAndMouse) ? string.Format(DewLocalization.GetUIValue("PlayTutorial_EditSkill_CloseMemoryEdit_PC"), DewInput.GetReadableTextForCurrentMode(DewSave.profileMain.controls.editSkillHold)) : string.Format(DewLocalization.GetUIValue("PlayTutorial_EditSkill_CloseMemoryEdit_Gamepad"), DewInput.GetReadableTextForCurrentMode(DewSave.profileMain.controls.back)));
		ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetRawText(text2, processBacktickExpressions: true).FollowScreenPos(() => skillEditPivot.position)
			.SetDestroyCondition(() => ManagerBase<EditSkillManager>.instance.mode == EditSkillManager.ModeType.None)
			.SetDuration(float.PositiveInfinity);
		yield return new WaitWhile(() => ManagerBase<EditSkillManager>.instance.mode != EditSkillManager.ModeType.None);
		if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			statDetailedTutorial.Show();
			yield return new WaitWhile(() => statDetailedTutorial.gameObject.activeSelf);
		}
		yield return SayRoutine("PlayTutorial.Dismantle");
		ManagerBase<ControlManager>.instance.dropConstraint = (UnityEngine.Object s) => s is St_C_Sneeze;
		St_C_Sneeze sneeze = Dew.CreateSkillTrigger<St_C_Sneeze>(Vector3.zero, 1);
		RefValue<int> sneezeCount = new RefValue<int>(0);
		sneeze.ClientSkillEvent_OnDismantleProgressChanged += (Action<float, float>)((float from, float to) =>
		{
			if (from < to && to < 1f)
			{
				InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
				{
					color = Color.white,
					rawText = DewLocalization.GetUIValue("PlayTutorial_Dismantle_KeepClicking_Popup"),
					worldPosGetter = () => sneeze.worldModel.iconQuad.transform.position
				});
			}
		});
		At_Mon_Polaris_Smite smite = polaris.Ability.GetAbility<At_Mon_Polaris_Smite>();
		sneeze.TriggerEvent_OnCastComplete += (Action<EventInfoCast>)((EventInfoCast _) =>
		{
			sneezeCount.value++;
			if ((int)sneezeCount % 3 == 0)
			{
				StartCoroutine(SayRoutine("PlayTutorial.Sneeze*"));
			}
			else if ((int)sneezeCount > 6)
			{
				smite.ResetCooldown(smite);
				polaris.Control.Cast(smite, h);
			}
		});
		HeroSkillLocation type = HeroSkillLocation.W;
		if ((UnityEngine.Object)(object)h.Skill.Q == null)
		{
			type = HeroSkillLocation.Q;
		}
		else if ((UnityEngine.Object)(object)h.Skill.W == null)
		{
			type = HeroSkillLocation.W;
		}
		else if ((UnityEngine.Object)(object)h.Skill.E == null)
		{
			type = HeroSkillLocation.E;
		}
		else if ((UnityEngine.Object)(object)h.Skill.R == null)
		{
			type = HeroSkillLocation.R;
		}
		h.Skill.CmdEquipSkill(type, sneeze);
		dismantleTutorial.Show();
		PlayTutorialManager.instance.UnlockDismantle();
		ManagerBase<ControlManager>.instance.dismantleConstraint = (UnityEngine.Object st) => st is St_C_Sneeze;
		while (!sneeze.IsNullOrInactive())
		{
			InputMode currentMode2 = DewInput.currentMode;
			if ((UnityEngine.Object)(object)sneeze.owner != null)
			{
				highlightRoutine = StartSkillHighlightCoroutine(() =>
				{
					h.Skill.TryGetSkillLocation(sneeze, out var type2);
					return type2;
				});
				if (ManagerBase<EditSkillManager>.instance.mode == EditSkillManager.ModeType.None)
				{
					TutorialArrow arr3 = ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetRawText(string.Format(DewLocalization.GetUIValue((DewInput.currentMode == InputMode.KeyboardAndMouse) ? "PlayTutorial_Dismantle_HoldToEnterEditMemory" : "PlayTutorial_Dismantle_HoldToEnterEditMemory_Gamepad"), DewInput.GetReadableTextForCurrentMode(DewSave.profileMain.controls.editSkillHold)), processBacktickExpressions: true).FollowScreenPos(() =>
					{
						h.Skill.TryGetSkillLocation(sneeze, out var type2);
						return PlayTutorialManager.instance.GetSkillArrowPivot(type2).position;
					})
						.SetDestroyCondition(() => ManagerBase<EditSkillManager>.instance.mode != EditSkillManager.ModeType.None || DewInput.currentMode != currentMode2)
						.SetDuration(float.PositiveInfinity);
					yield return new WaitWhile(() => arr3 != null);
				}
				else if (DewInput.currentMode == InputMode.KeyboardAndMouse)
				{
					TutorialArrow arr4 = ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetRawText(DewLocalization.GetUIValue("PlayTutorial_Dismantle_DragOutToDiscard"), processBacktickExpressions: true).FollowScreenPos(() =>
					{
						h.Skill.TryGetSkillLocation(sneeze, out var type2);
						return PlayTutorialManager.instance.GetSkillArrowPivot(type2).position;
					})
						.SetDestroyCondition(() => (UnityEngine.Object)(object)sneeze.owner == null || ManagerBase<EditSkillManager>.instance.mode == EditSkillManager.ModeType.None || DewInput.currentMode != currentMode2)
						.SetDuration(float.PositiveInfinity);
					yield return new WaitWhile(() => arr4 != null);
				}
				else if (!ManagerBase<EditSkillManager>.instance.isDragging || !(ManagerBase<EditSkillManager>.instance.draggingObject is St_C_Sneeze))
				{
					TutorialArrow arr5 = ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetRawText(string.Format(DewLocalization.GetUIValue("PlayTutorial_Dismantle_Gamepad_Select"), DewInput.GetReadableTextForCurrentMode(DewSave.profileMain.controls.confirm)), processBacktickExpressions: true).FollowScreenPos(() =>
					{
						h.Skill.TryGetSkillLocation(sneeze, out var type2);
						return PlayTutorialManager.instance.GetSkillArrowPivot(type2).position;
					})
						.SetDestroyCondition(() => (UnityEngine.Object)(object)sneeze.owner == null || ManagerBase<EditSkillManager>.instance.mode == EditSkillManager.ModeType.None || ManagerBase<EditSkillManager>.instance.draggingObject is St_C_Sneeze || DewInput.currentMode != currentMode2)
						.SetDuration(float.PositiveInfinity);
					yield return new WaitWhile(() => arr5 != null);
				}
				else if (!ManagerBase<EditSkillManager>.instance.isSelectingGround)
				{
					TutorialArrow arr6 = ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetRawText(DewLocalization.GetUIValue("PlayTutorial_Dismantle_Gamepad_DragUp"), processBacktickExpressions: true).FollowScreenPos(() =>
					{
						h.Skill.TryGetSkillLocation(sneeze, out var type2);
						return PlayTutorialManager.instance.GetSkillArrowPivot(type2).position;
					})
						.SetDestroyCondition(() => (UnityEngine.Object)(object)sneeze.owner == null || ManagerBase<EditSkillManager>.instance.mode == EditSkillManager.ModeType.None || ManagerBase<EditSkillManager>.instance.isSelectingGround || !(ManagerBase<EditSkillManager>.instance.draggingObject is St_C_Sneeze) || DewInput.currentMode != currentMode2)
						.SetDuration(float.PositiveInfinity);
					yield return new WaitWhile(() => arr6 != null);
				}
				else
				{
					TutorialArrow arr7 = ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetRawText(string.Format(DewLocalization.GetUIValue("PlayTutorial_Dismantle_Gamepad_Discard"), DewInput.GetReadableTextForCurrentMode(DewSave.profileMain.controls.confirm)), processBacktickExpressions: true).FollowScreenPos(() => floatingSkillTransform.position)
						.SetDestroyCondition(() => (UnityEngine.Object)(object)sneeze.owner == null || ManagerBase<EditSkillManager>.instance.mode == EditSkillManager.ModeType.None || !ManagerBase<EditSkillManager>.instance.isSelectingGround || !(ManagerBase<EditSkillManager>.instance.draggingObject is St_C_Sneeze) || DewInput.currentMode != currentMode2)
						.SetDuration(float.PositiveInfinity);
					yield return new WaitWhile(() => arr7 != null);
				}
				StopCoroutine(highlightRoutine);
			}
			else
			{
				if (DewInput.currentMode == InputMode.Gamepad && ManagerBase<EditSkillManager>.instance.mode != EditSkillManager.ModeType.None)
				{
					ManagerBase<EditSkillManager>.instance.EndEdit();
				}
				ManagerBase<InGameTutorialManager>.instance.CreateArrow().SetRawText(string.Format(DewLocalization.GetUIValue("PlayTutorial_Dismantle_Dismantle"), DewInput.GetReadableTextForCurrentMode(DewSave.profileMain.controls.interactAlt)), processBacktickExpressions: true).FollowWorldTarget(() => sneeze.worldModel.iconQuad.transform.position)
					.SetDestroyCondition(() => (UnityEngine.Object)(object)sneeze.owner != null || sneeze.IsNullOrInactive() || DewInput.currentMode != currentMode2)
					.SetDuration(float.PositiveInfinity);
				yield return new WaitUntil(() => (UnityEngine.Object)(object)sneeze.owner != null || sneeze.IsNullOrInactive());
			}
		}
		ManagerBase<ControlManager>.instance.dismantleConstraint = null;
		yield return PauseRoutine(1.5f);
		yield return SayRoutine("PlayTutorial.DismantleDreamDust");
		dismantleTutorial.Hide();
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			ManagerBase<ControlManager>.instance.DisableCharacterControls();
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				buttons = DewMessageSettings.ButtonType.Ok,
				rawContent = DewLocalization.GetUIValue("PlayTutorial_Message_EndTutorialGuidebook")
			});
			yield return new WaitWhile(() => ManagerBase<MessageManager>.instance.isShowingMessage);
			ManagerBase<CameraManager>.instance.DoGenericFadeOut();
			PlayTutorial_Tutorial_Hunt.GoToPlayLobby();
		}
		else
		{
			PlayTutorialManager.instance.AdvanceTutorial();
		}
	}

	private Vector3 GetAveragePosition(Transform[] transforms)
	{
		Vector3 zero = Vector3.zero;
		foreach (Transform transform in transforms)
		{
			zero += transform.position;
		}
		return zero / transforms.Length;
	}

	private IEnumerator SayRoutine(string key)
	{
		yield return NetworkedManagerBase<ConversationManager>.instance.StartConversationRoutine(new DewConversationSettings
		{
			player = DewPlayer.local,
			rotateTowardsCenter = true,
			speakers = new Entity[2] { h, polaris },
			startConversationKey = key,
			visibility = ConversationVisibility.Everyone
		});
	}

	private T SpawnHallucination<T>(Vector3 pos, Action<T> onBeforePrepare = null) where T : Entity
	{
		Quaternion value = Quaternion.LookRotation(h.position - pos).Flattened();
		T val = Dew.SpawnEntity(pos, value, null, DewPlayer.creep, 1, (T e) =>
		{
			e.Visual.skipSpawning = true;
			onBeforePrepare?.Invoke(e);
		});
		val.AI.disableAI = true;
		val.CreateStatusEffect<Se_PolarisHallucination>(val, default);
		AddToSpawnedEntity(val);
		return val;
	}

	private IEnumerator PauseRoutine(float duration)
	{
		ManagerBase<ControlManager>.instance.DisableCharacterControls();
		yield return new WaitForSeconds(duration);
		ManagerBase<ControlManager>.instance.EnableCharacterControls();
	}

	private IEnumerator WaitForPlayerPrescenceRoutine()
	{
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = polaris.position;
		yield return new WaitWhile(() => Vector2.Distance(polaris.agentPosition.ToXY(), h.agentPosition.ToXY()) > 4.65f);
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = null;
	}

	private IEnumerator BlinkPolarisRoutine(Vector3 destination)
	{
		polaris.Control.RotateTowards(h, immediately: false);
		Se_Mon_Polaris_Blink se = polaris.CreateStatusEffect<Se_Mon_Polaris_Blink>(polaris, new CastInfo(polaris, destination));
		yield return new WaitWhile(() => !se.IsNullOrInactive());
		polaris.Control.RotateTowards(h, immediately: false, float.PositiveInfinity);
	}

	protected override void OnCleanup()
	{
		if ((UnityEngine.Object)(object)PlayTutorialManager.instance != null)
		{
			PlayTutorialManager.instance.UnlockEditSkill();
			PlayTutorialManager.instance.UnlockDismantle();
		}
		if ((UnityEngine.Object)(object)polaris != null && polaris.Status.TryGetStatusEffect<Se_Mon_Polaris_Disappear>(out var effect))
		{
			effect.Destroy();
		}
		if ((UnityEngine.Object)(object)skillShrine != null)
		{
			skillShrine.isLocked = false;
			skillShrine.ResetUseCounts();
		}
		if ((UnityEngine.Object)(object)essenceShrine != null)
		{
			essenceShrine.isLocked = false;
			essenceShrine.ResetUseCounts();
		}
		if (ManagerBase<MusicManager>.instance != null)
		{
			ManagerBase<MusicManager>.instance.Play(SingletonDewNetworkBehaviour<Room>.instance.music);
		}
		if (ManagerBase<ControlManager>.instance != null)
		{
			while (!ManagerBase<ControlManager>.instance.isCharacterControlEnabled)
			{
				ManagerBase<ControlManager>.instance.EnableCharacterControls();
			}
			ManagerBase<ControlManager>.instance.dropConstraint = null;
		}
		uint[] array = NetworkedManagerBase<ConversationManager>.instance.convSettings.Keys.ToArray();
		foreach (uint id in array)
		{
			NetworkedManagerBase<ConversationManager>.instance.StopConversation(id);
		}
	}
}
