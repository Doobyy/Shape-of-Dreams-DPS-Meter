using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_I_ForestKillDreamDust : StarEffect
{
	public StarScalingValue speedStrength;

	public StarScalingValue dreamDustAmount;

	private ActorRef<Se_GenericEffectContainer> _speed;

	public override Type heroType => typeof(Hero_Nachia);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_speed = null;
			hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
			ClientEventOnZoneLoaded(new EventInfoLoadZone
			{
				to = ((NetworkedManagerBase<ZoneManager>.instance.currentZone != null) ? NetworkedManagerBase<ZoneManager>.instance.currentZone.name : "")
			});
		}
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		bool flag = obj.to == "Zone_Forest";
		if (flag && _speed.IsNullOrInactive())
		{
			_speed = CreateBasicEffect(hero, new SpeedEffect
			{
				strength = GetValue(speedStrength)
			}, float.PositiveInfinity);
		}
		else if (!flag && !_speed.IsNullOrInactive())
		{
			_speed.Get().Destroy();
			_speed = null;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)hero != null)
			{
				hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
			}
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
			}
			if (!_speed.IsNullOrInactive())
			{
				_speed.Get().Destroy();
			}
			_speed = null;
		}
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		if (obj.victim is Monster && NetworkedManagerBase<ZoneManager>.instance.currentZone != null && NetworkedManagerBase<ZoneManager>.instance.currentZone.name == "Zone_Forest")
		{
			NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(GetValue(dreamDustAmount)), obj.victim.agentPosition, hero);
		}
	}

	private void MirrorProcessed()
	{
	}
}
