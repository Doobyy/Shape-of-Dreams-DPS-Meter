using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class FxUnityEvent : MonoBehaviour, IEffectComponent
{
	public UnityEvent onPlay;

	public bool invokeOnClients_OnPlay = true;

	[Space(16f)]
	public UnityEvent onStop;

	public bool invokeOnClients_OnStop = true;

	public bool isPlaying { get; }

	public void Play()
	{
		if (invokeOnClients_OnPlay || NetworkServer.active)
		{
			onPlay?.Invoke();
		}
	}

	public void Stop()
	{
		if (invokeOnClients_OnStop || NetworkServer.active)
		{
			onStop?.Invoke();
		}
	}
}
