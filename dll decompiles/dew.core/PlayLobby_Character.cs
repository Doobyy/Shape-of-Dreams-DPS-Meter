using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

[LogicUpdatePriority(2000)]
public class PlayLobby_Character : LogicBehaviour
{
	public int playerIndex;

	public string forceSetup = "";

	public CharacterModelDisplay model;

	public GameObject fxClick;

	public float? characterRotation;

	private float _cv;

	private void Start()
	{
		LobbyUIManager.instance.onStateChanged += new Action<string, string>(OnStateChanged);
	}

	private void OnStateChanged(string arg1, string arg2)
	{
		model.isFocused = (LobbyUIManager.instance.IsState("Character") || LobbyUIManager.instance.IsState("Souvenirs")) && IsLocalPlayer();
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		string text = forceSetup;
		if (forceSetup == "" && playerIndex < DewPlayer.lobbyPlayers.Count && playerIndex >= 0 && DewPlayer.lobbyPlayers[playerIndex].isEveryInfoSet)
		{
			DewPlayer dewPlayer = DewPlayer.lobbyPlayers[playerIndex];
			text = (dewPlayer.IsAllowedToUseItem(dewPlayer.selectedSkin) ? dewPlayer.selectedSkin : Skin.GetDefaultSkin(dewPlayer.selectedHeroType));
		}
		if (DewNetworkManager.startSettings.continueData != null)
		{
			text = "";
		}
		if (model.skinType != text)
		{
			model.skinType = text;
			characterRotation = null;
			model.transform.localRotation = Quaternion.identity;
		}
		if (playerIndex < DewPlayer.lobbyPlayers.Count && playerIndex >= 0)
		{
			DewPlayer p = DewPlayer.lobbyPlayers[playerIndex];
			List<string> list = ((IEnumerable<string>)p.selectedAccessories).Where((string a) => p.IsAllowedToUseItem(a)).ToListNonAlloc(out var handle);
			if (!model.accessories.SequenceEqual(list))
			{
				model.accessories = list.ToList();
				model.UpdateAccessories();
			}
			handle.Return();
		}
	}

	private void UpdateTransparency(bool isLocalPlayer)
	{
		bool flag = (LobbyUIManager.instance.IsState("Character") || LobbyUIManager.instance.IsState("Souvenirs") || LobbyUIManager.instance.IsState("Emblems") || LobbyUIManager.instance.IsState("Constellations")) && !isLocalPlayer;
		model.opacity = Mathf.MoveTowards(model.opacity, flag ? 0f : 1f, Time.deltaTime * 1.5f);
	}

	public void Click()
	{
		DewEffect.Play(fxClick);
		if (IsLocalPlayer() && !DewPlayer.local.isReady)
		{
			LobbyUIManager.instance.OpenCharacterSelection();
		}
	}

	public bool IsLocalPlayer()
	{
		if (playerIndex >= 0 && playerIndex < DewPlayer.lobbyPlayers.Count)
		{
			return ((NetworkBehaviour)DewPlayer.lobbyPlayers[playerIndex]).isLocalPlayer;
		}
		return false;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		bool isLocalPlayer = IsLocalPlayer();
		UpdateTransparency(isLocalPlayer);
		if (characterRotation.HasValue)
		{
			model.transform.localRotation = Quaternion.Euler(0f, characterRotation.Value, 0f);
			_cv = 0f;
		}
		else
		{
			float y = model.transform.localRotation.eulerAngles.y;
			y = Mathf.SmoothDampAngle(y, 0f, ref _cv, 0.4f);
			model.transform.localRotation = Quaternion.Euler(0f, y, 0f);
		}
	}
}
