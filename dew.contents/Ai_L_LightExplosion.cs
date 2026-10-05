using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Ai_L_LightExplosion : AbilityInstance
{
	public GameObject fxTelegraph;

	public float explodeDelay = 1.5f;

	public float gracePeriod = 0.2f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		List<Entity> affected = new List<Entity>();
		do
		{
			if (info.target.IsNullOrInactive())
			{
				FxPlayNetworked(fxTelegraph);
			}
			else
			{
				FxPlayNetworked(fxTelegraph, info.target);
			}
			yield return new SI.WaitForSeconds(explodeDelay);
			affected.Clear();
			CreateAbilityInstance(position, null, new CastInfo(info.caster), (Ai_L_LightExplosion_Instance ai) =>
			{
				ai.ActorEvent_OnDealDamage += (Action<EventInfoDamage>)((EventInfoDamage dmg) =>
				{
					affected.Add(dmg.victim);
				});
			});
			FxStopNetworked(fxTelegraph);
			yield return new SI.WaitForSeconds(gracePeriod);
		}
		while (affected.Count != 0 && !affected.All((Entity e) => !e.IsNullOrInactive()));
		Destroy();
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!info.target.IsNullOrInactive())
		{
			position = info.target.agentPosition;
		}
	}

	private void MirrorProcessed()
	{
	}
}
