using Mirror;
using UnityEngine;

public class Se_Curse_Lethargy : CurseStatusEffect
{
	public float[] reducedDamageRatio;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.dealtDamageProcessor.Add(Processor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((Object)(object)victim == null))
		{
			victim.dealtDamageProcessor.Remove(Processor);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		data.ApplyReduction(GetValue(reducedDamageRatio));
	}

	private void MirrorProcessed()
	{
	}
}
