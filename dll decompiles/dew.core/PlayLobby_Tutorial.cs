using System.Collections;
using UnityEngine;

public class PlayLobby_Tutorial : MonoBehaviour
{
	public RectTransform changeTraveler;

	public RectTransform difficultyBox;

	public RectTransform startButton;

	public RectTransform heroList;

	public RectTransform skillConstellationsGroup;

	public RectTransform conSkillList;

	public RectTransform conStarList;

	public RectTransform conTravelerMastery;

	public RectTransform conHeroConstellations;

	public RectTransform conCategories;

	private bool _isFlexibleStarListSelected;

	private void Start()
	{
		if (!DewSave.profileMain.gameplay.disableTutorial)
		{
			if (!DewSave.profileMain.doneTutorials.Contains("PlayLobby_LobbyTutorial") || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
			{
				StartCoroutine(LobbyTutorialRoutine());
			}
			if (!DewSave.profileMain.doneTutorials.Contains("PlayLobby_TravelerSelection") || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
			{
				StartCoroutine(TravelerSelectionRoutine());
			}
			if (!DewSave.profileMain.doneTutorials.Contains("PlayLobby_NewConstellations") || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
			{
				StartCoroutine(NewConstellationsRoutine());
			}
			if (!DewSave.profileMain.doneTutorials.Contains("PlayLobby_FlexibleStars") && !DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
			{
				StartCoroutine(FlexibleStarsRoutine());
			}
		}
		IEnumerator FlexibleStarsRoutine()
		{
			yield return new WaitWhile(() => !LobbyUIManager.instance.IsState("Constellations") || !_isFlexibleStarListSelected);
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = conStarList,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Constellations_FlexibleStars"),
				textPlacement = TutorialHighlightTextPlacement.Bottom
			});
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = conHeroConstellations,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Constellations_FlexibleSlots"),
				textPlacement = TutorialHighlightTextPlacement.Right
			});
			if (!DewSave.profileMain.doneTutorials.Contains("PlayLobby_FlexibleStars"))
			{
				DewSave.profileMain.doneTutorials.Add("PlayLobby_FlexibleStars");
			}
		}
		IEnumerator LobbyTutorialRoutine()
		{
			yield return new WaitWhile(() => !LobbyUIManager.instance.IsState("Lobby"));
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = changeTraveler,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Lobby_ChangeTraveler"),
				textPlacement = TutorialHighlightTextPlacement.Top
			});
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = difficultyBox,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Lobby_Difficulty"),
				textPlacement = TutorialHighlightTextPlacement.Top
			});
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = startButton,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Lobby_StartIfReady"),
				textPlacement = TutorialHighlightTextPlacement.Top
			});
			if (!DewSave.profileMain.doneTutorials.Contains("PlayLobby_LobbyTutorial"))
			{
				DewSave.profileMain.doneTutorials.Add("PlayLobby_LobbyTutorial");
			}
		}
		IEnumerator NewConstellationsRoutine()
		{
			yield return new WaitWhile(() => !LobbyUIManager.instance.IsState("Constellations"));
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = conSkillList,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Character_ChangeMemory"),
				textPlacement = TutorialHighlightTextPlacement.Right
			});
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = conStarList,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Constellations_BrowseStarList"),
				textPlacement = TutorialHighlightTextPlacement.Bottom
			});
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = conTravelerMastery,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Constellations_TravelerMastery"),
				textPlacement = TutorialHighlightTextPlacement.Bottom
			});
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = conHeroConstellations,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Constellations_HeroConstellations"),
				textPlacement = TutorialHighlightTextPlacement.Bottom
			});
			if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
			{
				yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
				{
					target = conCategories,
					rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Constellations_CategorySlot"),
					textPlacement = TutorialHighlightTextPlacement.Bottom
				});
			}
			if (!DewSave.profileMain.doneTutorials.Contains("PlayLobby_NewConstellations"))
			{
				DewSave.profileMain.doneTutorials.Add("PlayLobby_NewConstellations");
			}
		}
		IEnumerator TravelerSelectionRoutine()
		{
			yield return new WaitWhile(() => !LobbyUIManager.instance.IsState("Character"));
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = heroList,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Character_ChangeTraveler"),
				textPlacement = TutorialHighlightTextPlacement.Right
			});
			yield return ManagerBase<GlobalUIManager>.instance.HighlightForTutorial(new TutorialHighlightSettings
			{
				target = skillConstellationsGroup,
				rawText = DewLocalization.GetUIValue("PlayLobby_Tutorial_Character_EditTraveler"),
				textPlacement = TutorialHighlightTextPlacement.Left
			});
			if (!DewSave.profileMain.doneTutorials.Contains("PlayLobby_TravelerSelection"))
			{
				DewSave.profileMain.doneTutorials.Add("PlayLobby_TravelerSelection");
			}
		}
	}

	public void SelectFlexibleStars(bool isSelected)
	{
		_isFlexibleStarListSelected = isSelected;
	}
}
