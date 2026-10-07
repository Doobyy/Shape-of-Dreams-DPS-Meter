public class Gem_R_Composure : Gem
{
	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (!oldOwner.IsNullOrInactive() && oldOwner.Status.TryGetStatusEffect<Se_R_Composure_Buff>(out var effect))
		{
			effect.Destroy();
		}
	}

	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		if (isValid)
		{
			if (owner.Status.TryGetStatusEffect<Se_R_Composure_Buff>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_R_Composure_Buff>(owner, new CastInfo(owner));
			}
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}
