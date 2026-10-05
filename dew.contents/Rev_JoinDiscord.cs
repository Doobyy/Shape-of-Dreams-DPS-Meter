public class Rev_JoinDiscord : DewReverieItem
{
	public override int grantedStardust => 90;

	public override bool excludeFromPool => true;

	public override string reverieListButtonText => DewLocalization.GetUIValue("Rev_JoinDiscord_Action");

	public override void OnReverieListButtonClick()
	{
		base.OnReverieListButtonClick();
		Dew.OpenURL((DewSave.profileMain.language == "ko-KR") ? "https://discord.com/invite/Fp2rKwtmKZ" : "https://discord.gg/7B3mPaAwgn");
		Complete();
	}
}
