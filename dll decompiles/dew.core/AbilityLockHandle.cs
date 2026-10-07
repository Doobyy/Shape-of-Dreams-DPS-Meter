using UnityEngine;

public class AbilityLockHandle
{
	private const ulong AllMainSkills = 31uL;

	private const ulong AllActiveMainSkills = 15uL;

	internal EntityAbility _parent;

	internal ulong _castLockBitmap;

	internal ulong _editLockBitmap;

	public bool shouldShowLockEffect { get; internal set; }

	public AbilityLockHandle Stop()
	{
		if ((Object)(object)_parent == null)
		{
			return this;
		}
		_parent.UnlockAbility(this);
		return this;
	}

	public AbilityLockHandle LockAbilityCast(int index)
	{
		_castLockBitmap |= (ulong)(1L << index);
		if ((Object)(object)_parent != null)
		{
			_parent.CalculateAbilityLockBitmap();
		}
		return this;
	}

	public AbilityLockHandle UnlockAbilityCast(int index)
	{
		_castLockBitmap &= (ulong)(~(1L << index));
		if ((Object)(object)_parent != null)
		{
			_parent.CalculateAbilityLockBitmap();
		}
		return this;
	}

	public AbilityLockHandle LockAttackAbilityCast()
	{
		return LockAbilityCast(63);
	}

	public AbilityLockHandle UnlockAttackAbilityCast()
	{
		return UnlockAbilityCast(63);
	}

	public AbilityLockHandle LockAllAbilitiesCast()
	{
		_castLockBitmap = ulong.MaxValue;
		if ((Object)(object)_parent != null)
		{
			_parent.CalculateAbilityLockBitmap();
		}
		return this;
	}

	public AbilityLockHandle UnlockAllAbilitiesCast()
	{
		_castLockBitmap = 0uL;
		if ((Object)(object)_parent != null)
		{
			_parent.CalculateAbilityLockBitmap();
		}
		return this;
	}

	public AbilityLockHandle LockAllMainSkillsCast()
	{
		_castLockBitmap |= 31uL;
		if ((Object)(object)_parent != null)
		{
			_parent.CalculateAbilityLockBitmap();
		}
		return this;
	}

	public AbilityLockHandle UnlockAllMainSkillsCast()
	{
		_castLockBitmap &= 18446744073709551584uL;
		if ((Object)(object)_parent != null)
		{
			_parent.CalculateAbilityLockBitmap();
		}
		return this;
	}

	public AbilityLockHandle LockAllActiveMainSkillsCast()
	{
		_castLockBitmap |= 15uL;
		if ((Object)(object)_parent != null)
		{
			_parent.CalculateAbilityLockBitmap();
		}
		return this;
	}

	public AbilityLockHandle UnlockAllActiveMainSkillsCast()
	{
		_castLockBitmap &= 18446744073709551600uL;
		if ((Object)(object)_parent != null)
		{
			_parent.CalculateAbilityLockBitmap();
		}
		return this;
	}

	public AbilityLockHandle LockMovementSkillCast()
	{
		_castLockBitmap |= 32uL;
		if ((Object)(object)_parent != null)
		{
			_parent.CalculateAbilityLockBitmap();
		}
		return this;
	}

	public AbilityLockHandle UnlockMovementSkillCast()
	{
		_castLockBitmap &= 18446744073709551583uL;
		if ((Object)(object)_parent != null)
		{
			_parent.CalculateAbilityLockBitmap();
		}
		return this;
	}

	public AbilityLockHandle LockAbilityEdit(int index)
	{
		_editLockBitmap |= (ulong)(1L << index);
		if ((Object)(object)_parent != null)
		{
			_parent.CalculateAbilityLockBitmap();
		}
		return this;
	}

	public AbilityLockHandle UnlockAbilityEdit(int index)
	{
		_editLockBitmap &= (ulong)(~(1L << index));
		if ((Object)(object)_parent != null)
		{
			_parent.CalculateAbilityLockBitmap();
		}
		return this;
	}

	public AbilityLockHandle LockAllMainSkillsEdit()
	{
		_editLockBitmap |= 31uL;
		if ((Object)(object)_parent != null)
		{
			_parent.CalculateAbilityLockBitmap();
		}
		return this;
	}

	public AbilityLockHandle UnlockAllMainSkillsEdit()
	{
		_editLockBitmap &= 18446744073709551584uL;
		if ((Object)(object)_parent != null)
		{
			_parent.CalculateAbilityLockBitmap();
		}
		return this;
	}
}
