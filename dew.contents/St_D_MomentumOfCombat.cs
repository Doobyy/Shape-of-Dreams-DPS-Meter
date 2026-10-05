using UnityEngine;

public class St_D_MomentumOfCombat : SkillTrigger
{
	public bool isSpeedDecay;

	public float speedDuration;

	public ScalingValue speedAmount;

	public GameObject fxSpeedEffect;

	public GameObject[] fxChargedEffect;

	protected override void OnEquip(Entity newOwner)
	{
	}

	protected override void OnUnequip(Entity formerOwner)
	{
	}

	private void CheckCritical(EventInfoAttackFired obj)
	{
	}

	private void MirrorProcessed()
	{
	}
}
