using Mirror;
using UnityEngine;

public class FxNetworkedEffect : NetworkBehaviour
{
	public void PlayNetworked()
	{
		DewEffect.PlayNetworked(((NetworkBehaviour)this).netIdentity, ((Component)this).gameObject);
	}

	public void StopNetworked()
	{
		DewEffect.StopNetworked(((NetworkBehaviour)this).netIdentity, ((Component)this).gameObject);
	}

	public void Play()
	{
		DewEffect.Play(((Component)this).gameObject);
	}

	public void Stop()
	{
		DewEffect.Stop(((Component)this).gameObject);
	}

	private void MirrorProcessed()
	{
	}
}
