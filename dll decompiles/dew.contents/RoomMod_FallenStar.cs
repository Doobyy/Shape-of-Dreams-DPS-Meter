using Mirror;
using UnityEngine;

public class RoomMod_FallenStar : RoomModifierBase
{
	public float bonusHealthPercentage = 50f;

	public float bonusPowerPercentage = 60f;

	public float spawnPopMultiplier = 1.5f;

	public float addedMirageChance = 0.1f;

	public override void OnStartServer()
	{
		base.OnStartServer();
		SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier *= spawnPopMultiplier;
		if ((Object)(object)GameMod_MirageSkin.instance != null && GameMod_MirageSkin.instance.GetCurrentBaseMirageChance() > 0.001f)
		{
			SingletonDewNetworkBehaviour<Room>.instance.monsters.addedMirageChance += addedMirageChance;
		}
		ModifyEntities((Entity e) =>
		{
			if (e is Monster)
			{
				e.CreateStatusEffect(e, new CastInfo(e), (Se_FallenStar_MonsterReinforcement se) =>
				{
					se.bonusHealthPercentage = bonusHealthPercentage;
					se.bonusPowerPercentage = bonusPowerPercentage;
				});
			}
		}, (Entity e) =>
		{
			if (e.Status.TryGetStatusEffect<Se_FallenStar_MonsterReinforcement>(out var effect))
			{
				effect.Destroy();
			}
		});
		PlaceShrine<Shrine_FallenStar>(new PlaceShrineSettings
		{
			lockedUntilCleared = true,
			spawnOnLastSection = true
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((Object)(object)SingletonDewNetworkBehaviour<Room>.instance == null))
		{
			SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier /= spawnPopMultiplier;
			SingletonDewNetworkBehaviour<Room>.instance.monsters.addedMirageChance -= addedMirageChance;
		}
	}

	private void MirrorProcessed()
	{
	}
}
