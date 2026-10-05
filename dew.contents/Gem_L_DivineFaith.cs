using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Gem_L_DivineFaith : Gem
{
	public float dmgAmpPerStack;

	public float maxHealthPerStack;

	public ScalingValue maxStacks;

	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public int currentStack;

	public float gracePeriod = 6f;

	public GameObject fxStackUp;

	private KillTracker _tracker;

	private StatBonus _bonus;

	public int NetworkcurrentStack
	{
		get
		{
			return currentStack;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentStack, 262144uL, (Action<int, int>)null);
		}
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = new StatBonus();
			newOwner.Status.AddStatBonus(_bonus);
			_bonus.maxHealthFlat = maxHealthPerStack * (float)currentStack;
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldOwner != null)
		{
			oldOwner.Status.RemoveStatBonus(_bonus);
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)owner == null))
		{
			numberDisplay = Mathf.RoundToInt(currentStack);
		}
	}

	private void IncreaseAmp(EventInfoKill obj)
	{
		if (obj.victim is Monster)
		{
			int num = Mathf.RoundToInt(GetValue(maxStacks));
			if (currentStack < num)
			{
				NetworkcurrentStack = Mathf.Clamp(currentStack + 1, 0, num);
				_bonus.maxHealthFlat = maxHealthPerStack * (float)currentStack;
				ShowStackUp(owner.owner, obj.victim.position);
				NotifyUse();
			}
		}
	}

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			_tracker = newSkill.TrackKills(gracePeriod, IncreaseAmp);
			newSkill.dealtDamageProcessor.Add(Amplify);
			newSkill.dealtHealProcessor.Add(Amplify);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)oldSkill != null)
			{
				oldSkill.dealtDamageProcessor.Remove(Amplify);
				oldSkill.dealtHealProcessor.Remove(Amplify);
			}
			_tracker.Stop();
		}
	}

	private void Amplify(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this) && owner.CheckEnemyOrNeutral(target))
		{
			data.ApplyAmplification((float)currentStack * dmgAmpPerStack);
			data.SetAmountModifiedBy(this);
		}
	}

	private void Amplify(ref HealData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this) && !owner.CheckEnemyOrNeutral(target))
		{
			data.ApplyAmplification((float)currentStack * dmgAmpPerStack);
			data.SetAmountModifiedBy(this);
		}
	}

	protected override void OnQualityChange(int oldQuality, int newQuality)
	{
		base.OnQualityChange(oldQuality, newQuality);
		if (((NetworkBehaviour)this).isServer)
		{
			int num = Mathf.RoundToInt(GetValue(maxStacks));
			if (currentStack > num)
			{
				NetworkcurrentStack = num;
			}
		}
	}

	[TargetRpc]
	public void ShowStackUp(NetworkConnectionToClient conn, Vector3 pos)
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		NetworkWriterExtensions.WriteVector3((NetworkWriter)(object)val, pos);
		((NetworkBehaviour)this).SendTargetRPCInternal((NetworkConnection)(object)conn, "System.Void Gem_L_DivineFaith::ShowStackUp(Mirror.NetworkConnectionToClient,UnityEngine.Vector3)", -1268230581, (NetworkWriter)(object)val, 0);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_ShowStackUp__NetworkConnectionToClient__Vector3(NetworkConnectionToClient conn, Vector3 pos)
	{
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.25f);
			FxPlayNew(fxStackUp, pos, null);
			InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
			{
				rawText = "+1",
				color = Color.white,
				worldPos = pos
			});
		}
	}

	protected static void InvokeUserCode_ShowStackUp__NetworkConnectionToClient__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC ShowStackUp called on server.");
		}
		else
		{
			((Gem_L_DivineFaith)(object)obj).UserCode_ShowStackUp__NetworkConnectionToClient__Vector3((NetworkConnectionToClient)(object)NetworkClient.connection, NetworkReaderExtensions.ReadVector3(reader));
		}
	}

	static Gem_L_DivineFaith()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Gem_L_DivineFaith), "System.Void Gem_L_DivineFaith::ShowStackUp(Mirror.NetworkConnectionToClient,UnityEngine.Vector3)", (RemoteCallDelegate)InvokeUserCode_ShowStackUp__NetworkConnectionToClient__Vector3);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, currentStack);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentStack);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentStack, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentStack, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
