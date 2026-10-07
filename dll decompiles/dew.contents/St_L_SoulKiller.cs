using System.Collections.Generic;

public class St_L_SoulKiller : SkillTrigger
{
	public int maxSoulCount;

	public ScalingValue baseDamage;

	private List<Ai_L_SoulKiller_Soul> _soulsList;

	protected override void OnEquip(Entity newOwner)
	{
	}

	protected override void OnUnequip(Entity formerOwner)
	{
	}

	public override void OnCastCompleteBeforePrepare(EventInfoCast cast)
	{
	}

	private void OnAnyEntityDeath(EventInfoKill obj)
	{
	}

	private void MirrorProcessed()
	{
	}
}
