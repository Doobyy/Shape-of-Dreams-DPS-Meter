public class Gem_R_Insatiable : Gem
{
	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		if (owner.Status.TryGetStatusEffect<Se_Gem_R_Insatiable>(out var effect))
		{
			effect.Destroy();
		}
		CreateStatusEffectWithSource<Se_Gem_R_Insatiable>(info.instance, owner, default);
	}

	private void MirrorProcessed()
	{
	}
}
