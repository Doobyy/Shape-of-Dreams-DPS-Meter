using UnityEngine;

public class Hero_Husk_CloseBladeInLobby : MonoBehaviour, ILobbyCharacterModelSetup
{
	private static readonly int CloseValue = Animator.StringToHash("CloseValue");

	public void OnLobbyCharacterSetup()
	{
		GetComponent<Animator>().SetFloat(CloseValue, 1f);
	}
}
