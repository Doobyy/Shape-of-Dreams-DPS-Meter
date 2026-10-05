using System;
using DG.Tweening;
using UnityEngine;

public class PlayLobby_RotateCharacterTutorial : MonoBehaviour
{
	public PlayLobby_Character[] lobbyCharacters;

	private void Start()
	{
		if (DewSave.profileMain.doneTutorials.Contains("PlayLobby_RotateCharacterTutorial"))
		{
			gameObject.SetActive(value: false);
		}
		LobbyUIManager.instance.onStateChanged += new Action<string, string>(OnStateChanged);
	}

	private void OnStateChanged(string arg1, string arg2)
	{
		ShortcutExtensions.DOKill((Component)transform, true);
		transform.localScale = Vector3.zero;
		TweenSettingsExtensions.SetId<Sequence>(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(DOTween.Sequence(), 1f), (Tween)(object)ShortcutExtensions.DOScale(transform, Vector3.one, 0.5f)), (object)transform);
	}

	private void Update()
	{
		PlayLobby_Character orDefault = lobbyCharacters.GetOrDefault(DewPlayer.lobbyPlayers.IndexOf(DewPlayer.local));
		if (!(orDefault != null))
		{
			return;
		}
		transform.position = ManagerBase<DewCamera>.instance.mainCamera.WorldToScreenPoint(orDefault.model.GetAbovePosition() * 0.35f + orDefault.model.GetCenterPosition() * 0.65f);
		if (orDefault.characterRotation.HasValue)
		{
			if (!DewSave.profileMain.doneTutorials.Contains("PlayLobby_RotateCharacterTutorial"))
			{
				DewSave.profileMain.doneTutorials.Add("PlayLobby_RotateCharacterTutorial");
			}
			gameObject.SetActive(value: false);
		}
	}
}
