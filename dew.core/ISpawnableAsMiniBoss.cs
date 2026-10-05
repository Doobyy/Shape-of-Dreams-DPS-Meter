using UnityEngine;

public interface ISpawnableAsMiniBoss
{
	static void GiveGenericMiniBossBonus(Entity entity, float postDelayMultiplier = 0.5f)
	{
		float num = 1.3f;
		entity.Visual.GetNewTransformModifier().scaleMultiplier = Vector3.one * num;
		entity.Control.outerRadius *= num;
		entity.Control.innerRadius *= num;
		StatBonus bonus = new StatBonus
		{
			maxHealthFlat = 1300f,
			abilityHasteFlat = 50f,
			attackSpeedPercentage = 15f,
			abilityPowerPercentage = 20f,
			attackDamagePercentage = 20f
		};
		entity.Status.AddStatBonus(bonus);
		foreach (AbilityTrigger value in entity.Ability.abilities.Values)
		{
			TriggerConfig[] configs = value.configs;
			for (int i = 0; i < configs.Length; i++)
			{
				configs[i].postDelay *= 0.5f;
			}
		}
		entity.CreateBasicEffect(entity, new UnstoppableEffect(), float.PositiveInfinity, "MiniBossUnstoppable");
	}

	void OnBeforeSpawnAsMiniBoss();

	void OnCreateAsMiniBoss();
}
