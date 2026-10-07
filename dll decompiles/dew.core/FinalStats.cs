using System;

[Serializable]
public struct FinalStats : IEquatable<FinalStats>
{
	public float attackDamage;

	public float abilityPower;

	public float maxHealth;

	public float maxHealthWithoutBonus;

	public float maxMana;

	public float healthRegen;

	public float manaRegen;

	public float attackSpeedMultiplier;

	public float critAmp;

	public float critChance;

	public float tenacity;

	public float abilityHaste;

	public float movementSpeedMultiplier;

	public float fireEffectAmp;

	public float coldEffectAmp;

	public float lightEffectAmp;

	public float darkEffectAmp;

	public float armor;

	public bool Equals(FinalStats other)
	{
		if (attackDamage.Equals(other.attackDamage) && abilityPower.Equals(other.abilityPower) && maxHealth.Equals(other.maxHealth) && maxMana.Equals(other.maxMana) && healthRegen.Equals(other.healthRegen) && manaRegen.Equals(other.manaRegen) && attackSpeedMultiplier.Equals(other.attackSpeedMultiplier) && critAmp.Equals(other.critAmp) && critChance.Equals(other.critChance) && tenacity.Equals(other.tenacity) && abilityHaste.Equals(other.abilityHaste) && movementSpeedMultiplier.Equals(other.movementSpeedMultiplier) && fireEffectAmp.Equals(other.fireEffectAmp) && coldEffectAmp.Equals(other.coldEffectAmp) && lightEffectAmp.Equals(other.lightEffectAmp) && darkEffectAmp.Equals(other.darkEffectAmp))
		{
			return armor.Equals(other.armor);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is FinalStats other)
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
		val.Add<float>(attackSpeedMultiplier);
		val.Add<float>(critAmp);
		val.Add<float>(critChance);
		val.Add<float>(tenacity);
		val.Add<float>(abilityHaste);
		val.Add<float>(movementSpeedMultiplier);
		val.Add<float>(fireEffectAmp);
		val.Add<float>(coldEffectAmp);
		val.Add<float>(lightEffectAmp);
		val.Add<float>(darkEffectAmp);
		val.Add<float>(armor);
		return val.ToHashCode();
	}

	public static bool operator ==(FinalStats left, FinalStats right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(FinalStats left, FinalStats right)
	{
		return !left.Equals(right);
	}
}
