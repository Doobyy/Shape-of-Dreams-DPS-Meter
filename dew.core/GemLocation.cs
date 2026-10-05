using System;

public struct GemLocation(HeroSkillLocation skill, int index) : IEquatable<GemLocation>
{
	public HeroSkillLocation skill = skill;

	public int index = index;

	public bool Equals(GemLocation other)
	{
		if (skill == other.skill)
		{
			return index == other.index;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is GemLocation other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<int, int>((int)skill, index);
	}

	public static bool operator ==(GemLocation c1, GemLocation c2)
	{
		return c1.Equals(c2);
	}

	public static bool operator !=(GemLocation c1, GemLocation c2)
	{
		return !c1.Equals(c2);
	}
}
