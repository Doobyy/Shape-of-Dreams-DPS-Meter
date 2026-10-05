using Mirror;
using UnityEngine;

public class Se_Elm_Cold : ElementalStatusEffect
{
	public float heroCCMultiplier;

	public float bossCCMultiplier;

	private SlowEffect _slow;

	private CrippleEffect _cripple;

	private float _knownAmp;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_knownAmp = 0f;
			float num = 1f;
			if (victim is Hero)
			{
				num = heroCCMultiplier;
			}
			else if (victim.IsAnyBoss())
			{
				num = bossCCMultiplier;
			}
			_slow = DoSlow((float)stack * 35f * num);
			_cripple = DoCripple((float)stack * 25f * num);
			victim.Status.hasCold = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((Object)(object)victim == null) && victim.Status.isAlive)
		{
			victim.Status.hasCold = false;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && (Object)(object)info.caster != null && ampAmount != _knownAmp)
		{
			UpdateEffectStrength();
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if (((NetworkBehaviour)this).isServer)
		{
			UpdateEffectStrength();
		}
	}

	[Server]
	private void UpdateEffectStrength()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_Elm_Cold::UpdateEffectStrength()' called when server was not active");
			return;
		}
		_knownAmp = GetColdEffectAmp();
		_slow.strength = (float)stack * 35f * (1f + GetColdEffectAmp());
		_cripple.strength = (float)stack * 25f * (1f + GetColdEffectAmp());
	}

	private float GetColdEffectAmp()
	{
		return ampAmount;
	}

	private void MirrorProcessed()
	{
	}
}
