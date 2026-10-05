using UnityEngine;

public class FxSelectiveQuality : MonoBehaviour, ISerializationCallbackReceiver
{
	private const float AboutOne = 1.06f;

	private const float AboutZero = 0.01f;

	private const float MaxParticlesMargin = 1.5f;

	private const int MaxParticlesCeilingLow = 512;

	private const int MaxParticlesCeilingMedium = 1024;

	public bool disableGameObject;

	public Quality3Levels allowedIfQualityIs;

	[Space(30f)]
	public bool adjustParticleCount = true;

	public bool includeChildrenWhenAdjustingParticleCount;

	[HideInInspector]
	public bool wasDisabled;

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}

	public static void ApplyQualityScaling(Quality3Levels currentQualityLevel, GameObject go, bool adjustEmission)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Invalid comparison between Unknown and I4
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		if (currentQualityLevel >= Quality3Levels.High || !go.TryGetComponent<ParticleSystem>(out var component))
		{
			return;
		}
		if (currentQualityLevel == Quality3Levels.Low && (Object)(object)go.GetComponentInParent<Rift>(includeInactive: true) == null)
		{
			CollisionModule collision = component.collision;
			if (collision.enabled)
			{
				collision.enabled = false;
			}
			LightsModule lights = component.lights;
			if (lights.enabled)
			{
				lights.enabled = false;
			}
		}
		MainModule main = component.main;
		int maxParticles = main.maxParticles;
		int num = ((currentQualityLevel == Quality3Levels.Low) ? 512 : 1024);
		if (main.maxParticles > num)
		{
			main.maxParticles = num;
		}
		if (!adjustEmission)
		{
			return;
		}
		ShapeModule shape = component.shape;
		if (shape.enabled)
		{
			shape = component.shape;
			if ((int)shape.arcMode == 3)
			{
				return;
			}
		}
		EmissionModule emission = component.emission;
		if (!emission.enabled)
		{
			return;
		}
		float num2 = GetMaxConstantOrInfinity(main.startLifetime);
		float num3 = GetMaxConstantOrInfinity(emission.rateOverTime);
		float num4 = num3;
		float num5 = 0f;
		int burstCount = emission.burstCount;
		Burst[] array = DewPool.GetArray(out ArrayReturnHandle<Burst> handle, burstCount);
		if (burstCount > 0)
		{
			emission.GetBursts(array);
			if (num3 <= 0.01f && burstCount == 1 && num2 <= 1.06f)
			{
				handle.Return();
				return;
			}
			for (int i = 0; i < burstCount; i++)
			{
				float num6 = GetMaxConstantOrInfinity(array[i].count);
				if (float.IsPositiveInfinity(num6))
				{
					num5 = float.PositiveInfinity;
					break;
				}
				num5 += num6;
			}
			float num7 = Mathf.Max(main.duration, 0.01f);
			num3 += num5 / num7;
		}
		if (num3 * num2 <= 1.06f)
		{
			handle.Return();
			return;
		}
		float num8 = currentQualityLevel switch
		{
			Quality3Levels.Low => 0.4f, 
			Quality3Levels.Medium => 0.6f, 
			_ => 1f, 
		};
		emission.rateOverTimeMultiplier *= num8;
		emission.rateOverDistanceMultiplier *= num8;
		if (burstCount > 0)
		{
			for (int j = 0; j < burstCount; j++)
			{
				MinMaxCurve count = array[j].count;
				short num9 = (short)Mathf.Max(1f, count.constantMin * num8);
				count = array[j].count;
				short num10 = (short)Mathf.Max(1f, count.constantMax * num8);
				array[j].count = new MinMaxCurve((float)num9, (float)num10);
			}
			emission.SetBursts(array, burstCount);
		}
		int num11;
		if (!float.IsInfinity(num4) && !float.IsInfinity(num2) && !float.IsInfinity(num5) && GetMaxConstantOrInfinity(emission.rateOverDistance) <= 0.01f)
		{
			SubEmittersModule subEmitters = component.subEmitters;
			if (subEmitters.enabled)
			{
				subEmitters = component.subEmitters;
				num11 = ((subEmitters.subEmittersCount <= 0) ? 1 : 0);
			}
			else
			{
				num11 = 1;
			}
		}
		else
		{
			num11 = 0;
		}
		bool flag = (byte)num11 != 0;
		int num12 = Mathf.Max(2, flag ? Mathf.CeilToInt((num4 * num2 + num5) * num8 * 1.5f) : Mathf.CeilToInt((float)maxParticles * num8));
		if (num12 < main.maxParticles)
		{
			main.maxParticles = num12;
		}
		handle.Return();
		static float GetMaxConstantOrInfinity(MinMaxCurve curve)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Invalid comparison between Unknown and I4
			ParticleSystemCurveMode mode = curve.mode;
			if ((int)mode == 0)
			{
				return curve.constant;
			}
			if ((int)mode == 3)
			{
				return curve.constantMin;
			}
			return float.PositiveInfinity;
		}
	}
}
