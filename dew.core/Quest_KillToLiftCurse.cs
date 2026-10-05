using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Quest_KillToLiftCurse : DewQuest
{
	[NonSerialized]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public CurseStatusEffect targetEffect;

	protected NetworkBehaviourSyncVar ___targetEffectNetId;

	public CurseStatusEffect NetworktargetEffect
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<CurseStatusEffect>(___targetEffectNetId, ref targetEffect);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<CurseStatusEffect>(value, ref targetEffect, 256uL, (Action<CurseStatusEffect, CurseStatusEffect>)null, ref ___targetEffectNetId);
		}
	}

	public override bool IsVisibleLocally()
	{
		return ((NetworkBehaviour)NetworktargetEffect.victim).isOwned;
	}

	protected override void OnStepStarted(bool isLoadedFromSave)
	{
		base.OnStepStarted(isLoadedFromSave);
		if (NetworktargetEffect.IsNullOrInactive())
		{
			if (((NetworkBehaviour)this).isServer)
			{
				DestroyIfActive();
			}
			return;
		}
		string curseKey = DewLocalization.GetCurseKey(((object)NetworktargetEffect).GetType().Name);
		questTitleRaw = NetworktargetEffect.GetName();
		questShortDescriptionRaw = DewLocalization.GetCurseShortDescription(curseKey).ToText(NetworktargetEffect);
		questDetailedDescriptionRaw = DewLocalization.GetCurseDescription(curseKey).ToText(NetworktargetEffect);
		if (((NetworkBehaviour)this).isServer)
		{
			NetworktargetEffect.CurseEvent_OnProgressChanged += new Action(UpdateProgress);
			UpdateProgress();
		}
	}

	private void UpdateProgress()
	{
		progressType = NetworktargetEffect.progressType;
		currentProgress = (NetworktargetEffect.requiredAmount - NetworktargetEffect.currentProgress).ToString("#,###");
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworktargetEffect != null)
		{
			NetworktargetEffect.CurseEvent_OnProgressChanged -= new Action(UpdateProgress);
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)NetworktargetEffect);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)NetworktargetEffect);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<CurseStatusEffect>(ref targetEffect, (Action<CurseStatusEffect, CurseStatusEffect>)null, reader, ref ___targetEffectNetId);
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<CurseStatusEffect>(ref targetEffect, (Action<CurseStatusEffect, CurseStatusEffect>)null, reader, ref ___targetEffectNetId);
		}
	}
}
