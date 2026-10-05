using Steamworks;
using UnityEngine;

public class SteamCallbackRunner : MonoBehaviour
{
	private void Update()
	{
		if (DewSteam.isInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}
}
