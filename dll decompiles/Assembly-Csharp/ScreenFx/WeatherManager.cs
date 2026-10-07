using UnityEngine;
using UnityEngine.Rendering;

namespace ScreenFx;

public class WeatherManager : MonoBehaviour
{
	public DirectorState _sun;

	public DirectorState _clouds;

	public ParticleSystem _snow;

	public float _snowMax;

	public void Update()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		WeatherSettings component = VolumeManager.instance.stack.GetComponent<WeatherSettings>();
		EmissionModule emission = _snow.emission;
		emission.rateOverTimeMultiplier = ((VolumeParameter<float>)(object)component._snow).value * _snowMax;
		_sun.SetTimeImmediate(((VolumeParameter<float>)(object)component._sun).value);
		_clouds.SetTimeImmediate(((VolumeParameter<float>)(object)component._clouds).value);
	}
}
