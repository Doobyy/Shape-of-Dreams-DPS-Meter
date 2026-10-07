using Mirror;
using UnityEngine;

public class Se_E_Permafrost_Shield : StatusEffect
{
	public float armorAmount;

	public float shieldDuration;

	public ScalingValue shieldAmount;

	private ShieldEffect _shield;

	private StatBonus _armor;

	private float _initialShieldAmount;

	private int _armorStacks;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_armorStacks = 1;
			_armor = DoStatBonus(new StatBonus
			{
				armorFlat = armorAmount
			});
			_shield = DoShield(GetValue(shieldAmount));
			_initialShieldAmount = _shield.amount;
			SetTimer(shieldDuration);
		}
	}

	[Server]
	public void AddStack()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_E_Permafrost_Shield::AddStack()' called when server was not active");
			return;
		}
		SetTimer(shieldDuration);
		if (_armorStacks < 3)
		{
			_armorStacks++;
			victim.Status.RemoveStatBonus(_armor);
			_armor = DoStatBonus(new StatBonus
			{
				armorFlat = armorAmount * (float)_armorStacks
			});
		}
		if (_shield.amount < _initialShieldAmount)
		{
			_shield.amount = _initialShieldAmount;
		}
	}

	private void MirrorProcessed()
	{
	}
}
