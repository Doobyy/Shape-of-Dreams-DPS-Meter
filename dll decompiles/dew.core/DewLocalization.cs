using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using DewInternal;
using Mirror;
using UnityEngine;

public static class DewLocalization
{
	public struct EntityStats
	{
		public float attackDamage;

		public float abilityPower;

		public float armor;

		public float addedHp;

		public float critChance;

		public EntityStats(DewGameResult.PlayerData data)
		{
			attackDamage = data.attackDamage;
			abilityPower = data.abilityPower;
			armor = data.armor;
			addedHp = data.addedHp;
			critChance = data.critChance;
		}

		public EntityStats(Entity entity)
		{
			attackDamage = entity.Status.attackDamage;
			abilityPower = entity.Status.abilityPower;
			armor = entity.Status.armor;
			addedHp = entity.Status.GetBonusHealth();
			critChance = entity.Status.critChance;
		}
	}

	public struct DescriptionSettings
	{
		public Entity contextEntity;

		public UnityEngine.Object contextObject;

		public int[] starLevels;

		public int? currentLevel;

		public int? previousLevel;

		public bool showLevelScaling;

		public bool isSkillStar;

		public float? starStrength;

		public EntityStats? stats;

		public Dictionary<string, string> capturedFields;
	}

	private const string MainLocalization = "MainLocalization";

	private static DewLocalizationBuildData _buildData;

	private static DataTable _calculator = new DataTable
	{
		Locale = CultureInfo.InvariantCulture
	};

	private static StringBuilder _expression = new StringBuilder();

	private static readonly StringBuilder _textSb = new StringBuilder();

	private static readonly StringBuilder _expSb = new StringBuilder();

	private static readonly Dictionary<Type, string> _skillKeyByType = new Dictionary<Type, string>();

	private static readonly Dictionary<Type, string> _gemKeyByType = new Dictionary<Type, string>();

	public static DewLocalizationBuildData buildData
	{
		get
		{
			if ((UnityEngine.Object)(object)_buildData == null)
			{
				_buildData = Resources.Load<DewLocalizationBuildData>("MainLocalization");
				if ((UnityEngine.Object)(object)_buildData == null)
				{
					throw new Exception("Could not load main DewLocalizationBuildData!");
				}
				_buildData.InitForRuntime();
			}
			return _buildData;
		}
	}

	public static PerLanguageLocalizationData data
	{
		get
		{
			if (DewSave.profileMain == null || string.IsNullOrEmpty(DewSave.profileMain.language))
			{
				return buildData.dataByLanguage["en-US"];
			}
			return buildData.dataByLanguage[DewSave.profileMain.language];
		}
	}

	public static void Initialize()
	{
		Resources.UnloadUnusedAssets();
		_buildData = null;
		_ = buildData;
	}

	private static List<LocaleNode> DebugNodeList(string message)
	{
		return new List<LocaleNode>
		{
			new LocaleNode
			{
				type = LocaleNodeType.Text,
				textData = message
			}
		};
	}

	public static string ConvertDescriptionNodesToText(IReadOnlyList<LocaleNode> nodes, DescriptionSettings settings)
	{
		bool flag = (UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.softInstance != null;
		bool flag2 = false;
		if (flag)
		{
			if (settings.contextObject is SkillTrigger skillTrigger && (UnityEngine.Object)(object)((NetworkBehaviour)skillTrigger).netIdentity != null && (UnityEngine.Object)(object)skillTrigger.owner != null)
			{
				flag2 = true;
			}
			else if (settings.contextObject is Gem gem && (UnityEngine.Object)(object)((NetworkBehaviour)gem).netIdentity != null && (UnityEngine.Object)(object)gem.owner != null)
			{
				flag2 = true;
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag3 = Application.isPlaying && DewInput.GetButton(DewSave.profileMain.controls.showDetails, checkGameAreaForMouse: false);
		if (flag3)
		{
			settings.showLevelScaling = true;
		}
		bool flag4 = false;
		bool flag5 = false;
		foreach (LocaleNode node in nodes)
		{
			if (node.type == LocaleNodeType.Text)
			{
				if ((!flag4 || flag) && (!flag5 || flag2))
				{
					stringBuilder.Append(node.textData);
				}
			}
			else if (node.type == LocaleNodeType.Tag)
			{
				if (node.textData == "equipped")
				{
					flag5 = true;
				}
				if (node.textData == "/equipped")
				{
					flag5 = false;
				}
				if (node.textData == "ingame")
				{
					flag4 = true;
				}
				if (node.textData == "/ingame")
				{
					flag4 = false;
				}
			}
			else if ((!flag4 || flag) && (!flag5 || flag2))
			{
				stringBuilder.Append(EvaluateAndRenderExpression(node.expressionData, settings, flag3));
			}
		}
		return stringBuilder.ToString();
	}

	public static void CaptureDescriptionExpressions(IReadOnlyList<LocaleNode> nodes, Dictionary<string, string> data, DescriptionSettings settings)
	{
		foreach (LocaleNode node in nodes)
		{
			if (node.type == LocaleNodeType.Expression)
			{
				data[node.expressionData.raw] = EvaluateAndRenderExpression(node.expressionData, settings, shouldShowDetail: false);
			}
		}
	}

	public static string EvaluateAndRenderExpression(ExpressionData exp, DescriptionSettings settings, bool shouldShowDetail)
	{
		if (settings.capturedFields != null)
		{
			return CollectionExtensions.GetValueOrDefault<string, string>((IReadOnlyDictionary<string, string>)settings.capturedFields, exp.raw, "<color=grey>???</color>");
		}
		if (!settings.stats.HasValue && (UnityEngine.Object)(object)settings.contextEntity != null)
		{
			settings.stats = new EntityStats(settings.contextEntity);
		}
		if (!settings.stats.HasValue && !settings.starStrength.HasValue)
		{
			shouldShowDetail = true;
		}
		float num = Evaluate(0, 0f, 0f, 0f, 0f, 0f, 1f);
		bool hasAdFactor = num != Evaluate(0, 100f, 0f, 0f, 0f, 0f, 1f);
		bool hasApFactor = num != Evaluate(0, 0f, 100f, 0f, 0f, 0f, 1f);
		bool hasArmorFactor = num != Evaluate(0, 0f, 0f, 100f, 0f, 0f, 1f);
		bool hasAddedHpFactor = num != Evaluate(0, 0f, 0f, 0f, 100f, 0f, 1f);
		bool hasCritChanceFactor = num != Evaluate(0, 0f, 0f, 0f, 0f, 100f, 1f);
		try
		{
			if (settings.starLevels != null && settings.starLevels.Length >= 2)
			{
				string text = RenderForLevel(settings.starLevels[0], settings.starStrength ?? 1f, onlyShowFinalValue: false, colorize: true, hideSprites: false);
				bool flag = false;
				for (int i = 1; i < settings.starLevels.Length; i++)
				{
					if (text != RenderForLevel(settings.starLevels[i], settings.starStrength ?? 1f, onlyShowFinalValue: false, colorize: true, hideSprites: false))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					string text2 = "";
					for (int j = 0; j < settings.starLevels.Length; j++)
					{
						settings.showLevelScaling = settings.starLevels.Length != 1 && j == settings.starLevels.Length - 1;
						if (settings.currentLevel.HasValue)
						{
							text2 = ((settings.currentLevel == settings.starLevels[j]) ? (text2 + RenderForLevel(settings.starLevels[j], settings.starStrength ?? 1f, onlyShowFinalValue: false, colorize: true, hideSprites: false)) : (text2 + "<color=grey>" + RenderForLevel(settings.starLevels[j], settings.starStrength ?? 1f, onlyShowFinalValue: false, colorize: true, hideSprites: false) + "</color>"));
						}
						else
						{
							text2 += RenderForLevel(settings.starLevels[j], settings.starStrength ?? 1f, onlyShowFinalValue: false, colorize: true, hideSprites: false);
						}
						if (j != settings.starLevels.Length - 1)
						{
							text2 += "<color=grey> / </color>";
						}
					}
					return text2 + "<sprite=5>";
				}
			}
			int num2;
			if (settings.currentLevel.HasValue)
			{
				num2 = settings.currentLevel.Value;
			}
			else
			{
				num2 = ((!(settings.contextObject is Gem gem)) ? ((!(settings.contextObject is SkillTrigger skillTrigger)) ? 1 : skillTrigger.level) : gem.effectiveLevel);
			}
			if (settings.previousLevel.HasValue)
			{
				string text3 = RenderForLevel(settings.previousLevel.Value, settings.starStrength ?? 1f, !shouldShowDetail, colorize: false, hideSprites: false);
				string text4 = RenderForLevel(num2, settings.starStrength ?? 1f, !shouldShowDetail, colorize: false, hideSprites: false);
				if (text3 != text4)
				{
					string after = RenderForLevel(num2, settings.starStrength ?? 1f, !shouldShowDetail, colorize: false, !shouldShowDetail);
					return RenderChangedValue(text3, after) + "<sprite=5>";
				}
			}
			if (settings.starStrength.HasValue)
			{
				string text5 = RenderForLevel(num2, settings.starStrength ?? 1f, onlyShowFinalValue: true, colorize: false, hideSprites: false);
				string text6 = RenderForLevel(num2, 1f, onlyShowFinalValue: true, colorize: false, hideSprites: false);
				if (text5 != text6)
				{
					return RenderWeakenedValue(RenderForLevel(num2, 1f, onlyShowFinalValue: true, colorize: false, hideSprites: true), text5);
				}
			}
			bool flag2 = Evaluate(num2, 1f, 1f, 1f, 1f, 1f, 1f) != Evaluate(num2 + 200, 1f, 1f, 1f, 1f, 1f, 1f);
			string text7 = RenderForLevel(num2, settings.starStrength ?? 1f, !shouldShowDetail, colorize: true, hideSprites: false);
			if ((flag2 & shouldShowDetail) && !settings.isSkillStar)
			{
				return " " + text7 + "<sprite=5> ";
			}
			return text7;
		}
		catch (DewExpressionException ex)
		{
			Debug.LogException(ex);
			return ex.Message;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return "!exp";
		}
		static string ColorizeByFactorGradient(string input, string gradientStr, string tint = "#fff")
		{
			if (string.IsNullOrEmpty(gradientStr))
			{
				return input;
			}
			return "<color=" + tint + "><gradient=sv_" + gradientStr + ">" + input + "</gradient></color>";
		}
		float Evaluate(int level, float attackDamage, float abilityPower, float armor, float addedHp, float critChance, float starStrength)
		{
			_expression.Clear();
			foreach (ExpressionChildNode node in exp.nodes)
			{
				if (node.type == ExpressionChildNodeType.Expression)
				{
					_expression.Append(node.value);
				}
				else
				{
					if (!buildData.fieldInfos.TryGetValue(node.value, out var value))
					{
						throw new DewExpressionException("!noinfo(" + node.value + ")");
					}
					object obj = null;
					if (value.requiresTarget)
					{
						string text8 = node.value.Split("::", StringSplitOptions.None)[0];
						UnityEngine.Object obj2;
						if (text8.StartsWith("Star_"))
						{
							obj2 = null;
						}
						else
						{
							obj2 = ((!(settings.contextObject != null) || !(settings.contextObject.GetType().Name == text8)) ? DewResources.GetByShortTypeName(text8, ResourceLoadSettings.Light) : settings.contextObject);
						}
						obj = obj2;
					}
					try
					{
						ScalingValue.levelOverride = level;
						Func<object, object>[] valueGetters = value.valueGetters;
						for (int k = 0; k < valueGetters.Length; k++)
						{
							obj = valueGetters[k](obj);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						throw new DewExpressionException("!field(" + node.value + ")");
					}
					finally
					{
						ScalingValue.levelOverride = null;
					}
					if (obj is StarScalingValue starScalingValue)
					{
						_expression.Append(starScalingValue.GetValue(level, starStrength).ToString(CultureInfo.InvariantCulture));
					}
					else if (obj is ScalingValue scalingValue)
					{
						_expression.Append(scalingValue.GetValue(level, attackDamage, abilityPower, armor, addedHp, critChance).ToString(CultureInfo.InvariantCulture));
					}
					else if (obj is float num3)
					{
						_expression.Append(num3.ToString(CultureInfo.InvariantCulture));
					}
					else if (obj is double num4)
					{
						_expression.Append(num4.ToString(CultureInfo.InvariantCulture));
					}
					else if (obj is int num5)
					{
						_expression.Append(num5.ToString(CultureInfo.InvariantCulture));
					}
					else if (obj is int[] arr)
					{
						_expression.Append(arr.GetClamped(level - 1).ToString(CultureInfo.InvariantCulture));
					}
					else if (obj is float[] arr2)
					{
						_expression.Append(arr2.GetClamped(level - 1).ToString(CultureInfo.InvariantCulture));
					}
					else if (obj is double[] arr3)
					{
						_expression.Append(((float)arr3.GetClamped(level - 1)).ToString(CultureInfo.InvariantCulture));
					}
					else
					{
						_expression.Append(obj);
					}
				}
			}
			return float.Parse(_calculator.Compute(_expression.ToString(), null).ToString());
		}
		string GetCompositeGradientStr()
		{
			string text8 = "";
			if (hasAdFactor)
			{
				text8 += "ad";
			}
			if (hasApFactor)
			{
				text8 += "ap";
			}
			if (hasArmorFactor)
			{
				text8 += "arm";
			}
			if (hasAddedHpFactor)
			{
				text8 += "ahp";
			}
			if (hasCritChanceFactor)
			{
				text8 += "critp";
			}
			return text8;
		}
		string RenderForLevel(int targetLvl, float targetStarStrength, bool onlyShowFinalValue, bool colorize, bool hideSprites)
		{
			if (!settings.stats.HasValue)
			{
				onlyShowFinalValue = false;
			}
			EntityStats valueOrDefault = settings.stats.GetValueOrDefault();
			if (onlyShowFinalValue)
			{
				string text8 = "";
				if (!hideSprites)
				{
					if (hasAdFactor)
					{
						text8 += "<sprite=2>";
					}
					if (hasApFactor)
					{
						text8 += "<sprite=1>";
					}
					if (hasArmorFactor)
					{
						text8 += "<sprite=7>";
					}
					if (hasAddedHpFactor)
					{
						text8 += "<sprite=3>";
					}
					if (hasCritChanceFactor)
					{
						text8 += "<sprite=10>";
					}
				}
				string text9 = Evaluate(targetLvl, valueOrDefault.attackDamage, valueOrDefault.abilityPower, valueOrDefault.armor, valueOrDefault.addedHp, valueOrDefault.critChance, targetStarStrength).ToString(exp.format);
				if (colorize)
				{
					text9 = ColorizeByFactorGradient(text9, GetCompositeGradientStr());
				}
				return text8 + text9;
			}
			string finalRender = "";
			float baseValue = Evaluate(targetLvl, 0f, 0f, 0f, 0f, 0f, targetStarStrength);
			if (Mathf.Abs(baseValue) > 1E-05f)
			{
				AddPlusIfNeeded();
				finalRender += baseValue.ToString(exp.format);
			}
			if (hasAdFactor)
			{
				AppendRender("<sprite=2>", Evaluate(targetLvl, 1f, 0f, 0f, 0f, 0f, targetStarStrength), Evaluate(targetLvl, valueOrDefault.attackDamage, 0f, 0f, 0f, 0f, targetStarStrength), "ad");
			}
			if (hasApFactor)
			{
				AppendRender("<sprite=1>", Evaluate(targetLvl, 0f, 1f, 0f, 0f, 0f, targetStarStrength), Evaluate(targetLvl, 0f, valueOrDefault.abilityPower, 0f, 0f, 0f, targetStarStrength), "ap");
			}
			if (hasArmorFactor)
			{
				AppendRender("<sprite=7>", Evaluate(targetLvl, 0f, 0f, 1f, 0f, 0f, targetStarStrength), Evaluate(targetLvl, 0f, 0f, valueOrDefault.armor, 0f, 0f, targetStarStrength), "arm");
			}
			if (hasAddedHpFactor)
			{
				AppendRender("<color=#7cf248>" + GetUIValue("BonusHealth_Sprite") + "</color>", Evaluate(targetLvl, 0f, 0f, 0f, 1f, 0f, targetStarStrength), Evaluate(targetLvl, 0f, 0f, 0f, valueOrDefault.addedHp, 0f, targetStarStrength), "ahp");
			}
			if (hasCritChanceFactor)
			{
				AppendRender("<sprite=10>", Evaluate(targetLvl, 0f, 0f, 0f, 0f, 1f, targetStarStrength) * 0.01f, Evaluate(targetLvl, 0f, 0f, 0f, 0f, valueOrDefault.critChance, targetStarStrength), "critp");
			}
			if (string.IsNullOrEmpty(finalRender))
			{
				finalRender = baseValue.ToString(exp.format);
			}
			return finalRender;
			void AddPlusIfNeeded()
			{
				if (!string.IsNullOrEmpty(finalRender))
				{
					finalRender += "<color=white><size=80%> + </size></color>";
				}
			}
			void AppendRender(string sprite, float factorEval, float finalEval, string gradient)
			{
				AddPlusIfNeeded();
				if (!hideSprites)
				{
					finalRender += sprite;
				}
				string text10 = (factorEval - baseValue).ToString("P0");
				if (colorize)
				{
					text10 = ColorizeByFactorGradient(text10, gradient);
				}
				finalRender += text10;
				if (settings.stats.HasValue)
				{
					string text11 = " (" + (finalEval - baseValue).ToString(exp.format) + ")";
					if (colorize)
					{
						text11 = ColorizeByFactorGradient(text11, gradient);
					}
					finalRender += text11;
				}
			}
		}
	}

	public static string RenderWeakenedValue(string prev, string after)
	{
		return "<color=#888>" + prev + "</color><sprite=8><color=#ff9999>" + after + "</color>";
	}

	public static string RenderChangedValue(string prev, string after)
	{
		return "<color=#888>" + prev + "</color><sprite=0><color=#fffd5c>" + after + "</color>";
	}

	public static string GetSkillLevelTemplate(int level, int? toLevel = null)
	{
		if (toLevel.HasValue)
		{
			return "{0}" + (GetSkillLevelSuffix(level).StartsWith(" ") ? " " : "") + RenderChangedValue(GetSkillLevelSuffix(level).Trim(), GetSkillLevelSuffix(toLevel.Value).Trim());
		}
		return "{0}" + GetSkillLevelSuffix(level);
	}

	public static string GetSkillLevelSuffix(int level)
	{
		level = Mathf.Max(level, 1);
		if (level < 5)
		{
			return new string('+', level - 1) ?? "";
		}
		return $" +{level - 1}";
	}

	public static string HighlightKeywords(string input)
	{
		if (input == null)
		{
			input = "";
		}
		return input.Replace("[", "<color=yellow>").Replace("]", "</color>");
	}

	private static bool TryGetConstValue(Type type, string fieldName, out string value)
	{
		if (type == null)
		{
			value = null;
			return false;
		}
		FieldInfo field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		if (field == null || !field.IsLiteral || field.IsInitOnly)
		{
			value = null;
			return false;
		}
		value = Dew.ConvertToStringInvariantCulture(field.GetRawConstantValue());
		return true;
	}

	public static string ProcessGenericBacktickedString(string text, object refObject, string lang = null)
	{
		_textSb.Clear();
		DewLocalizationNodeParser.ParseBacktickedString(text, (string normal) =>
		{
			_textSb.Append(normal);
		}, (string tag) =>
		{
			_textSb.Append("<" + tag + ">");
		}, (string expression) =>
		{
			_expSb.Clear();
			string[] array = expression.Split("|", StringSplitOptions.None);
			string format = GetFormat((array.Length > 1) ? array[1] : null, lang);
			DewLocalizationNodeParser.ParseExpression(array[0], (string exp) =>
			{
				_expSb.Append(exp);
			}, (string field) =>
			{
				string text3;
				FieldInfo field2;
				PropertyInfo property;
				Type type2;
				if (field.Contains("::"))
				{
					string[] array2 = field.Split("::", StringSplitOptions.None);
					string text2 = array2[0];
					text3 = array2[1];
					if (!Dew.TryGetTypeFromShortName(text2, out var type))
					{
						Error("notype-" + text2);
						return;
					}
					field2 = type.GetField(text3, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					property = type.GetProperty(text3, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					type2 = type;
				}
				else
				{
					if (refObject == null)
					{
						Error("context-" + field);
						return;
					}
					text3 = field;
					type2 = ((refObject is Type) ? ((Type)refObject) : refObject.GetType());
					field2 = type2.GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					property = type2.GetProperty(text3, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				}
				if (TryGetConstValue(type2, text3, out var value))
				{
					_expSb.Append(value);
				}
				else if (field2 == null && property == null)
				{
					Error("field-" + field);
				}
				else if (refObject != null && type2.IsInstanceOfType(refObject))
				{
					object value2 = ((field2 == null) ? property.GetValue(refObject) : field2.GetValue(refObject));
					_expSb.Append(Dew.ConvertToStringInvariantCulture(value2));
				}
				else if (DewResources.GetByShortTypeName(type2.Name, ResourceLoadSettings.Light) != null)
				{
					object value3 = ((field2 == null) ? property.GetValue(refObject) : field2.GetValue(refObject));
					_expSb.Append(Dew.ConvertToStringInvariantCulture(value3));
				}
				else
				{
					Error("fail-" + field);
				}
			});
			try
			{
				object obj = _calculator.Compute(_expSb.ToString(), null);
				_textSb.Append(float.Parse(obj.ToString()).ToString(format));
			}
			catch
			{
				_textSb.Append(_expSb.ToString());
			}
		});
		return _textSb.ToString();
		static void Error(string err)
		{
			Debug.LogWarning("!" + err + "!");
			_expSb.Append("!" + err + "!");
		}
	}

	public static string GetRecommendedSupportedLanguage()
	{
		string[] source = buildData.dataByLanguage.Keys.ToArray();
		string name = CultureInfo.CurrentUICulture.Name;
		if (source.Contains(name))
		{
			return name;
		}
		if (name.Equals("zh-Hans", StringComparison.InvariantCultureIgnoreCase))
		{
			return "zh-CN";
		}
		if (name.Equals("zh-SG", StringComparison.InvariantCultureIgnoreCase))
		{
			return "zh-CN";
		}
		if (name.Equals("zh-Hant", StringComparison.InvariantCultureIgnoreCase))
		{
			return "zh-TW";
		}
		if (name.Equals("zh-HK", StringComparison.InvariantCultureIgnoreCase))
		{
			return "zh-TW";
		}
		if (name.Equals("zh-MO", StringComparison.InvariantCultureIgnoreCase))
		{
			return "zh-TW";
		}
		string currentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
		string text = source.FirstOrDefault((string culture) => culture.StartsWith(currentLanguage));
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return "en-US";
	}

	public static string GetFormat(string formatString, string lang = null)
	{
		if (lang == null)
		{
			lang = DewSave.profileMain.language;
		}
		if (string.IsNullOrEmpty(formatString))
		{
			return "#,##0";
		}
		switch (formatString)
		{
		case "time":
			return "#,##0.#";
		case "percent":
			if (lang.StartsWith("tr-"))
			{
				return "'%'#,##0";
			}
			return "#,##0'%'";
		case "percent*100":
			return "P0";
		default:
			return formatString;
		}
	}

	public static bool TryGetUIValue(string key, out string value)
	{
		if (key == null)
		{
			value = null;
			return false;
		}
		if (data.ui.TryGetValue(key, out value))
		{
			return true;
		}
		return false;
	}

	public static string GetUIValue(string key)
	{
		if (key == null)
		{
			return "ui.!null";
		}
		if (data.ui.TryGetValue(key, out var value))
		{
			return value;
		}
		return "ui.!" + key;
	}

	public static string GetSkillKey(Type skillType)
	{
		if (!_skillKeyByType.TryGetValue(skillType, out var value))
		{
			value = (_skillKeyByType[skillType] = GetSkillKey(skillType.Name));
		}
		return value;
	}

	public static string GetSkillKey(string skillType)
	{
		return skillType.Substring(3);
	}

	public static string GetSkillName(SkillTrigger skill, int configIndex)
	{
		return GetSkillName(GetSkillKey(((object)skill).GetType()), configIndex);
	}

	public static string GetSkillName(string key, int configIndex)
	{
		if (!data.skills.TryGetValue(key, out var value))
		{
			return $"skills.!{key}[{configIndex}].name";
		}
		if (configIndex >= value.configs.Count)
		{
			return $"skills.{key}[!{configIndex}].name";
		}
		return value.configs[configIndex].name;
	}

	public static string GetSkillShortDesc(string key, int configIndex)
	{
		if (!data.skills.TryGetValue(key, out var value))
		{
			return $"skills.!{key}[{configIndex}].short";
		}
		if (configIndex >= value.configs.Count)
		{
			return $"skills.{key}[!{configIndex}].short";
		}
		return value.configs[configIndex].shortDescription;
	}

	public static List<LocaleNode> GetSkillDescription(string key, int configIndex)
	{
		if (!data.skills.TryGetValue(key, out var value))
		{
			return DebugNodeList($"skills.!{key}[{configIndex}].desc");
		}
		if (configIndex >= value.configs.Count)
		{
			return DebugNodeList($"skills.{key}[!{configIndex}].desc");
		}
		return value.configs[configIndex].description;
	}

	public static string GetSkillMemory(string key)
	{
		if (!data.skills.TryGetValue(key, out var value))
		{
			return "skills.!" + key + ".memory";
		}
		return value.memory;
	}

	public static string GetSkillNameKey(SkillTrigger skill, int configIndex)
	{
		string name = ((object)skill).GetType().Name;
		if (name.StartsWith("St_", StringComparison.OrdinalIgnoreCase))
		{
			return string.Format("Skill.{0}.{1}.name", name.Substring("St_".Length), configIndex);
		}
		return name;
	}

	public static string GetSkillDescriptionKey(SkillTrigger skill, int configIndex)
	{
		string name = ((object)skill).GetType().Name;
		if (name.StartsWith("St_", StringComparison.OrdinalIgnoreCase))
		{
			return string.Format("Skill.{0}.{1}.desc", name.Substring("St_".Length), configIndex);
		}
		return name;
	}

	public static string GetSkillLevelKey(int level)
	{
		return $"UI.SkillLevel{level}";
	}

	public static string GetGemKey(Type gemType)
	{
		if (!_gemKeyByType.TryGetValue(gemType, out var value))
		{
			value = (_gemKeyByType[gemType] = GetGemKey(gemType.Name));
		}
		return value;
	}

	public static string GetGemKey(string gemType)
	{
		return gemType.Substring(4);
	}

	public static string GetGemName(Gem gem)
	{
		return GetGemName(GetGemKey(((object)gem).GetType().Name));
	}

	public static string GetGemName(string key)
	{
		if (!data.gems.TryGetValue(key, out var value))
		{
			return "gems.!" + key + ".name";
		}
		return value.name;
	}

	public static string GetGemShortDescription(string key)
	{
		if (!data.gems.TryGetValue(key, out var value))
		{
			return "gems.!" + key + ".short";
		}
		return value.shortDescription;
	}

	public static string GetGemTemplate(string key)
	{
		if (!data.gems.TryGetValue(key, out var value))
		{
			return "gems.!" + key + ".template {0}";
		}
		return value.template;
	}

	public static IReadOnlyList<LocaleNode> GetGemDescription(string key)
	{
		if (!data.gems.TryGetValue(key, out var value))
		{
			return DebugNodeList("gems.!" + key + ".desc");
		}
		return value.description;
	}

	public static string GetGemMemory(string key)
	{
		if (!data.gems.TryGetValue(key, out var value))
		{
			return "gems.!" + key + ".memory";
		}
		return value.memory;
	}

	public static string GetGemNameKey(Gem gem)
	{
		string name = ((object)gem).GetType().Name;
		if (name.StartsWith("Gem_", StringComparison.OrdinalIgnoreCase))
		{
			return "Gem_." + name.Substring("Gem_".Length) + ".name";
		}
		return name;
	}

	public static string GetGemDescriptionKey(Gem gem)
	{
		string name = ((object)gem).GetType().Name;
		if (name.StartsWith("Gem_", StringComparison.OrdinalIgnoreCase))
		{
			return "Gem_." + name.Substring("Gem_".Length) + ".desc";
		}
		return name;
	}

	public static string GetArtifactKey(Type artifactType)
	{
		return GetArtifactKey(artifactType.Name);
	}

	public static string GetArtifactKey(string artifactType)
	{
		return artifactType.Substring(9);
	}

	public static string GetAchievementName(string key)
	{
		if (!data.achievements.TryGetValue(key, out var value))
		{
			return "achievements.!" + key + ".name";
		}
		return value.name;
	}

	public static string GetAchievementDescription(string key)
	{
		if (!data.achievements.TryGetValue(key, out var value))
		{
			return "achievements.!" + key + ".desc";
		}
		return ProcessGenericBacktickedString(value.description, Dew.achievementsByName[key]);
	}

	public static string GetStarName(Type type)
	{
		return GetStarName(type.Name);
	}

	public static string GetStarName(string key)
	{
		if (!data.stars.TryGetValue(key, out var value))
		{
			return "stars.!" + key + ".name";
		}
		return value.name;
	}

	public static string GetStarLore(string key)
	{
		if (!data.stars.TryGetValue(key, out var value))
		{
			return "stars.!" + key + ".lore";
		}
		return value.lore;
	}

	public static List<LocaleNode> GetStarDescription(Type type)
	{
		return GetStarDescription(type.Name);
	}

	public static List<LocaleNode> GetStarDescription(string key)
	{
		if (!data.stars.TryGetValue(key, out var value))
		{
			return DebugNodeList("stars.!" + key + ".desc");
		}
		return value.description;
	}

	public static string GetCurseName(string key)
	{
		if (!data.curses.TryGetValue(key, out var value))
		{
			return "curses.!" + key + ".name";
		}
		return value.name;
	}

	public static List<LocaleNode> GetCurseDescription(string key)
	{
		if (!data.curses.TryGetValue(key, out var value))
		{
			return DebugNodeList("curses.!" + key + ".desc");
		}
		return value.description;
	}

	public static List<LocaleNode> GetCurseShortDescription(string key)
	{
		if (!data.curses.TryGetValue(key, out var value))
		{
			return DebugNodeList("curses.!" + key + ".short");
		}
		return value.shortDesc;
	}

	public static string GetCurseKey(string name)
	{
		return name.Substring(9);
	}

	public static ConversationData GetConversationData(string key)
	{
		if (key == null)
		{
			return null;
		}
		if (data.conversations.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public static string GetArtifactName(string key)
	{
		if (key == null)
		{
			return "artifacts.!null.name";
		}
		if (!data.artifacts.TryGetValue(key, out var value))
		{
			return "artifacts.!" + key + ".name";
		}
		return value.name;
	}

	public static string GetArtifactStory(string key)
	{
		if (key == null)
		{
			return "artifacts.!null.story";
		}
		if (!data.artifacts.TryGetValue(key, out var value))
		{
			return "artifacts.!" + key + ".story";
		}
		return value.story;
	}

	public static string[] GetArtifactShortStory(string key)
	{
		if (key == null)
		{
			return new string[1] { "artifacts.!null.shortstory" };
		}
		if (!data.artifacts.TryGetValue(key, out var value))
		{
			return new string[1] { "artifacts.!" + key + ".shortstory" };
		}
		return value.shortStory;
	}

	public static string GetTreasureKey(string treasureType)
	{
		return treasureType.Substring(9);
	}

	public static string GetTreasureName(string key)
	{
		if (key == null)
		{
			return "treasures.!null.name";
		}
		if (!data.treasures.TryGetValue(key, out var value))
		{
			return "treasures.!" + key + ".name";
		}
		return value.name;
	}

	public static string GetTreasureLore(string key)
	{
		if (key == null)
		{
			return "treasures.!null.name";
		}
		if (!data.treasures.TryGetValue(key, out var value))
		{
			return "treasures.!" + key + ".lore";
		}
		return value.lore;
	}

	public static IReadOnlyList<LocaleNode> GetTreasureDescription(string key)
	{
		if (!data.treasures.TryGetValue(key, out var value))
		{
			return DebugNodeList("treasures.!" + key + ".desc");
		}
		return value.description;
	}
}
