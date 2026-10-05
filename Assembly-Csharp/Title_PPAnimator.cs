using SCPE;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Title_PPAnimator : MonoBehaviour
{
	public Volume volume;

	private ChromaticAberration _ca;

	private Refraction _ref;

	private void Start()
	{
		volume.profile.TryGet<ChromaticAberration>(ref _ca);
		volume.profile.TryGet<Refraction>(ref _ref);
	}

	private void Update()
	{
		((VolumeParameter<float>)(object)_ref.amount).Override(0.1563f + Mathf.Sin(Time.time * 1.125f) * 0.07f);
		((VolumeParameter<float>)(object)_ca.intensity).Override(0.15f + Mathf.Sin(Time.time * 0.953f) * 0.075f);
	}
}
