using UnityEngine;

public class FxSimulateParticleSystem : MonoBehaviour, IEffectComponent
{
	public float time;

	public bool isPlaying => false;

	public void Play()
	{
		if (TryGetComponent<ParticleSystem>(out var component))
		{
			component.Simulate(time);
			component.Play();
		}
	}

	public void Stop()
	{
	}
}
