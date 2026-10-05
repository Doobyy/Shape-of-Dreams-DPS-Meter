using System;
using UnityEngine;

[Serializable]
public class SkillBonus
{
	public bool ignoreReceiveCooldownReductionFlag;

	[SerializeField]
	private float _cooldownMultiplier = 1f;

	[SerializeField]
	private int _addedCharge;

	[SerializeField]
	private float _cooldownOffset;

	public float cooldownMultiplier
	{
		get
		{
			return _cooldownMultiplier;
		}
		set
		{
			_cooldownMultiplier = value;
			if ((UnityEngine.Object)(object)parent != null)
			{
				parent._isSkillBonusDirty = true;
			}
		}
	}

	public int addedCharge
	{
		get
		{
			return _addedCharge;
		}
		set
		{
			_addedCharge = value;
			if ((UnityEngine.Object)(object)parent != null)
			{
				parent._isSkillBonusDirty = true;
			}
		}
	}

	public float cooldownOffset
	{
		get
		{
			return _cooldownOffset;
		}
		set
		{
			_cooldownOffset = value;
			if ((UnityEngine.Object)(object)parent != null)
			{
				parent._isSkillBonusDirty = true;
			}
		}
	}

	public SkillTrigger parent { get; internal set; }

	public void Stop()
	{
		if (!((UnityEngine.Object)(object)parent == null))
		{
			parent.RemoveSkillBonus(this);
		}
	}

	public SkillBonus Clone()
	{
		SkillBonus skillBonus = (SkillBonus)MemberwiseClone();
		skillBonus.parent = null;
		return skillBonus;
	}
}
