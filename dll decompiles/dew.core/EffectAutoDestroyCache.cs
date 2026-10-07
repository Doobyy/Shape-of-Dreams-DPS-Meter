using UnityEngine;

public sealed class EffectAutoDestroyCache : MonoBehaviour
{
	public ParticleSystem rootParticleSystem;

	public IEffectComponent[] fxComponents;

	public ParticleSystem[] resetParticleSystems;

	public ParticleSystem[] loopingParticleSystems;

	public IEffectComponent[] loopingFxComponents;

	public IAttachableToEntity[] attachables;
}
