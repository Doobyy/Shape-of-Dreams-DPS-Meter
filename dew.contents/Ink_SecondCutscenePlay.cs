using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ink_SecondCutscenePlay : DewNetworkBehaviour
{
	public DewCutsceneDirector secondDirector;

	public float startDelay;

	private bool _firstTime = true;

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (Ge_Shrine_Anitya.IsDoubleSpawn())
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += new Action<Entity>(OnEntityAdd);
		}
	}

	private void OnEntityAdd(Entity entity)
	{
		if (entity is Mon_Ink_BossWhiteNight && !((Mon_Ink_BossWhiteNight)entity)._isSolo)
		{
			entity.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	private void OnTakeDamage(EventInfoDamage obj)
	{
		if (obj.victim is Mon_Ink_BossWhiteNight && obj.victim.normalizedHealth <= 0.5f)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(startDelay);
			secondDirector.PlayNetworked();
			obj.victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(OnTakeDamage);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (((NetworkBehaviour)this).isServer && Ge_Shrine_Anitya.IsDoubleSpawn())
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd -= new Action<Entity>(OnEntityAdd);
		}
	}

	private void MirrorProcessed()
	{
	}
}
