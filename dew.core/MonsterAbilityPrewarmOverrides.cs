using System.Collections.Generic;

public static class MonsterAbilityPrewarmOverrides
{
	public struct Entry
	{
		public string monsterName;

		public string abilityName;

		public int countPerMonster;

		public bool isBasicAttack;
	}

	public struct ForcePrewarmEntry
	{
		public string roomName;

		public string monsterName;

		public int count;
	}

	public struct GlobalEntry
	{
		public string abilityName;

		public float count;

		public bool perMonster;
	}

	public struct MirageSub
	{
		public string abilityName;

		public int countPerMirage;
	}

	public struct ConditionalEntry
	{
		public string modifierType;

		public string abilityName;

		public float count;

		public bool perMonster;
	}

	public static readonly ForcePrewarmEntry[] forceMonsterPrewarms = new ForcePrewarmEntry[7]
	{
		new ForcePrewarmEntry
		{
			roomName = "Room_Sky_Boss_0",
			monsterName = "Mon_Sky_BossNyx",
			count = 1
		},
		new ForcePrewarmEntry
		{
			roomName = "Room_Sky_Boss_0",
			monsterName = "Mon_Sky_StarSeed",
			count = 30
		},
		new ForcePrewarmEntry
		{
			roomName = "Room_Ink_Boss_0",
			monsterName = "Mon_Ink_BossWhiteNightHallucination",
			count = 3
		},
		new ForcePrewarmEntry
		{
			roomName = "Room_Forest_Boss_0",
			monsterName = "Mon_Forest_Hound",
			count = 12
		},
		new ForcePrewarmEntry
		{
			roomName = "Room_Forest_Boss_0",
			monsterName = "Mon_Forest_SpiderSpitter",
			count = 12
		},
		new ForcePrewarmEntry
		{
			roomName = "Room_Forest_Boss_0",
			monsterName = "Mon_Forest_SpiderWarrior",
			count = 12
		},
		new ForcePrewarmEntry
		{
			roomName = "Room_Forest_Boss_0",
			monsterName = "Mon_Forest_Treant",
			count = 12
		}
	};

	public static readonly Entry[] entries = new Entry[128]
	{
		new Entry
		{
			monsterName = "Mon_Forest_BossDemon",
			abilityName = "Ai_Mon_Forest_BossDemon_MainSkill_SpawnMissiles_Missile",
			countPerMonster = 200,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Forest_BossDemon",
			abilityName = "Ai_Mon_Forest_BossDemon_MainSkill_Stomp_Tree",
			countPerMonster = 200,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Forest_BossDemon",
			abilityName = "Ai_Mon_Forest_BossDemon_AltSkill_LineAtk",
			countPerMonster = 200,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Forest_BossDemon",
			abilityName = "Ai_Mon_Forest_BossDemon_AltSkill_Stomp_Tree",
			countPerMonster = 200,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Forest_BossDemon",
			abilityName = "Ai_Mon_Forest_BossDemon_SpecialAtk_Instance",
			countPerMonster = 20,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_LavaLand_Magmadon",
			abilityName = "Ai_Mon_LavaLand_Magmadon_Projectile",
			countPerMonster = 10,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_DarkCave_DarkElemental",
			abilityName = "Ai_Mon_DarkCave_DarkElemental_Barrage_Arrow",
			countPerMonster = 30,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_DarkCave_NightOlm",
			abilityName = "Ai_Mon_DarkCave_NightOlm_Atk_Projectile",
			countPerMonster = 40,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_DarkCave_BossSeeker",
			abilityName = "Se_Mon_DarkCave_BossSeeker_TunnelVision_Disappear",
			countPerMonster = 30,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_DarkCave_BossSeeker",
			abilityName = "Ai_Mon_DarkCave_BossSeeker_Atk_DelayedExplosion",
			countPerMonster = 200,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_DarkCave_BossSeeker",
			abilityName = "Ai_Mon_DarkCave_BossSeeker_Atk_Projectile",
			countPerMonster = 200,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_DarkCave_BossSeeker",
			abilityName = "Ai_Mon_DarkCave_BossSeeker_PurpleOrb_Instance",
			countPerMonster = 30,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_DarkCave_BossSeeker",
			abilityName = "Ai_Mon_DarkCave_BossSeeker_YellowDiagram_MainInstance",
			countPerMonster = 30,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_DarkCave_BossSeeker",
			abilityName = "Ai_Mon_DarkCave_BossSeeker_YellowDiagram_SubInstance",
			countPerMonster = 30,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_DarkCave_BossSeeker",
			abilityName = "Ai_Mon_DarkCave_BossSeeker_TunnelVision_SecondPoolAtk",
			countPerMonster = 30,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_LavaLand_FireElemental",
			abilityName = "Ai_Mon_LavaLand_FireElemental_ExplosionSub",
			countPerMonster = 3,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_LavaLand_BossInfernus",
			abilityName = "Ai_Mon_LavaLand_BossInfernus_WallStunKnockback",
			countPerMonster = 29,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_LavaLand_BossInfernus",
			abilityName = "Ai_Mon_LavaLand_BossInfernus_MeteorSpawner",
			countPerMonster = 12,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_LavaLand_BossInfernus",
			abilityName = "Ai_Mon_LavaLand_BossInfernus_Roar",
			countPerMonster = 12,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_LavaLand_BossInfernus",
			abilityName = "Ai_Mon_LavaLand_BossInfernus_Roar_Projectile",
			countPerMonster = 20,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_LavaLand_BossInfernus",
			abilityName = "Ai_Mon_LavaLand_BossInfernus_PowerBomb_FlamePillar",
			countPerMonster = 20,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_LavaLand_BossInfernus",
			abilityName = "Ai_Mon_LavaLand_BossInfernus_Stomp_Eruption",
			countPerMonster = 64,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_LavaLand_BossInfernus",
			abilityName = "Ai_Mon_LavaLand_BossInfernus_Meteor",
			countPerMonster = 300,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_LavaLand_BossInfernus",
			abilityName = "Ai_Mon_LavaLand_BossInfernus_Jump_Land",
			countPerMonster = 12,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_LavaLand_BossInfernus",
			abilityName = "Se_Mon_LavaLand_BossInfernus_Jump_Invul",
			countPerMonster = 12,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_LavaLand_InfernusPillar",
			abilityName = "Ai_Mon_LavaLand_InfernusPillar_Projectile",
			countPerMonster = 90,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Ink_GhostBlade",
			abilityName = "Ai_Mon_Ink_GhostBlade_SwiftStep_Atk",
			countPerMonster = 1,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Ink_GhostBlade",
			abilityName = "Ai_Mon_Ink_GhostBlade_SwiftStep_Projectile",
			countPerMonster = 1,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Ink_DivineAnimal",
			abilityName = "Ai_Mon_Ink_DivineAnimal_Missile_Sub",
			countPerMonster = 2,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Ink_DivineAnimal",
			abilityName = "Ai_Mon_Ink_DivineAnimal_Stomp",
			countPerMonster = 1,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Ink_BossWhiteNight",
			abilityName = "Ai_Mon_Ink_BossWhiteNight_RageInstance",
			countPerMonster = 129,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Ink_BossWhiteNight",
			abilityName = "Ai_Mon_Ink_BossWhiteNight_BuddhasPalm_Instance",
			countPerMonster = 45,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Ink_BossWhiteNight",
			abilityName = "Ai_Mon_Ink_BossWhiteNight_RageInstance_Rock",
			countPerMonster = 36,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Ink_BossWhiteNight",
			abilityName = "Ai_Mon_Ink_BossWhiteNight_DestructionWave_Wave",
			countPerMonster = 6,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Ink_BossWhiteNight",
			abilityName = "Ai_Mon_Ink_BossWhiteNight_Cataclysm_SafeZone",
			countPerMonster = 6,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Ink_BossWhiteNight",
			abilityName = "Ai_Mon_Ink_BossWhiteNight_Cataclysm_Instance",
			countPerMonster = 6,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Ink_BossWhiteNight",
			abilityName = "Ai_Mon_Ink_BossWhiteNight_AtkInstance",
			countPerMonster = 12,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Ink_BossWhiteNight",
			abilityName = "Ai_Mon_Ink_BossWhiteNight_AltAtk_Ready",
			countPerMonster = 20,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Ink_BossWhiteNight",
			abilityName = "Ai_Mon_Ink_BossWhiteNight_AltAtk",
			countPerMonster = 20,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Ink_BossWhiteNight",
			abilityName = "Se_Mon_Ink_BossWhiteNight_Cataclysm_NotSafe",
			countPerMonster = 6,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Ink_BossWhiteNight",
			abilityName = "Se_Mon_Ink_BossWhiteNight_Cataclysm",
			countPerMonster = 2,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_ParalyticFly",
			abilityName = "Ai_Mon_Despair_ParalyticFly_Atk",
			countPerMonster = 8,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Despair_ParalyticFly",
			abilityName = "Se_Mon_Despair_ParalyticFly_Atk_Instance",
			countPerMonster = 1,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_ParalyticFly",
			abilityName = "Se_Mon_Despair_ParalyticFly_Paralyze",
			countPerMonster = 1,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_UnstableRat",
			abilityName = "Ai_Mon_Despair_UnstableRat_Explosion",
			countPerMonster = 1,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_WretchedArtillery",
			abilityName = "Ai_Mon_Despair_WretchedArtillery_BarrageAtk_AoE",
			countPerMonster = 15,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_WretchedArtillery",
			abilityName = "Ai_Mon_Despair_WretchedArtillery_BarrageAtk_Missile",
			countPerMonster = 15,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_WretchedArtillery",
			abilityName = "Ai_Mon_Despair_WretchedArtillery_BarrageAtk",
			countPerMonster = 3,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_DreadBug",
			abilityName = "Ai_Mon_Despair_DreadBug_Atk",
			countPerMonster = 4,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Despair_DreadBug",
			abilityName = "Ai_Mon_Despair_DreadBug_Jump",
			countPerMonster = 2,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_DreadBug",
			abilityName = "Ai_Mon_Despair_DreadBug_JumpBack",
			countPerMonster = 3,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_Displacer",
			abilityName = "Ai_Mon_Despair_Displacer_Atk",
			countPerMonster = 6,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Despair_Displacer",
			abilityName = "Ai_Mon_Despair_Displacer_Blink",
			countPerMonster = 2,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_Displacer",
			abilityName = "Ai_Mon_Despair_Displacer_Dash",
			countPerMonster = 2,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_Displacer",
			abilityName = "Ai_Mon_Despair_Displacer_SpawnEgg",
			countPerMonster = 2,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_BossAzurak",
			abilityName = "Ai_Mon_Despair_BossAzurak_StompBlock_Artillery",
			countPerMonster = 90,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_BossAzurak",
			abilityName = "Ai_Mon_Despair_BossAzurak_Atk_SubSpawner",
			countPerMonster = 59,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_BossAzurak",
			abilityName = "Ai_Mon_Despair_BossAzurak_Atk_SubDamage",
			countPerMonster = 293,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_BossAzurak",
			abilityName = "Ai_Mon_Despair_BossAzurak_StompBlock_Artillery_Damage",
			countPerMonster = 90,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_BossAzurak",
			abilityName = "Se_Mon_Despair_BossAzurak_Roll_DeathInterrupt",
			countPerMonster = 3,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Despair_BossAzurak",
			abilityName = "Se_Mon_Despair_BossAzurak_Hide",
			countPerMonster = 3,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_SnowMountain_Scavenger",
			abilityName = "Ai_Mon_SnowMountain_Scavenger_Atk",
			countPerMonster = 14,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_SnowMountain_SnowWolf",
			abilityName = "Ai_Mon_SnowMountain_SnowWolf_Atk",
			countPerMonster = 7,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_SnowMountain_IceElemental",
			abilityName = "Ai_Mon_SnowMountain_IceElemental_Atk",
			countPerMonster = 7,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_SnowMountain_IceElemental",
			abilityName = "Ai_Mon_SnowMountain_IceElemental_IceBreaker_SubExplosion",
			countPerMonster = 4,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_SnowMountain_LivingShards",
			abilityName = "Ai_Mon_SnowMountain_LivingShards_Atk",
			countPerMonster = 3,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_SnowMountain_SnowWolf",
			abilityName = "Ai_Mon_SnowMountain_SnowWolf_Pounce",
			countPerMonster = 2,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_SnowMountain_BossSkoll",
			abilityName = "Ai_Mon_SnowMountain_BossSkoll_Atk",
			countPerMonster = 39,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_SnowMountain_BossSkoll",
			abilityName = "Ai_Mon_SnowMountain_BossSkoll_Atk_Damage",
			countPerMonster = 39,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_SnowMountain_BossSkoll",
			abilityName = "Ai_Mon_SnowMountain_BossSkoll_SummonArrow_Instance",
			countPerMonster = 38,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_SnowMountain_BossSkoll",
			abilityName = "Ai_Mon_SnowMountain_BossSkoll_AuraBlade",
			countPerMonster = 10,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_SnowMountain_BossSkoll",
			abilityName = "Ai_Mon_SnowMountain_BossSkoll_AuraBladeRain_Instance",
			countPerMonster = 180,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_SnowMountain_BossSkoll",
			abilityName = "Ai_Mon_SnowMountain_BossSkoll_AuraExplosion_AfterAtk",
			countPerMonster = 30,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_SnowMountain_BossSkoll",
			abilityName = "Ai_Mon_SnowMountain_BossSkoll_DeathFromAbove_Land",
			countPerMonster = 3,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_SnowMountain_BossSkoll",
			abilityName = "Se_Mon_SnowMountain_BossSkoll_DeathFromAbove_Invulnerable",
			countPerMonster = 3,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_Baam",
			abilityName = "Ai_Mon_Sky_Baam_Atk",
			countPerMonster = 13,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Sky_Baam",
			abilityName = "Ai_Mon_Sky_Baam_TeleportSequence",
			countPerMonster = 5,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_Baam",
			abilityName = "Ai_Mon_Sky_Baam_Teleport",
			countPerMonster = 5,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_LittleBaam",
			abilityName = "Ai_Mon_Sky_LittleBaam_Atk",
			countPerMonster = 5,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Sky_StarSeed",
			abilityName = "Ai_Mon_Sky_StarSeed_Atk",
			countPerMonster = 9,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Sky_LittleBaam",
			abilityName = "Ai_Mon_Sky_LittleBaam_Reposition",
			countPerMonster = 1,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_BigBaam_Regular",
			abilityName = "Ai_Mon_Sky_BigBaam_BeamAtk",
			countPerMonster = 14,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Sky_BigBaam_Regular",
			abilityName = "Ai_Mon_Sky_BigBaam_Melee",
			countPerMonster = 8,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Sky_BigBaam_Rooter",
			abilityName = "Se_Mon_Sky_BigBaam_Main_Root_Slowed",
			countPerMonster = 12,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_BigBaam_Rooter",
			abilityName = "Ai_Mon_Sky_BigBaam_Main_Root",
			countPerMonster = 6,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_BigBaam_Rooter",
			abilityName = "Ai_Mon_Sky_BigBaam_BeamAtk",
			countPerMonster = 16,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Sky_BossNyx",
			abilityName = "Ai_Mon_Sky_BossNyx_Atk",
			countPerMonster = 35,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Sky_BossNyx",
			abilityName = "Ai_Mon_Sky_BossNyx_Swipe",
			countPerMonster = 21,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_BossNyx",
			abilityName = "Ai_Mon_Sky_BossNyx_Teleport",
			countPerMonster = 38,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_BossNyx",
			abilityName = "Ai_Mon_Sky_BossNyx_PillarOfStars",
			countPerMonster = 35,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_BossNyx",
			abilityName = "Ai_Mon_Sky_BossNyx_StellarDash",
			countPerMonster = 30,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_BossNyx",
			abilityName = "Ai_Mon_Sky_BossNyx_Starfall",
			countPerMonster = 27,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_BossNyx",
			abilityName = "Ai_Mon_Sky_BossNyx_Starfall_Instance",
			countPerMonster = 40,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_BossNyx",
			abilityName = "Ai_Mon_Sky_BossNyx_CreateSeeds",
			countPerMonster = 11,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_BossNyx",
			abilityName = "Ai_Mon_Sky_BossNyx_Blackhole",
			countPerMonster = 3,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_BossNyx",
			abilityName = "Ai_Mon_Sky_BossNyx_AltTeleport",
			countPerMonster = 24,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_BossNyx",
			abilityName = "Ai_Mon_Sky_BossNyx_LineAtk",
			countPerMonster = 76,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_BossNyx",
			abilityName = "Ai_Mon_Sky_BossNyx_LaserAtk_Instance",
			countPerMonster = 40,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_StarSeed",
			abilityName = "Ai_Mon_Sky_StarSeed_SelfDestruct_Projectile",
			countPerMonster = 4,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Sky_StarSeed",
			abilityName = "Ai_Mon_Sky_StarSeed_SelfDestruct",
			countPerMonster = 1,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Adapt_Doom_Meteor_SubFireball",
			countPerMonster = 400,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Adapt_Atk",
			countPerMonster = 257,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Dash",
			countPerMonster = 95,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Rage_Atk",
			countPerMonster = 39,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Adaptation_Orb",
			countPerMonster = 35,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Adapt_Arbalest_Projectile",
			countPerMonster = 29,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_PhaseSwitcher_GreatSword",
			countPerMonster = 2,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_SpawnAttack",
			countPerMonster = 2,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Adapt_SmiteStorm_Smite",
			countPerMonster = 305,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Force_GoldRain_RainInstance",
			countPerMonster = 223,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Adapt_Starfall_Instance",
			countPerMonster = 168,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Force_Atk",
			countPerMonster = 155,
			isBasicAttack = true
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Force_JumpAttack_ConeInstance",
			countPerMonster = 135,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Force_GoldRain_Attack",
			countPerMonster = 129,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Force_Swipe",
			countPerMonster = 78,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Rage_Atk_FirstSwipe",
			countPerMonster = 38,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Rage_Atk_SecondSwipe",
			countPerMonster = 38,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Force_JumpAttack_CircleInstance",
			countPerMonster = 27,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Force_GoldRain_Attack_SubInstance",
			countPerMonster = 24,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Adapt_Doom_Meteor",
			countPerMonster = 24,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Rage_MassSilence_Instance",
			countPerMonster = 21,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Ai_Mon_Primus_BossPrimusAeron_Adapt_IceBlock_Damage",
			countPerMonster = 11,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Se_Mon_Primus_BossPrimusAeron_Force_GoldRain_Disappear",
			countPerMonster = 24,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Se_Mon_Primus_BossPrimusAeron_Adapt_Doom_Ongoing",
			countPerMonster = 3,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Se_Mon_Primus_BossPrimusAeron_PhaseSwitcher",
			countPerMonster = 2,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Se_Mon_Primus_BossPrimusAeron_Adaptation",
			countPerMonster = 2,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Se_Mon_Primus_BossPrimusAeron_Rage_DecayingShield",
			countPerMonster = 2,
			isBasicAttack = false
		},
		new Entry
		{
			monsterName = "Mon_Primus_BossPrimusAeron",
			abilityName = "Se_Primus_Pizza_Fly",
			countPerMonster = 3,
			isBasicAttack = false
		}
	};

	public static readonly GlobalEntry[] globalPrewarms = new GlobalEntry[5]
	{
		new GlobalEntry
		{
			abilityName = "Se_GenericEffectContainer",
			count = 50f,
			perMonster = false
		},
		new GlobalEntry
		{
			abilityName = "Se_Elm_Dark",
			count = 20f,
			perMonster = false
		},
		new GlobalEntry
		{
			abilityName = "Se_Elm_Light",
			count = 20f,
			perMonster = false
		},
		new GlobalEntry
		{
			abilityName = "Se_Elm_Cold",
			count = 20f,
			perMonster = false
		},
		new GlobalEntry
		{
			abilityName = "Se_Elm_Fire",
			count = 20f,
			perMonster = false
		}
	};

	public static readonly Dictionary<string, MirageSub[]> mirageSubEffects = new Dictionary<string, MirageSub[]>
	{
		{
			"Se_MirageSkin_Sanctification",
			new MirageSub[1]
			{
				new MirageSub
				{
					abilityName = "Se_MirageSkin_Sanctification_Protected",
					countPerMirage = 2
				}
			}
		},
		{
			"Se_MirageSkin_Delusion",
			new MirageSub[3]
			{
				new MirageSub
				{
					abilityName = "Se_MirageSkin_Delusion_Delusional",
					countPerMirage = 2
				},
				new MirageSub
				{
					abilityName = "Ai_MirageSkin_Delusion_Missile",
					countPerMirage = 2
				},
				new MirageSub
				{
					abilityName = "Ai_MirageSkin_Delusion_Missile_Explode",
					countPerMirage = 2
				}
			}
		},
		{
			"Se_MirageSkin_Oblivion",
			new MirageSub[2]
			{
				new MirageSub
				{
					abilityName = "Se_MirageSkin_Oblivion_Silenced",
					countPerMirage = 2
				},
				new MirageSub
				{
					abilityName = "Ai_MirageSkin_Oblivion_Orb",
					countPerMirage = 2
				}
			}
		},
		{
			"Se_MirageSkin_Oppression",
			new MirageSub[1]
			{
				new MirageSub
				{
					abilityName = "Ai_MirageSkin_Oppression_Explode",
					countPerMirage = 2
				}
			}
		},
		{
			"Se_MirageSkin_Pulverization",
			new MirageSub[1]
			{
				new MirageSub
				{
					abilityName = "Ai_MirageSkin_Pulverization_Stomp",
					countPerMirage = 2
				}
			}
		}
	};

	public static readonly ConditionalEntry[] conditionalPrewarms = new ConditionalEntry[72]
	{
		new ConditionalEntry
		{
			modifierType = "RoomMod_PureDream",
			abilityName = "Se_PureDream",
			count = 1f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_PureDream",
			abilityName = "Se_PureDream_Statue",
			count = 10f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_FallenStar",
			abilityName = "Se_FallenStar_MonsterReinforcement",
			count = 1f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_SpawnMiniBoss",
			abilityName = "Se_MiniBoss_OrbSpitter",
			count = 1f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_SpawnMiniBoss",
			abilityName = "Se_MiniBoss_BloodThorn",
			count = 1f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_SpawnMiniBoss",
			abilityName = "Se_MiniBoss_IceAura",
			count = 1f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_SpawnMiniBoss",
			abilityName = "Se_MiniBoss_SpinningArrow",
			count = 1f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_SpawnMiniBoss",
			abilityName = "Se_MiniBoss_UnstableExplosive",
			count = 1f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_SpawnMiniBoss",
			abilityName = "Ai_MiniBoss_OrbSpitter_Orb",
			count = 50f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_SpawnMiniBoss",
			abilityName = "Ai_MiniBoss_BloodThorn_Projectile",
			count = 20f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_SpawnMiniBoss",
			abilityName = "Ai_MiniBoss_BloodThorn_SubThorn",
			count = 20f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_SpawnMiniBoss",
			abilityName = "Ai_MiniBoss_SpinningArrow_Arrow",
			count = 50f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_SpawnMiniBoss",
			abilityName = "Ai_MiniBoss_IceAura_AoE",
			count = 40f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_SpawnMiniBoss",
			abilityName = "Se_MiniBoss_IceAura_Slow",
			count = 40f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_ChallengingFight",
			abilityName = "Se_MiniBoss_OrbSpitter",
			count = 2f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_ChallengingFight",
			abilityName = "Se_MiniBoss_BloodThorn",
			count = 2f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_ChallengingFight",
			abilityName = "Se_MiniBoss_IceAura",
			count = 2f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_ChallengingFight",
			abilityName = "Se_MiniBoss_SpinningArrow",
			count = 2f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_ChallengingFight",
			abilityName = "Se_MiniBoss_UnstableExplosive",
			count = 2f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_ChallengingFight",
			abilityName = "Ai_MiniBoss_OrbSpitter_Orb",
			count = 50f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_ChallengingFight",
			abilityName = "Ai_MiniBoss_BloodThorn_Projectile",
			count = 20f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_ChallengingFight",
			abilityName = "Ai_MiniBoss_BloodThorn_SubThorn",
			count = 20f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_ChallengingFight",
			abilityName = "Ai_MiniBoss_SpinningArrow_Arrow",
			count = 50f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_ChallengingFight",
			abilityName = "Ai_MiniBoss_IceAura_AoE",
			count = 40f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_ChallengingFight",
			abilityName = "Se_MiniBoss_IceAura_Slow",
			count = 40f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_CorruptedChaos_AuraOfPain",
			abilityName = "Se_MiniBoss_OrbSpitter",
			count = 2f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_CorruptedChaos_AuraOfPain",
			abilityName = "Se_MiniBoss_BloodThorn",
			count = 2f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_CorruptedChaos_AuraOfPain",
			abilityName = "Se_MiniBoss_IceAura",
			count = 2f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_CorruptedChaos_AuraOfPain",
			abilityName = "Se_MiniBoss_SpinningArrow",
			count = 2f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_CorruptedChaos_AuraOfPain",
			abilityName = "Se_MiniBoss_UnstableExplosive",
			count = 2f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_CorruptedChaos_AuraOfPain",
			abilityName = "Ai_MiniBoss_OrbSpitter_Orb",
			count = 50f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_CorruptedChaos_AuraOfPain",
			abilityName = "Ai_MiniBoss_BloodThorn_Projectile",
			count = 20f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_CorruptedChaos_AuraOfPain",
			abilityName = "Ai_MiniBoss_BloodThorn_SubThorn",
			count = 20f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_CorruptedChaos_AuraOfPain",
			abilityName = "Ai_MiniBoss_SpinningArrow_Arrow",
			count = 50f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_CorruptedChaos_AuraOfPain",
			abilityName = "Ai_MiniBoss_IceAura_AoE",
			count = 40f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_CorruptedChaos_AuraOfPain",
			abilityName = "Se_MiniBoss_IceAura_Slow",
			count = 40f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_CorruptedChaos_AuraOfPain",
			abilityName = "Se_LimboBossDecorator",
			count = 20f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_Hunted",
			abilityName = "Ai_HunterBuff_ShadowWalk",
			count = 0.5f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_Hunted",
			abilityName = "Se_HunterBuff",
			count = 0.5f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_Hunted",
			abilityName = "Se_HunterBuff_ShadowWalk_Disappear",
			count = 0.5f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_Hunted",
			abilityName = "Ai_HunterArtillery_Small",
			count = 30f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_Hunted",
			abilityName = "Ai_HunterArtillery_Big",
			count = 30f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_Ambush",
			abilityName = "Ai_Mon_Special_AmbushSpawner_Projectile",
			count = 30f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_GoldEverywhere",
			abilityName = "Se_GoldEverywhere",
			count = 1.5f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_GoldEverywhere",
			abilityName = "Pickup_LargeGoldOrb",
			count = 5f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_GoldEverywhere",
			abilityName = "Pickup_MediumGoldOrb",
			count = 5f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_GoldEverywhere",
			abilityName = "Pickup_SmallGoldOrb",
			count = 5f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_SmallSoul",
			abilityName = "Ai_Shrine_SmallSoul_Attack",
			count = 100f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_DarkCondensationZone",
			abilityName = "Se_DarkCondensationZone",
			count = 1f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_ArcticTerritory",
			abilityName = "Ai_ArcticTerritory",
			count = 10f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_ArcticTerritory",
			abilityName = "Se_ArcticTerritory",
			count = 4f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_EngulfedInFlame",
			abilityName = "Se_EngulfedInFlame",
			count = 1f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_UnstableRatSwarm",
			abilityName = "Ai_UnstableRatSwarm_Projectile",
			count = 60f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_RiskOfMeteors",
			abilityName = "Ai_RoomMod_RiskOfMeteors_Meteor",
			count = 150f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_StardustEverywhere",
			abilityName = "Ai_RoomMod_StardustEverywhere_Starfall",
			count = 100f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_AcceleratedTime",
			abilityName = "Se_AcceleratedTime",
			count = 1f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_UnstableVeilOfTime",
			abilityName = "Se_RoomMod_UnstableVeilOfTime_ModifyCooldowns",
			count = 1f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_WarpingField",
			abilityName = "Se_RoomMod_WarpingField_Unstable",
			count = 1.5f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_WarpingField",
			abilityName = "Se_RoomMod_WarpingField_Warp",
			count = 0.3f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_SymbioteHabitat",
			abilityName = "Se_RoomMod_Symbiote",
			count = 1f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_ToxicArea",
			abilityName = "Se_RoomMod_ToxicArea_Debuff",
			count = 1f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_BlackRain",
			abilityName = "Se_RoomMod_BlackRain",
			count = 1.5f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_GravityTraining",
			abilityName = "Se_GravityTraining",
			count = 1f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_CallOfTheRavenous",
			abilityName = "Se_CallOfTheRavenous",
			count = 1f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_LingeringAuraOfGuidance",
			abilityName = "Se_LingeringAuraOfGuidance",
			count = 1.5f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_DistantMemories",
			abilityName = "Se_DistantMemories",
			count = 1f,
			perMonster = true
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_VeilOfDark",
			abilityName = "Ai_VeilOfDark",
			count = 80f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_VeilOfDark",
			abilityName = "Se_VeilOfDark",
			count = 80f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_GazeOfErebos",
			abilityName = "Ai_Mon_Special_BossErebos_Gaze_Instance",
			count = 60f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_FireDevil",
			abilityName = "Ai_RoomMod_FireDevil_Tornado",
			count = 20f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_InkStrikeWarning",
			abilityName = "Ai_RoomMod_InkStrikeWarning_Artillery",
			count = 40f,
			perMonster = false
		},
		new ConditionalEntry
		{
			modifierType = "RoomMod_InkStrikeWarning",
			abilityName = "Se_RoomMod_InkStrikeWarning_Slow",
			count = 40f,
			perMonster = false
		}
	};

	public static IEnumerable<Entry> GetForMonster(string monsterName)
	{
		for (int i = 0; i < entries.Length; i++)
		{
			if (entries[i].monsterName == monsterName)
			{
				yield return entries[i];
			}
		}
	}

	public static bool TryGet(string monsterName, string abilityName, out Entry result)
	{
		for (int i = 0; i < entries.Length; i++)
		{
			if (entries[i].monsterName == monsterName && entries[i].abilityName == abilityName)
			{
				result = entries[i];
				return true;
			}
		}
		result = default;
		return false;
	}
}
