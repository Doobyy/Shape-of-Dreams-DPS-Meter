using UnityEngine;

public class Pickup_RegenOrb : PickupInstance
{
	public DewCollider range;

	public override bool reuseInRoom => true;

	protected override void OnPickup(Hero hero)
	{
		base.OnPickup(hero);
		ListReturnHandle<Entity> handle;
		foreach (Entity entity in range.GetEntities(out handle, (Entity ent) =>
		{
			EntityRelation relation = hero.GetRelation(ent);
			if (ent is Hero { isKnockedOut: not false })
			{
				return false;
			}
			return relation == EntityRelation.Ally || relation == EntityRelation.Self;
		}, new CollisionCheckSettings
		{
			includeUncollidable = true
		}))
		{
			CreateAbilityInstance<Ai_RegenOrb_Projectile>(position, Quaternion.identity, new CastInfo(hero, entity));
		}
		handle.Return();
	}

	protected override bool CanBeUsedBy(Hero hero)
	{
		return base.CanBeUsedBy(hero);
	}

	private void MirrorProcessed()
	{
	}
}
