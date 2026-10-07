public abstract class HeroComponent : EntityComponent
{
	public Hero hero => (Hero)entity;

	private void MirrorProcessed()
	{
	}
}
