using System;

public class Star_Global_ElementResistance : DewStarItemOld
{
	public static readonly float[] DurationReduction = new float[4] { 0.1f, 0.2f, 0.3f, 0.5f };

	public override int maxLevel => 4;

	public override bool ShouldInitInGame()
	{
		return isServer;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
		hero.ClientEntityEvent_OnStatusEffectAdded += new Action<EventInfoStatusEffect>(ClientEntityEventOnStatusEffectAdded);
	}

	private void ClientEntityEventOnStatusEffectAdded(EventInfoStatusEffect obj)
	{
		if (obj.effect is ElementalStatusEffect elementalStatusEffect)
		{
			if (obj.effect is Se_Elm_Fire)
			{
				elementalStatusEffect.dealtDamageProcessor.Add(ReduceBurnDamage);
			}
			else
			{
				elementalStatusEffect.decayTime *= 1f - DurationReduction.GetClamped(level - 1);
			}
		}
	}

	private void ReduceBurnDamage(ref DamageData data, Actor actor, Entity target)
	{
		data.ApplyReduction(DurationReduction.GetClamped(level - 1));
	}
}
