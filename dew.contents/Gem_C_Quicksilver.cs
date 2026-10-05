public class Gem_C_Quicksilver : Gem
{
	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		if (owner.Status.TryGetStatusEffect<Se_Gem_C_Quicksilver>(out var effect))
		{
			effect.ResetTimer();
		}
		else
		{
			CreateStatusEffect<Se_Gem_C_Quicksilver>(owner, new CastInfo(owner));
		}
		NotifyUse();
	}

	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		if (info.damage.elemental == ElementalType.Fire)
		{
			if (owner.Status.TryGetStatusEffect<Se_Gem_C_Quicksilver>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_Gem_C_Quicksilver>(owner, new CastInfo(owner));
			}
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}
