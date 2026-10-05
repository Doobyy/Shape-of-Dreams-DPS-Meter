public class Gem_C_Wind : Gem
{
	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		if (owner.Status.TryGetStatusEffect<Se_Gem_C_Wind>(out var effect))
		{
			effect.Destroy();
		}
		CreateStatusEffectWithSource<Se_Gem_C_Wind>(info.instance, owner, new CastInfo(owner));
		NotifyUse();
	}

	private void MirrorProcessed()
	{
	}
}
