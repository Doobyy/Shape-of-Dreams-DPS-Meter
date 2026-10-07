using Mirror;
using UnityEngine;

public class Se_D_QuartetOfDeath_DeathMark : StatusEffect
{
	public GameObject fxExplosion;

	public GameObject fxStack1;

	public GameObject fxStack2;

	public GameObject fxStack3;

	private int _currentStack;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			RefreshEffect();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_currentStack = 0;
	}

	public void RefreshEffect()
	{
		_currentStack++;
		ChangeFxByStack();
	}

	public void OnExplosion()
	{
		StopAllFx();
		FxPlayNetworked(fxExplosion, victim);
		CreateAbilityInstance(victim.position, null, new CastInfo(info.caster, victim), (Ai_D_QuartetOfDeath_Explosion ai) =>
		{
			ai.currentStack = _currentStack;
		});
		DestroyIfActive();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			StopAllFx();
		}
	}

	private void StopAllFx()
	{
		FxStopNetworked(startEffectVictim);
		FxStopNetworked(fxStack1);
		FxStopNetworked(fxStack2);
		FxStopNetworked(fxStack3);
	}

	private void ChangeFxByStack()
	{
		switch (_currentStack)
		{
		case 1:
			FxPlayNetworked(fxStack1, victim);
			break;
		case 2:
			FxPlayNetworked(fxStack2, victim);
			break;
		case 3:
			FxPlayNetworked(fxStack3, victim);
			break;
		default:
			StopAllFx();
			break;
		}
	}

	private void MirrorProcessed()
	{
	}
}
