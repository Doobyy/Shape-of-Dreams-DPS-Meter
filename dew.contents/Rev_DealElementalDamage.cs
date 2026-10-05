using UnityEngine;

public class Rev_DealElementalDamage : DewReverieItem
{
	[AchPersistentVar]
	private int _elementalType;

	[AchPersistentVar]
	private float _damageAmount;

	public override int grantedStardust => 45;

	public string elementalName => DewLocalization.GetUIValue($"Se_Elm_{elementalType}_Name");

	public ElementalType elementalType => (ElementalType)_elementalType;

	public override int GetCurrentProgress()
	{
		return Mathf.FloorToInt(_damageAmount);
	}

	public override int GetMaxProgress()
	{
		return 50000;
	}

	public override void OnSetupReverie()
	{
		base.OnSetupReverie();
		_elementalType = Random.Range(0, 4);
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnDealDamage((EventInfoDamage d) =>
		{
			if (d.damage.elemental == elementalType)
			{
				_damageAmount += d.damage.amount + d.damage.discardedAmount;
				if (_damageAmount > 50000f)
				{
					Complete();
				}
			}
		});
	}
}
