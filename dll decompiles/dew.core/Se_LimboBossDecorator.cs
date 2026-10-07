using System;
using Mirror;
using UnityEngine;

public class Se_LimboBossDecorator : StatusEffect
{
	public GameObject fxLoop;

	private ParticleSystem[] _particleSystems;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((UnityEngine.Object)(object)victim == null))
		{
			if (_particleSystems == null)
			{
				_particleSystems = fxLoop.GetComponentsInChildren<ParticleSystem>();
			}
			if (!victim.Visual.isSpawning)
			{
				PlayEffect();
			}
			else
			{
				victim.Visual.ClientEvent_OnSpawnComplete += new Action(PlayEffect);
			}
			victim.Visual.ClientEvent_OnRendererEnabledChanged += new Action<bool>(OnRendererEnableChanged);
			if (((NetworkBehaviour)this).isServer)
			{
				DestroyOnDeath(victim);
			}
		}
	}

	private void PlayEffect()
	{
		FxPlay(fxLoop, victim);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(fxLoop);
		if ((bool)(UnityEngine.Object)(object)victim)
		{
			victim.Visual.ClientEvent_OnSpawnComplete -= new Action(PlayEffect);
			victim.Visual.ClientEvent_OnRendererEnabledChanged -= new Action<bool>(OnRendererEnableChanged);
		}
	}

	private void OnRendererEnableChanged(bool obj)
	{
		ParticleSystem[] particleSystems;
		if (obj)
		{
			particleSystems = _particleSystems;
			for (int i = 0; i < particleSystems.Length; i++)
			{
				particleSystems[i].Play();
			}
			return;
		}
		particleSystems = _particleSystems;
		foreach (ParticleSystem obj2 in particleSystems)
		{
			obj2.Clear();
			obj2.Stop();
		}
	}

	private void MirrorProcessed()
	{
	}
}
