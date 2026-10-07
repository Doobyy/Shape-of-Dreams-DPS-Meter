using UnityEngine;

public class Hero_Bismuth_LobbyBook : MonoBehaviour, ILobbyCharacterModelSetup
{
	private bool _isLobby;

	private float _startTime;

	public void OnLobbyCharacterSetup()
	{
		_isLobby = true;
		_startTime = Time.time;
	}

	private void Update()
	{
		if (_isLobby)
		{
			Transform parent = transform.parent;
			transform.position = parent.position + parent.forward * -1.25f + Vector3.up * (Mathf.Sin(Time.time - _startTime) * 0.1f + 0.5f);
			transform.rotation = Quaternion.Euler(0f, -60f, 0f) * parent.rotation;
		}
	}
}
