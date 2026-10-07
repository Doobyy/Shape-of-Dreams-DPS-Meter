public class Rev_Special_Founders : DewSpecialReverieItem
{
	[AchPersistentVar]
	private int _killCount;

	public override int grantedStardust => 0;

	public override bool excludeFromPool => true;

	public override string[] grantedItems => new string[1] { "Nametag_ShapeOfDreams_ShapeOfDreams" };

	public override int GetCurrentProgress()
	{
		return _killCount;
	}

	public override int GetMaxProgress()
	{
		return 8;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnKillOrAssist((EventInfoKill k) =>
		{
			if (k.victim is BossMonster)
			{
				_killCount++;
				if (_killCount >= GetMaxProgress())
				{
					Complete();
				}
			}
		});
	}
}
