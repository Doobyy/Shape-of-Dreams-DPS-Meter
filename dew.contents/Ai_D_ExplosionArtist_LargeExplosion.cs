using Mirror;

public class Ai_D_ExplosionArtist_LargeExplosion : InstantDamageInstance
{
	private St_D_ExplosionArtist _parent;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_parent = firstTrigger as St_D_ExplosionArtist;
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (((NetworkBehaviour)this).isServer)
		{
			_parent.HandleHit(this, entity);
		}
	}

	private void MirrorProcessed()
	{
	}
}
