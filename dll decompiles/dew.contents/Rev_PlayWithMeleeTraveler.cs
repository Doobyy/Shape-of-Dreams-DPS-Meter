public class Rev_PlayWithMeleeTraveler : DewReverieItem
{
	public override int grantedStardust => 40;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (Dew.IsMeleeHero(hero.classType))
		{
			Complete();
		}
	}
}
