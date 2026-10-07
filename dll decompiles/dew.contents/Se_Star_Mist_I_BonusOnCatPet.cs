using System;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_I_BonusOnCatPet : StarEffect
{
	public StarScalingValue goldAmount;

	public StarScalingValue dreamDustAmount;

	private Shrine_LoopCat _cat;

	public override Type heroType => typeof(Hero_Mist);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAdd);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
			{
				NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnActorAdd);
			}
			if ((UnityEngine.Object)(object)_cat != null)
			{
				_cat.ClientEvent_OnSuccessfulUse -= new Action<Entity>(ClientEventOnSuccessfulUse);
			}
		}
	}

	private void OnActorAdd(Actor obj)
	{
		if (obj is Shrine_LoopCat shrine_LoopCat)
		{
			_cat = shrine_LoopCat;
			shrine_LoopCat.ClientEvent_OnSuccessfulUse += new Action<Entity>(ClientEventOnSuccessfulUse);
		}
	}

	private void ClientEventOnSuccessfulUse(Entity obj)
	{
		if (!this.IsNullOrInactive() && !victim.IsNullInactiveDeadOrKnockedOut())
		{
			NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: false, GetValueInt(goldAmount), ((UnityEngine.Object)(object)_cat == null) ? victim.position : _cat.position, (Hero)victim);
			NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, GetValueInt(dreamDustAmount), ((UnityEngine.Object)(object)_cat == null) ? victim.position : _cat.position, (Hero)victim);
		}
	}

	private void MirrorProcessed()
	{
	}
}
