using System;
using UnityEngine;

public abstract class DewHeroStarItemOld : DewStarItemOld
{
	public abstract Type heroType { get; }

	public override bool ShouldInitInGame()
	{
		if ((UnityEngine.Object)(object)hero != null)
		{
			return ((object)hero).GetType() == heroType;
		}
		return false;
	}
}
