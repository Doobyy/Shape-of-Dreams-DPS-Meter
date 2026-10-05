using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeroLoadoutData
{
	public const int HeroLoadoutCount = 5;

	public int skillQ;

	public int skillR;

	public int skillTrait;

	public int skillMovement;

	public List<LoadoutStarItem> cDestruction = new List<LoadoutStarItem>();

	public List<LoadoutStarItem> cLife = new List<LoadoutStarItem>();

	public List<LoadoutStarItem> cImagination = new List<LoadoutStarItem>();

	public List<LoadoutStarItem> cFlexible = new List<LoadoutStarItem>();

	public HeroLoadoutData()
	{
	}

	public HeroLoadoutData(HeroLoadoutData source)
	{
		skillQ = source.skillQ;
		skillR = source.skillR;
		skillTrait = source.skillTrait;
		skillMovement = source.skillMovement;
		cDestruction = new List<LoadoutStarItem>(source.cDestruction);
		cLife = new List<LoadoutStarItem>(source.cLife);
		cImagination = new List<LoadoutStarItem>(source.cImagination);
		cFlexible = new List<LoadoutStarItem>(source.cFlexible);
	}

	public List<LoadoutStarItem> GetStarList(StarType type)
	{
		return type switch
		{
			StarType.Destruction => cDestruction, 
			StarType.Life => cLife, 
			StarType.Imagination => cImagination, 
			StarType.Flexible => cFlexible, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public int GetSkill(HeroSkillLocation type)
	{
		return type switch
		{
			HeroSkillLocation.Q => skillQ, 
			HeroSkillLocation.R => skillR, 
			HeroSkillLocation.Identity => skillTrait, 
			HeroSkillLocation.Movement => skillMovement, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public void SetSkill(HeroSkillLocation type, int value)
	{
		switch (type)
		{
		case HeroSkillLocation.Q:
			skillQ = value;
			break;
		case HeroSkillLocation.R:
			skillR = value;
			break;
		case HeroSkillLocation.Identity:
			skillTrait = value;
			break;
		case HeroSkillLocation.Movement:
			skillMovement = value;
			break;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}

	public void PopulateLevelsByLocalSaveData()
	{
		PopulateLevels(StarType.Destruction);
		PopulateLevels(StarType.Life);
		PopulateLevels(StarType.Imagination);
		PopulateLevels(StarType.Flexible);
		void PopulateLevels(StarType type)
		{
			List<LoadoutStarItem> starList = GetStarList(type);
			for (int i = 0; i < starList.Count; i++)
			{
				if (!string.IsNullOrEmpty(starList[i].name))
				{
					LoadoutStarItem value = starList[i];
					value.level = (DewSave.profileMain.newStars.TryGetValue(value.name, out var value2) ? value2.level : 0);
					if (value.level <= 0)
					{
						value.name = null;
					}
					else
					{
						StarEffect byShortTypeName = DewResources.GetByShortTypeName<StarEffect>(value.name, default(ResourceLoadSettings));
						if ((UnityEngine.Object)(object)byShortTypeName != null && value.level > byShortTypeName.maxStarLevel)
						{
							value.level = byShortTypeName.maxStarLevel;
						}
						starList[i] = value;
					}
				}
			}
		}
	}

	public bool ContainsStar(string starName)
	{
		if (!cDestruction.Any((LoadoutStarItem i) => i.name == starName) && !cImagination.Any((LoadoutStarItem i) => i.name == starName) && !cLife.Any((LoadoutStarItem i) => i.name == starName))
		{
			return cFlexible.Any((LoadoutStarItem i) => i.name == starName);
		}
		return true;
	}

	public bool IsValidFor(string heroType)
	{
		return Validate_Imp(heroType, isRepair: false, checkStarLevels: true, null);
	}

	public bool Validate_Imp(string heroType, bool isRepair, bool checkStarLevels, DewProfile.HeroStarSlotUnlockData unlockedSlots)
	{
		bool isValid = true;
		Hero hero = DewResources.GetByShortTypeName<Hero>(heroType, default(ResourceLoadSettings));
		if ((UnityEngine.Object)(object)hero == null)
		{
			return false;
		}
		HeroSkill component = ((Component)(object)hero).GetComponent<HeroSkill>();
		CheckSkill(ref skillQ, component.loadoutQ.Values<SkillTrigger>());
		CheckSkill(ref skillR, component.loadoutR.Values<SkillTrigger>());
		CheckSkill(ref skillTrait, component.loadoutTrait.Values<SkillTrigger>());
		CheckSkill(ref skillMovement, component.loadoutMovement.Values<SkillTrigger>());
		if (!isValid && !isRepair)
		{
			return false;
		}
		ValidateConstellation(StarType.Destruction, ref cDestruction);
		ValidateConstellation(StarType.Life, ref cLife);
		ValidateConstellation(StarType.Imagination, ref cImagination);
		ValidateConstellation(StarType.Flexible, ref cFlexible);
		if (!isValid && !isRepair)
		{
			return false;
		}
		return isValid;
		void CheckSkill(ref int current, SkillTrigger[] skills)
		{
			if (current < 0 || current >= skills.Length)
			{
				if (isRepair)
				{
					current = Mathf.Clamp(current, 0, skills.Length - 1);
				}
				isValid = false;
			}
		}
		void ValidateConstellation(StarType type, ref List<LoadoutStarItem> stars)
		{
			if (stars == null)
			{
				isValid = false;
				if (!isRepair)
				{
					return;
				}
				stars = new List<LoadoutStarItem>();
			}
			HeroConstellationSettings constellationSettings = hero.GetConstellationSettings(type);
			if (isRepair)
			{
				while (stars.Count < constellationSettings.maxCount)
				{
					stars.Add(default);
					isValid = false;
				}
				while (stars.Count > constellationSettings.maxCount)
				{
					stars.RemoveAt(stars.Count - 1);
					isValid = false;
				}
			}
			else if (stars.Count != constellationSettings.maxCount)
			{
				isValid = false;
				return;
			}
			HashSet<string> hashSet = new HashSet<string>();
			for (int i = 0; i < stars.Count; i++)
			{
				if (!string.IsNullOrEmpty(stars[i].name))
				{
					if (!hashSet.Add(stars[i].name))
					{
						isValid = false;
						if (!isRepair)
						{
							break;
						}
						stars[i] = default;
					}
					else
					{
						StarEffect byShortTypeName = DewResources.GetByShortTypeName<StarEffect>(stars[i].name, default(ResourceLoadSettings));
						if ((UnityEngine.Object)(object)byShortTypeName == null || (byShortTypeName.heroType != null && byShortTypeName.heroType.Name != heroType))
						{
							isValid = false;
							if (!isRepair)
							{
								break;
							}
							stars[i] = default;
						}
						else
						{
							if (unlockedSlots != null)
							{
								List<int> list = unlockedSlots.Get(type);
								if (i >= constellationSettings.defaultCount && !list.Contains(i))
								{
									isValid = false;
									if (!isRepair)
									{
										break;
									}
									stars[i] = default;
									continue;
								}
							}
							if (checkStarLevels && byShortTypeName.type != StarType.Flexible && (stars[i].level < 1 || stars[i].level > byShortTypeName.maxStarLevel))
							{
								isValid = false;
								if (!isRepair)
								{
									break;
								}
								LoadoutStarItem value = stars[i];
								value.level = Mathf.Clamp(value.level, 1, byShortTypeName.maxStarLevel);
								stars[i] = value;
							}
						}
					}
				}
			}
		}
	}
}
