using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class At_Mon_Special_BossLightElemental_Summon : AbilityTrigger
{
	public float maxPopulation = 6f;

	public float cooldownMultOnNoPopulation;

	public float cooldownMultOnMaxPopulation;

	[SyncVar]
	private float _currentCooldownMult;

	public float Network_currentCooldownMult
	{
		get
		{
			return _currentCooldownMult;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _currentCooldownMult, 1024uL, (Action<float, float>)null);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			float populationCost = ((Monster)owner).populationCost;
			float num = maxPopulation * NetworkedManagerBase<GameManager>.instance.difficulty.maxPopulationMultiplier - populationCost;
			float t = (NetworkedManagerBase<GameManager>.instance.spawnedPopulation - populationCost) / num;
			Network_currentCooldownMult = Mathf.Lerp(cooldownMultOnNoPopulation, cooldownMultOnMaxPopulation, t);
		}
	}

	public override float GetCooldownTimeMultiplier(int configIndex)
	{
		return base.GetCooldownTimeMultiplier(configIndex) * _currentCooldownMult;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _currentCooldownMult);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _currentCooldownMult);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentCooldownMult, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _currentCooldownMult, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
