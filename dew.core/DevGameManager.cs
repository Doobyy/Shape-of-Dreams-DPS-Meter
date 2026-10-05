using System;
using IngameDebugConsole;
using Mirror;
using Mirror.RemoteCalls;
using Steamworks;
using UnityEngine;

public class DevGameManager : GameManager
{
	public Zone startZonePrefab;

	[Command(requiresAuthority = false)]
	public void SpawnHero(Type heroType, int level, HeroLoadoutData loadout, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkWriter)(object)val).WriteType(heroType);
		NetworkWriterExtensions.WriteInt((NetworkWriter)(object)val, level);
		GeneratedNetworkCode._Write_HeroLoadoutData((NetworkWriter)(object)val, loadout);
		((NetworkBehaviour)this).SendCommandInternal("System.Void DevGameManager::SpawnHero(System.Type,System.Int32,HeroLoadoutData,Mirror.NetworkConnectionToClient)", -150208123, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	public override void OnLateStartServer()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.OnLateStartServer();
		NetworkedManagerBase<ConsoleManager>.instance.EnableCheats();
		NetworkedManagerBase<ZoneManager>.instance.TravelToZone(startZonePrefab);
		Debug.Log($"User Steam ID {SteamUser.GetSteamID()}");
	}

	public override void OnLateStart()
	{
		base.OnLateStart();
		SpawnHero(DewResources.GetByShortTypeName("Hero_Mist").GetType(), 1, new HeroLoadoutData());
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		InGameUIManager.instance.SetState("Playing");
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		InGameUIManager.instance.SetState("Loading");
	}

	public void HostGame()
	{
		DebugLogConsole.ExecuteCommand("host", false);
	}

	public void StartLocalGame()
	{
		DebugLogConsole.ExecuteCommand("local", false);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_SpawnHero__Type__Int32__HeroLoadoutData__NetworkConnectionToClient(Type heroType, int level, HeroLoadoutData loadout, NetworkConnectionToClient sender)
	{
		DewPlayer player = sender.GetPlayer();
		if ((UnityEngine.Object)(object)player.hero != null)
		{
			Dew.Destroy(((Component)(object)player.hero).gameObject);
		}
		Vector3 position = (((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance.currentRoom == null) ? Vector3.zero : NetworkedManagerBase<ZoneManager>.instance.currentRoom.GetHeroSpawnPosition());
		Hero controllingEntity = (player.hero = Dew.SpawnHero((Hero)(object)DewResources.GetByType(heroType), position, Quaternion.identity, player, level, loadout, null));
		player.controllingEntity = controllingEntity;
	}

	protected static void InvokeUserCode_SpawnHero__Type__Int32__HeroLoadoutData__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command SpawnHero called on client.");
		}
		else
		{
			((DevGameManager)(object)obj).UserCode_SpawnHero__Type__Int32__HeroLoadoutData__NetworkConnectionToClient(reader.ReadType(), NetworkReaderExtensions.ReadInt(reader), GeneratedNetworkCode._Read_HeroLoadoutData(reader), senderConnection);
		}
	}

	static DevGameManager()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(DevGameManager), "System.Void DevGameManager::SpawnHero(System.Type,System.Int32,HeroLoadoutData,Mirror.NetworkConnectionToClient)", (RemoteCallDelegate)InvokeUserCode_SpawnHero__Type__Int32__HeroLoadoutData__NetworkConnectionToClient, false);
	}
}
