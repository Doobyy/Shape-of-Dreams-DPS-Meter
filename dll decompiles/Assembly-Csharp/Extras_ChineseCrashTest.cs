using System;
using System.Collections;
using System.Collections.Generic;
using DewInternal;
using Febucci.UI.Core;
using TMPro;
using UnityEngine;

public class Extras_ChineseCrashTest : MonoBehaviour
{
	public TextMeshProUGUI[] texts;

	public TypewriterCore[] typewriters;

	private string _current;

	private void Awake()
	{
		Application.logMessageReceived += ApplicationOnlogMessageReceived;
	}

	private void OnDestroy()
	{
		Application.logMessageReceived -= ApplicationOnlogMessageReceived;
	}

	private void ApplicationOnlogMessageReceived(string condition, string stacktrace, LogType type)
	{
		try
		{
			Debug.Log("[ChineseCrashTest] Log entry detected: " + _current);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			DewSave.LoadProfile(null);
			foreach (KeyValuePair<string, PerLanguageLocalizationData> item in DewLocalization.buildData.dataByLanguage)
			{
				_current = null;
				Debug.Log("[ChineseCrashTest] Starting: " + item.Key);
				string lang = item.Key;
				PerLanguageLocalizationData data = item.Value;
				DewSave.profileMain.language = lang;
				DewSave.ApplySettings();
				yield return null;
				TextMeshProUGUI[] array = texts;
				foreach (TextMeshProUGUI t in array)
				{
					((Component)(object)t).gameObject.SetActive(value: false);
					yield return null;
					((Component)(object)t).gameObject.SetActive(value: true);
					yield return null;
				}
				foreach (KeyValuePair<string, string> item2 in data.ui)
				{
					yield return Test("ui", item2.Key, item2.Value);
				}
				foreach (KeyValuePair<string, SkillData> p in data.skills)
				{
					yield return Test("skills", p.Key, p.Value.memory);
					foreach (SkillConfigData config in p.Value.configs)
					{
						Test("skills", p.Key, config.name);
						Test("skills", p.Key, DewLocalization.ConvertDescriptionNodesToText(config.description, default));
						Test("skills", p.Key, config.shortDescription);
					}
				}
				foreach (KeyValuePair<string, GemData> gem in data.gems)
				{
					Test("gems", gem.Key, gem.Value.name);
					Test("gems", gem.Key, gem.Value.shortDescription);
					Test("gems", gem.Key, gem.Value.memory);
					Test("gems", gem.Key, DewLocalization.ConvertDescriptionNodesToText(gem.Value.description, default));
				}
				foreach (KeyValuePair<string, AchievementData> achievement in data.achievements)
				{
					Test("ach", achievement.Key, achievement.Value.name);
					Test("ach", achievement.Key, achievement.Value.description);
				}
				foreach (KeyValuePair<string, CurseData> curse in data.curses)
				{
					Test("curses", curse.Key, curse.Value.name);
					Test("curses", curse.Key, DewLocalization.ConvertDescriptionNodesToText(curse.Value.description, default));
					Test("curses", curse.Key, DewLocalization.ConvertDescriptionNodesToText(curse.Value.shortDesc, default));
				}
				foreach (KeyValuePair<string, string> tip in data.tips)
				{
					Test("tips", tip.Key, tip.Value);
					yield return null;
				}
				foreach (KeyValuePair<string, StarData> star in data.stars)
				{
					Test("stars", star.Key, star.Value.name);
					Test("stars", star.Key, star.Value.lore);
					Test("stars", star.Key, DewLocalization.ConvertDescriptionNodesToText(star.Value.description, default));
				}
				foreach (KeyValuePair<string, ConversationData> conversation in data.conversations)
				{
					LineData[] lines = conversation.Value.lines;
					for (int j = 0; j < lines.Length; j++)
					{
						LineData lineData = lines[j];
						Test("conv", conversation.Key, lineData.text);
					}
				}
				foreach (KeyValuePair<string, ArtifactData> artifact in data.artifacts)
				{
					Test("art", artifact.Key, artifact.Value.name);
					Test("art", artifact.Key, artifact.Value.story);
					string[] shortStory = artifact.Value.shortStory;
					foreach (string value in shortStory)
					{
						Test("art", artifact.Key, value);
					}
				}
				foreach (KeyValuePair<string, TreasureData> treasure in data.treasures)
				{
					Test("treas", treasure.Key, treasure.Value.name);
					Test("treas", treasure.Key, treasure.Value.lore);
					Test("treas", treasure.Key, DewLocalization.ConvertDescriptionNodesToText(treasure.Value.description, default));
				}
				IEnumerator Test(string category, string key, string text)
				{
					if (!string.IsNullOrEmpty(text))
					{
						_current = lang + "." + category + "." + key;
						TextMeshProUGUI[] array2 = texts;
						foreach (TextMeshProUGUI obj in array2)
						{
							((TMP_Text)obj).text = text;
							((TMP_Text)obj).maxVisibleCharacters = text.Length;
						}
						TypewriterCore[] array3 = typewriters;
						foreach (TypewriterCore obj2 in array3)
						{
							obj2.ShowText("");
							obj2.StartShowingText(false);
							obj2.ShowText(text);
						}
						yield return null;
						array2 = texts;
						for (int k = 0; k < array2.Length; k++)
						{
							((TMP_Text)array2[k]).maxVisibleCharacters = Mathf.RoundToInt((float)text.Length * 0.25f);
						}
					}
				}
			}
		}
	}
}
