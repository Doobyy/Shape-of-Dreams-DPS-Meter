using Mirror;

public class Ai_E_Aftershock_Damage : InstantDamageInstance
{
	public float stunDuration = 0.5f;

	private int _hitCount;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_hitCount = 0;
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (entity.IsAnyBoss())
		{
			_hitCount += 9999;
		}
		else
		{
			_hitCount++;
		}
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !info.caster.IsNullInactiveDeadOrKnockedOut() && _hitCount != 0)
		{
			Se_E_Aftershock_Armor effect;
			while (info.caster.Status.TryGetStatusEffect<Se_E_Aftershock_Armor>(out effect))
			{
				effect.Destroy();
			}
			CreateStatusEffect(info.caster, (Se_E_Aftershock_Armor se) =>
			{
				se._hitCount = _hitCount;
				se.gem = gem;
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
