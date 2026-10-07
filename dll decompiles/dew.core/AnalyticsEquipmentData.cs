using System;
using System.Collections.Generic;
using System.IO;

public struct AnalyticsEquipmentData
{
	private const ushort CurrentVersion = 0;

	public List<(HeroSkillLocation, string, int)> skills;

	public List<(GemLocation, string, int)> gems;

	public AnalyticsEquipmentData(Hero hero)
	{
		List<(HeroSkillLocation, string, int)> newSkills = new List<(HeroSkillLocation, string, int)>();
		List<(GemLocation, string, int)> list = new List<(GemLocation, string, int)>();
		ProcessSkill(HeroSkillLocation.Q);
		ProcessSkill(HeroSkillLocation.W);
		ProcessSkill(HeroSkillLocation.E);
		ProcessSkill(HeroSkillLocation.R);
		ProcessSkill(HeroSkillLocation.Identity);
		foreach (KeyValuePair<GemLocation, Gem> gem in hero.Skill.gems)
		{
			if (ItemNumericIdDatabase.instance.TryGetHash(((object)gem.Value).GetType().Name, out var _))
			{
				list.Add((gem.Key, ((object)gem.Value).GetType().Name, gem.Value.quality));
			}
		}
		skills = newSkills;
		gems = list;
		void ProcessSkill(HeroSkillLocation type)
		{
			if (hero.Skill.TryGetSkill(type, out var skill) && ItemNumericIdDatabase.instance.TryGetHash(((object)skill).GetType().Name, out var _))
			{
				newSkills.Add((type, ((object)skill).GetType().Name, skill.level));
			}
		}
	}

	public AnalyticsEquipmentData(string base64)
	{
		skills = new List<(HeroSkillLocation, string, int)>();
		gems = new List<(GemLocation, string, int)>();
		using MemoryStream input = new MemoryStream(Convert.FromBase64String(base64));
		using BinaryReader binaryReader = new BinaryReader(input);
		if (binaryReader.ReadUInt16() == 0)
		{
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				HeroSkillLocation item = (HeroSkillLocation)binaryReader.ReadByte();
				uint id = binaryReader.ReadUInt32();
				int item2 = binaryReader.ReadInt32();
				if (ItemNumericIdDatabase.instance.TryGetItem(id, out var item3))
				{
					skills.Add((item, item3, item2));
				}
			}
			int num2 = binaryReader.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				GemLocation item4 = new GemLocation((HeroSkillLocation)binaryReader.ReadByte(), binaryReader.ReadByte());
				uint id2 = binaryReader.ReadUInt32();
				int item5 = binaryReader.ReadInt32();
				if (ItemNumericIdDatabase.instance.TryGetItem(id2, out var item6))
				{
					gems.Add((item4, item6, item5));
				}
			}
			return;
		}
		throw new InvalidOperationException("Data format not supported");
	}

	public string ToBase64()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((ushort)0);
		binaryWriter.Write(skills.Count);
		foreach (var skill in skills)
		{
			binaryWriter.Write((byte)skill.Item1);
			ItemNumericIdDatabase.instance.TryGetHash(skill.Item2, out var id);
			binaryWriter.Write(id);
			binaryWriter.Write(skill.Item3);
		}
		binaryWriter.Write(gems.Count);
		foreach (var gem in gems)
		{
			binaryWriter.Write((byte)gem.Item1.skill);
			binaryWriter.Write((byte)gem.Item1.index);
			ItemNumericIdDatabase.instance.TryGetHash(gem.Item2, out var id2);
			binaryWriter.Write(id2);
			binaryWriter.Write(gem.Item3);
		}
		return Convert.ToBase64String(memoryStream.ToArray());
	}
}
