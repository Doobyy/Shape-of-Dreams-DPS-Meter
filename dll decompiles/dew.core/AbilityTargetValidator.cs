using System;
using UnityEngine;

[Serializable]
public class AbilityTargetValidator : IBinaryEntityValidator
{
	public EntityRelation targets = EntityRelation.Neutral | EntityRelation.Enemy;

	public bool Evaluate(Entity self, Entity target)
	{
		if ((UnityEngine.Object)(object)target == null)
		{
			return false;
		}
		if ((UnityEngine.Object)(object)self == null)
		{
			return false;
		}
		EntityRelation relation = self.GetRelation(target);
		if (target.isActive)
		{
			return (relation & targets) == relation;
		}
		return false;
	}

	bool IBinaryEntityValidator.Evaluate(Entity self, Entity target)
	{
		return Evaluate(self, target);
	}
}
