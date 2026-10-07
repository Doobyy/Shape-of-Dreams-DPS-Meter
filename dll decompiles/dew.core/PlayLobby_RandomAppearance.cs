using UnityEngine;

public class PlayLobby_RandomAppearance : MonoBehaviour
{
	public float appearChance;

	private void Start()
	{
		gameObject.SetActive(Random.value < appearChance);
	}
}
