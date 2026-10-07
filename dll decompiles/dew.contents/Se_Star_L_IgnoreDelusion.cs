using System;
using Mirror;
using UnityEngine;

public class Se_Star_L_IgnoreDelusion : StarEffect
{
	public StarScalingValue cooldownTime;

	public float graceTime = 0.55f;

	private bool _ignoreDelusionReady;

	private float _protectionEndTime;

	private float _lastActivateTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			showIcon = true;
			_ignoreDelusionReady = true;
			victim.ClientEntityEvent_OnStatusEffectAdded += new Action<EventInfoStatusEffect>(OnStatusEffectAdded);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !victim.IsNullOrInactive())
		{
			victim.ClientEntityEvent_OnStatusEffectAdded -= new Action<EventInfoStatusEffect>(OnStatusEffectAdded);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !victim.IsNullOrInactive() && !_ignoreDelusionReady && !(Time.time - _lastActivateTime <= GetValue(cooldownTime)))
		{
			_ignoreDelusionReady = true;
			showIcon = true;
		}
	}

	private void OnStatusEffectAdded(EventInfoStatusEffect obj)
	{
		if (obj.effect is Se_MirageSkin_Delusion_Delusional)
		{
			if (Time.time < _protectionEndTime)
			{
				obj.effect.Destroy();
				_lastActivateTime = Time.time;
			}
			else if (_ignoreDelusionReady)
			{
				obj.effect.Destroy();
				_protectionEndTime = Time.time + graceTime;
				_lastActivateTime = Time.time;
				_ignoreDelusionReady = false;
				showIcon = false;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
