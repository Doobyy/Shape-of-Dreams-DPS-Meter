using Mirror;
using UnityEngine;

public class Se_Gem_E_Fever_LivingBomb : StatusEffect
{
	public Transform[] scaledTransforms;

	public float duration = 1.5f;

	public float radiusAmpPerStack = 0.3f;

	public float dmgAmpPerStack = 0.1f;

	public ParticleSystem[] colorAdjustedParticles;

	public Gradient gradient;

	private Vector3[] _baseScales;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		if (scaledTransforms == null)
		{
			return;
		}
		_baseScales = new Vector3[scaledTransforms.Length];
		for (int i = 0; i < scaledTransforms.Length; i++)
		{
			if (scaledTransforms[i] != null)
			{
				_baseScales[i] = scaledTransforms[i].localScale;
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_baseScales == null)
		{
			return;
		}
		for (int i = 0; i < scaledTransforms.Length && i < _baseScales.Length; i++)
		{
			if (scaledTransforms[i] != null)
			{
				scaledTransforms[i].localScale = _baseScales[i];
			}
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		UpdateColor(0f);
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		float explosionRadius = GetExplosionRadius();
		Transform[] array = scaledTransforms;
		foreach (Transform transform in array)
		{
			if (!(transform == null))
			{
				transform.localScale = new Vector3(explosionRadius, explosionRadius, explosionRadius);
			}
		}
		if (normalizedDuration.HasValue)
		{
			UpdateColor(1f - normalizedDuration.Value);
		}
	}

	private void UpdateColor(float normalized)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		ParticleSystem[] array = colorAdjustedParticles;
		foreach (ParticleSystem val in array)
		{
			if (!((Object)(object)val == null))
			{
				MainModule main = val.main;
				main.startColor = MinMaxGradient.op_Implicit(gradient.Evaluate(normalized));
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !gem.IsNullOrInactive() && gem.isValid && !((Object)(object)victim == null))
		{
			CreateAbilityInstance(Dew.GetPositionOnGround(victim.agentPosition), null, info, (Ai_Gem_E_Fever_Explosion ai) =>
			{
				ai.chain = chain;
				ai.NetworkexplosionRadius = GetExplosionRadius();
				ai.NetworkdamageAmp = GetDamageAmp();
			});
		}
	}

	public float GetExplosionRadius()
	{
		return 1.5f * (1f + (float)Mathf.Clamp(victim.Status.fireStack, 0, 25) * radiusAmpPerStack);
	}

	public float GetDamageAmp()
	{
		return (float)victim.Status.fireStack * dmgAmpPerStack;
	}

	private void MirrorProcessed()
	{
	}
}
