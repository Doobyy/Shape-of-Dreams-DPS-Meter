using System;

public struct VariantDef : IEquatable<VariantDef>
{
	public int id0;

	public int id1;

	public int id2;

	public int id3;

	public int id4;

	public int id5;

	public VariantDef Add(int id)
	{
		if (id <= 0)
		{
			throw new InvalidOperationException("Invalid variant id");
		}
		VariantDef result = this;
		if (result.id0 == 0)
		{
			result.id0 = id;
		}
		else if (result.id1 == 0)
		{
			result.id1 = id;
		}
		else if (result.id2 == 0)
		{
			result.id2 = id;
		}
		else if (result.id3 == 0)
		{
			result.id3 = id;
		}
		else if (result.id4 == 0)
		{
			result.id4 = id;
		}
		else
		{
			if (result.id5 != 0)
			{
				throw new InvalidOperationException("Cannot add more than 6 variant");
			}
			result.id5 = id;
		}
		return result;
	}

	public bool Contains(int id)
	{
		if (id <= 0)
		{
			throw new InvalidOperationException("Invalid variant id");
		}
		if (id0 != id && id1 != id && id2 != id && id3 != id && id4 != id)
		{
			return id5 == id;
		}
		return true;
	}

	public static implicit operator VariantDef(int id)
	{
		return new VariantDef
		{
			id0 = id
		};
	}

	public bool Equals(VariantDef other)
	{
		if (id0 == other.id0 && id1 == other.id1 && id2 == other.id2 && id3 == other.id3 && id4 == other.id4)
		{
			return id5 == other.id5;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is VariantDef other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<int, int, int, int, int, int>(id0, id1, id2, id3, id4, id5);
	}

	public static bool operator ==(VariantDef left, VariantDef right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(VariantDef left, VariantDef right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return $"VariantDef({id0}, {id1}, {id2}, {id3}, {id4}, {id5})";
	}
}
