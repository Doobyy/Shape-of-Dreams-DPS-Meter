using UnityEngine;

public class Vesper_LobbyHammer : MonoBehaviour, ILobbyCharacterModelSetup
{
	public GameObject characterHammer;

	public GameObject lobbyHammer;

	public void OnLobbyCharacterSetup()
	{
		characterHammer.SetActive(value: false);
		lobbyHammer.transform.parent = null;
	}

	private void OnDestroy()
	{
		if (lobbyHammer != null)
		{
			Object.Destroy(lobbyHammer.gameObject);
		}
	}
}
