using System;
using UnityEngine;

[Serializable]
public struct Cost : IEquatable<Cost>
{
	public int gold;

	public int dreamDust;

	public int stardust;

	public int healthPercentage;

	public int platinumCoin;

	public AffordType CanAfford(Entity entity, int playerStardust = -1)
	{
		if (entity.owner.gold < gold)
		{
			return AffordType.NoGold;
		}
		if (entity.currentHealth / entity.maxHealth < (float)healthPercentage / 100f)
		{
			return AffordType.NoHealth;
		}
		if (entity.owner.dreamDust < dreamDust)
		{
			return AffordType.NoDreamDust;
		}
		if (playerStardust >= 0 && playerStardust < stardust)
		{
			return AffordType.NoStardust;
		}
		if (entity.owner.platinumCoin < platinumCoin)
		{
			return AffordType.NoPlatinumCoin;
		}
		return AffordType.Yes;
	}

	public AffordType CanAfford(DewPlayer player, int playerStardust = -1)
	{
		return CanAfford(player.hero, playerStardust);
	}

	public static Cost Gold(int amount)
	{
		return new Cost
		{
			gold = amount
		};
	}

	public static Cost DreamDust(int amount)
	{
		return new Cost
		{
			dreamDust = amount
		};
	}

	public static Cost Stardust(int amount)
	{
		return new Cost
		{
			stardust = amount
		};
	}

	public static Cost HealthPercentage(int amount)
	{
		return new Cost
		{
			healthPercentage = amount
		};
	}

	public static Cost Platinum(int amount)
	{
		return new Cost
		{
			platinumCoin = amount
		};
	}

	public override string ToString()
	{
		if (gold == 0 && dreamDust == 0 && stardust == 0 && healthPercentage == 0 && platinumCoin == 0)
		{
			return DewLocalization.GetUIValue("Generic_Free_NoCost");
		}
		string text = "";
		if (gold != 0)
		{
			if (text != "")
			{
				text += ", ";
			}
			text += string.Format(DewLocalization.GetUIValue("Currency_Template_Gold"), Mathf.Abs(gold).ToString("#,##0"));
		}
		if (dreamDust != 0)
		{
			if (text != "")
			{
				text += ", ";
			}
			text += string.Format(DewLocalization.GetUIValue("Currency_Template_DreamDust"), Mathf.Abs(dreamDust).ToString("#,##0"));
		}
		if (stardust != 0)
		{
			if (text != "")
			{
				text += ", ";
			}
			text += string.Format(DewLocalization.GetUIValue("Currency_Template_Stardust"), Mathf.Abs(stardust).ToString("#,##0"));
		}
		if (healthPercentage != 0)
		{
			if (text != "")
			{
				text += ", ";
			}
			text += string.Format(DewLocalization.GetUIValue("Currency_Template_HealthPercentage"), Mathf.Abs(healthPercentage).ToString("#,##0"));
		}
		if (platinumCoin != 0)
		{
			if (text != "")
			{
				text += ", ";
			}
			text += string.Format(DewLocalization.GetUIValue("Currency_Template_PlatinumCoin"), Mathf.Abs(platinumCoin).ToString("#,##0"));
		}
		return text;
	}

	public static Cost operator +(Cost a, Cost b)
	{
		return new Cost
		{
			gold = a.gold + b.gold,
			dreamDust = a.dreamDust + b.dreamDust,
			stardust = a.stardust + b.stardust,
			healthPercentage = a.healthPercentage + b.healthPercentage,
			platinumCoin = a.platinumCoin + b.platinumCoin
		};
	}

	public static FloatCost operator +(Cost a, FloatCost b)
	{
		return new FloatCost
		{
			gold = (float)a.gold + b.gold,
			dreamDust = (float)a.dreamDust + b.dreamDust,
			stardust = (float)a.stardust + b.stardust,
			healthPercentage = (float)a.healthPercentage + b.healthPercentage,
			platinumCoin = (float)a.platinumCoin + b.platinumCoin
		};
	}

	public static Cost operator -(Cost a, Cost b)
	{
		return new Cost
		{
			gold = a.gold - b.gold,
			dreamDust = a.dreamDust - b.dreamDust,
			stardust = a.stardust - b.stardust,
			healthPercentage = a.healthPercentage - b.healthPercentage,
			platinumCoin = a.platinumCoin - b.platinumCoin
		};
	}

	public static FloatCost operator -(Cost a, FloatCost b)
	{
		return new FloatCost
		{
			gold = (float)a.gold - b.gold,
			dreamDust = (float)a.dreamDust - b.dreamDust,
			stardust = (float)a.stardust - b.stardust,
			healthPercentage = (float)a.healthPercentage - b.healthPercentage,
			platinumCoin = (float)a.platinumCoin - b.platinumCoin
		};
	}

	public static Cost operator *(Cost a, int scalar)
	{
		return new Cost
		{
			gold = a.gold * scalar,
			dreamDust = a.dreamDust * scalar,
			stardust = a.stardust * scalar,
			healthPercentage = a.healthPercentage * scalar,
			platinumCoin = a.platinumCoin * scalar
		};
	}

	public static Cost operator *(int scalar, Cost a)
	{
		return a * scalar;
	}

	public static FloatCost operator *(Cost a, float scalar)
	{
		return new FloatCost
		{
			gold = (float)a.gold * scalar,
			dreamDust = (float)a.dreamDust * scalar,
			stardust = (float)a.stardust * scalar,
			healthPercentage = (float)a.healthPercentage * scalar,
			platinumCoin = (float)a.platinumCoin * scalar
		};
	}

	public static FloatCost operator *(float scalar, Cost a)
	{
		return a * scalar;
	}

	public static FloatCost operator /(Cost a, float scalar)
	{
		return new FloatCost
		{
			gold = (float)a.gold / scalar,
			dreamDust = (float)a.dreamDust / scalar,
			stardust = (float)a.stardust / scalar,
			healthPercentage = (float)a.healthPercentage / scalar,
			platinumCoin = (float)a.platinumCoin / scalar
		};
	}

	public static Cost operator /(Cost a, int scalar)
	{
		return new Cost
		{
			gold = a.gold / scalar,
			dreamDust = a.dreamDust / scalar,
			stardust = a.stardust / scalar,
			healthPercentage = a.healthPercentage / scalar,
			platinumCoin = a.platinumCoin / scalar
		};
	}

	public static Cost operator -(Cost a)
	{
		return new Cost
		{
			gold = -a.gold,
			dreamDust = -a.dreamDust,
			stardust = -a.stardust,
			healthPercentage = -a.healthPercentage,
			platinumCoin = -a.platinumCoin
		};
	}

	public Cost MultiplyGold(float scalar)
	{
		Cost result = this;
		result.gold = Mathf.RoundToInt((float)gold * scalar);
		return result;
	}

	public bool Equals(Cost other)
	{
		if (gold == other.gold && dreamDust == other.dreamDust && stardust == other.stardust && healthPercentage == other.healthPercentage)
		{
			return platinumCoin == other.platinumCoin;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is Cost other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<int, int, int, int, int>(gold, dreamDust, stardust, healthPercentage, platinumCoin);
	}

	public static bool operator ==(Cost left, Cost right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Cost left, Cost right)
	{
		return !left.Equals(right);
	}
}
