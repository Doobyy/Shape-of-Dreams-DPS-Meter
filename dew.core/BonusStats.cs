using System;
using UnityEngine.Serialization;

[Serializable]
public struct BonusStats : IEquatable<BonusStats>
{
	public static readonly BonusStats Default;

	public float attackDamageFlat;

	public float attackDamagePercentage;

	public float abilityPowerFlat;

	public float abilityPowerPercentage;

	public float maxHealthFlat;

	public float maxHealthPercentage;

	public float maxManaFlat;

	public float maxManaPercentage;

	public float healthRegenFlat;

	public float healthRegenPercentage;

	public float manaRegenFlat;

	public float manaRegenPercentage;

	public float attackSpeedPercentage;

	public float critAmpFlat;

	public float critAmpPercentage;

	public float critChanceFlat;

	public float critChancePercentage;

	public float abilityHasteFlat;

	public float abilityHastePercentage;

	public float tenacityFlat;

	public float tenacityPercentage;

	public float attackRangeFlat;

	public float attackRangePercentage;

	public float movementSpeedPercentage;

	public float fireEffectAmpFlat;

	public float coldEffectAmpFlat;

	public float lightEffectAmpFlat;

	public float darkEffectAmpFlat;

	public float armorFlat;

	[FormerlySerializedAs("armorPercetnage")]
	public float armorPercentage;

	public int everyFourAttackStartIndexFlat;

	public static BonusStats operator +(BonusStats x, BonusStats y)
	{
		return new BonusStats
		{
			attackDamageFlat = x.attackDamageFlat + y.attackDamageFlat,
			attackDamagePercentage = DewMath.MultiplyPercentageBonuses(x.attackDamagePercentage, y.attackDamagePercentage),
			abilityPowerFlat = x.abilityPowerFlat + y.abilityPowerFlat,
			abilityPowerPercentage = DewMath.MultiplyPercentageBonuses(x.abilityPowerPercentage, y.abilityPowerPercentage),
			maxHealthFlat = x.maxHealthFlat + y.maxHealthFlat,
			maxHealthPercentage = DewMath.MultiplyPercentageBonuses(x.maxHealthPercentage, y.maxHealthPercentage),
			maxManaFlat = x.maxManaFlat + y.maxManaFlat,
			maxManaPercentage = DewMath.MultiplyPercentageBonuses(x.maxManaPercentage, y.maxManaPercentage),
			healthRegenFlat = x.healthRegenFlat + y.healthRegenFlat,
			healthRegenPercentage = DewMath.MultiplyPercentageBonuses(x.healthRegenPercentage, y.healthRegenPercentage),
			manaRegenFlat = x.manaRegenFlat + y.manaRegenFlat,
			manaRegenPercentage = DewMath.MultiplyPercentageBonuses(x.manaRegenPercentage, y.manaRegenPercentage),
			attackSpeedPercentage = x.attackSpeedPercentage + y.attackSpeedPercentage,
			critAmpFlat = x.critAmpFlat + y.critAmpFlat,
			critAmpPercentage = x.critAmpPercentage + y.critAmpPercentage,
			critChanceFlat = x.critChanceFlat + y.critChanceFlat,
			critChancePercentage = DewMath.MultiplyPercentageBonuses(x.critChancePercentage, y.critChancePercentage),
			abilityHasteFlat = x.abilityHasteFlat + y.abilityHasteFlat,
			abilityHastePercentage = DewMath.MultiplyPercentageBonuses(x.abilityHastePercentage, y.abilityHastePercentage),
			tenacityFlat = x.tenacityFlat + y.tenacityFlat,
			tenacityPercentage = DewMath.MultiplyPercentageBonuses(x.tenacityPercentage, y.tenacityPercentage),
			attackRangeFlat = x.attackRangeFlat + y.attackRangeFlat,
			attackRangePercentage = DewMath.MultiplyPercentageBonuses(x.attackRangePercentage, y.attackRangePercentage),
			movementSpeedPercentage = x.movementSpeedPercentage + y.movementSpeedPercentage,
			fireEffectAmpFlat = x.fireEffectAmpFlat + y.fireEffectAmpFlat,
			coldEffectAmpFlat = x.coldEffectAmpFlat + y.coldEffectAmpFlat,
			lightEffectAmpFlat = x.lightEffectAmpFlat + y.lightEffectAmpFlat,
			darkEffectAmpFlat = x.darkEffectAmpFlat + y.darkEffectAmpFlat,
			armorFlat = x.armorFlat + y.armorFlat,
			armorPercentage = DewMath.MultiplyPercentageBonuses(x.armorPercentage, y.armorPercentage),
			everyFourAttackStartIndexFlat = x.everyFourAttackStartIndexFlat + y.everyFourAttackStartIndexFlat
		};
	}

	public static BonusStats operator +(BonusStats x, StatBonus y)
	{
		return new BonusStats
		{
			attackDamageFlat = x.attackDamageFlat + y.attackDamageFlat,
			attackDamagePercentage = DewMath.MultiplyPercentageBonuses(x.attackDamagePercentage, y.attackDamagePercentage),
			abilityPowerFlat = x.abilityPowerFlat + y.abilityPowerFlat,
			abilityPowerPercentage = DewMath.MultiplyPercentageBonuses(x.abilityPowerPercentage, y.abilityPowerPercentage),
			maxHealthFlat = x.maxHealthFlat + y.maxHealthFlat,
			maxHealthPercentage = DewMath.MultiplyPercentageBonuses(x.maxHealthPercentage, y.maxHealthPercentage),
			maxManaFlat = x.maxManaFlat + y.maxManaFlat,
			maxManaPercentage = DewMath.MultiplyPercentageBonuses(x.maxManaPercentage, y.maxManaPercentage),
			healthRegenFlat = x.healthRegenFlat + y.healthRegenFlat,
			healthRegenPercentage = DewMath.MultiplyPercentageBonuses(x.healthRegenPercentage, y.healthRegenPercentage),
			manaRegenFlat = x.manaRegenFlat + y.manaRegenFlat,
			manaRegenPercentage = DewMath.MultiplyPercentageBonuses(x.manaRegenPercentage, y.manaRegenPercentage),
			attackSpeedPercentage = x.attackSpeedPercentage + y.attackSpeedPercentage,
			critAmpFlat = x.critAmpFlat + y.critAmpFlat,
			critAmpPercentage = x.critAmpPercentage + y.critAmpPercentage,
			critChanceFlat = x.critChanceFlat + y.critChanceFlat,
			critChancePercentage = DewMath.MultiplyPercentageBonuses(x.critChancePercentage, y.critChancePercentage),
			abilityHasteFlat = x.abilityHasteFlat + y.abilityHasteFlat,
			abilityHastePercentage = DewMath.MultiplyPercentageBonuses(x.abilityHastePercentage, y.abilityHastePercentage),
			tenacityFlat = x.tenacityFlat + y.tenacityFlat,
			tenacityPercentage = DewMath.MultiplyPercentageBonuses(x.tenacityPercentage, y.tenacityPercentage),
			attackRangeFlat = x.attackRangeFlat + y.attackRangeFlat,
			attackRangePercentage = DewMath.MultiplyPercentageBonuses(x.attackRangePercentage, y.attackRangePercentage),
			movementSpeedPercentage = x.movementSpeedPercentage + y.movementSpeedPercentage,
			fireEffectAmpFlat = x.fireEffectAmpFlat + y.fireEffectAmpFlat,
			coldEffectAmpFlat = x.coldEffectAmpFlat + y.coldEffectAmpFlat,
			lightEffectAmpFlat = x.lightEffectAmpFlat + y.lightEffectAmpFlat,
			darkEffectAmpFlat = x.darkEffectAmpFlat + y.darkEffectAmpFlat,
			armorFlat = x.armorFlat + y.armorFlat,
			armorPercentage = DewMath.MultiplyPercentageBonuses(x.armorPercentage, y.armorPercentage),
			everyFourAttackStartIndexFlat = x.everyFourAttackStartIndexFlat + y.everyFourAttackStartIndexFlat
		};
	}

	public bool Equals(BonusStats other)
	{
		if (attackDamageFlat.Equals(other.attackDamageFlat) && attackDamagePercentage.Equals(other.attackDamagePercentage) && abilityPowerFlat.Equals(other.abilityPowerFlat) && abilityPowerPercentage.Equals(other.abilityPowerPercentage) && maxHealthFlat.Equals(other.maxHealthFlat) && maxHealthPercentage.Equals(other.maxHealthPercentage) && maxManaFlat.Equals(other.maxManaFlat) && maxManaPercentage.Equals(other.maxManaPercentage) && healthRegenFlat.Equals(other.healthRegenFlat) && healthRegenPercentage.Equals(other.healthRegenPercentage) && manaRegenFlat.Equals(other.manaRegenFlat) && manaRegenPercentage.Equals(other.manaRegenPercentage) && attackSpeedPercentage.Equals(other.attackSpeedPercentage) && critAmpFlat.Equals(other.critAmpFlat) && critAmpPercentage.Equals(other.critAmpPercentage) && critChanceFlat.Equals(other.critChanceFlat) && critChancePercentage.Equals(other.critChancePercentage) && abilityHasteFlat.Equals(other.abilityHasteFlat) && abilityHastePercentage.Equals(other.abilityHastePercentage) && tenacityFlat.Equals(other.tenacityFlat) && tenacityPercentage.Equals(other.tenacityPercentage) && movementSpeedPercentage.Equals(other.movementSpeedPercentage) && fireEffectAmpFlat.Equals(other.fireEffectAmpFlat) && coldEffectAmpFlat.Equals(other.coldEffectAmpFlat) && lightEffectAmpFlat.Equals(other.lightEffectAmpFlat) && darkEffectAmpFlat.Equals(other.darkEffectAmpFlat) && armorFlat.Equals(other.armorFlat) && armorPercentage.Equals(other.armorPercentage) && attackRangeFlat.Equals(other.attackRangeFlat) && attackRangePercentage.Equals(other.attackRangePercentage))
		{
			return everyFourAttackStartIndexFlat.Equals(other.everyFourAttackStartIndexFlat);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BonusStats other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		HashCode val = default;
		val.Add<float>(attackDamageFlat);
		val.Add<float>(attackDamagePercentage);
		val.Add<float>(abilityPowerFlat);
		val.Add<float>(abilityPowerPercentage);
		val.Add<float>(maxHealthFlat);
		val.Add<float>(maxHealthPercentage);
		val.Add<float>(maxManaFlat);
		val.Add<float>(maxManaPercentage);
		val.Add<float>(healthRegenFlat);
		val.Add<float>(healthRegenPercentage);
		val.Add<float>(manaRegenFlat);
		val.Add<float>(manaRegenPercentage);
		val.Add<float>(attackSpeedPercentage);
		val.Add<float>(critAmpFlat);
		val.Add<float>(critAmpPercentage);
		val.Add<float>(critChanceFlat);
		val.Add<float>(critChancePercentage);
		val.Add<float>(abilityHasteFlat);
		val.Add<float>(abilityHastePercentage);
		val.Add<float>(tenacityFlat);
		val.Add<float>(tenacityPercentage);
		val.Add<float>(movementSpeedPercentage);
		val.Add<float>(fireEffectAmpFlat);
		val.Add<float>(coldEffectAmpFlat);
		val.Add<float>(lightEffectAmpFlat);
		val.Add<float>(darkEffectAmpFlat);
		val.Add<float>(armorFlat);
		val.Add<float>(armorPercentage);
		val.Add<float>(attackRangeFlat);
		val.Add<float>(attackRangePercentage);
		val.Add<int>(everyFourAttackStartIndexFlat);
		return val.ToHashCode();
	}

	public static bool operator ==(BonusStats left, BonusStats right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BonusStats left, BonusStats right)
	{
		return !left.Equals(right);
	}
}
