using Mirror;
using UnityEngine;

public class Gem_L_Liberty : Gem
{
	protected override void OnCastComplete(EventInfoCast info)
	{
		base.OnCastComplete(info);
		if (((NetworkBehaviour)this).isServer)
		{
			if (owner.Status.TryGetStatusEffect<Se_Gem_L_Liberty_Buff>(out var effect))
			{
				effect.ResetAndApply();
			}
			else
			{
				CreateStatusEffectWithSource<Se_Gem_L_Liberty_Buff>(info.instance, owner, new CastInfo(owner));
			}
			NotifyUse();
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && (Object)(object)oldOwner != null && oldOwner.Status.TryGetStatusEffect<Se_Gem_L_Liberty_Buff>(out var effect))
		{
			effect.Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
