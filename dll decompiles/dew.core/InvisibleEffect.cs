using UnityEngine;

public class InvisibleEffect : BasicEffect
{
	private bool _ignoreReveal;

	public override BasicEffectMask mask => BasicEffectMask.Invisible;

	public bool ignoreReveal
	{
		get
		{
			return _ignoreReveal;
		}
		set
		{
			_ignoreReveal = value;
			if ((Object)(object)victim != null)
			{
				victim.Status.DirtyStatusInfo();
			}
		}
	}
}
