using System;

public class Star_Lacerta_HandCannon : DewHeroStarItemOld
{
	public static readonly float DamageAmp = 0.3f;

	public static readonly float SlowDuration = 1.5f;

	public static readonly float SlowStrength = 50f;

	public override Type heroType => typeof(Hero_Lacerta);

	public override Type affectedSkill => typeof(St_Q_HandCannon);

	public override bool ShouldInitInGame()
	{
		if (base.ShouldInitInGame())
		{
			return isServer;
		}
		return false;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
		hero.dealtDamageProcessor.Add(HeroOndealtDamageProcessor);
	}

	private void HeroOndealtDamageProcessor(ref DamageData data, Actor actor, Entity target)
	{
		if (actor is Ai_Q_HandCannon)
		{
			data.ApplyAmplification(DamageAmp);
			actor.CreateBasicEffect(target, new SlowEffect
			{
				strength = SlowStrength
			}, SlowDuration, "StarHandCannonSlow", DuplicateEffectBehavior.UsePrevious);
		}
	}
}
