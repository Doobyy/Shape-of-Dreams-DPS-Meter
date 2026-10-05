using Mirror;
using UnityEngine;

public class Sum_R_AnnoyingBanner : Summon
{
	public float tickInterval = 1.5f;

	public ScalingValue tickDamage;

	public float pulseRadius = 3f;

	public GameObject fxTick;

	public GameObject fxHit;

	public GameObject fxSummoner;

	private float _nextTickTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(fxSummoner, owner.hero);
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
			_nextTickTime = Time.time + tickInterval * 0.25f;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			if (Time.time - creationTime >= maxDuration)
			{
				Kill();
			}
			else if (Time.time >= _nextTickTime)
			{
				_nextTickTime += tickInterval;
				Pulse();
			}
		}
	}

	private void Pulse()
	{
		if (fxTick != null)
		{
			FxPlayNewNetworked(fxTick, this);
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, agentPosition, pulseRadius, tvDefaultHarmfulEffectTargets))
		{
			if (item is Monster && !item.IsNullInactiveDeadOrKnockedOut())
			{
				FxPlayNewNetworked(fxHit, item);
				Damage(tickDamage).Dispatch(item);
				if (!(item.AI.context.targetEnemy is Sum_R_AnnoyingBanner))
				{
					item.Control.ClearActionQueue();
					item.Control.Stop();
					item.AI.Aggro(this);
				}
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
