using Mirror;
using UnityEngine;

public class RoomMod_HeroSoul : RoomModifierBase
{
	private ActorRef<Shrine> _spawnedShrine;

	public override void OnStartServer()
	{
		base.OnStartServer();
		DewPlayer targetPlayer = DewPlayer.gamePlayers.Find((DewPlayer p) => p.guid == modData.clientData);
		if (!((Object)(object)targetPlayer == null) && !((Object)(object)targetPlayer.hero == null) && targetPlayer.hero.isKnockedOut)
		{
			SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out var pos);
			_spawnedShrine = CreateActor(pos, null, (Shrine_HeroSoul r) =>
			{
				r.targetHero = targetPlayer.hero;
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !_spawnedShrine.IsNullOrInactive())
		{
			_spawnedShrine.Get().Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
