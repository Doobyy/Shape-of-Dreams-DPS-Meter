public class Rev_PlayThreeTimes : DewReverieItem
{
	[AchPersistentVar]
	private int _playCount;

	public override int grantedStardust => 60;

	public override int GetMaxProgress()
	{
		return 3;
	}

	public override int GetCurrentProgress()
	{
		return _playCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		_playCount++;
		if (_playCount >= 3)
		{
			Complete();
		}
	}
}
