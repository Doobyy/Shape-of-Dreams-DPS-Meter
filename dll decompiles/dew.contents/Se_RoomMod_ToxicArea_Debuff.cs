using Mirror;
using UnityEngine;

public class Se_RoomMod_ToxicArea_Debuff : StatusEffect
{
	public float healReduction;

	public float shieldReduction;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.takenHealProcessor.Add(TakenHealProcessor);
			victim.takenShieldProcessor.Add(TakenShieldProcessor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.takenHealProcessor.Remove(TakenHealProcessor);
			victim.takenShieldProcessor.Remove(TakenShieldProcessor);
		}
	}

	private void TakenHealProcessor(ref HealData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyReduction(healReduction);
		}
	}

	private void TakenShieldProcessor(ref HealData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyReduction(shieldReduction);
		}
	}

	private void MirrorProcessed()
	{
	}
}
