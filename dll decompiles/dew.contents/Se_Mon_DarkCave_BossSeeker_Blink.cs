using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Mon_DarkCave_BossSeeker_Blink : StatusEffect
{
	public bool resetAtkCooldown = true;

	public float duration;

	public DewEase ease;

	[NonSerialized]
	public Vector3? customDestination;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		SetTimer(duration);
		DoUnstoppable();
		DoUncollidable();
		DoUntargetable();
		DoInvisible(ignoreReveal: true);
		victim.Control.StartDaze(duration);
		victim.Visual.DisableRenderers();
		Vector3 destination = info.caster.agentPosition;
		if (customDestination.HasValue)
		{
			destination = customDestination.Value;
			destination = Dew.GetPositionOnGround(destination);
			destination = Dew.GetValidAgentPosition(destination);
		}
		else if (DarkCave_BlinkPosition.instances.Count > 1)
		{
			int num = Dew.SelectBestIndexWithScore((IList<DarkCave_BlinkPosition>)DarkCave_BlinkPosition.instances, (Func<DarkCave_BlinkPosition, int, float>)((DarkCave_BlinkPosition bp, int _) => 0f - Vector2.Distance(bp.transform.position.ToXY(), info.caster.agentPosition.ToXY())), 0f, (DewRandom)null);
			List<int> list = DewPool.GetList(out ListReturnHandle<int> handle);
			for (int num2 = 0; num2 < DarkCave_BlinkPosition.instances.Count; num2++)
			{
				if (num2 != num)
				{
					list.Add(num2);
				}
			}
			destination = DarkCave_BlinkPosition.instances[list[UnityEngine.Random.Range(0, list.Count)]].transform.position;
			destination = Dew.GetPositionOnGround(destination);
			destination = Dew.GetValidAgentPosition(destination);
			handle.Return();
		}
		victim.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = false,
			canGoOverTerrain = true,
			destination = destination,
			duration = duration,
			ease = ease,
			isCanceledByCC = false,
			isFriendly = true
		});
		if (info.caster is Mon_DarkCave_BossSeeker mon_DarkCave_BossSeeker)
		{
			mon_DarkCave_BossSeeker.UpdateBlinkHealth();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		customDestination = null;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer || !((UnityEngine.Object)(object)victim != null))
		{
			return;
		}
		victim.Visual.EnableRenderers();
		if (victim.isAlive)
		{
			if (resetAtkCooldown)
			{
				ResetCooldown(victim.Ability.attackAbility);
			}
			Hero closestAliveHero = Dew.GetClosestAliveHero(victim.position, fallbackToDead: false, victim);
			if ((UnityEngine.Object)(object)closestAliveHero != null)
			{
				victim.AI.Aggro(closestAliveHero);
				victim.Control.RotateTowards(closestAliveHero, immediately: false);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
