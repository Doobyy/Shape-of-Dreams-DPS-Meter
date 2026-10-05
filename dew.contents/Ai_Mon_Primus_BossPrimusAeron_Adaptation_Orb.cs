using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Adaptation_Orb : AbilityInstance
{
	public float delay = 1f;

	[NonSerialized]
	[SyncVar]
	public Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType type;

	[NonSerialized]
	public float amount;

	private ParticleSystem[] _tintParticles;

	private MinMaxGradient[] _tintParticleStartColors;

	private FxEntityColor[] _tintEntityColors;

	private Color[] _tintEntityBaseColors;

	private Color[] _tintEntityEmissions;

	public override bool reuseInRoom => true;

	public Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType Networktype
	{
		get
		{
			return type;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType>(value, ref type, 64uL, (Action<Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType, Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType>)null);
		}
	}

	protected override void Awake()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		_tintParticles = ((Component)(object)this).GetComponentsInChildren<ParticleSystem>(true);
		_tintParticleStartColors = new MinMaxGradient[_tintParticles.Length];
		for (int i = 0; i < _tintParticles.Length; i++)
		{
			MinMaxGradient[] tintParticleStartColors = _tintParticleStartColors;
			int num = i;
			MainModule main = _tintParticles[i].main;
			tintParticleStartColors[num] = main.startColor;
		}
		_tintEntityColors = ((Component)(object)this).GetComponentsInChildren<FxEntityColor>(true);
		_tintEntityBaseColors = new Color[_tintEntityColors.Length];
		_tintEntityEmissions = new Color[_tintEntityColors.Length];
		for (int j = 0; j < _tintEntityColors.Length; j++)
		{
			_tintEntityBaseColors[j] = _tintEntityColors[j].baseColor;
			_tintEntityEmissions[j] = _tintEntityColors[j].emission;
		}
	}

	protected override void OnCreate()
	{
		switch (type)
		{
		case Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType.MaxHealthPercentage:
			DewEffect.TintRecursively(((Component)(object)this).gameObject, new Color(0.6f, 1f, 0.3f));
			break;
		case Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType.Speed:
			DewEffect.TintRecursively(((Component)(object)this).gameObject, new Color(0.9f, 0.9f, 0.4f));
			break;
		case Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType.Damage:
			DewEffect.TintRecursively(((Component)(object)this).gameObject, new Color(0.9f, 0.35f, 0.3f));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		base.OnCreate();
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		yield return new SI.WaitForSeconds(delay);
		if (parentActor is Se_Mon_Primus_BossPrimusAeron_Adaptation se_Mon_Primus_BossPrimusAeron_Adaptation)
		{
			switch (type)
			{
			case Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType.MaxHealthPercentage:
				se_Mon_Primus_BossPrimusAeron_Adaptation.bonus.maxHealthPercentage += amount;
				break;
			case Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType.Speed:
				se_Mon_Primus_BossPrimusAeron_Adaptation.bonus.attackSpeedPercentage += amount;
				se_Mon_Primus_BossPrimusAeron_Adaptation.bonus.movementSpeedPercentage += amount;
				break;
			case Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType.Damage:
				se_Mon_Primus_BossPrimusAeron_Adaptation.bonus.attackDamagePercentage += amount;
				se_Mon_Primus_BossPrimusAeron_Adaptation.bonus.abilityPowerPercentage += amount;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		Destroy();
	}

	protected override void OnDisable()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		base.OnDisable();
		for (int i = 0; i < _tintParticles.Length; i++)
		{
			MainModule main = _tintParticles[i].main;
			main.startColor = _tintParticleStartColors[i];
		}
		for (int j = 0; j < _tintEntityColors.Length; j++)
		{
			_tintEntityColors[j].baseColor = _tintEntityBaseColors[j];
			_tintEntityColors[j].emission = _tintEntityEmissions[j];
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
			GeneratedNetworkCode._Write_Se_Mon_Primus_BossPrimusAeron_Adaptation_002FStatType(writer, type);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			GeneratedNetworkCode._Write_Se_Mon_Primus_BossPrimusAeron_Adaptation_002FStatType(writer, type);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType>(ref type, (Action<Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType, Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType>)null, GeneratedNetworkCode._Read_Se_Mon_Primus_BossPrimusAeron_Adaptation_002FStatType(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType>(ref type, (Action<Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType, Se_Mon_Primus_BossPrimusAeron_Adaptation.StatType>)null, GeneratedNetworkCode._Read_Se_Mon_Primus_BossPrimusAeron_Adaptation_002FStatType(reader));
		}
	}
}
