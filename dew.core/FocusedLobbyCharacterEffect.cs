using UnityEngine;

public class FocusedLobbyCharacterEffect : MonoBehaviour, ILobbyCharacterModelOnFocus
{
	public void OnLobbyCharacterFocus(bool value)
	{
		if (value)
		{
			DewEffect.Play(gameObject);
		}
		else
		{
			DewEffect.Stop(gameObject);
		}
	}
}
