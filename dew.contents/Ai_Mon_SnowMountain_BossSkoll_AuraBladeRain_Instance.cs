using System;
using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_BossSkoll_AuraBladeRain_Instance : InstantDamageInstance
{
	public GameObject fxTelegraph;

	[NonSerialized]
	public bool isLastPhase;

	[NonSerialized]
	private float _pristineDamageDelay;

	[NonSerialized]
	private ParticleSystem[] _fxTelegraphParticles;

	[NonSerialized]
	private float[] _fxTelegraphSimSpeeds;

	[NonSerialized]
	private IEffectWithSpeed[] _fxTelegraphSpeedEffects;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		_pristineDamageDelay = damageDelay;
		if (fxTelegraph != null)
		{
			_fxTelegraphParticles = fxTelegraph.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
			_fxTelegraphSimSpeeds = new float[_fxTelegraphParticles.Length];
			for (int i = 0; i < _fxTelegraphParticles.Length; i++)
			{
				float[] fxTelegraphSimSpeeds = _fxTelegraphSimSpeeds;
				int num = i;
				MainModule main = _fxTelegraphParticles[i].main;
				fxTelegraphSimSpeeds[num] = main.simulationSpeed;
			}
			_fxTelegraphSpeedEffects = fxTelegraph.GetComponentsInChildren<IEffectWithSpeed>(includeInactive: true);
		}
	}

	protected override void OnCreate()
	{
		if (isLastPhase)
		{
			damageDelay = _pristineDamageDelay * 2f;
		}
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (isLastPhase)
			{
				FxApplySpeedMultiplierNetworked(fxTelegraph, 0.5f);
			}
			FxPlayNetworked(fxTelegraph, position, rotation);
		}
	}

	protected override void OnDisable()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		base.OnDisable();
		isLastPhase = false;
		damageDelay = _pristineDamageDelay;
		if (_fxTelegraphParticles != null)
		{
			for (int i = 0; i < _fxTelegraphParticles.Length; i++)
			{
				MainModule main = _fxTelegraphParticles[i].main;
				main.simulationSpeed = _fxTelegraphSimSpeeds[i];
			}
		}
		if (_fxTelegraphSpeedEffects != null)
		{
			for (int j = 0; j < _fxTelegraphSpeedEffects.Length; j++)
			{
				_fxTelegraphSpeedEffects[j].ApplySpeedMultiplier(1f);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
