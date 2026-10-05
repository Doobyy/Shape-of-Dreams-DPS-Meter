using UnityEngine;

public class PlayLobby_CharacterCamera : MonoBehaviour
{
	public PlayLobby_Character basisCharacter;

	private Vector3 _localPos;

	private Quaternion _localRot;

	private PlayLobby_Character[] _characters;

	private void Awake()
	{
		_characters = Object.FindObjectsOfType<PlayLobby_Character>(includeInactive: true);
		Transform transform = basisCharacter.transform;
		_localPos = transform.InverseTransformPoint(base.transform.position);
		_localRot = Quaternion.Inverse(transform.rotation) * base.transform.rotation;
	}

	private void OnEnable()
	{
		UpdatePosition();
	}

	private void LateUpdate()
	{
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		PlayLobby_Character localPlayerCharacter = GetLocalPlayerCharacter();
		if (!(localPlayerCharacter == null))
		{
			Transform transform = localPlayerCharacter.transform;
			base.transform.SetPositionAndRotation(transform.TransformPoint(_localPos), transform.rotation * _localRot);
		}
	}

	private PlayLobby_Character GetLocalPlayerCharacter()
	{
		if ((Object)(object)DewPlayer.local == null)
		{
			return null;
		}
		PlayLobby_Character[] characters = _characters;
		foreach (PlayLobby_Character playLobby_Character in characters)
		{
			if (playLobby_Character.playerIndex >= 0 && playLobby_Character.playerIndex < DewPlayer.lobbyPlayers.Count && (Object)(object)DewPlayer.lobbyPlayers[playLobby_Character.playerIndex] == (Object)(object)DewPlayer.local)
			{
				return playLobby_Character;
			}
		}
		return null;
	}
}
