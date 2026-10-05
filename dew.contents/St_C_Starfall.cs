using UnityEngine;

public class St_C_Starfall : SkillTrigger
{
	public AbilityTargetValidator hittable;

	public float radius;

	public override bool CanBeCast()
	{
		if (base.CanBeCast())
		{
			return Check();
		}
		return false;
	}

	public override bool CanBeReserved()
	{
		if (base.CanBeReserved())
		{
			return Check();
		}
		return false;
	}

	private bool Check()
	{
		if ((Object)(object)owner == null)
		{
			return false;
		}
		if (DewPhysics.OverlapCircleAllEntities(out var handle, owner.agentPosition, radius, hittable, owner).Count == 0)
		{
			handle.Return();
			return false;
		}
		handle.Return();
		return true;
	}

	private void MirrorProcessed()
	{
	}
}
