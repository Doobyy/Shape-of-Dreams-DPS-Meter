using System;
using Mirror;
using UnityEngine;

public class Se_MirageSkin_Sanctification_Protected : StatusEffect
{
	public DewBeamRenderer beam;

	public float armorAmount;

	public float unleashDist = 6f;

	public GameObject fxTakeDamage;

	private float _lastTakeDamageTime;

	private Action<EventInfoDamage> _cachedOnTakeDamage;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		UpdateBeamPositions();
		beam.enabled = true;
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(1f);
			DoProtected(null);
			DoArmorBoost(armorAmount);
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			DestroyOnDestroy(parentActor);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (!obj.damage.HasAttr(DamageAttribute.DamageOverTime) && !(Time.time - _lastTakeDamageTime < 0.3f))
		{
			_lastTakeDamageTime = Time.time;
			FxPlayNewNetworked(fxTakeDamage, victim);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		UpdateBeamPositions();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Vector2.Distance(info.caster.agentPosition.ToXY(), victim.agentPosition.ToXY()) > unleashDist)
		{
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		beam.enabled = false;
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnTakeDamage -= _cachedOnTakeDamage;
		}
	}

	private void UpdateBeamPositions()
	{
		beam.SetPoints(victim.Visual.GetCenterPosition(), info.caster.Visual.GetCenterPosition());
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_lastTakeDamageTime = 0f;
	}

	private void MirrorProcessed()
	{
	}
}
