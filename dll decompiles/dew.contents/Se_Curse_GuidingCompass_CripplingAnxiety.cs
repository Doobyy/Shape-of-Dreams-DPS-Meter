using System;
using Mirror;
using UnityEngine;

public class Se_Curse_GuidingCompass_CripplingAnxiety : CurseStatusEffect
{
	public float maxHealthReduction = 0.25f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				maxHealthPercentage = (0f - maxHealthReduction) * 100f
			});
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(OnRoomLoadedCheckCompass);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomLoadedCheckCompass);
		}
	}

	private void OnRoomLoadedCheckCompass(EventInfoLoadRoom obj)
	{
		if (!obj.isTraveling)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			if (!this.IsNullOrInactive())
			{
				LiftAllIfCompassMissing();
			}
		});
	}

	public static void LiftAllIfCompassMissingDelayed()
	{
		Dew.CallDelayed(LiftAllIfCompassMissing);
	}

	public static void LiftAllIfCompassMissing()
	{
		if (!NetworkServer.active || (UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance == null)
		{
			return;
		}
		foreach (Actor allActor in NetworkedManagerBase<ActorManager>.instance.allActors)
		{
			if ((allActor is Gem_U_GuidingCompass_NotCharged || allActor is Gem_U_GuidingCompass_Charged) && allActor.isActive)
			{
				return;
			}
		}
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			if (allHero.Status.TryGetStatusEffect<Se_Curse_GuidingCompass_CripplingAnxiety>(out var effect))
			{
				effect.Destroy();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
