using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Game Content Settings", menuName = "Game Content Settings")]
public class DewGameContentSettings : ScriptableObject
{
	public bool includeAllContents;

	public DewGameContentSettings parent;

	[NonSerialized]
	public List<string> includedZones;

	[NonSerialized]
	public List<int> zoneCountByTier;

	[NonSerialized]
	public List<string> availableSkills;

	[NonSerialized]
	public List<string> availableGems;

	[NonSerialized]
	public List<string> availableArtifacts;

	[NonSerialized]
	public List<string> availableHeroes;

	[NonSerialized]
	public List<string> availableGameModifiers;

	[NonSerialized]
	public List<string> availableRoomModifiers;

	[NonSerialized]
	public List<string> availableCurses;

	[NonSerialized]
	public List<string> availableLucidDreams;

	[NonSerialized]
	public List<string> availableTreasures;

	[NonSerialized]
	public List<string> availableStars;

	[NonSerialized]
	public List<string> excludedMonsters;

	[NonSerialized]
	public List<string> availableAccessories;

	[NonSerialized]
	public List<string> availableNametags;

	[NonSerialized]
	public List<string> availableEmotes;

	[NonSerialized]
	public List<string> availableSkins;

	[FormerlySerializedAs("includedZones")]
	[SerializeField]
	public string[] _includedZones = new string[0];

	[FormerlySerializedAs("zoneCountByTier")]
	[SerializeField]
	public int[] _zoneCountByTier = new int[1] { 1 };

	[FormerlySerializedAs("availableSkills")]
	[SerializeField]
	public string[] _availableSkills = new string[0];

	[FormerlySerializedAs("availableGems")]
	[SerializeField]
	public string[] _availableGems = new string[0];

	[FormerlySerializedAs("availableArtifacts")]
	[SerializeField]
	public string[] _availableArtifacts = new string[0];

	[FormerlySerializedAs("availableHeroes")]
	[SerializeField]
	public string[] _availableHeroes = new string[0];

	[FormerlySerializedAs("availableGameModifiers")]
	[SerializeField]
	public string[] _availableGameModifiers = new string[0];

	[FormerlySerializedAs("availableRoomModifiers")]
	[SerializeField]
	public string[] _availableRoomModifiers = new string[0];

	[FormerlySerializedAs("availableCurses")]
	[SerializeField]
	public string[] _availableCurses = new string[0];

	[FormerlySerializedAs("availableLucidDreams")]
	[SerializeField]
	public string[] _availableLucidDreams = new string[0];

	[SerializeField]
	public string[] _availableTreasures = new string[0];

	[SerializeField]
	public string[] _availableStars = new string[0];

	[SerializeField]
	public string[] _excludedMonsters = new string[0];

	[SerializeField]
	public string[] _availableAccessories = new string[0];

	[SerializeField]
	public string[] _availableNametags = new string[0];

	[SerializeField]
	public string[] _availableEmotes = new string[0];

	[SerializeField]
	public string[] _availableSkins = new string[0];

	public string[] includedRooms = new string[0];

	public string[] includedMonsters = new string[0];

	public void Init(bool skipValidation = false)
	{
		List<DewGameContentSettings> list = new List<DewGameContentSettings>();
		DewGameContentSettings dewGameContentSettings = this;
		while (dewGameContentSettings != null)
		{
			list.Insert(0, dewGameContentSettings);
			dewGameContentSettings = dewGameContentSettings.parent;
			if (list.Count > 100)
			{
				Debug.LogError("Cyclic parenting detected", this);
				return;
			}
		}
		if (!skipValidation)
		{
			foreach (DewGameContentSettings item in list)
			{
				item.Validate();
			}
		}
		includedZones = new List<string>();
		zoneCountByTier = new List<int>();
		availableSkills = new List<string>();
		availableGems = new List<string>();
		availableArtifacts = new List<string>();
		availableHeroes = new List<string>();
		availableGameModifiers = new List<string>();
		availableRoomModifiers = new List<string>();
		availableCurses = new List<string>();
		availableLucidDreams = new List<string>();
		availableTreasures = new List<string>();
		availableStars = new List<string>();
		excludedMonsters = new List<string>();
		availableAccessories = new List<string>();
		availableNametags = new List<string>();
		availableEmotes = new List<string>();
		availableSkins = new List<string>();
		foreach (DewGameContentSettings item2 in list)
		{
			Add(item2._includedZones, includedZones);
			Add(item2._availableSkills, availableSkills);
			Add(item2._availableGems, availableGems);
			Add(item2._availableArtifacts, availableArtifacts);
			Add(item2._availableHeroes, availableHeroes);
			Add(item2._availableGameModifiers, availableGameModifiers);
			Add(item2._availableRoomModifiers, availableRoomModifiers);
			Add(item2._availableCurses, availableCurses);
			Add(item2._availableLucidDreams, availableLucidDreams);
			Add(item2._availableTreasures, availableTreasures);
			Add(item2._availableStars, availableStars);
			Add(item2._excludedMonsters, excludedMonsters);
			Add(item2._availableAccessories, availableAccessories);
			Add(item2._availableNametags, availableNametags);
			Add(item2._availableEmotes, availableEmotes);
			Add(item2._availableSkins, availableSkins);
			if (item2._zoneCountByTier.Length != 0)
			{
				zoneCountByTier.Clear();
				zoneCountByTier.AddRange(item2._zoneCountByTier);
			}
		}
		static void Add(string[] from, List<string> to)
		{
			foreach (string text in from)
			{
				if (text == "Clear")
				{
					to.Clear();
				}
				else if (text.StartsWith("!"))
				{
					to.Remove(text.Substring(1));
				}
				else if (!to.Contains(text))
				{
					to.Add(text);
				}
			}
		}
	}

	public string[] FilterZones(string[] zones)
	{
		List<string> list = new List<string>(zones);
		if (includedZones != null && includedZones.Count > 0)
		{
			List<string> list2 = includedZones;
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (!list2.Contains(list[num]))
				{
					list.RemoveAt(num);
				}
			}
		}
		return list.ToArray();
	}

	public Zone[] FilterZones(Zone[] zones)
	{
		List<Zone> list = new List<Zone>(zones);
		if (includedZones != null && includedZones.Count > 0)
		{
			List<string> list2 = includedZones;
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (!list2.Contains(list[num].name))
				{
					list.RemoveAt(num);
				}
			}
		}
		return list.ToArray();
	}

	private void PrintValidationResult()
	{
		try
		{
			Validate();
			Debug.Log("No problem has been found in content settings: " + name);
		}
		catch (Exception exception)
		{
			Debug.LogError("Problem has been found in content settings: " + name);
			Debug.LogException(exception);
		}
	}

	public void Validate()
	{
	}

	private void OnValidate()
	{
		Validate();
	}
}
