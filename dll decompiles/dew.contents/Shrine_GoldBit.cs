public class Shrine_GoldBit : Shrine, ICustomInteractable
{
	public string nameRawText => DewLocalization.GetUIValue("InGame_Currency_Gold");

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Tooltip_PickUp");

	protected override bool OnUse(Entity entity)
	{
		if (!(entity is Hero target))
		{
			return false;
		}
		NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, 3, position, target);
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
