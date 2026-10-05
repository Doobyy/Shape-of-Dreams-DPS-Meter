public class Rift_Sidetrack_TheDream : Rift_Sidetrack
{
	protected override string GetConfirmMessage()
	{
		return DewLocalization.GetUIValue("InGame_Message_MeetTheBegining");
	}

	public override void TravelImmediately()
	{
		NetworkedManagerBase<ZoneManager>.instance.TravelToZone(DewResources.GetByName<Zone>("Zone_Primus"));
	}

	private void MirrorProcessed()
	{
	}
}
