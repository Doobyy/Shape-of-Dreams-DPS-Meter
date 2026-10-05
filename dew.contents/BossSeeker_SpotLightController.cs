using UnityEngine;

public class BossSeeker_SpotLightController : MonoBehaviour
{
	public float hallucinateMultiplier = 0.25f;

	private float _defaultIntensity;

	private Entity _entity;

	private Light _light;

	private float _cv;

	private void Start()
	{
		_light = GetComponent<Light>();
		_entity = GetComponentInParent<Entity>(includeInactive: true);
		_defaultIntensity = _light.intensity;
		_light.intensity = 0f;
	}

	private void Update()
	{
		_light.intensity = Mathf.SmoothDamp(_light.intensity, GetTargetIntensity(), ref _cv, 0.2f);
		_entity.IsNullInactiveDeadOrKnockedOut();
	}

	private float GetTargetIntensity()
	{
		if (_entity.IsNullInactiveDeadOrKnockedOut() || _entity.Visual.isRendererOff || _entity.Status.HasStatusEffect<Se_Mon_DarkCave_BossSeeker_TunnelVision>())
		{
			return 0f;
		}
		if (_entity is Mon_DarkCave_SeekerHallucination || _entity.Status.HasStatusEffect<Se_Mon_DarkCave_BossSeeker_Hallucination>())
		{
			return _defaultIntensity * hallucinateMultiplier;
		}
		return _defaultIntensity;
	}
}
