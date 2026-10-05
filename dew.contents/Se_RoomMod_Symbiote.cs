using Mirror;
using UnityEngine;

public class Se_RoomMod_Symbiote : StatusEffect
{
	public float atkSpeedBonus;

	public float armorReductionAmount;

	private StatBonus _statBonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_statBonus = new StatBonus
			{
				attackSpeedPercentage = atkSpeedBonus,
				armorFlat = 0f - armorReductionAmount
			};
			victim.Status.AddStatBonus(_statBonus);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(Object)(object)victim)
		{
			victim.Status.RemoveStatBonus(_statBonus);
		}
	}

	private void MirrorProcessed()
	{
	}
}
