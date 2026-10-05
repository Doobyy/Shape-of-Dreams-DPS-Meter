using Mirror;
using UnityEngine;

public class Gem_R_Accuracy : Gem
{
	public GameObject fxRange;

	public float radius;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)newOwner).isOwned)
		{
			FxPlay(fxRange, newOwner);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		FxStop(fxRange);
	}

	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		if (!info.chain.DidReact(this) && owner.CheckEnemyOrNeutral(info.victim) && !(Vector2.Distance(owner.position.ToXY(), info.victim.position.ToXY()) < radius + info.victim.Control.outerRadius))
		{
			CreateAbilityInstanceWithSource(info.actor, owner.position, null, new CastInfo(owner, info.victim), (Ai_Gem_R_Accuracy_Projectile ai) =>
			{
				ai.chain = info.chain.New(this);
				ai.data = info.damage;
			});
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}
