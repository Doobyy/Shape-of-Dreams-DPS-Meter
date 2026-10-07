public class Gem_C_Regeneration : Gem
{
	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		if (owner.isInCombat)
		{
			if (owner.Status.TryGetStatusEffect<Se_Gem_Regeneration>(out var effect))
			{
				effect.Destroy();
			}
			CreateStatusEffectWithSource<Se_Gem_Regeneration>(info.instance, owner, new CastInfo(owner));
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}
