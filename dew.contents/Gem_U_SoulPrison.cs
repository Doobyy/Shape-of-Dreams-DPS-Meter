using Mirror;
using UnityEngine;

public class Gem_U_SoulPrison : Gem
{
	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			CreateStatusEffect<Se_Gem_U_SoulPrison_DeathInterrupt>(newOwner, new CastInfo(newOwner));
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && !((Object)(object)oldOwner == null) && oldOwner.Status.TryGetStatusEffect<Se_Gem_U_SoulPrison_DeathInterrupt>(out var effect))
		{
			effect.Destroy();
		}
	}

	protected override void OnQualityChange(int oldQuality, int newQuality)
	{
		base.OnQualityChange(oldQuality, newQuality);
		if (((NetworkBehaviour)this).isServer && !((Object)(object)owner == null))
		{
			if (owner.Status.TryGetStatusEffect<Se_Gem_U_SoulPrison_DeathInterrupt>(out var effect))
			{
				effect.Destroy();
			}
			CreateStatusEffect<Se_Gem_U_SoulPrison_DeathInterrupt>(owner, new CastInfo(owner));
		}
	}

	private void MirrorProcessed()
	{
	}
}
