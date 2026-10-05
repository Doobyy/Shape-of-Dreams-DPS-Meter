using System;

public class Ai_Gem_U_GuidingCompass_PowerGainProjectile : StandardProjectile
{
	[NonSerialized]
	public Gem targetGem;

	[NonSerialized]
	public int qualityIncrease;

	protected override void OnComplete()
	{
		base.OnComplete();
		if (!targetGem.IsNullOrInactive())
		{
			targetGem.quality += qualityIncrease;
		}
	}

	private void MirrorProcessed()
	{
	}
}
