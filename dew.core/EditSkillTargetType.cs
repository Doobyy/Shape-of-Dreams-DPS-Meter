using System;

[Flags]
public enum EditSkillTargetType
{
	None = 0,
	Skill = 1,
	SkillEmptySlot = 2,
	Gem = 4,
	GemEmptySlot = 8
}
