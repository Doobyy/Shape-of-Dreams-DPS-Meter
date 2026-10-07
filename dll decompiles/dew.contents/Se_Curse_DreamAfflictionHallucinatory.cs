using Mirror;
using UnityEngine;

public class Se_Curse_DreamAfflictionHallucinatory : CurseStatusEffect
{
	public float[] reductionRatios;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				healthRegenPercentage = GetValue(reductionRatios) * -100f
			});
			victim.takenHealProcessor.Add(VictimOntakenHealProcessor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.takenHealProcessor.Remove(VictimOntakenHealProcessor);
		}
	}

	private void VictimOntakenHealProcessor(ref HealData data, Actor actor, Entity target)
	{
		data.ApplyReduction(GetValue(reductionRatios));
	}

	private void MirrorProcessed()
	{
	}
}
