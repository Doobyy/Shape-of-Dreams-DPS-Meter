public class Shrine_DreamdustBox : Shrine, ICustomInteractable
{
	public float amountMultiplier = 0.25f;

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Interact_Steal");

	public string nameRawText => DewLocalization.GetUIValue(((object)this).GetType().Name + "_Name");

	protected override bool OnUse(Entity entity)
	{
		int amount = DewMath.RandomRoundToInt(NetworkedManagerBase<GameManager>.instance.GetSpecialRewardAmount_DreamDust() * amountMultiplier);
		NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, amount, position, (Hero)entity);
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
