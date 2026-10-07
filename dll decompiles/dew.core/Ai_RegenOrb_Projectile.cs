using System;
using UnityEngine;

public class Ai_RegenOrb_Projectile : StandardProjectile
{
	public float healRatio = 0.15f;

	public int ticks = 6;

	public float ticksInterval;

	public SafeAction<Entity> actionOverride;

	public override bool reuseInRoom => true;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (actionOverride == null || actionOverride.Count == 0)
		{
			CreateStatusEffect(hit.entity, (Se_GenericHealOverTime h) =>
			{
				h.Setup(healRatio * hit.entity.maxHealth, ticksInterval, ticks);
			});
		}
		else
		{
			try
			{
				actionOverride?.Invoke(hit.entity);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		Destroy();
	}

	public override void ClearPooledEventsAndProcessors()
	{
		base.ClearPooledEventsAndProcessors();
		actionOverride?.Clear();
	}

	private void MirrorProcessed()
	{
	}
}
