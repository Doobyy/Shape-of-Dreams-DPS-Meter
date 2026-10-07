using System;
using Mirror;
using UnityEngine;

public class Se_HeroOneShotProtection : StatusEffect
{
	public float activeHpRatio = 0.8f;

	public float protectionDuration = 0.5f;

	[SaveVar(SaveVarFlags.Default)]
	private bool _canBeProtected;

	private float _protectionExpirationTime;

	private int _disableProtectionCounter;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			if (isNewInstance)
			{
				_canBeProtected = true;
			}
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (!(Time.time > _protectionExpirationTime) && !obj.damage.HasAttr(DamageAttribute.IgnoreDamageImmunity))
		{
			float num = obj.victim.maxHealth * 0.15f;
			if (obj.victim.currentHealth <= num)
			{
				obj.victim.Status.SetHealth(num);
				_canBeProtected = false;
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && _canBeProtected && victim.normalizedHealth > activeHpRatio && _disableProtectionCounter <= 0)
		{
			_protectionExpirationTime = Time.time + protectionDuration;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			}
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			}
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		if (obj.isTraveling)
		{
			_canBeProtected = true;
		}
	}

	[Server]
	public void DisableProtection()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_HeroOneShotProtection::DisableProtection()' called when server was not active");
		}
		else
		{
			_disableProtectionCounter++;
		}
	}

	[Server]
	public void ReenableProtection()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_HeroOneShotProtection::ReenableProtection()' called when server was not active");
		}
		else
		{
			_disableProtectionCounter--;
		}
	}

	private void MirrorProcessed()
	{
	}
}
