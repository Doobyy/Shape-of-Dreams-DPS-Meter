using UnityEngine;

public class Star_Global_DecreaseDamageFromChilled : DewStarItemOld
{
	public static readonly float[] DamageReduction = new float[4] { 0.05f, 0.07f, 0.1f, 0.15f };

	public override int maxLevel => 4;

	public override bool ShouldInitInGame()
	{
		return isServer;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
		hero.takenDamageProcessor.Add(HeroOntakenDamageProcessor);
	}

	private void HeroOntakenDamageProcessor(ref DamageData data, Actor actor, Entity target)
	{
		Entity firstEntity = actor.firstEntity;
		if (!((Object)(object)firstEntity == null) && target.CheckEnemyOrNeutral(firstEntity) && firstEntity.Status.hasCold)
		{
			data.ApplyReduction(DamageReduction.GetClamped(level - 1));
		}
	}
}
