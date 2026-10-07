using UnityEngine;

public class FxParticleSystemCacheData : MonoBehaviour
{
	private struct OptimizationSettings
	{
		public int emissionRateOverDistance;

		public Burst[] bursts;

		public float rateOverTimeMultiplier;

		public float rateOverDistanceMultiplier;

		public int maxParticles;
	}

	private OptimizationSettings _initSettings;

	private ParticleSystem _particleSystem;

	private void Awake()
	{
		CacheDefault();
	}

	public void CacheDefault()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		_particleSystem = GetComponent<ParticleSystem>();
		if ((bool)(Object)(object)_particleSystem)
		{
			EmissionModule emission = _particleSystem.emission;
			if (emission.enabled)
			{
				_initSettings = default;
				Burst[] array = new Burst[emission.burstCount];
				emission.GetBursts(array);
				_initSettings.bursts = array;
				_initSettings.rateOverTimeMultiplier = emission.rateOverTimeMultiplier;
				_initSettings.rateOverDistanceMultiplier = emission.rateOverDistanceMultiplier;
				ref OptimizationSettings initSettings = ref _initSettings;
				MainModule main = _particleSystem.main;
				initSettings.maxParticles = main.maxParticles;
			}
		}
	}

	public void SetDefault()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if ((bool)(Object)(object)_particleSystem)
		{
			EmissionModule emission = _particleSystem.emission;
			if (emission.enabled)
			{
				emission.rateOverTimeMultiplier = _initSettings.rateOverTimeMultiplier;
				emission.rateOverDistanceMultiplier = _initSettings.rateOverDistanceMultiplier;
				emission.SetBursts(_initSettings.bursts);
				MainModule main = _particleSystem.main;
				main.maxParticles = _initSettings.maxParticles;
			}
		}
	}
}
