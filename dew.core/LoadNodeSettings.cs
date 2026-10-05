public class LoadNodeSettings
{
	public int from = -1;

	public int to;

	public bool advanceTurn = true;

	public bool isSidetrackTransition;

	public Zone newZone;

	public bool newZoneNoAdvance;

	public bool isLoadingFromSave;

	public bool isTravelingRoom;

	public bool isTravelingZone;

	public bool isWhiteTransition;

	public bool dontDoRiftTransition;

	public bool dontStopLoading;

	public LoadNodeSettings Clone()
	{
		return (LoadNodeSettings)MemberwiseClone();
	}
}
