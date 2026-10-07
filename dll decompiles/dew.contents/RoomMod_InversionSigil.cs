using Mirror;
using UnityEngine;

public class RoomMod_InversionSigil : RoomModifierBase
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			ModifyEntities((Entity e) =>
			{
				e.dealtDamageProcessor.Add(Processor, -1000);
			}, (Entity e) =>
			{
				e.dealtDamageProcessor.Remove(Processor);
			});
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (!(actor is ElementalStatusEffect) && !(actor is Gem_E_Flexibility))
		{
			if (data.elemental == ElementalType.Fire)
			{
				data.SetElemental(ElementalType.Cold);
			}
			else if (data.elemental == ElementalType.Cold)
			{
				data.SetElemental(ElementalType.Fire);
			}
			else if (data.elemental == ElementalType.Dark)
			{
				data.SetElemental(ElementalType.Light);
			}
			else if (data.elemental == ElementalType.Light)
			{
				data.SetElemental(ElementalType.Dark);
			}
			else
			{
				data.SetElemental((ElementalType)Random.Range(0, 4));
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
