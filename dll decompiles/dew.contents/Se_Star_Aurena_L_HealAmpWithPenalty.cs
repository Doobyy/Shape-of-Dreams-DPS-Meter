using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_L_HealAmpWithPenalty : StarEffect
{
	public float takenDamageAmp;

	public StarScalingValue healAmp;

	public override Type heroType => typeof(Hero_Aurena);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.takenDamageProcessor.Add(TakenDmgAmp);
			hero.dealtHealProcessor.Add(Processor);
			hero.takenHealProcessor.Add(Processor);
		}
	}

	private void TakenDmgAmp(ref DamageData data, Actor actor, Entity target)
	{
		Entity entity = actor.firstEntity;
		if (!((UnityEngine.Object)(object)entity == null) && hero.CheckEnemyOrNeutral(entity) && !data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyAmplification(takenDamageAmp);
		}
	}

	private void Processor(ref HealData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyAmplification(GetValue(healAmp));
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.takenDamageProcessor.Remove(TakenDmgAmp);
			hero.dealtHealProcessor.Remove(Processor);
			hero.takenHealProcessor.Remove(Processor);
		}
	}

	private void MirrorProcessed()
	{
	}
}
