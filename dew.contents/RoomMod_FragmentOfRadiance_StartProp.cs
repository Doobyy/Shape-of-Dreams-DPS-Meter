using UnityEngine;

public class RoomMod_FragmentOfRadiance_StartProp : RoomModifierBase
{
	private RoomSection _section;

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (!isNewInstance)
		{
			PlaceShrine<Shrine_FragmentOfRadiance>(new PlaceShrineSettings
			{
				removeModifierOnUse = true
			});
		}
		else
		{
			_section = SingletonDewNetworkBehaviour<Room>.instance.sections[Random.Range(0, SingletonDewNetworkBehaviour<Room>.instance.sections.Count)];
			_section.onEnterFirstTime.AddListener(OnEnterFirstTime);
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		if (_section != null)
		{
			_section.onEnterFirstTime.RemoveListener(OnEnterFirstTime);
		}
	}

	private void OnEnterFirstTime()
	{
		if (_section.monsters.isMarkedAsCombatArea)
		{
			_section.monsters.onClearCombatArea.AddListener(() =>
			{
				PlaceShrine<Shrine_FragmentOfRadiance>(new PlaceShrineSettings
				{
					customPosition = Dew.GetGoodRewardPosition(Dew.SelectRandomAliveHero().agentPosition),
					removeModifierOnUse = true
				});
			});
		}
		else
		{
			_section.TryGetGoodNodePosition(out var value);
			PlaceShrine<Shrine_FragmentOfRadiance>(new PlaceShrineSettings
			{
				customPosition = value,
				removeModifierOnUse = true
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
