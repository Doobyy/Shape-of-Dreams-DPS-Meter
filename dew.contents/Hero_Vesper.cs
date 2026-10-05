public class Hero_Vesper : Hero
{
	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		Vesper_LobbyHammer componentInChildren = Visual.model.GetComponentInChildren<Vesper_LobbyHammer>();
		if (componentInChildren != null)
		{
			componentInChildren.lobbyHammer.SetActive(value: false);
		}
	}

	private void MirrorProcessed()
	{
	}
}
