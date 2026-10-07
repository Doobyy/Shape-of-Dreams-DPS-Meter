using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class NetworkReaderWriterExtensions
{
	private static class CommonReaderWriters
	{
		public static void WriteAsset<T>(NetworkWriter writer, T asset) where T : UnityEngine.Object
		{
			NetworkWriterExtensions.WriteString(writer, (asset != null && DewResources.TryGetGuidOfAsset(asset, out var guid)) ? guid : "");
		}

		public static T ReadAsset<T>(NetworkReader reader) where T : UnityEngine.Object
		{
			string text = NetworkReaderExtensions.ReadString(reader);
			if (string.IsNullOrWhiteSpace(text))
			{
				return null;
			}
			return DewResources.GetByGuid<T>(text);
		}
	}

	public static void WriteBasicEffect(this NetworkWriter writer, BasicEffect eff)
	{
	}

	public static BasicEffect ReadBasicEffect(this NetworkReader reader)
	{
		return null;
	}

	public static void WriteIInteractable(this NetworkWriter writer, IInteractable interactable)
	{
		if (!(interactable is Component component))
		{
			NetworkWriterExtensions.WriteNetworkIdentity(writer, (NetworkIdentity)null);
		}
		else
		{
			NetworkWriterExtensions.WriteNetworkIdentity(writer, component.GetComponent<NetworkIdentity>());
		}
	}

	public static IInteractable ReadIInteractable(this NetworkReader reader)
	{
		NetworkIdentity val = NetworkReaderExtensions.ReadNetworkIdentity(reader);
		if ((UnityEngine.Object)(object)val == null)
		{
			return null;
		}
		return ((Component)(object)val).GetComponent<IInteractable>();
	}

	public static void WriteType(this NetworkWriter writer, Type type)
	{
		NetworkWriterExtensions.WriteString(writer, type.AssemblyQualifiedName);
	}

	public static Type ReadType(this NetworkReader reader)
	{
		return Type.GetType(NetworkReaderExtensions.ReadString(reader));
	}

	public static void WriteDewAnimationClip(this NetworkWriter writer, DewAnimationClip asset)
	{
		CommonReaderWriters.WriteAsset(writer, asset);
	}

	public static DewAnimationClip ReadDewAnimationClip(this NetworkReader reader)
	{
		return CommonReaderWriters.ReadAsset<DewAnimationClip>(reader);
	}

	public static void WriteDewAudioClip(this NetworkWriter writer, DewAudioClip asset)
	{
		CommonReaderWriters.WriteAsset(writer, asset);
	}

	public static DewAudioClip ReadDewAudioClip(this NetworkReader reader)
	{
		return CommonReaderWriters.ReadAsset<DewAudioClip>(reader);
	}

	public static void WriteAnimationClip(this NetworkWriter writer, AnimationClip asset)
	{
		CommonReaderWriters.WriteAsset<AnimationClip>(writer, asset);
	}

	public static AnimationClip ReadAnimationClip(this NetworkReader reader)
	{
		return CommonReaderWriters.ReadAsset<AnimationClip>(reader);
	}

	public static void WriteDewDifficultySettings(this NetworkWriter writer, DewDifficultySettings asset)
	{
		CommonReaderWriters.WriteAsset(writer, asset);
	}

	public static DewDifficultySettings ReadDewDifficultySettings(this NetworkReader reader)
	{
		return CommonReaderWriters.ReadAsset<DewDifficultySettings>(reader);
	}

	public static void WriteZone(this NetworkWriter writer, Zone asset)
	{
		CommonReaderWriters.WriteAsset(writer, asset);
	}

	public static Zone ReadZone(this NetworkReader reader)
	{
		return CommonReaderWriters.ReadAsset<Zone>(reader);
	}

	public static void WriteNullableFloat(this NetworkWriter writer, float? nullable)
	{
		if (!nullable.HasValue)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteFloat(writer, nullable.Value);
	}

	public static float? ReadNullableFloat(this NetworkReader reader)
	{
		if (NetworkReaderExtensions.ReadBool(reader))
		{
			return NetworkReaderExtensions.ReadFloat(reader);
		}
		return null;
	}

	public static void WriteNullableKey(this NetworkWriter writer, Key? nullable)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected I4, but got Unknown
		if (!nullable.HasValue)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteInt(writer, (int)nullable.Value);
	}

	public static Key? ReadNullableKey(this NetworkReader reader)
	{
		if (NetworkReaderExtensions.ReadBool(reader))
		{
			return (Key)NetworkReaderExtensions.ReadInt(reader);
		}
		return null;
	}

	public static void WriteNullableGamepadButton(this NetworkWriter writer, GamepadButton? nullable)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected I4, but got Unknown
		if (!nullable.HasValue)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		NetworkWriterExtensions.WriteInt(writer, (int)nullable.Value);
	}

	public static GamepadButton? ReadNullableGamepadButton(this NetworkReader reader)
	{
		if (NetworkReaderExtensions.ReadBool(reader))
		{
			return (GamepadButton)NetworkReaderExtensions.ReadInt(reader);
		}
		return null;
	}

	public static void WriteNullableKeyCode(this NetworkWriter writer, KeyCode? nullable)
	{
		if (!nullable.HasValue)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		writer.Write<KeyCode>(nullable.Value);
	}

	public static KeyCode? ReadNullableKeyCode(this NetworkReader reader)
	{
		if (NetworkReaderExtensions.ReadBool(reader))
		{
			return reader.Read<KeyCode>();
		}
		return null;
	}

	public static void WriteIHoldableInHand(this NetworkWriter writer, IItem ownable)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected Obj, but got Unknown
		NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)ownable);
	}

	public static IItem ReadIHoldableInHand(this NetworkReader reader)
	{
		return NetworkReaderExtensions.ReadNetworkBehaviour(reader) as IItem;
	}

	public static void WriteDisplacement(this NetworkWriter writer, Displacement d)
	{
		if (d == null)
		{
			writer.WriteByte((byte)0);
			return;
		}
		if (d is DispByTarget dispByTarget)
		{
			writer.WriteByte((byte)1);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)dispByTarget.target);
			NetworkWriterExtensions.WriteFloat(writer, dispByTarget.goalDistance);
			NetworkWriterExtensions.WriteFloat(writer, dispByTarget.speed);
			NetworkWriterExtensions.WriteFloat(writer, dispByTarget.cancelTime);
			NetworkWriterExtensions.WriteBool(writer, d.isFriendly);
			NetworkWriterExtensions.WriteBool(writer, d.isDodging);
			NetworkWriterExtensions.WriteBool(writer, d.isCanceledByCC);
			NetworkWriterExtensions.WriteBool(writer, d.affectedByMovementSpeed);
			NetworkWriterExtensions.WriteBool(writer, d.rotateForward);
			NetworkWriterExtensions.WriteBool(writer, d.rotateSmoothly);
			return;
		}
		if (d is DispByDestination dispByDestination)
		{
			writer.WriteByte((byte)2);
			writer.WriteByte((byte)dispByDestination.ease);
			NetworkWriterExtensions.WriteVector3(writer, dispByDestination.destination);
			NetworkWriterExtensions.WriteFloat(writer, dispByDestination.duration);
			NetworkWriterExtensions.WriteBool(writer, dispByDestination.canGoOverTerrain);
			NetworkWriterExtensions.WriteBool(writer, d.isFriendly);
			NetworkWriterExtensions.WriteBool(writer, d.isDodging);
			NetworkWriterExtensions.WriteBool(writer, d.isCanceledByCC);
			NetworkWriterExtensions.WriteBool(writer, d.affectedByMovementSpeed);
			NetworkWriterExtensions.WriteBool(writer, d.rotateForward);
			NetworkWriterExtensions.WriteBool(writer, d.rotateSmoothly);
			return;
		}
		throw new ArgumentOutOfRangeException("d");
	}

	public static Displacement ReadDisplacement(this NetworkReader reader)
	{
		return reader.ReadByte() switch
		{
			0 => (Displacement)null, 
			1 => new DispByTarget
			{
				target = (Entity)(object)NetworkReaderExtensions.ReadNetworkBehaviour(reader),
				goalDistance = NetworkReaderExtensions.ReadFloat(reader),
				speed = NetworkReaderExtensions.ReadFloat(reader),
				cancelTime = NetworkReaderExtensions.ReadFloat(reader),
				isFriendly = NetworkReaderExtensions.ReadBool(reader),
				isDodging = NetworkReaderExtensions.ReadBool(reader),
				isCanceledByCC = NetworkReaderExtensions.ReadBool(reader),
				affectedByMovementSpeed = NetworkReaderExtensions.ReadBool(reader),
				rotateForward = NetworkReaderExtensions.ReadBool(reader),
				rotateSmoothly = NetworkReaderExtensions.ReadBool(reader)
			}, 
			2 => new DispByDestination
			{
				ease = (DewEase)reader.ReadByte(),
				destination = NetworkReaderExtensions.ReadVector3(reader),
				duration = NetworkReaderExtensions.ReadFloat(reader),
				canGoOverTerrain = NetworkReaderExtensions.ReadBool(reader),
				isFriendly = NetworkReaderExtensions.ReadBool(reader),
				isDodging = NetworkReaderExtensions.ReadBool(reader),
				isCanceledByCC = NetworkReaderExtensions.ReadBool(reader),
				affectedByMovementSpeed = NetworkReaderExtensions.ReadBool(reader),
				rotateForward = NetworkReaderExtensions.ReadBool(reader),
				rotateSmoothly = NetworkReaderExtensions.ReadBool(reader)
			}, 
			_ => throw new ArgumentOutOfRangeException("type"), 
		};
	}

	public static void WriteDictionary<TKey, TValue>(NetworkWriter writer, Dictionary<TKey, TValue> dict)
	{
		if (dict == null)
		{
			NetworkWriterExtensions.WriteInt(writer, -1);
			return;
		}
		NetworkWriterExtensions.WriteInt(writer, dict.Count);
		foreach (KeyValuePair<TKey, TValue> item in dict)
		{
			writer.Write<TKey>(item.Key);
			writer.Write<TValue>(item.Value);
		}
	}

	public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(this NetworkReader reader)
	{
		int num = NetworkReaderExtensions.ReadInt(reader);
		if (num < 0)
		{
			return null;
		}
		Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
		for (int i = 0; i < num; i++)
		{
			dictionary.Add(reader.Read<TKey>(), reader.Read<TValue>());
		}
		return dictionary;
	}

	public static void WriteDictionary0(this NetworkWriter writer, Dictionary<string, string> dict)
	{
		WriteDictionary(writer, dict);
	}

	public static Dictionary<string, string> ReadDictionary0(this NetworkReader reader)
	{
		return reader.ReadDictionary<string, string>();
	}

	public static void WriteDictionary1(this NetworkWriter writer, Dictionary<string, DewProfileStats.HeroData> dict)
	{
		WriteDictionary(writer, dict);
	}

	public static Dictionary<string, DewProfileStats.HeroData> ReadDictionary1(this NetworkReader reader)
	{
		return reader.ReadDictionary<string, DewProfileStats.HeroData>();
	}

	public static void WriteDictionary2(this NetworkWriter writer, Dictionary<string, DewProfileStats.MonsterData> dict)
	{
		WriteDictionary(writer, dict);
	}

	public static Dictionary<string, DewProfileStats.MonsterData> ReadDictionary2(this NetworkReader reader)
	{
		return reader.ReadDictionary<string, DewProfileStats.MonsterData>();
	}

	public static void WriteDictionary3(this NetworkWriter writer, Dictionary<string, DewProfileStats.ZoneData> dict)
	{
		WriteDictionary(writer, dict);
	}

	public static Dictionary<string, DewProfileStats.ZoneData> ReadDictionary3(this NetworkReader reader)
	{
		return reader.ReadDictionary<string, DewProfileStats.ZoneData>();
	}

	public static void WriteDictionary4(this NetworkWriter writer, Dictionary<string, DewProfileStats.ItemData> dict)
	{
		WriteDictionary(writer, dict);
	}

	public static Dictionary<string, DewProfileStats.ItemData> ReadDictionary4(this NetworkReader reader)
	{
		return reader.ReadDictionary<string, DewProfileStats.ItemData>();
	}

	public static void WriteCounterBool(this NetworkWriter writer, CounterBool value)
	{
		NetworkWriterExtensions.WriteInt(writer, value.count);
	}

	public static CounterBool ReadCounterBool(this NetworkReader reader)
	{
		return new CounterBool(NetworkReaderExtensions.ReadInt(reader));
	}

	public static void WriteAssetRef<T>(this NetworkWriter writer, AssetRef<T> asset) where T : UnityEngine.Object
	{
		writer.Write<string>(asset.guid);
		writer.Write<string>(asset.typeName);
		writer.Write<string>(asset.typeAssemblyQualifiedName);
		writer.Write<bool>(asset.isMonoBehaviour);
		writer.Write<bool>(asset.isActor);
	}

	public static AssetRef<T> ReadAssetRef<T>(this NetworkReader reader) where T : UnityEngine.Object
	{
		return new AssetRef<T>
		{
			guid = NetworkReaderExtensions.ReadString(reader),
			typeName = NetworkReaderExtensions.ReadString(reader),
			typeAssemblyQualifiedName = NetworkReaderExtensions.ReadString(reader),
			isMonoBehaviour = NetworkReaderExtensions.ReadBool(reader),
			isActor = NetworkReaderExtensions.ReadBool(reader)
		};
	}

	public static void WriteAssetRef0(this NetworkWriter writer, AssetRef<DewDifficultySettings> value)
	{
		writer.WriteAssetRef(value);
	}

	public static AssetRef<DewDifficultySettings> ReadAssetRef0(this NetworkReader reader)
	{
		return reader.ReadAssetRef<DewDifficultySettings>();
	}

	public static void WriteNullableGemLocation(this NetworkWriter writer, GemLocation? nullable)
	{
		if (!nullable.HasValue)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		writer.Write<GemLocation>(nullable.Value);
	}

	public static GemLocation? ReadNullableGemLocation(this NetworkReader reader)
	{
		if (NetworkReaderExtensions.ReadBool(reader))
		{
			return reader.Read<GemLocation>();
		}
		return null;
	}

	public static void WriteNullableHeroSkillLocation(this NetworkWriter writer, HeroSkillLocation? nullable)
	{
		if (!nullable.HasValue)
		{
			NetworkWriterExtensions.WriteBool(writer, false);
			return;
		}
		NetworkWriterExtensions.WriteBool(writer, true);
		writer.Write<HeroSkillLocation>(nullable.Value);
	}

	public static HeroSkillLocation? ReadNullableHeroSkillLocation(this NetworkReader reader)
	{
		if (NetworkReaderExtensions.ReadBool(reader))
		{
			return reader.Read<HeroSkillLocation>();
		}
		return null;
	}
}
