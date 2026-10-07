using System;
using System.Collections.Generic;

public class ReactionChainComparer : IEqualityComparer<ReactionChain>
{
	public bool Equals(ReactionChain x, ReactionChain y)
	{
		return x.Equals(y);
	}

	public int GetHashCode(ReactionChain obj)
	{
		int num = 0;
		if (obj._actors != null)
		{
			foreach (Actor actor in obj._actors)
			{
				num = HashCode.Combine<int, Actor>(num, actor);
			}
		}
		return num;
	}
}
