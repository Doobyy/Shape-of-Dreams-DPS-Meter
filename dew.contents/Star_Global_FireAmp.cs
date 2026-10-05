public class Star_Global_FireAmp : DewStarItemOld
{
	public static float[] FireAmp = new float[5] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

	public override int maxLevel => 5;

	public override bool ShouldInitInGame()
	{
		return isServer;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
		hero.Status.AddStatBonus(new StatBonus
		{
			fireEffectAmpFlat = FireAmp.GetClamped(level - 1)
		});
	}
}
