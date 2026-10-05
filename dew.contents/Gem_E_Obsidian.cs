public class Gem_E_Obsidian : Gem
{
	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		ResetCooldown(owner.Ability.attackAbility);
		if (owner.Status.TryGetStatusEffect<Se_Gem_E_Obsidian_EmpowerAttack>(out var effect))
		{
			effect.Destroy();
		}
		CreateStatusEffectWithSource<Se_Gem_E_Obsidian_EmpowerAttack>(info.instance, owner, new CastInfo(owner));
	}

	private void MirrorProcessed()
	{
	}
}
