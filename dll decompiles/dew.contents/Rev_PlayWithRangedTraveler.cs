public class Rev_PlayWithRangedTraveler : DewReverieItem
{
	public override int grantedStardust => 40;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (Dew.IsRangedHero(hero.classType))
		{
			Complete();
		}
	}
}
