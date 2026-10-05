using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CI.QuickSave;
using CI.QuickSave.Core.Storage;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Newtonsoft.Json;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using URPUnlocker.API;
using URPUnlocker.Core;
using VolumetricFogAndMist2;

public static class DewSave
{
	internal class SaveFile<T>
	{
		private static QuickSaveSettings Settings = new QuickSaveSettings
		{
			CompressionMode = (CompressionMode)0,
			SecurityMode = (SecurityMode)0
		};

		private QuickSaveReader _reader;

		private QuickSaveWriter _writer;

		private string _root;

		public SaveFile(string path)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			string text = Path.Join(string.op_Implicit(Application.persistentDataPath), string.op_Implicit("QuickSave"));
			string text2 = (_root = Path.ChangeExtension(path, null).Substring(text.Length + 1));
			_writer = QuickSaveWriter.Create(text2, Settings);
			_reader = QuickSaveReader.Create(text2, Settings);
		}

		public SaveFile(string path, T defaultValue)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			_writer = QuickSaveWriter.Create(fileNameWithoutExtension, Settings);
			try
			{
				_reader = QuickSaveReader.Create(fileNameWithoutExtension, Settings);
				Read();
			}
			catch (Exception)
			{
				_writer.Write<T>("root", defaultValue);
				_writer.Commit();
				_reader = QuickSaveReader.Create(fileNameWithoutExtension, Settings);
				Read();
			}
		}

		public bool Exists()
		{
			return ((QuickSaveBase)_reader).Exists("root");
		}

		public T Read()
		{
			try
			{
				return _reader.Read<T>("root");
			}
			finally
			{
			}
		}

		public bool TryRead(out T value)
		{
			try
			{
				return _reader.TryRead<T>("root", ref value);
			}
			finally
			{
			}
		}

		public void Write(T value)
		{
			_writer.Write<T>("root", value);
			_writer.Commit();
			_reader.Reload();
		}

		public void Delete()
		{
			_writer = null;
			_reader = null;
			QuickSaveRaw.Delete(_root + ".json");
		}

		static SaveFile()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected Obj, but got Unknown
		}
	}

	internal class AnalyticsPayload
	{
		public string user_id;

		public string country;

		public string ticket;

		public object main;

		public object stats;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CUpsertProfile_003Ed__72 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<bool> _003C_003Et__builder;

		private string _003CANALYTICS_URL_003E5__2;

		private SteamTicketForWebApi _003Cres_003E5__3;

		private Awaiter<SteamTicketForWebApi> _003C_003Eu__1;

		private UnityWebRequest _003Crequest_003E5__4;

		private UnityWebRequestAsyncOperationAwaiter _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Expected Obj, but got Unknown
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Expected Obj, but got Unknown
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Expected Obj, but got Unknown
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_020c: Invalid comparison between Unknown and I4
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			bool result;
			try
			{
				if ((uint)num <= 1u)
				{
					goto IL_003f;
				}
				_003CANALYTICS_URL_003E5__2 = "https://ntxojqrachcxqpwezfzw.supabase.co/functions/v1/steam-profile-upsert";
				if (!DewSteam.isInitialized)
				{
					result = false;
				}
				else
				{
					if (DewSave.profileMain != null && !string.IsNullOrEmpty(profileMainPath))
					{
						goto IL_003f;
					}
					result = false;
				}
				goto end_IL_0007;
				IL_003f:
				try
				{
					Awaiter<SteamTicketForWebApi> val;
					if (num != 0)
					{
						if (num == 1)
						{
							goto IL_00b7;
						}
						val = DewSteam.GetTicketForWebApi().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<SteamTicketForWebApi>, _003CUpsertProfile_003Ed__72>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
					}
					SteamTicketForWebApi result2 = val.GetResult();
					_003Cres_003E5__3 = result2;
					goto IL_00b7;
					IL_00b7:
					try
					{
						string text = default;
						if (num != 1)
						{
							string iPCountry = SteamUtils.GetIPCountry();
							string user_id = SteamUser.GetSteamID().m_SteamID.ToString();
							DewProfile profileMain = DewSave.profileMain;
							DewProfileStats profileStats = DewSave.profileStats;
							text = JsonConvert.SerializeObject((object)new AnalyticsPayload
							{
								user_id = user_id,
								country = iPCountry,
								ticket = _003Cres_003E5__3.ticket,
								main = profileMain,
								stats = profileStats
							});
							_003Crequest_003E5__4 = new UnityWebRequest(_003CANALYTICS_URL_003E5__2, "POST");
						}
						try
						{
							UnityWebRequestAsyncOperationAwaiter val2;
							if (num != 1)
							{
								byte[] bytes = Encoding.UTF8.GetBytes(text);
								_003Crequest_003E5__4.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
								_003Crequest_003E5__4.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
								_003Crequest_003E5__4.SetRequestHeader("Content-Type", "application/json");
								UnityEngine.Debug.Log($"[Analytics] Sending profile upsert of length {text.Length}");
								val2 = UnityAsyncExtensions.GetAwaiter(_003Crequest_003E5__4.SendWebRequest());
								if (!val2.IsCompleted)
								{
									num = (_003C_003E1__state = 1);
									_003C_003Eu__2 = val2;
									_003C_003Et__builder.AwaitUnsafeOnCompleted<UnityWebRequestAsyncOperationAwaiter, _003CUpsertProfile_003Ed__72>(ref val2, ref this);
									return;
								}
							}
							else
							{
								val2 = _003C_003Eu__2;
								_003C_003Eu__2 = default;
								num = (_003C_003E1__state = -1);
							}
							val2.GetResult();
							if ((int)_003Crequest_003E5__4.result == 1)
							{
								UnityEngine.Debug.Log("[Analytics] " + _003Crequest_003E5__4.downloadHandler.text);
								DewSave.profileMain.lastUpsert = DateTime.UtcNow.ToTimestamp();
								SaveProfileMain();
								result = true;
							}
							else
							{
								UnityEngine.Debug.Log("[Analytics] " + _003Crequest_003E5__4.error);
								result = false;
							}
						}
						finally
						{
							if (num < 0 && _003Crequest_003E5__4 != null)
							{
								((IDisposable)_003Crequest_003E5__4).Dispose();
							}
						}
					}
					finally
					{
						if (num < 0 && _003Cres_003E5__3 != null)
						{
							((IDisposable)_003Cres_003E5__3).Dispose();
						}
					}
				}
				catch (Exception message)
				{
					UnityEngine.Debug.Log(message);
					result = false;
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CANALYTICS_URL_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CANALYTICS_URL_003E5__2 = null;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	public static SafeAction onSaveStarted;

	public static SafeAction onSaveEnded;

	public static SafeAction onCosmeticsChanged;

	private static bool _needsSaving_Main;

	private static bool _needsSaving_Stats;

	private static bool _needsSaving_Continue;

	private static Coroutine _saveRoutine;

	private static SaveFile<DewItemsData> _itemsFile;

	public static DewItemsData items;

	public static DewPlatformSettings platformSettings;

	private static SaveFile<DewPlatformSettings> _platformSettingsFile;

	public static DewProfile profileMain;

	private static SaveFile<DewProfile> _profileMainFile;

	public static DewProfileStats profileStats;

	private static SaveFile<DewProfileStats> _profileStatsFile;

	public static DewProfileContinue profileContinue;

	private static SaveFile<DewProfileContinue> _profileContinueFile;

	public static SafeAction onSettingsChanged;

	public static string SavePrefix => DewBuildProfile.current.savePrefix;

	public static string profileMainPath { get; private set; }

	public static string profileStatsPath => GetProfileSubFilePath(profileMainPath, "stats");

	public static string profileContinuePath => GetProfileSubFilePath(profileMainPath, "continue");

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitAll()
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		onSaveStarted = null;
		onSaveEnded = null;
		onCosmeticsChanged = null;
		_saveRoutine = null;
		profileMain = null;
		profileStats = null;
		profileContinue = null;
		_profileMainFile = null;
		_profileStatsFile = null;
		_profileContinueFile = null;
		platformSettings = null;
		_platformSettingsFile = null;
		SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
		SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
		try
		{
			MigrateSave_DreamToDreams();
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		try
		{
			ManageDailyBackups();
		}
		catch (Exception exception2)
		{
			UnityEngine.Debug.LogException(exception2);
		}
		try
		{
			ManageIncrementalBackups();
		}
		catch (Exception exception3)
		{
			UnityEngine.Debug.LogException(exception3);
		}
		try
		{
			DoAutoRecoveryIfNecessary();
		}
		catch (Exception exception4)
		{
			UnityEngine.Debug.LogException(exception4);
		}
		try
		{
			DoRollbackLostProgressIfNecessary();
		}
		catch (Exception exception5)
		{
			UnityEngine.Debug.LogException(exception5);
		}
		if (platformSettings == null && !LoadPlatformSettings())
		{
			UnityEngine.Debug.LogWarning("Platform settings broken? Resetting platform settings...");
			try
			{
				string path = Path.Join(string.op_Implicit(Application.persistentDataPath), string.op_Implicit("QuickSave"), string.op_Implicit(GetPlatformSettingsFileName()));
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch (Exception exception6)
			{
				UnityEngine.Debug.LogException(exception6);
			}
			if (!LoadPlatformSettings())
			{
				platformSettings = new DewPlatformSettings();
				platformSettings.Initialize();
				platformSettings.Validate();
				ShowSaveLoadErrorAndExit();
			}
		}
		try
		{
			LoadAndDoMaintenanceOfItems();
		}
		catch (Exception exception7)
		{
			UnityEngine.Debug.LogException(exception7);
			ShowSaveLoadErrorAndExit();
			return;
		}
		if (profileMain == null)
		{
			LoadProfile();
		}
	}

	private static void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		try
		{
			ApplyPerSceneSettings();
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	public static void ShowSaveLoadErrorAndExit()
	{
		GlobalLogicPackage.CallOnReady(() =>
		{
			DewSessionError.ShowError(new DewException(DewExceptionType.SaveLoadFailed_NewSaveFailed), isFatal: true);
		});
	}

	public static bool Exists(string path)
	{
		if (path == null)
		{
			return true;
		}
		return File.Exists(path);
	}

	internal static void ValidateEnumValues(object obj)
	{
		FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
		List<long> list = new List<long>();
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (!fieldInfo.FieldType.IsEnum)
			{
				continue;
			}
			long num = Convert.ToInt64(fieldInfo.GetValue(obj));
			foreach (object value in Enum.GetValues(fieldInfo.FieldType))
			{
				list.Add(Convert.ToInt64(value));
			}
			if (list.Count > 0 && !list.Contains(num))
			{
				long num2 = list[0];
				float num3 = Mathf.Abs(num - list[0]);
				for (int j = 1; j < list.Count; j++)
				{
					float num4 = Mathf.Abs(num - list[j]);
					if (num4 < num3)
					{
						num3 = num4;
						num2 = list[j];
					}
				}
				object obj2 = Enum.ToObject(fieldInfo.FieldType, num2);
				fieldInfo.SetValue(obj, obj2);
				UnityEngine.Debug.LogWarning($"Fixing out of range value: {obj.GetType().Name}.{fieldInfo.Name} = {num} => {obj2}({num2})");
			}
			list.Clear();
		}
	}

	public static void ConsumeGameResult(DewGameResult result, ref LastGamePlayReward rewardSummary)
	{
		if (rewardSummary == null)
		{
			rewardSummary = new LastGamePlayReward();
		}
		int num = profileMain.lastGameResults.FindIndex((DewGameResult r) => r.runId == result.runId);
		if (num >= 0)
		{
			if (result.result == DewGameResult.ResultType.Conceded && profileMain.lastGameResults[num].result != DewGameResult.ResultType.Conceded)
			{
				return;
			}
			profileMain.lastGameResults[num] = result;
		}
		else
		{
			profileMain.lastGameResults.Insert(0, result);
			while (profileMain.lastGameResults.Count > 20)
			{
				profileMain.lastGameResults.RemoveAt(profileMain.lastGameResults.Count - 1);
			}
		}
		int num2 = profileMain.favoriteGameResults.FindIndex((DewGameResult r) => r.runId == result.runId);
		if (num2 >= 0)
		{
			profileMain.favoriteGameResults[num2] = result;
		}
		long num3 = 0L;
		int num4 = profileMain.recentlyConcededGames.FindIndex((DewProfile.ConcededGameData d) => d.result.runId == result.runId);
		if (num4 >= 0)
		{
			DewProfile.ConcededGameData concededGameData = profileMain.recentlyConcededGames[num4];
			DewGameResult.PlayerData playerData = concededGameData.result.players.Find((DewGameResult.PlayerData p) => p.isLocalPlayer);
			if (playerData != null && profileStats.heroes.TryGetValue(playerData.heroType, out var value))
			{
				value.earnedGold -= playerData.totalGoldIncome;
				value.earnedDreamDust -= playerData.totalDreamDustIncome;
				value.kills -= playerData.kills;
				value.miniBossKills -= playerData.miniBossKills;
				value.heroicBossKills -= playerData.heroicBossKills;
				value.hunterKills -= playerData.hunterKills;
				value.deaths -= playerData.deaths;
				value.levelUps -= playerData.level - 1;
				value.damageDealt -= playerData.dealtDamageToEnemies;
				value.damageTaken -= playerData.receivedDamage;
				value.healToSelf -= playerData.healToSelf;
				value.healToOthers -= playerData.healToOthers;
			}
			num3 = concededGameData.receivedTravelerMastery;
			profileStats.RemoveMasteryPoints(concededGameData.heroType, concededGameData.receivedTravelerMastery);
			profileMain.recentlyConcededGames.RemoveAt(num4);
		}
		DewGameResult.PlayerData playerData2 = result.players.Find((DewGameResult.PlayerData p) => p.isLocalPlayer);
		if (playerData2 != null)
		{
			bool flag = result.result.IsWin();
			bool flag2 = result.difficulty == "diffNightmare" || result.difficulty == "diffLimbo";
			if (profileStats.heroes.TryGetValue(playerData2.heroType, out var value2))
			{
				if (result.result != DewGameResult.ResultType.Conceded)
				{
					if (flag)
					{
						value2.wins++;
					}
					else
					{
						value2.loses++;
					}
				}
				if (result.result == DewGameResult.ResultType.PureWhiteDream)
				{
					value2.pureWhiteDreams++;
					if (flag2)
					{
						value2.pureWhiteDreamsNightmare++;
					}
				}
				else if (result.result == DewGameResult.ResultType.StarlessPath)
				{
					value2.starlessPaths++;
					if (flag2)
					{
						value2.starlessPathsNightmare++;
					}
				}
				else if (result.result == DewGameResult.ResultType.UnknownFate)
				{
					value2.unknownFates++;
					if (flag2)
					{
						value2.unknownFatesNightmare++;
					}
				}
				value2.winsNightmare = value2.pureWhiteDreamsNightmare + value2.unknownFatesNightmare + value2.starlessPathsNightmare;
				value2.maxElapsedGameTimeSeconds = Math.Max(value2.maxElapsedGameTimeSeconds, result.elapsedGameTimeSeconds);
				value2.maxVisitedWorlds = Math.Max(value2.maxVisitedWorlds, result.visitedWorlds);
				value2.maxEarnedGold = Math.Max(value2.maxEarnedGold, playerData2.totalGoldIncome);
				value2.maxEarnedDreamDust = Math.Max(value2.maxEarnedDreamDust, playerData2.totalDreamDustIncome);
				value2.maxEarnedStardust = Math.Max(value2.maxEarnedStardust, playerData2.totalStardustIncome);
				value2.maxTotalDamage = Math.Max(value2.maxTotalDamage, playerData2.dealtDamageToEnemies);
				value2.maxSingleTargetDamage = Math.Max(value2.maxSingleTargetDamage, playerData2.maxDealtSingleDamageToEnemy);
				value2.earnedGold += playerData2.totalGoldIncome;
				value2.earnedDreamDust += playerData2.totalDreamDustIncome;
				value2.kills += playerData2.kills;
				value2.miniBossKills += playerData2.miniBossKills;
				value2.heroicBossKills += playerData2.heroicBossKills;
				value2.hunterKills += playerData2.hunterKills;
				value2.deaths += playerData2.deaths;
				value2.levelUps += playerData2.level - 1;
				value2.damageDealt += playerData2.dealtDamageToEnemies;
				value2.damageTaken += playerData2.receivedDamage;
				value2.healToSelf += playerData2.healToSelf;
				value2.healToOthers += playerData2.healToOthers;
			}
			if (result.result != DewGameResult.ResultType.Conceded)
			{
				foreach (DewGameResult.SkillData skill in playerData2.skills)
				{
					if (profileStats.skills.TryGetValue(skill.name, out var value3))
					{
						if (flag)
						{
							value3.wins++;
						}
						else
						{
							value3.loses++;
						}
					}
				}
				foreach (DewGameResult.GemData gem in playerData2.gems)
				{
					if (profileStats.gems.TryGetValue(gem.name, out var value4))
					{
						if (flag)
						{
							value4.wins++;
						}
						else
						{
							value4.loses++;
						}
					}
				}
			}
			rewardSummary.heroType = playerData2.heroType;
			float num5 = (float)playerData2.combatTime / 60f * 1.3f;
			float num6 = (float)playerData2.heroicBossKills * 7f + (float)playerData2.miniBossKills * 1.5f;
			if (num5 < num6)
			{
				num5 = num6;
			}
			long rewardedMasteryPoints = Dew.GetRewardedMasteryPoints(num5);
			rewardSummary.heroMasteryPoints = rewardedMasteryPoints - num3;
			profileStats.AddMasteryPoints(playerData2.heroType, rewardedMasteryPoints);
			if (result.result == DewGameResult.ResultType.Conceded)
			{
				profileMain.recentlyConcededGames.Insert(0, new DewProfile.ConcededGameData
				{
					result = result,
					heroType = playerData2.heroType,
					receivedTravelerMastery = rewardedMasteryPoints
				});
				while (profileMain.recentlyConcededGames.Count > 10)
				{
					profileMain.recentlyConcededGames.RemoveAt(profileMain.recentlyConcededGames.Count - 1);
				}
			}
		}
		SaveProfileStats();
		SaveProfileMain();
	}

	private static void LoadAndDoMaintenanceOfItems()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		string path = Path.Join(string.op_Implicit(Path.Join(string.op_Implicit(Application.persistentDataPath), string.op_Implicit("QuickSave"))), string.op_Implicit(SavePrefix + "items.json"));
		try
		{
			_itemsFile = new SaveFile<DewItemsData>(SavePrefix + "items", new DewItemsData());
			if (!_itemsFile.TryRead(out items))
			{
				throw new Exception("Failed to read items file");
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
			UnityEngine.Debug.LogWarning("Failed to read items file. Creating a new one...");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			try
			{
				_itemsFile = new SaveFile<DewItemsData>(SavePrefix + "items", new DewItemsData());
				if (!_itemsFile.TryRead(out items))
				{
					throw new Exception("Failed to create and read items file");
				}
			}
			catch (Exception exception2)
			{
				UnityEngine.Debug.LogException(exception2);
				ShowSaveLoadErrorAndExit();
				return;
			}
		}
		items.Validate();
		_itemsFile.Write(items);
	}

	public static void AddMissingServerGeneratedItemsToProfile(DewProfile p)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (items == null || !DewSteam.isInitialized)
		{
			return;
		}
		foreach (string encryptedItem in items.encryptedItems)
		{
			DecryptedItemData decryptedItemData = DewItem.GetDecryptedItemData(encryptedItem);
			if (decryptedItemData != null && !(decryptedItemData.owner != DewSteam.steamId.m_SteamID.ToString()) && !decryptedItemData.IsExpired() && !decryptedItemData.IsDLCNotOwned())
			{
				p.UnlockServerGeneratedItem(decryptedItemData);
			}
		}
	}

	public static void AddServerGeneratedItem(DecryptedItemData item)
	{
		if (item == null || string.IsNullOrEmpty(item.ownershipKey))
		{
			return;
		}
		if (items.encryptedItems.Contains(item.ownershipKey))
		{
			profileMain.UnlockServerGeneratedItem(item);
			return;
		}
		DecryptedItemData decryptedItemData = item;
		for (int num = items.encryptedItems.Count - 1; num >= 0; num--)
		{
			DecryptedItemData decryptedItemData2 = DewItem.GetDecryptedItemData(items.encryptedItems[num]);
			if (decryptedItemData2 != null && !(decryptedItemData2.item != item.item) && !(decryptedItemData2.owner != item.owner))
			{
				if (decryptedItemData2.timestamp >= decryptedItemData.timestamp)
				{
					decryptedItemData = decryptedItemData2;
				}
				else
				{
					items.encryptedItems.RemoveAt(num);
				}
			}
		}
		profileMain.UnlockServerGeneratedItem(decryptedItemData);
		if (decryptedItemData == item)
		{
			items.encryptedItems.Add(item.ownershipKey);
		}
		if ((UnityEngine.Object)(object)DewPlayer.local != null)
		{
			DewPlayer.local.CmdAuthorizeForUse(decryptedItemData.ownershipKey);
		}
	}

	public static void ClearItemsFromItemStorage()
	{
		items = new DewItemsData();
		SaveItems();
	}

	public static void SaveItems()
	{
		_itemsFile.Write(items);
		new SaveFile<DewItemsData>("backup_items", new DewItemsData()).Write(items);
	}

	private static string GetDailyBackupDir()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return Path.Join(string.op_Implicit(Application.persistentDataPath), string.op_Implicit("QuickSave"), string.op_Implicit("Backups"));
	}

	private static void ManageDailyBackups()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		string text = DateTime.Now.ToString("yyyyMMdd") + ".zip";
		string sourceDirectory = Path.Join(string.op_Implicit(Application.persistentDataPath), string.op_Implicit("QuickSave"));
		string dailyBackupDir = GetDailyBackupDir();
		Directory.CreateDirectory(dailyBackupDir);
		string text2 = Path.Join(string.op_Implicit(dailyBackupDir), string.op_Implicit(text));
		if (File.Exists(text2))
		{
			UnityEngine.Debug.Log("Already made a daily backup today. Skipping...");
			return;
		}
		UnityEngine.Debug.Log("Making a daily backup");
		CreateZipWithJsonFiles(sourceDirectory, text2);
		UnityEngine.Debug.Log("Successfully created a daily backup at " + text2);
		List<FileInfo> list = (from f in Directory.GetFiles(dailyBackupDir, "*.zip")
			select new FileInfo(f) into f
			orderby f.CreationTime descending
			select f).ToList();
		int num = 30;
		if (list.Count <= num)
		{
			return;
		}
		UnityEngine.Debug.Log($"Too many daily backups({list.Count}), purging...");
		foreach (FileInfo item in list.Skip(num))
		{
			try
			{
				item.Delete();
				UnityEngine.Debug.Log("Deleted old daily backup: " + item.Name);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogError("Error deleting daily backup " + item.Name);
				UnityEngine.Debug.LogException(exception);
			}
		}
	}

	private static string GetIncrementalBackupDir()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return Path.Join(string.op_Implicit(Application.persistentDataPath), string.op_Implicit("QuickSave"), string.op_Implicit("AutoRecovery"));
	}

	private static void ManageIncrementalBackups()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		string path = Path.Join(string.op_Implicit(Application.persistentDataPath), string.op_Implicit("QuickSave"));
		string incrementalBackupDir = GetIncrementalBackupDir();
		Directory.CreateDirectory(incrementalBackupDir);
		try
		{
			List<string> source = Directory.GetFiles(path, "*.json").Select(Path.GetFileNameWithoutExtension).ToList();
			foreach (FileInfo b in (from f in Directory.GetFiles(incrementalBackupDir, "*.json")
				select new FileInfo(f)).ToList())
			{
				if (!source.Any((string pName) => b.Name.StartsWith(pName)))
				{
					UnityEngine.Debug.Log("Deleting stray auto-recovery backup: " + b.Name);
					b.Delete();
				}
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	private static void DoAutoRecoveryIfNecessary()
	{
		string incrementalDir = GetIncrementalBackupDir();
		string dailyDir = GetDailyBackupDir();
		List<FileInfo> dailyBackups = (from f in Directory.GetFiles(dailyDir, "*.zip")
			select new FileInfo(f)).OrderByDescending((FileInfo f) =>
		{
			try
			{
				return long.Parse(Path.GetFileNameWithoutExtension(f.FullName));
			}
			catch (Exception)
			{
				return long.MinValue;
			}
		}).ToList();
		HashSet<string> hashSet = new HashSet<string>();
		foreach (DewProfileItem profile in GetProfiles())
		{
			if (profile.state != DewProfileState.Corrupted)
			{
				continue;
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(profile.path);
			if (fileNameWithoutExtension.StartsWith("err-"))
			{
				continue;
			}
			UnityEngine.Debug.Log("Attempting auto-recovery for profile related to " + fileNameWithoutExtension);
			string profileName = profile.peek?.name ?? "Unknown Profile";
			bool flag = false;
			try
			{
				flag |= RecoverPart<DewProfile>(null, profile.path, ref profileName);
				flag |= RecoverPart<DewProfileStats>("stats", profile.path, ref profileName);
				if (flag | RecoverPart<DewProfileContinue>("continue", profile.path, ref profileName))
				{
					UnityEngine.Debug.Log("Auto-recovery process completed for profile '" + profileName + "'.");
					if (profileName != "Unknown Profile")
					{
						hashSet.Add(profileName);
					}
				}
				else
				{
					UnityEngine.Debug.LogWarning("Auto-recovery failed for profile related to " + fileNameWithoutExtension + ". The profile may be lost or incomplete.");
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
		if (hashSet.Any())
		{
			string namesString = string.Join(", ", hashSet);
			GlobalLogicPackage.CallOnReady(() =>
			{
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					rawContent = string.Format(DewLocalization.GetUIValue("Title_Profile_Message_RecoveredCorruptSaveFile"), namesString)
				});
			});
		}
		bool RecoverPart<T>(string subprofileType, string mainProfilePath, ref string reference) where T : class
		{
			string text = ((subprofileType == null) ? mainProfilePath : GetProfileSubFilePath(mainProfilePath, subprofileType));
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			if (TryPeekProfile_Imp<T>(text, out var _) == DewSubProfileState.Normal)
			{
				return false;
			}
			UnityEngine.Debug.Log("Corrupt or missing sub-profile '" + typeof(T).Name + "' for profile '" + reference + "'. Looking for backups...");
			string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(text);
			foreach (FileInfo item in (from f in Directory.GetFiles(incrementalDir, fileNameWithoutExtension2 + "_*.json")
				select new FileInfo(f)).OrderByDescending((FileInfo f) =>
			{
				try
				{
					string[] array = Path.GetFileNameWithoutExtension(f.Name).Split('_', StringSplitOptions.None);
					return long.Parse(array[array.Length - 1]);
				}
				catch (Exception)
				{
					return long.MinValue;
				}
			}).ToList())
			{
				if (TryPeekProfile_Imp<T>(item.FullName, out var peek2) == DewSubProfileState.Normal)
				{
					UnityEngine.Debug.Log("Found valid incremental backup: " + item.Name + ". Restoring...");
					File.Copy(item.FullName, text, overwrite: true);
					if (peek2 is DewProfile dewProfile)
					{
						reference = dewProfile.name;
					}
					return true;
				}
			}
			UnityEngine.Debug.Log("No valid incremental backup found for '" + typeof(T).Name + "'. Trying daily backups...");
			string text2 = Path.Combine(dailyDir, "temp_extract_" + Guid.NewGuid().ToString());
			string fileName = Path.GetFileName(text);
			try
			{
				foreach (FileInfo item2 in dailyBackups)
				{
					if (Directory.Exists(text2))
					{
						Directory.Delete(text2, recursive: true);
					}
					Directory.CreateDirectory(text2);
					try
					{
						ZipFile.ExtractToDirectory(item2.FullName, text2);
						string text3 = Path.Combine(text2, fileName);
						if (File.Exists(text3) && TryPeekProfile_Imp<T>(text3, out var peek3) == DewSubProfileState.Normal)
						{
							UnityEngine.Debug.Log("Found valid daily backup from " + item2.Name + ". Restoring...");
							File.Copy(text3, text, overwrite: true);
							if (peek3 is DewProfile dewProfile2)
							{
								reference = dewProfile2.name;
							}
							return true;
						}
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.LogError("Error processing daily backup " + item2.Name + ": " + ex.Message);
					}
				}
			}
			finally
			{
				if (Directory.Exists(text2))
				{
					try
					{
						Directory.Delete(text2, recursive: true);
					}
					catch
					{
					}
				}
			}
			UnityEngine.Debug.Log("No valid backups of any kind found for '" + typeof(T).Name + "'.");
			if (File.Exists(text))
			{
				string text4 = Path.Combine(Path.GetDirectoryName(text), "err-" + Path.GetFileName(text));
				int num = 0;
				while (File.Exists(text4) && num < 100)
				{
					text4 = Path.Combine(Path.GetDirectoryName(text), $"err-{num++}-" + Path.GetFileName(text));
				}
				try
				{
					if (File.Exists(text4))
					{
						File.Delete(text4);
					}
					File.Move(text, text4);
					UnityEngine.Debug.LogWarning("Renamed corrupted file to '" + Path.GetFileName(text4) + "'.");
				}
				catch (Exception ex2)
				{
					UnityEngine.Debug.LogError("Failed to move corrupted file " + text + ": " + ex2.Message);
				}
			}
			if (subprofileType != null)
			{
				UnityEngine.Debug.Log("Recovery for sub-profile '" + typeof(T).Name + "' succeeded by removing the corrupt file. A new one will be generated on load.");
				return true;
			}
			return false;
		}
	}

	private static void DoRollbackLostProgressIfNecessary()
	{
		string dailyBackupDir = GetDailyBackupDir();
		if (!Directory.Exists(dailyBackupDir))
		{
			return;
		}
		List<DewProfileItem> profiles = GetProfiles();
		bool flag = false;
		List<string> list = new List<string>();
		foreach (DewProfileItem item2 in profiles)
		{
			if (item2.state != DewProfileState.Normal || item2.peek == null || item2.peekStats == null)
			{
				continue;
			}
			SortedDictionary<string, DewProfileStats> sortedDictionary = new SortedDictionary<string, DewProfileStats>();
			string fileName = Path.GetFileName(GetProfileSubFilePath(item2.path, "stats"));
			if (string.IsNullOrEmpty(fileName))
			{
				continue;
			}
			List<FileInfo> list2 = (from f in Directory.GetFiles(dailyBackupDir, "*.zip")
				select new FileInfo(f)).OrderBy((FileInfo f) =>
			{
				try
				{
					return long.Parse(Path.GetFileNameWithoutExtension(f.FullName));
				}
				catch
				{
					return long.MaxValue;
				}
			}).ToList();
			string text = Path.Combine(dailyBackupDir, "temp_extract_rollback_" + Guid.NewGuid().ToString());
			try
			{
				foreach (FileInfo item3 in list2)
				{
					if (Directory.Exists(text))
					{
						Directory.Delete(text, recursive: true);
					}
					Directory.CreateDirectory(text);
					try
					{
						using ZipArchive zipArchive = ZipFile.OpenRead(item3.FullName);
						ZipArchiveEntry entry = zipArchive.GetEntry(fileName);
						if (entry != null)
						{
							string text2 = Path.Combine(text, fileName);
							entry.ExtractToFile(text2, overwrite: true);
							if (TryPeekProfile_Imp<DewProfileStats>(text2, out var peek) == DewSubProfileState.Normal)
							{
								string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item3.Name);
								sortedDictionary[fileNameWithoutExtension] = peek;
							}
						}
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.LogWarning("Could not process backup " + item3.Name + " for rollback check. " + ex.Message);
					}
				}
			}
			finally
			{
				if (Directory.Exists(text))
				{
					try
					{
						Directory.Delete(text, recursive: true);
					}
					catch
					{
					}
				}
			}
			if (sortedDictionary.Count == 0)
			{
				continue;
			}
			List<DewProfileStats> list3 = sortedDictionary.Values.ToList();
			List<string> list4 = sortedDictionary.Keys.ToList();
			list3.Add(item2.peekStats);
			list4.Add("current");
			bool flag2 = false;
			for (int num = 1; num < list3.Count; num++)
			{
				DewProfileStats dewProfileStats = list3[num - 1];
				DewProfileStats dewProfileStats2 = list3[num];
				dewProfileStats.UpdateTotalData(0L);
				dewProfileStats2.UpdateTotalData(0L);
				if (dewProfileStats.total != null && dewProfileStats2.total != null && (dewProfileStats2.total.playTimeMinutes < dewProfileStats.total.playTimeMinutes || dewProfileStats2.total.masteryLevel < dewProfileStats.total.masteryLevel))
				{
					string item = $"loss_{list4[num - 1]}_{dewProfileStats.total.playTimeMinutes}_{dewProfileStats.total.masteryLevel}";
					if (item2.peekStats.recoveredLossPoints == null)
					{
						item2.peekStats.recoveredLossPoints = new List<string>();
					}
					if (!item2.peekStats.recoveredLossPoints.Contains(item))
					{
						UnityEngine.Debug.Log("Progress loss detected for profile '" + item2.peek.name + "' between point '" + list4[num - 1] + "' and '" + list4[num] + "'.");
						UnityEngine.Debug.Log($"Before: PlayTime={dewProfileStats.total.playTimeMinutes}, Mastery={dewProfileStats.total.masteryLevel}. After: PlayTime={dewProfileStats2.total.playTimeMinutes}, Mastery={dewProfileStats2.total.masteryLevel}");
						DewProfileStats recoveryDelta = DewProfileStats.GetRecoveryDelta(dewProfileStats, dewProfileStats2);
						item2.peekStats.ApplyRecoveryDelta(recoveryDelta);
						item2.peekStats.recoveredLossPoints.Add(item);
						flag2 = true;
						UnityEngine.Debug.Log("Progress delta applied to current save file.");
					}
				}
			}
			if (!flag2)
			{
				continue;
			}
			try
			{
				new SaveFile<DewProfileStats>(GetProfileSubFilePath(item2.path, "stats")).Write(item2.peekStats);
				UnityEngine.Debug.Log("Saved recovered stats for profile '" + item2.peek.name + "'.");
				flag = true;
				if (!list.Contains(item2.peek.name))
				{
					list.Add(item2.peek.name);
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogError("Failed to save recovered stats for profile '" + item2.peek.name + "'.");
				UnityEngine.Debug.LogException(exception);
			}
		}
		if (flag)
		{
			string namesString = string.Join(", ", list);
			GlobalLogicPackage.CallOnReady(() =>
			{
				string uIValue = DewLocalization.GetUIValue("Title_Profile_Message_RecoveredLostProgress");
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					rawContent = string.Format(uIValue, namesString)
				});
			});
		}
	}

	private static void CreateAutoRecoveryBackup(string path)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string text = Path.Join(string.op_Implicit(Application.persistentDataPath), string.op_Implicit("QuickSave"), string.op_Implicit("AutoRecovery"));
			Directory.CreateDirectory(text);
			if (string.IsNullOrEmpty(path))
			{
				return;
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			long num = 0L;
			List<FileInfo> list = (from f in Directory.GetFiles(text, fileNameWithoutExtension + "_*.json")
				select new FileInfo(f)).OrderBy((FileInfo f) =>
			{
				try
				{
					string[] array2 = Path.GetFileNameWithoutExtension(f.FullName).Split("_", StringSplitOptions.None);
					return long.Parse(array2[array2.Length - 1]);
				}
				catch (Exception)
				{
					return long.MinValue;
				}
			}).ToList();
			foreach (FileInfo item in list)
			{
				try
				{
					string[] array = Path.GetFileNameWithoutExtension(item.FullName).Split("_", StringSplitOptions.None);
					long num2 = long.Parse(array[array.Length - 1]);
					if (num2 + 1 > num)
					{
						num = num2 + 1;
					}
				}
				catch (Exception)
				{
				}
			}
			try
			{
				while (list.Count > 9)
				{
					UnityEngine.Debug.Log("Deleting old auto-recovery backup: " + list[0].Name);
					list[0].Delete();
					list.RemoveAt(0);
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
			File.Copy(path, Path.Join(string.op_Implicit(text), string.op_Implicit($"{fileNameWithoutExtension}_{num}.json")));
			UnityEngine.Debug.Log($"Created auto-recovery backup {num} for {fileNameWithoutExtension}");
		}
		catch (Exception exception2)
		{
			UnityEngine.Debug.LogException(exception2);
		}
	}

	public static void CreateZipWithJsonFiles(string sourceDirectory, string zipFilePath)
	{
		string[] files = Directory.GetFiles(sourceDirectory, "*.json", SearchOption.TopDirectoryOnly);
		if (files.Length == 0)
		{
			return;
		}
		using ZipArchive destination = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);
		string[] array = files;
		foreach (string text in array)
		{
			destination.CreateEntryFromFile(text, Path.GetFileName(text));
		}
	}

	private static void MigrateSave_DreamToDreams()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		string persistentDataPath = Application.persistentDataPath;
		DirectoryInfo parent = Directory.GetParent(persistentDataPath);
		if (parent == null)
		{
			return;
		}
		string text = Path.Join(string.op_Implicit(parent.FullName), string.op_Implicit("Shape of Dream"));
		if (!Directory.Exists(text))
		{
			UnityEngine.Debug.Log("No pre-name-change persistent data found");
			return;
		}
		UnityEngine.Debug.Log("Found pre-name-change persistent data");
		string text2 = Path.Join(string.op_Implicit(persistentDataPath), string.op_Implicit("QuickSave"));
		string path = Path.Join(string.op_Implicit(text), string.op_Implicit("QuickSave"));
		if (!Directory.Exists(path))
		{
			UnityEngine.Debug.Log("No pre-name-change save found. Probably been migrated already");
			return;
		}
		UnityEngine.Debug.Log("Starting D2D migration");
		Directory.CreateDirectory(text2);
		string[] files = Directory.GetFiles(path, "*.json");
		foreach (string text3 in files)
		{
			string fileName = Path.GetFileName(text3);
			string text4 = Path.Join(string.op_Implicit(text2), string.op_Implicit(fileName));
			for (int j = 0; j < 100; j++)
			{
				if (File.Exists(text4))
				{
					string text5 = $"conf-{j}-d2d-{fileName}";
					text4 = Path.Join(string.op_Implicit(text2), string.op_Implicit(text5));
				}
				if (!File.Exists(text4))
				{
					break;
				}
			}
			File.Move(text3, text4);
		}
		Directory.Delete(path, recursive: true);
		UnityEngine.Debug.Log("D2D migration success");
	}

	public static string GetPlatformSettingsFileName()
	{
		return SavePrefix + "platform.json";
	}

	public static void SavePlatformSettings()
	{
		_platformSettingsFile.Write(platformSettings);
		UnityEngine.Debug.Log("Platform settings saved.");
	}

	public static bool LoadPlatformSettings()
	{
		try
		{
			_platformSettingsFile = new SaveFile<DewPlatformSettings>(GetPlatformSettingsFileName(), new DewPlatformSettings());
			platformSettings = _platformSettingsFile.Read();
			platformSettings.Validate();
			UnityEngine.Debug.Log("Loaded platform settings.");
			return true;
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
			return false;
		}
	}

	public static void ResetPlatformSettings()
	{
		platformSettings = new DewPlatformSettings();
		if (_platformSettingsFile == null)
		{
			_platformSettingsFile = new SaveFile<DewPlatformSettings>(GetPlatformSettingsFileName(), new DewPlatformSettings());
		}
		_platformSettingsFile.Write(platformSettings);
		UnityEngine.Debug.Log("Platform settings reset.");
	}

	public static string GetProfileMainFileName(string guid)
	{
		return string.Format(SavePrefix + "profile{0}.json", guid);
	}

	public static bool LoadProfile()
	{
		if (profileMainPath == null || !Exists(profileMainPath) || GetProfileState(profileMainPath, out var _, out var _, out var _) != DewProfileState.Normal)
		{
			List<DewProfileItem> normalProfiles = GetNormalProfiles();
			if (normalProfiles.Count == 1)
			{
				profileMainPath = normalProfiles[0].path;
			}
			else if (!string.IsNullOrEmpty(platformSettings.lastProfilePath) && normalProfiles.FindIndex((DewProfileItem p) => p.path == platformSettings.lastProfilePath) != -1)
			{
				profileMainPath = platformSettings.lastProfilePath;
			}
			else
			{
				profileMainPath = null;
			}
		}
		return LoadProfile(profileMainPath);
	}

	public static bool LoadProfile(string path)
	{
		try
		{
			_profileMainFile = null;
			profileMainPath = path;
			if (profileMainPath == null)
			{
				profileMain = new DewProfile
				{
					name = "Transient"
				};
				profileMain.Initialize();
				profileMain.Validate();
				profileStats = new DewProfileStats();
				profileStats.Validate();
				profileContinue = new DewProfileContinue();
				profileContinue.Validate();
				ApplySettings();
				UnityEngine.Debug.Log("Using transient profile from now on.");
				return true;
			}
			DewProfileState profileState = GetProfileState(path, out var _, out var _, out var _);
			if (profileState != DewProfileState.Normal)
			{
				UnityEngine.Debug.LogError($"Profile load failed, the profile state is invalid. ({profileState})");
				return false;
			}
			_profileMainFile = new SaveFile<DewProfile>(profileMainPath);
			if (!_profileMainFile.TryRead(out profileMain))
			{
				UnityEngine.Debug.LogError("Profile Main load failed");
				return false;
			}
			profileMain.Validate();
			_profileStatsFile = new SaveFile<DewProfileStats>(profileStatsPath);
			_profileContinueFile = new SaveFile<DewProfileContinue>(profileContinuePath, new DewProfileContinue());
			if (!_profileStatsFile.TryRead(out profileStats))
			{
				UnityEngine.Debug.LogError("Profile Stats load failed");
				return false;
			}
			profileStats.Validate();
			if (!_profileContinueFile.TryRead(out profileContinue))
			{
				UnityEngine.Debug.Log("Profile Continue load failed, creating new one");
				profileContinue = new DewProfileContinue();
				_profileContinueFile.Write(profileContinue);
			}
			profileContinue.Validate();
			ApplySettings();
			UnityEngine.Debug.Log("Loaded User Profile " + profileMain.name + ": " + profileMainPath);
			if (platformSettings != null && platformSettings.lastProfilePath != profileMainPath)
			{
				platformSettings.lastProfilePath = profileMainPath;
				SavePlatformSettings();
			}
			if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
			{
				_ = Application.isPlaying;
			}
			if (DewSteam.isAchievementReady)
			{
				SyncAchievements();
			}
			return true;
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
			return false;
		}
	}

	public static void CreateProfile(string name)
	{
		_profileMainFile = null;
		profileMainPath = null;
		while (profileMainPath == null || Exists(profileMainPath))
		{
			profileMainPath = Path.Combine(FileAccess.BasePath, GetProfileMainFileName(Guid.NewGuid().ToString()));
		}
		string language = profileMain.language;
		profileMain = new DewProfile
		{
			name = name,
			language = language
		};
		profileMain.Initialize();
		profileMain.Validate();
		_profileMainFile = new SaveFile<DewProfile>(profileMainPath, profileMain);
		profileStats = new DewProfileStats();
		profileStats.Validate();
		_profileStatsFile = new SaveFile<DewProfileStats>(profileStatsPath, profileStats);
		profileContinue = new DewProfileContinue();
		profileContinue.Validate();
		_profileContinueFile = new SaveFile<DewProfileContinue>(profileContinuePath, profileContinue);
		ApplySettings();
		UnityEngine.Debug.Log("Created new profile " + name + ": " + profileMainPath);
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			foreach (KeyValuePair<string, DewProfile.CosmeticsData> accessory in profileMain.accessories)
			{
				if (!DewItem.IsItemGeneratedFromServer(accessory.Key))
				{
					accessory.Value.isUnlocked = true;
					accessory.Value.isNew = true;
				}
			}
		}
		if (!DewBuildProfile.current.HasFeature(BuildFeatureTag.UnlockEverything))
		{
			return;
		}
		foreach (KeyValuePair<string, DewProfile.CosmeticsData> accessory2 in profileMain.accessories)
		{
			if (!DewItem.IsItemGeneratedFromServer(accessory2.Key))
			{
				accessory2.Value.isUnlocked = true;
				accessory2.Value.isNew = true;
			}
		}
		foreach (KeyValuePair<string, DewProfile.AchievementData> achievement in profileMain.achievements)
		{
			if (!achievement.Value.isCompleted)
			{
				achievement.Value.completeTimestamp = DateTime.UtcNow.ToTimestamp();
				achievement.Value.isCompleted = true;
				achievement.Value.currentProgress = achievement.Value.maxProgress;
				achievement.Value.persistentVariables = null;
				DewAchievementItem dewAchievementItem = (DewAchievementItem)Activator.CreateInstance(Dew.achievementsByName[achievement.Key]);
				profileMain.stardust += dewAchievementItem.grantedStardust;
			}
		}
		foreach (KeyValuePair<string, DewProfile.UnlockData> skill in profileMain.skills)
		{
			if (skill.Value.status == UnlockStatus.NotDiscovered)
			{
				profileMain.DiscoverSkill(skill.Key);
			}
		}
		foreach (KeyValuePair<string, DewProfile.UnlockData> gem in profileMain.gems)
		{
			if (gem.Value.status == UnlockStatus.NotDiscovered)
			{
				profileMain.DiscoverGem(gem.Key);
			}
		}
		foreach (KeyValuePair<string, DewProfile.UnlockData> artifact in profileMain.artifacts)
		{
			if (artifact.Value.status == UnlockStatus.NotDiscovered)
			{
				profileMain.DiscoverArtifact(artifact.Key);
			}
		}
		foreach (KeyValuePair<string, DewProfileStats.HeroData> hero in profileStats.heroes)
		{
			hero.Value.pureWhiteDreamsNightmare = 10L;
			hero.Value.masteryLevel = 30;
		}
		foreach (KeyValuePair<string, DewProfile.StarData> newStar in profileMain.newStars)
		{
			StarEffect byShortTypeName = DewResources.GetByShortTypeName<StarEffect>(newStar.Key, default(ResourceLoadSettings));
			if (((byShortTypeName.heroType == null) ? profileStats.total.masteryLevel : profileStats.heroes[byShortTypeName.heroType.Name].masteryLevel) < byShortTypeName.requiredLevel)
			{
				newStar.Value.level = 0;
			}
			else
			{
				newStar.Value.level = UnityEngine.Random.Range(0, byShortTypeName.maxStarLevel + 1);
			}
		}
		foreach (KeyValuePair<string, DewProfileStats.MonsterData> monster in profileStats.monsters)
		{
			monster.Value.nightmareKills = UnityEngine.Random.Range(15, 30);
		}
		foreach (KeyValuePair<string, DewProfileStats.ItemData> gem2 in profileStats.gems)
		{
			gem2.Value.wins = UnityEngine.Random.Range(0, 6);
		}
		foreach (KeyValuePair<string, DewProfileStats.ItemData> skill2 in profileStats.skills)
		{
			skill2.Value.wins = UnityEngine.Random.Range(0, 6);
		}
		profileStats.UpdateTotalData(0L);
		SaveProfileMain(immediate: true);
		profileMain.Validate();
	}

	public static void ConvertProfile(string path)
	{
		DewProfileState profileState = GetProfileState(path, out var peekMain, out var peekStats, out var _);
		if (profileState != DewProfileState.Convertible)
		{
			UnityEngine.Debug.LogError("Save conversion failed, invalid state: " + profileState);
			return;
		}
		_profileMainFile = null;
		profileMainPath = null;
		while (profileMainPath == null || Exists(profileMainPath))
		{
			profileMainPath = Path.Combine(FileAccess.BasePath, GetProfileMainFileName(Guid.NewGuid().ToString()));
		}
		profileMain = peekMain;
		profileMain.Validate();
		_profileMainFile = new SaveFile<DewProfile>(profileMainPath, profileMain);
		profileStats = peekStats ?? new DewProfileStats();
		profileStats.Validate();
		_profileStatsFile = new SaveFile<DewProfileStats>(profileStatsPath, profileStats);
		profileContinue = new DewProfileContinue();
		profileContinue.Validate();
		_profileContinueFile = new SaveFile<DewProfileContinue>(profileContinuePath, profileContinue);
		ApplySettings();
		DeleteProfile(path);
		UnityEngine.Debug.Log("Converted profile " + profileMain.name + ": " + profileMainPath);
	}

	public static void DeleteProfile(string path)
	{
		if (path == null)
		{
			UnityEngine.Debug.Log("DeleteProfile: Tried to delete transient profile.");
			return;
		}
		try
		{
			if (Exists(path))
			{
				QuickSaveRaw.Delete(Path.GetFileName(path));
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		try
		{
			string profileSubFilePath = GetProfileSubFilePath(path, "continue");
			if (Exists(profileSubFilePath))
			{
				QuickSaveRaw.Delete(Path.GetFileName(profileSubFilePath));
			}
		}
		catch (Exception exception2)
		{
			UnityEngine.Debug.LogException(exception2);
		}
		try
		{
			string profileSubFilePath2 = GetProfileSubFilePath(path, "stats");
			if (Exists(profileSubFilePath2))
			{
				QuickSaveRaw.Delete(Path.GetFileName(profileSubFilePath2));
			}
		}
		catch (Exception exception3)
		{
			UnityEngine.Debug.LogException(exception3);
		}
	}

	public static void SaveProfileAll(bool immediate = false)
	{
		if (profileMainPath == null)
		{
			UnityEngine.Debug.Log("SaveProfileAll: Currently using transient profile.");
			return;
		}
		SaveProfileMain(immediate);
		SaveProfileStats(immediate);
		SaveProfileContinue(immediate);
	}

	public static void SaveProfileMain(bool immediate = false)
	{
		_needsSaving_Main = true;
		StartSaveRoutine(immediate);
	}

	public static void SaveProfileStats(bool immediate = false)
	{
		_needsSaving_Stats = true;
		StartSaveRoutine(immediate);
	}

	public static void SaveProfileContinue(bool immediate = false)
	{
		_needsSaving_Continue = true;
		StartSaveRoutine(immediate);
	}

	private static void StartSaveRoutine(bool immediate)
	{
		if (ManagerBase<GlobalLogicPackage>.instance == null)
		{
			immediate = true;
		}
		if (((_saveRoutine != null) & immediate) && ManagerBase<GlobalLogicPackage>.instance != null)
		{
			ManagerBase<GlobalLogicPackage>.instance.StopCoroutine(_saveRoutine);
			_saveRoutine = null;
		}
		onSaveStarted?.Invoke();
		if (immediate)
		{
			Save();
		}
		else
		{
			_saveRoutine = ManagerBase<GlobalLogicPackage>.instance.StartCoroutine(Routine());
		}
		static IEnumerator Routine()
		{
			yield return new WaitForSecondsRealtime(0.5f);
			Save();
		}
		static void Save()
		{
			if (_needsSaving_Main)
			{
				_needsSaving_Main = false;
				try
				{
					if (profileMainPath == null)
					{
						UnityEngine.Debug.Log("SaveProfileMain: Currently using transient profile.");
					}
					else
					{
						_profileMainFile.Write(profileMain);
						CreateAutoRecoveryBackup(profileMainPath);
						UnityEngine.Debug.Log("Profile Main saved.");
					}
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
			}
			if (_needsSaving_Continue)
			{
				_needsSaving_Continue = false;
				try
				{
					if (profileContinuePath == null)
					{
						UnityEngine.Debug.Log("SaveProfileContinue: Currently using transient profile.");
					}
					else
					{
						_profileContinueFile.Write(profileContinue);
						CreateAutoRecoveryBackup(profileContinuePath);
						UnityEngine.Debug.Log("Profile Continue saved.");
					}
				}
				catch (Exception exception2)
				{
					UnityEngine.Debug.LogException(exception2);
				}
			}
			if (_needsSaving_Stats)
			{
				_needsSaving_Stats = false;
				try
				{
					profileStats.UpdateTotalData(profileMain.totalPlayTimeMinutes);
					if (profileStatsPath == null)
					{
						UnityEngine.Debug.Log("SaveProfileStats: Currently using transient profile.");
					}
					else
					{
						_profileStatsFile.Write(profileStats);
						CreateAutoRecoveryBackup(profileStatsPath);
						UnityEngine.Debug.Log("Profile Stats saved.");
					}
				}
				catch (Exception exception3)
				{
					UnityEngine.Debug.LogException(exception3);
				}
			}
			_saveRoutine = null;
			onSaveEnded?.Invoke();
		}
	}

	private static DewProfileState GetProfileState(string path, out DewProfile peekMain, out DewProfileStats peekStats, out DewProfileContinue peekContinue)
	{
		peekMain = null;
		peekStats = null;
		peekContinue = null;
		if (!Exists(path))
		{
			return DewProfileState.Corrupted;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
		if (fileNameWithoutExtension.StartsWith("err-"))
		{
			return DewProfileState.Corrupted;
		}
		try
		{
			if (!new SaveFile<DewProfile>(path).TryRead(out peekMain))
			{
				peekMain = null;
			}
			if (peekMain == null)
			{
				return DewProfileState.Corrupted;
			}
			DewSubProfileState dewSubProfileState = TryPeekProfile<DewProfileStats>(path, "stats", out peekStats);
			if (dewSubProfileState == DewSubProfileState.NonExistent)
			{
				peekStats = new DewProfileStats();
			}
			if (dewSubProfileState == DewSubProfileState.Corrupted)
			{
				return DewProfileState.Corrupted;
			}
			DewSubProfileState dewSubProfileState2 = TryPeekProfile<DewProfileContinue>(path, "continue", out peekContinue);
			if (dewSubProfileState2 == DewSubProfileState.NonExistent)
			{
				peekContinue = new DewProfileContinue();
			}
			if (dewSubProfileState2 == DewSubProfileState.Corrupted)
			{
				return DewProfileState.Corrupted;
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
			return DewProfileState.Corrupted;
		}
		if (!fileNameWithoutExtension.StartsWith(SavePrefix))
		{
			if (peekMain.saveVersion > 10)
			{
				return DewProfileState.UnsupportedEdition;
			}
			return DewProfileState.Convertible;
		}
		if (peekMain.saveVersion > 10)
		{
			return DewProfileState.UnsupportedVersion;
		}
		if (peekStats == null)
		{
			return DewProfileState.Corrupted;
		}
		return DewProfileState.Normal;
	}

	private static DewSubProfileState TryPeekProfile<T>(string path, string subprofileType, out T peek) where T : class
	{
		if (subprofileType != null)
		{
			path = GetProfileSubFilePath(path, subprofileType);
		}
		return TryPeekProfile_Imp<T>(path, out peek);
	}

	private static DewSubProfileState TryPeekProfile_Imp<T>(string path, out T peek) where T : class
	{
		try
		{
			peek = null;
			if (!Exists(path))
			{
				return DewSubProfileState.NonExistent;
			}
			if (!new SaveFile<T>(path).TryRead(out peek))
			{
				peek = null;
				return DewSubProfileState.Corrupted;
			}
			return (peek == null) ? DewSubProfileState.Corrupted : DewSubProfileState.Normal;
		}
		catch (Exception message)
		{
			UnityEngine.Debug.LogWarning(message);
			peek = null;
			return DewSubProfileState.Corrupted;
		}
	}

	public static List<DewProfileItem> GetProfiles()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		List<DewProfileItem> list = new List<DewProfileItem>();
		string[] files = Directory.GetFiles(Path.Join(string.op_Implicit(Application.persistentDataPath), string.op_Implicit("QuickSave")), "*_profile*.json");
		foreach (string path in files)
		{
			DewProfileItem dewProfileItem = new DewProfileItem
			{
				path = path
			};
			dewProfileItem.state = GetProfileState(path, out dewProfileItem.peek, out dewProfileItem.peekStats, out var _);
			list.Add(dewProfileItem);
		}
		return list;
	}

	public static List<DewProfileItem> GetNormalProfiles()
	{
		List<DewProfileItem> profiles = GetProfiles();
		for (int num = profiles.Count - 1; num >= 0; num--)
		{
			if (profiles[num].state != DewProfileState.Normal)
			{
				profiles.RemoveAt(num);
			}
		}
		return profiles;
	}

	private static string GetProfileSubFilePath(string mainPath, string type)
	{
		if (string.IsNullOrEmpty(mainPath))
		{
			return null;
		}
		return Path.Combine(Path.GetDirectoryName(mainPath), string.Concat(str2: Path.GetFileNameWithoutExtension(mainPath).Replace(SavePrefix + "profile", ""), str0: SavePrefix, str1: type, str3: ".json"));
	}

	public static void CreateSubProfilesIfNonExistent(string mainPath)
	{
		if (!string.IsNullOrEmpty(mainPath))
		{
			string profileSubFilePath = GetProfileSubFilePath(mainPath, "continue");
			string profileSubFilePath2 = GetProfileSubFilePath(mainPath, "stats");
			if (!Exists(profileSubFilePath))
			{
				new SaveFile<DewProfileContinue>(profileSubFilePath, new DewProfileContinue());
			}
			if (!Exists(profileSubFilePath2))
			{
				new SaveFile<DewProfileStats>(profileSubFilePath2, new DewProfileStats());
			}
		}
	}

	public static void SyncAchievements()
	{
		if (!DewSteam.isInitialized || !DewSteam.isAchievementReady)
		{
			return;
		}
		foreach (KeyValuePair<string, DewProfile.AchievementData> achievement in profileMain.achievements)
		{
			SteamUserStats.SetStat("STAT_" + achievement.Key, achievement.Value.currentProgress);
			if (achievement.Value.isCompleted)
			{
				SteamUserStats.SetAchievement(achievement.Key);
			}
		}
		SteamUserStats.StoreStats();
	}

	[AsyncStateMachine(typeof(_003CUpsertProfile_003Ed__72))]
	internal static UniTask<bool> UpsertProfile()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CUpsertProfile_003Ed__72 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder<bool>.Create();
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CUpsertProfile_003Ed__72>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit()
	{
		onSettingsChanged = null;
	}

	private static void ApplyPerSceneSettings()
	{
		if (Application.isPlaying)
		{
			ApplyConsoleSettings();
			UpdateTerrainAndVegetationQuality();
			UpdateFogQuality();
			UpdateShadowQuality();
			UpdateTextureQuality();
			string name = SceneManager.GetActiveScene().name;
			if (name == "PlayLobby")
			{
				Dew.BakeMeshInterval = 1f / 60f;
			}
			else
			{
				Dew.BakeMeshInterval = 1f / 15f;
			}
			if (ManagerBase<InputManager>.instance != null)
			{
				ManagerBase<InputManager>.instance.ResetInputDevices();
			}
			UnityEngine.Debug.Log("Current scene: " + name);
		}
	}

	private static void ApplyConsoleSettings()
	{
	}

	private static void UpdateTextureQuality()
	{
		switch (platformSettings.graphics.textureQuality)
		{
		case Quality3Levels.Low:
			QualitySettings.globalTextureMipmapLimit = 2;
			break;
		case Quality3Levels.Medium:
			QualitySettings.globalTextureMipmapLimit = 1;
			break;
		case Quality3Levels.High:
			QualitySettings.globalTextureMipmapLimit = 0;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private static void UpdateShadowQuality()
	{
		SetShadowQuality((platformSettings.graphics.shadowQuality != QualityOff4Levels.Low) ? QualityOff4Levels.Low : QualityOff4Levels.Off);
		Dew.CallDelayed(() =>
		{
			SetShadowQuality(platformSettings.graphics.shadowQuality);
		}, 3);
	}

	public static void SetShadowQuality(QualityOff4Levels q)
	{
		UnlockedURPAsset currentUnlockedURPAsset = URPUnlockerAPI.CurrentUnlockedURPAsset;
		switch (q)
		{
		case QualityOff4Levels.Off:
			currentUnlockedURPAsset.Lighting.MainLightShadowsCasting = false;
			currentUnlockedURPAsset.Lighting.AdditionalLightsShadowsCasting = false;
			currentUnlockedURPAsset.Lighting.SupportsLightCookies = true;
			currentUnlockedURPAsset.Lighting.AdditionalLightsCookieResolution = (LightCookieResolution)512;
			break;
		case QualityOff4Levels.Low:
			currentUnlockedURPAsset.Lighting.MainLightShadowsCasting = true;
			currentUnlockedURPAsset.Lighting.MainLightShadowResolution = (ShadowResolution)1024;
			currentUnlockedURPAsset.Lighting.AdditionalLightsShadowsCasting = false;
			currentUnlockedURPAsset.Lighting.SupportsLightCookies = true;
			currentUnlockedURPAsset.Lighting.AdditionalLightsCookieResolution = (LightCookieResolution)512;
			break;
		case QualityOff4Levels.Medium:
			currentUnlockedURPAsset.Lighting.MainLightShadowsCasting = true;
			currentUnlockedURPAsset.Lighting.MainLightShadowResolution = (ShadowResolution)1024;
			currentUnlockedURPAsset.Lighting.AdditionalLightsShadowsCasting = true;
			currentUnlockedURPAsset.Lighting.AdditionalLightsShadowResolutionTierHigh = 256;
			currentUnlockedURPAsset.Lighting.AdditionalLightsShadowResolutionTierMedium = 256;
			currentUnlockedURPAsset.Lighting.AdditionalLightsShadowResolutionTierLow = 256;
			currentUnlockedURPAsset.Lighting.SupportsLightCookies = true;
			currentUnlockedURPAsset.Lighting.AdditionalLightsCookieResolution = (LightCookieResolution)1024;
			break;
		case QualityOff4Levels.High:
			currentUnlockedURPAsset.Lighting.MainLightShadowsCasting = true;
			currentUnlockedURPAsset.Lighting.MainLightShadowResolution = (ShadowResolution)2048;
			currentUnlockedURPAsset.Lighting.AdditionalLightsShadowsCasting = true;
			currentUnlockedURPAsset.Lighting.AdditionalLightsShadowResolutionTierHigh = 512;
			currentUnlockedURPAsset.Lighting.AdditionalLightsShadowResolutionTierMedium = 512;
			currentUnlockedURPAsset.Lighting.AdditionalLightsShadowResolutionTierLow = 512;
			currentUnlockedURPAsset.Lighting.SupportsLightCookies = true;
			currentUnlockedURPAsset.Lighting.AdditionalLightsCookieResolution = (LightCookieResolution)2048;
			break;
		case QualityOff4Levels.Ultra:
			currentUnlockedURPAsset.Lighting.MainLightShadowsCasting = true;
			currentUnlockedURPAsset.Lighting.MainLightShadowResolution = (ShadowResolution)4096;
			currentUnlockedURPAsset.Lighting.AdditionalLightsShadowsCasting = true;
			currentUnlockedURPAsset.Lighting.AdditionalLightsShadowResolutionTierHigh = 1024;
			currentUnlockedURPAsset.Lighting.AdditionalLightsShadowResolutionTierMedium = 1024;
			currentUnlockedURPAsset.Lighting.AdditionalLightsShadowResolutionTierLow = 1024;
			currentUnlockedURPAsset.Lighting.SupportsLightCookies = true;
			currentUnlockedURPAsset.Lighting.AdditionalLightsCookieResolution = (LightCookieResolution)2048;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private static void UpdateTerrainAndVegetationQuality()
	{
		int num = platformSettings.graphics.terrainQuality switch
		{
			Quality3Levels.Low => 100, 
			Quality3Levels.Medium => 65, 
			Quality3Levels.High => 15, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		float detailObjectDensity = platformSettings.graphics.vegetationQuality switch
		{
			Quality3Levels.Low => 0.25f, 
			Quality3Levels.Medium => 0.7f, 
			Quality3Levels.High => 1f, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		Terrain[] array = UnityEngine.Object.FindObjectsOfType<Terrain>();
		foreach (Terrain obj in array)
		{
			obj.heightmapPixelError = num;
			obj.detailObjectDensity = detailObjectDensity;
			obj.shadowCastingMode = ((platformSettings.graphics.shadowQuality >= QualityOff4Levels.Medium) ? ShadowCastingMode.On : ShadowCastingMode.Off);
		}
	}

	private static void UpdateFogQuality()
	{
		QualityOff3Levels fogQuality = platformSettings.graphics.fogQuality;
		VolumetricFog[] array = UnityEngine.Object.FindObjectsOfType<VolumetricFog>(true);
		foreach (VolumetricFog val in array)
		{
			try
			{
				if (!((UnityEngine.Object)(object)val.profile).name.Contains("(Clone)"))
				{
					val.profile = UnityEngine.Object.Instantiate<VolumetricFogProfile>(val.profile);
				}
				switch (fogQuality)
				{
				case QualityOff3Levels.Off:
				case QualityOff3Levels.Low:
					val.profile.raymarchQuality = 3;
					val.profile.raymarchMinStep = 2.5f;
					val.profile.jittering = 4f;
					break;
				case QualityOff3Levels.Medium:
					val.profile.raymarchQuality = 4;
					val.profile.raymarchMinStep = 1.5f;
					val.profile.jittering = 3f;
					break;
				case QualityOff3Levels.High:
					val.profile.raymarchQuality = 5;
					val.profile.raymarchMinStep = 1f;
					val.profile.jittering = 2f;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				bool flag = ((Component)(object)val).TryGetComponent(out IImportantFogVolume _);
				((Behaviour)(object)val).enabled = flag || fogQuality != QualityOff3Levels.Off;
				((Component)(object)val).GetComponent<MeshRenderer>().enabled = ((Behaviour)(object)val).enabled;
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
		VolumetricFogManager[] array2 = UnityEngine.Object.FindObjectsOfType<VolumetricFogManager>();
		switch (fogQuality)
		{
		case QualityOff3Levels.Off:
		case QualityOff3Levels.Low:
		{
			VolumetricFogManager[] array3 = array2;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].downscaling = 6f;
			}
			break;
		}
		case QualityOff3Levels.Medium:
		{
			VolumetricFogManager[] array3 = array2;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].downscaling = 6f;
			}
			break;
		}
		case QualityOff3Levels.High:
		{
			VolumetricFogManager[] array3 = array2;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].downscaling = 2f;
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		array = UnityEngine.Object.FindObjectsOfType<VolumetricFog>(true);
		foreach (VolumetricFog val2 in array)
		{
			if (((Component)(object)val2).gameObject.activeSelf)
			{
				((Component)(object)val2).gameObject.SetActive(value: false);
				((Component)(object)val2).gameObject.SetActive(value: true);
			}
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void ApplySettings()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		try
		{
			if (ManagerBase<AudioManager>.instance != null)
			{
				ManagerBase<AudioManager>.instance.UpdateMixerAttenuations();
			}
			if (ManagerBase<InGameTutorialManager>.instance != null && ManagerBase<InGameTutorialManager>.instance.isTutorialActive && profileMain.gameplay.disableTutorial)
			{
				ManagerBase<InGameTutorialManager>.instance.StopTutorials();
			}
			if (ManagerBase<CursorManager>.instance != null)
			{
				ManagerBase<CursorManager>.instance.ResizeTextures();
			}
			bool flag = false;
			flag = DewInput.currentMode == InputMode.Gamepad && ManagerBase<GlobalUIManager>.instance != null && (Screen.width != platformSettings.graphics.resolutionWidth || Screen.height != platformSettings.graphics.resolutionHeight || Screen.fullScreenMode != platformSettings.graphics.fullScreenMode);
			Screen.SetResolution(platformSettings.graphics.resolutionWidth, platformSettings.graphics.resolutionHeight, platformSettings.graphics.fullScreenMode);
			if (ManagerBase<GlobalUIManager>.instance != null)
			{
				ManagerBase<GlobalUIManager>.instance.EnforceFontFallbackOrder();
			}
			Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>(includeInactive: true);
			foreach (Transform transform in array)
			{
				ILangaugeChangedCallback[] components = transform.GetComponents<ILangaugeChangedCallback>();
				foreach (ILangaugeChangedCallback langaugeChangedCallback in components)
				{
					try
					{
						langaugeChangedCallback.OnLanguageChanged();
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception, langaugeChangedCallback as UnityEngine.Object);
					}
				}
				ISettingsChangedCallback[] components2 = transform.GetComponents<ISettingsChangedCallback>();
				foreach (ISettingsChangedCallback settingsChangedCallback in components2)
				{
					try
					{
						settingsChangedCallback.OnSettingsChanged();
					}
					catch (Exception exception2)
					{
						UnityEngine.Debug.LogException(exception2, settingsChangedCallback as UnityEngine.Object);
					}
				}
			}
			ApplyPerSceneSettings();
			ApplyCultureSettings();
			if (ManagerBase<LobbyManager>.instance != null)
			{
				ManagerBase<LobbyManager>.instance.SwitchService();
			}
			DewResources.ClearVariantsOfVarDef(DewResources.vOtherPlayersTonedDown, repairReferences: true);
			DewResources.ClearVariantsOfVarDef(DewResources.vQualityAdjusted, repairReferences: true);
			onSettingsChanged?.Invoke();
			if (flag)
			{
				ManagerBase<GlobalUIManager>.instance.ClearFocusWithoutHistory();
			}
		}
		catch (Exception exception3)
		{
			UnityEngine.Debug.LogException(exception3);
		}
	}

	public static void ApplyCultureSettings()
	{
		try
		{
			CultureInfo cultureInfo = (CultureInfo)CultureInfo.GetCultureInfo(CultureInfo.CurrentUICulture.Name).Clone();
			cultureInfo.NumberFormat.PercentSymbol = "%";
			if (profileMain.language.StartsWith("zh-"))
			{
				cultureInfo.NumberFormat.NumberGroupSeparator = "";
				cultureInfo.NumberFormat.CurrencyGroupSeparator = "";
			}
			if (profileMain.language.StartsWith("tr-"))
			{
				cultureInfo.NumberFormat.PercentPositivePattern = 2;
				cultureInfo.NumberFormat.PercentNegativePattern = 2;
			}
			else
			{
				cultureInfo.NumberFormat.PercentPositivePattern = 1;
				cultureInfo.NumberFormat.PercentNegativePattern = 1;
			}
			CultureInfo.CurrentUICulture = cultureInfo;
			CultureInfo cultureInfo2 = (CultureInfo)CultureInfo.GetCultureInfo(CultureInfo.CurrentCulture.Name).Clone();
			cultureInfo2.NumberFormat.PercentSymbol = "%";
			if (profileMain.language.StartsWith("zh-"))
			{
				cultureInfo2.NumberFormat.NumberGroupSeparator = "";
				cultureInfo2.NumberFormat.CurrencyGroupSeparator = "";
			}
			if (profileMain.language.StartsWith("tr-"))
			{
				cultureInfo2.NumberFormat.PercentPositivePattern = 2;
				cultureInfo2.NumberFormat.PercentNegativePattern = 2;
			}
			else
			{
				cultureInfo2.NumberFormat.PercentPositivePattern = 1;
				cultureInfo2.NumberFormat.PercentNegativePattern = 1;
			}
			CultureInfo.CurrentCulture = cultureInfo2;
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}
}
