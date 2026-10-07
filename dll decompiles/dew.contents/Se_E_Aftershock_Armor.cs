using Mirror;
using UnityEngine;

public class Se_E_Aftershock_Armor : StatusEffect
{
	public ScalingValue armorPerHit;

	public int maxHitCount = 5;

	internal int _hitCount;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_hitCount = 0;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_hitCount = Mathf.Min(maxHitCount, _hitCount);
			float num = (float)_hitCount * GetValue(armorPerHit);
			DoArmorBoost(num);
			if ((Object)(object)gem != null)
			{
				gem.numberDisplay = Mathf.RoundToInt(num);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)gem != null)
		{
			gem.numberDisplay = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
