using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ink_BossSelector : DewNetworkBehaviour
{
	public GameObject bossWhiteNight;

	public GameObject bossDarkMoon;

	public MonsterSpawnRule whiteNightSpawnRule;

	public MonsterSpawnRule darkMoonSpawnRule;

	[Space(10f)]
	public bool forceSpawnWhite;

	[NonSerialized]
	public bool enableWhiteBoss = true;

	private RoomSection _finalSection;

	public override void OnStartServer()
	{
		base.OnStartServer();
		GameManager.CallOnReady(() =>
		{
			enableWhiteBoss = UnityEngine.Random.value <= 0.5f;
			_finalSection = SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection();
			if (Ge_Shrine_Anitya.IsDoubleSpawn())
			{
				_finalSection.monsters.ruleOverride = whiteNightSpawnRule;
				SetObject(isWhiteBossActive: true);
			}
			else if (enableWhiteBoss || forceSpawnWhite)
			{
				_finalSection.monsters.ruleOverride = whiteNightSpawnRule;
				SetObject(isWhiteBossActive: true);
			}
			else
			{
				_finalSection.monsters.ruleOverride = darkMoonSpawnRule;
				SetObject(isWhiteBossActive: false);
			}
		});
	}

	[ClientRpc]
	public void SetObject(bool isWhiteBossActive)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteBool((NetworkWriter)(object)val, isWhiteBossActive);
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ink_BossSelector::SetObject(System.Boolean)", 738662256, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_SetObject__Boolean(bool isWhiteBossActive)
	{
		if (isWhiteBossActive)
		{
			bossDarkMoon.SetActive(value: false);
		}
		else
		{
			bossWhiteNight.SetActive(value: false);
		}
	}

	protected static void InvokeUserCode_SetObject__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetObject called on server.");
		}
		else
		{
			((Ink_BossSelector)(object)obj).UserCode_SetObject__Boolean(NetworkReaderExtensions.ReadBool(reader));
		}
	}

	static Ink_BossSelector()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ink_BossSelector), "System.Void Ink_BossSelector::SetObject(System.Boolean)", (RemoteCallDelegate)InvokeUserCode_SetObject__Boolean);
	}
}
