using UnityEngine;

public abstract class BasicEffectWithStrength : BasicEffect
{
	private float _strength;

	private bool _decay;

	public float strength
	{
		get
		{
			return _strength;
		}
		set
		{
			_strength = value;
			if ((Object)(object)victim != null)
			{
				victim.Status.DirtyStatusInfo();
			}
		}
	}

	public bool decay
	{
		get
		{
			return _decay;
		}
		set
		{
			_decay = value;
			if ((Object)(object)victim != null)
			{
				victim.Status.DirtyStatusInfo();
			}
		}
	}
}
