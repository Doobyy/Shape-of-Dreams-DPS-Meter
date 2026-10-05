using UnityEngine;

public class Aurena_Claws : MonoBehaviour, ILobbyCharacterModelSetup
{
	public void OnLobbyCharacterSetup()
	{
		Object.Destroy(gameObject);
	}
}
