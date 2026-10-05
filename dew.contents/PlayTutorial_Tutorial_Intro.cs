using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class PlayTutorial_Tutorial_Intro : PlayTutorial_Tutorial
{
	public DewAnimationClip animLayDown;

	public GameObject fxStatue;

	public GameObject fxStatueActivate;

	public GameObject fxStatueHeal;

	public GameObject fxDying;

	public UI_ShowAndHideObject activateObject;

	public DewAnimationClip animGetUp;

	public Entity statueEntity;

	public UI_ShowAndHideObject moveTutorial;

	public GameObject[] deactivatedObjectsInBlackScreen;

	private CameraModifierZoom _zoom;

	public RoomSection firstSpiderSection;

	public Transform[] firstSpiderPositions;

	public Room_Barrier firstBarrier;

	public UI_ShowAndHideObject attackTutorial;

	public RoomSection secondSpiderSection;

	public Transform[] secondSpiderPositions;

	public Room_Barrier secondBarrier;

	public UI_ShowAndHideObject characterSkillDetailTutorial;

	public RoomSection riftSection;

	public Rift_MockExit rift;

	public Entity riftEntity;

	public Transform nextTutorialTeleportPos;

	protected override IEnumerator OnStart()
	{
		((Hero_Lacerta)h).actDeadInTutorial = true;
		GameObject[] array = deactivatedObjectsInBlackScreen;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		ManagerBase<ObjectHighlightManager>.instance.isObjectHighlightDisabled = true;
		ManagerBase<MusicManager>.instance.Stop();
		ManagerBase<CameraManager>.instance.DoGenericFadeOut(immediately: true);
		PlayTutorialManager.instance.LockSkills();
		PlayTutorialManager.instance.LockEditSkill();
		ManagerBase<ControlManager>.instance.DisableCharacterControls();
		DewEffect.Play(fxStatue);
		h.Control.RotateTowards(fxStatue.transform.position, immediately: true);
		h.Animation.PlayAbilityAnimation(animLayDown);
		h.Status.SetHealth(1f);
		yield return new WaitForSeconds(1f);
		activateObject.Show();
		float animDuration = 5.5f;
		int startZoom = 7;
		int endZoom = 0;
		_zoom = new CameraModifierZoom
		{
			zoomIndex = startZoom
		};
		_zoom.Apply();
		yield return new WaitUntil(() => DewInput.GetButtonDown(DewSave.profileMain.controls.interact, checkGameAreaForMouse: false));
		DewEffect.Play(fxDying);
		h.Visual.FixClothes();
		array = deactivatedObjectsInBlackScreen;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		ManagerBase<CameraManager>.instance.DoGenericFadeIn();
		ManagerBase<MusicManager>.instance.Play(SingletonDewNetworkBehaviour<Room>.instance.music);
		DewEffect.Play(fxStatueActivate);
		activateObject.Hide();
		h.Control.RotateTowards(fxStatue.transform.position, immediately: true);
		yield return new WaitForSeconds(1f);
		DewEffect.Play(fxStatueHeal);
		DewEffect.Stop(fxDying);
		h.CreateStatusEffect(h, new CastInfo(h), (Se_GenericHealOverTime heal) =>
		{
			heal.ticks = 8;
			heal.totalAmount = h.maxHealth;
			heal.tickInterval = 0.1f;
		});
		yield return new WaitForSeconds(1.5f);
		((Hero_Lacerta)h).actDeadInTutorial = false;
		h.Animation.PlayAbilityAnimation(animGetUp);
		for (float t = 0f; t < animDuration; t += Time.deltaTime)
		{
			ManagerBase<CameraManager>.instance.SetZoomLevel(endZoom);
			_zoom.zoomIndex = Mathf.Lerp(startZoom, endZoom, t / animDuration);
			yield return null;
		}
		_zoom.Remove();
		_zoom = null;
		ManagerBase<ControlManager>.instance.EnableCharacterControls();
		ManagerBase<ObjectHighlightManager>.instance.isObjectHighlightDisabled = false;
		h.Visual.FixClothes();
		if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			yield return NetworkedManagerBase<ConversationManager>.instance.StartConversationRoutine(new DewConversationSettings
			{
				player = DewPlayer.local,
				rotateTowardsCenter = true,
				speakers = new Entity[2] { h, statueEntity },
				startConversationKey = "PlayTutorial.VoiceFromStatue",
				visibility = ConversationVisibility.Everyone
			});
		}
		IControlPresetWindow window = Dew.GetControlPresetWindow();
		window.Show(showCancel: false);
		ManagerBase<ControlManager>.instance.DisableCharacterControls();
		yield return new WaitWhile(() => window.IsShown());
		ManagerBase<ControlManager>.instance.EnableCharacterControls();
		yield return new WaitForSeconds(1f);
		moveTutorial.Show();
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = firstSpiderSection.transform.position;
		yield return new WaitForEvent<Entity>(firstSpiderSection.onEntityEnter);
		moveTutorial.Hide();
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = null;
		firstBarrier.Close();
		yield return new WaitForSeconds(0.5f);
		attackTutorial.Show();
		Transform[] array2 = firstSpiderPositions;
		foreach (Transform transform in array2)
		{
			Quaternion value = Quaternion.LookRotation(h.position - transform.position).Flattened();
			Mon_Forest_SpiderSpitter entity = Dew.SpawnEntity<Mon_Forest_SpiderSpitter>(transform.position, value, null, DewPlayer.creep, 1);
			AddToSpawnedEntity(entity);
			yield return new WaitForSeconds(0.35f);
		}
		yield return WaitForSpawnedEntityDeathRoutine();
		attackTutorial.Hide();
		yield return new WaitForSeconds(0.35f);
		firstBarrier.Open();
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = secondSpiderSection.transform.position;
		yield return new WaitForEvent<Entity>(secondSpiderSection.onEntityEnter);
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = null;
		secondBarrier.Close();
		firstBarrier.Close();
		yield return new WaitForSeconds(0.5f);
		PlayTutorialManager.instance.UnlockSkills();
		if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			EntityAI.DisableAI = true;
			characterSkillDetailTutorial.Show();
			ManagerBase<ControlManager>.instance.DisableCharacterControls();
		}
		for (int num = 0; num < secondSpiderPositions.Length; num++)
		{
			Transform transform2 = secondSpiderPositions[num];
			Quaternion value2 = Quaternion.LookRotation(h.position - transform2.position).Flattened();
			Entity entity2 = ((num == 0) ? ((Entity)Dew.SpawnEntity<Mon_Forest_SpiderWarrior>(transform2.position, value2, null, DewPlayer.creep, 1)) : ((Entity)Dew.SpawnEntity<Mon_Forest_SpiderSpitter>(transform2.position, value2, null, DewPlayer.creep, 1)));
			AddToSpawnedEntity(entity2);
			yield return new WaitForSeconds(0.35f);
		}
		if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			yield return new WaitWhile(() => characterSkillDetailTutorial.gameObject.activeSelf);
			ManagerBase<ControlManager>.instance.EnableCharacterControls();
			EntityAI.DisableAI = false;
		}
		else
		{
			yield return new WaitForSeconds(0.55f);
		}
		RefValue<bool> didCastQ = new RefValue<bool>(v: false);
		RefValue<bool> didCastR = new RefValue<bool>(v: false);
		TutorialArrow arrQ = CreateTutorialArrow().SetBoxPlacementByOffset(new Vector2(-0.02f, 0.07f)).SetDuration(0.5f, float.PositiveInfinity).SetRawText("<size=110%>[" + DewInput.GetReadableTextForCurrentMode(DewSave.profileMain.controls.skillQ) + "] - " + DewLocalization.GetUIValue("PlayTutorial_Cast") + "</size>", processBacktickExpressions: true)
			.SetDestroyCondition(() => didCastQ)
			.FollowUIElement(ManagerBase<InGameTutorialManager>.instance.refSkillButtonQ);
		TutorialArrow arrR = CreateTutorialArrow().SetBoxPlacementByOffset(new Vector2(0.02f, 0.07f)).SetDuration(0.5f, float.PositiveInfinity).SetRawText("<size=110%>[" + DewInput.GetReadableTextForCurrentMode(DewSave.profileMain.controls.skillR) + "] - " + DewLocalization.GetUIValue("PlayTutorial_Cast") + "</size>", processBacktickExpressions: true)
			.SetDestroyCondition(() => didCastR)
			.FollowUIElement(ManagerBase<InGameTutorialManager>.instance.refSkillButtonR);
		Coroutine highlightQ = StartSkillHighlightCoroutine(() => HeroSkillLocation.Q);
		Coroutine highlightR = StartSkillHighlightCoroutine(() => HeroSkillLocation.R);
		DewPlayer.local.hero.ClientHeroEvent_OnSkillUse += (Action<EventInfoSkillUse>)((EventInfoSkillUse info) =>
		{
			if (info.type == HeroSkillLocation.Q)
			{
				didCastQ.value = true;
			}
			if (info.type == HeroSkillLocation.R)
			{
				didCastR.value = true;
			}
		});
		yield return WaitForSpawnedEntityDeathRoutine();
		for (int num = 0; num < secondSpiderPositions.Length - 1; num++)
		{
			Transform transform3 = secondSpiderPositions[num];
			Quaternion value3 = Quaternion.LookRotation(h.position - transform3.position).Flattened();
			Entity entity3 = ((num == 0 || num == 1) ? ((Entity)Dew.SpawnEntity<Mon_Forest_SpiderWarrior>(transform3.position, value3, null, DewPlayer.creep, 1)) : ((Entity)Dew.SpawnEntity<Mon_Forest_SpiderSpitter>(transform3.position, value3, null, DewPlayer.creep, 1)));
			AddToSpawnedEntity(entity3);
			yield return new WaitForSeconds(0.35f);
		}
		yield return WaitForSpawnedEntityDeathRoutine();
		yield return new WaitForSeconds(0.5f);
		if (arrQ != null)
		{
			UnityEngine.Object.Destroy(arrQ.gameObject);
		}
		if (arrR != null)
		{
			UnityEngine.Object.Destroy(arrR.gameObject);
		}
		StopCoroutine(highlightQ);
		StopCoroutine(highlightR);
		secondBarrier.Open();
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = riftSection.transform.position;
		yield return new WaitForEvent<Entity>(riftSection.onEntityEnter);
		ManagerBase<ObjectiveArrowManager>.instance.objectivePosition = null;
		if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			ManagerBase<ControlManager>.instance.DisableCharacterControls();
			Vector3 position = ((Component)(object)rift).transform.position + (h.position - ((Component)(object)rift).transform.position).normalized * 3f;
			position = Dew.GetPositionOnGround(position);
			h.Control.MoveToDestination(position, immediately: true, 0.75f);
		}
		yield return new WaitForSeconds(0.5f);
		rift.Open();
		if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			yield return new WaitForSeconds(2.5f);
			ManagerBase<ControlManager>.instance.EnableCharacterControls();
			yield return NetworkedManagerBase<ConversationManager>.instance.StartConversationRoutine(new DewConversationSettings
			{
				player = DewPlayer.local,
				rotateTowardsCenter = true,
				speakers = new Entity[2] { h, riftEntity },
				startConversationKey = "PlayTutorial.Rift",
				visibility = ConversationVisibility.Everyone
			});
		}
		yield return new WaitForEvent<int>(rift.onTravel);
		ManagerBase<ControlManager>.instance.DisableCharacterControls();
		yield return new WaitForSeconds(0.5f);
		Se_PortalTransition transition = h.CreateStatusEffect<Se_PortalTransition>(h, default);
		yield return new WaitForSeconds(0.5f);
		yield return ManagerBase<TransitionManager>.instance.FadeOutRoutine(showTips: false);
		h.Control.Teleport(Dew.GetValidAgentPosition(Dew.GetPositionOnGround(nextTutorialTeleportPos.position)));
		yield return new WaitForSeconds(1f);
		ManagerBase<TransitionManager>.instance.FadeIn();
		transition.Destroy();
		ManagerBase<ControlManager>.instance.EnableCharacterControls();
		PlayTutorialManager.instance.AdvanceTutorial();
	}

	protected override void OnCleanup()
	{
		DewEffect.Stop(fxDying);
		EntityAI.DisableAI = false;
		Dew.GetControlPresetWindow()?.Hide();
		GameObject[] array = deactivatedObjectsInBlackScreen;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(value: true);
			}
		}
		if (ManagerBase<ObjectHighlightManager>.instance != null)
		{
			ManagerBase<ObjectHighlightManager>.instance.isObjectHighlightDisabled = false;
		}
		if (ManagerBase<ControlManager>.instance != null)
		{
			while (!ManagerBase<ControlManager>.instance.isCharacterControlEnabled)
			{
				ManagerBase<ControlManager>.instance.EnableCharacterControls();
			}
		}
		if (_zoom != null)
		{
			_zoom.Remove();
			_zoom = null;
		}
		if (ManagerBase<MusicManager>.instance != null)
		{
			ManagerBase<MusicManager>.instance.Play(SingletonDewNetworkBehaviour<Room>.instance.music);
		}
		if (ManagerBase<CameraManager>.instance != null)
		{
			ManagerBase<CameraManager>.instance.DoGenericFadeIn(immediately: true);
		}
		uint[] array2 = NetworkedManagerBase<ConversationManager>.instance.convSettings.Keys.ToArray();
		foreach (uint id in array2)
		{
			NetworkedManagerBase<ConversationManager>.instance.StopConversation(id);
		}
		if ((UnityEngine.Object)(object)firstBarrier != null)
		{
			firstBarrier.Open();
		}
		if ((UnityEngine.Object)(object)secondBarrier != null)
		{
			secondBarrier.Open();
		}
		if ((UnityEngine.Object)(object)rift != null)
		{
			rift.onTravel.RemoveAllListeners();
			rift.Close();
		}
		DewEffect.Stop(fxStatue);
		if ((UnityEngine.Object)(object)PlayTutorialManager.instance != null)
		{
			PlayTutorialManager.instance.UnlockSkills();
			PlayTutorialManager.instance.UnlockEditSkill();
		}
		if ((UnityEngine.Object)(object)h != null)
		{
			h.Animation.StopAbilityAnimation();
		}
		if (moveTutorial != null)
		{
			moveTutorial.Hide();
		}
		if (attackTutorial != null)
		{
			attackTutorial.Hide();
		}
		if (h is Hero_Lacerta hero_Lacerta && (UnityEngine.Object)(object)hero_Lacerta != null)
		{
			hero_Lacerta.actDeadInTutorial = false;
		}
	}
}
