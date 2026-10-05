using UnityEngine;

public class St_D_RoaringFist : SkillTrigger
{
	public GameObject fxCharged;

	private int _lastCharges;

	protected override void OnEquip(Entity newOwner)
	{
	}

	protected override void OnUnequip(Entity formerOwner)
	{
	}

	protected override void ActiveLogicUpdate(float dt)
	{
	}

	private void EntityEventOnAttackFired(EventInfoAttackFired obj)
	{
	}

	private void MirrorProcessed()
	{
	}
}
