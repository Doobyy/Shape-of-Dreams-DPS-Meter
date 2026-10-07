using UnityEngine;

public class Star_Global_DodgeAbilityCooldown : DewStarItemOld
{
	public static readonly float[] CooldownMultiplier = new float[4] { 0.93f, 0.88f, 0.82f, 0.75f };

	public override int maxLevel => 4;

	public override bool affectsMovementSkill => true;

	public override bool ShouldInitInGame()
	{
		return isServer;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
		if (!((Object)(object)hero.Skill.Movement == null))
		{
			hero.Skill.Movement.configs[0].cooldownTime *= CooldownMultiplier.GetClamped(level - 1);
		}
	}
}
