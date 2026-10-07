public class Shrine_BlessedGuidance : Shrine_Guidance
{
	public override void OnHealEntity(Entity e)
	{
		base.OnHealEntity(e);
		e.Status.SetHealth(e.Status.maxHealth);
	}

	private void MirrorProcessed()
	{
	}
}
