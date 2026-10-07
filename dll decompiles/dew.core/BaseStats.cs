using System;

[Serializable]
public struct BaseStats : IEquatable<BaseStats>
{
	public static readonly BaseStats Default = new BaseStats
	{
		attackDamage = 60f,
		abilityPower = 80f,
		maxHealth = 100f,
		maxMana = 100f,
		healthRegen = 0f,
		manaRegen = 0f,
		critAmp = 0f,
		critChance = 0f,
		abilityHaste = 0f,
		fireEffectAmp = 0f,
		coldEffectAmp = 0f,
		lightEffectAmp = 0f,
		darkEffectAmp = 0f,
		armor = 0f
	};

	public float attackDamage;

	public float abilityPower;

	public float maxHealth;

	public float maxMana;

	public float healthRegen;

	public float manaRegen;

	public float critAmp;

	public float critChance;

	public float abilityHaste;

	public float tenacity;

	public float fireEffectAmp;

	public float coldEffectAmp;

	public float lightEffectAmp;

	public float darkEffectAmp;

	public float armor;

	public bool Equals(BaseStats other)
	{
		if (attackDamage.Equals(other.attackDamage) && abilityPower.Equals(other.abilityPower) && maxHealth.Equals(other.maxHealth) && maxMana.Equals(other.maxMana) && healthRegen.Equals(other.healthRegen) && manaRegen.Equals(other.manaRegen) && critAmp.Equals(other.critAmp) && critChance.Equals(other.critChance) && abilityHaste.Equals(other.abilityHaste) && tenacity.Equals(other.tenacity) && fireEffectAmp.Equals(other.fireEffectAmp) && coldEffectAmp.Equals(other.coldEffectAmp) && lightEffectAmp.Equals(other.lightEffectAmp) && darkEffectAmp.Equals(other.darkEffectAmp))
		{
			return armor.Equals(other.armor);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BaseStats other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		HashCode val = default;
		val.Add<float>(attackDamage);
		val.Add<float>(abilityPower);
		val.Add<float>(maxHealth);
		val.Add<float>(maxMana);
		val.Add<float>(healthRegen);
		val.Add<float>(manaRegen);
		val.Add<float>(critAmp);
		val.Add<float>(critChance);
		val.Add<float>(abilityHaste);
		val.Add<float>(tenacity);
		val.Add<float>(fireEffectAmp);
		val.Add<float>(coldEffectAmp);
		val.Add<float>(lightEffectAmp);
		val.Add<float>(darkEffectAmp);
		val.Add<float>(armor);
		return val.ToHashCode();
	}

	public static bool operator ==(BaseStats left, BaseStats right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BaseStats left, BaseStats right)
	{
		return !left.Equals(right);
	}
}
