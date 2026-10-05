using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class DewGameResult
{
	public struct SkillData(HeroSkillLocation loc, SkillTrigger skill)
	{
		public uint netId = ((NetworkBehaviour)skill).netId;

		public HeroSkillLocation loc = loc;

		public SkillType type = skill.type;

		public string name = ((object)skill).GetType().Name;

		public int level = skill.level;

		public int maxCharges = skill.configs[0].maxCharges;

		public float cooldownTime = Mathf.Max(0f, skill.GetMaxCooldownTime(0));

		public Dictionary<string, string> capturedTooltipFields = new Dictionary<string, string>();

		public void CaptureTooltipFields(SkillTrigger skill)
		{
			DewLocalization.CaptureDescriptionExpressions(DewLocalization.GetSkillDescription(DewLocalization.GetSkillKey(((object)skill).GetType()), 0), capturedTooltipFields, new DewLocalization.DescriptionSettings
			{
				contextEntity = skill.owner,
				contextObject = (Object)(object)skill
			});
		}

		public SkillTrigger GetSkillTrigger()
		{
			return DewResources.GetByShortTypeName<SkillTrigger>(name, default(ResourceLoadSettings));
		}
	}

	public struct GemData(GemLocation loc, Gem gem)
	{
		public uint netId = ((NetworkBehaviour)gem).netId;

		public GemLocation location = loc;

		public string name = ((object)gem).GetType().Name;

		public int quality = gem.quality;

		public Dictionary<string, string> capturedTooltipFields = new Dictionary<string, string>();

		public void CaptureTooltipFields(Gem gem)
		{
			DewLocalization.CaptureDescriptionExpressions(DewLocalization.GetGemDescription(DewLocalization.GetGemKey(((object)gem).GetType())), capturedTooltipFields, new DewLocalization.DescriptionSettings
			{
				contextEntity = gem.owner,
				contextObject = (Object)(object)gem
			});
		}

		public Gem GetGem()
		{
			return DewResources.GetByShortTypeName<Gem>(name, default(ResourceLoadSettings));
		}
	}

	public class PlayerData
	{
		public bool isLocalPlayer;

		public string playerGuid;

		public string platform;

		public string platformID;

		public string playerProfileName;

		public string heroType;

		public float maxHealth;

		public float attackDamage;

		public float abilityPower;

		public float skillHaste;

		public float attackSpeed;

		public float fireAmp;

		public float armor;

		public float addedHp;

		public float critChance;

		public int level;

		public int kills;

		public int heroicBossKills;

		public int miniBossKills;

		public int hunterKills;

		public int totalGoldIncome;

		public int totalDreamDustIncome;

		public int totalStardustIncome;

		public int deaths;

		public int combatTime;

		public float dealtDamageToEnemies;

		public float maxDealtSingleDamageToEnemy;

		public float healToSelf;

		public float healToOthers;

		public float receivedDamage;

		public string causeOfDeathActor = "";

		public string causeOfDeathEntity = "";

		public HeroLoadoutData loadout;

		public Dictionary<string, string> capturedStarTooltipFields = new Dictionary<string, string>();

		public List<SkillData> skills = new List<SkillData>();

		public List<GemData> gems = new List<GemData>();

		public List<int> maxGemCounts = new List<int>();

		public bool TryGetSkillData(HeroSkillLocation type, out SkillData data)
		{
			int num = skills.FindIndex((SkillData s) => s.loc == type);
			if (num < 0)
			{
				data = default;
				return false;
			}
			data = skills[num];
			return true;
		}

		public bool TryGetGemData(GemLocation location, out GemData data)
		{
			int num = gems.FindIndex((GemData g) => g.location == location);
			if (num < 0)
			{
				data = default;
				return false;
			}
			data = gems[num];
			return true;
		}

		public DewPlayer GetPlayer()
		{
			return DewPlayer.gamePlayers.FirstOrDefault((DewPlayer h) => h.guid == playerGuid);
		}
	}

	public enum ResultType
	{
		Conceded = 0,
		GameOver = 1,
		PureWhiteDream = 100,
		StarlessPath = 101,
		UnknownFate = 1000
	}

	public string runId;

	public ResultType result;

	public long startTimestamp;

	public int elapsedGameTimeSeconds;

	public int visitedWorlds;

	public int visitedLocations;

	public string difficulty;

	public int limboDepth;

	public List<PlayerData> players = new List<PlayerData>();
}
