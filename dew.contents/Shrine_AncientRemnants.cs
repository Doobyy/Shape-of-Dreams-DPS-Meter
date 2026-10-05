[DewResourceLink(ResourceLinkBy.None)]
public class Shrine_AncientRemnants : Shrine, ICustomInteractable
{
	public string nameRawText => null;

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Interact_ShrineActivate");

	protected override bool OnUse(Entity entity)
	{
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
