public class Gem_E_Protection : Gem
{
	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		if (IsReady())
		{
			if (owner.Status.TryGetStatusEffect<Se_Gem_E_Protection>(out var effect))
			{
				effect.Destroy();
			}
			CreateStatusEffectWithSource<Se_Gem_E_Protection>(info.instance, owner, new CastInfo(owner));
			StartCooldown();
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}
