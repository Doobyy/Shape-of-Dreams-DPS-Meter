using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;

public static class DewReverie
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CReceiveRewardOfReverie_003Ed__1 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<bool> _003C_003Et__builder;

		public DewProfile.ReverieDataBase data;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			bool result;
			try
			{
				try
				{
					Awaiter<bool> val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
						goto IL_0090;
					}
					if (data != null)
					{
						if (data.grantedItems != null)
						{
							val = DewItem.GenerateItems(data.grantedItems.ToList()).GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<bool>, _003CReceiveRewardOfReverie_003Ed__1>(ref val, ref this);
								return;
							}
							goto IL_0090;
						}
						goto IL_009d;
					}
					result = false;
					goto end_IL_000a;
					IL_0090:
					if (val.GetResult())
					{
						goto IL_009d;
					}
					result = false;
					goto end_IL_000a;
					IL_009d:
					DewSave.profileMain.completedReveries++;
					DewSave.profileMain.stardust += data.grantedStardust;
					result = true;
					end_IL_000a:;
				}
				catch (Exception e)
				{
					DewSessionError.ShowError(e, isFatal: false, isGame: false);
					result = false;
				}
				finally
				{
					if (num < 0 && ManagerBase<TransitionManager>.instance != null)
					{
						ManagerBase<TransitionManager>.instance.SetBusy(value: false);
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
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

	public static void CheckSpecialReveries()
	{
		if ((DewSave.profileMain.specialReverie == null || DewSave.profileMain.specialReverie.IsEmpty()) && CheckSpecialReveries_T1Emote())
		{
			DewSave.SaveProfileMain();
		}
	}

	[AsyncStateMachine(typeof(_003CReceiveRewardOfReverie_003Ed__1))]
	public static UniTask<bool> ReceiveRewardOfReverie(DewProfile.ReverieDataBase data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CReceiveRewardOfReverie_003Ed__1 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder<bool>.Create();
		obj.data = data;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CReceiveRewardOfReverie_003Ed__1>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	public static void ClearDailyReverie(int index)
	{
		DewSave.profileMain.reverieSlots[index] = new DewProfile.DailyReverieData
		{
			type = null,
			nextRefillTimestamp = 1L
		};
		DewSave.SaveProfileMain();
	}

	public static void StartNextSpecialReverieOrClear()
	{
		if (Dew.reveriesByName.TryGetValue(DewSave.profileMain.specialReverie.type, out var value) && value is DewSpecialReverieItem dewSpecialReverieItem && dewSpecialReverieItem.nextReverie != null)
		{
			SetSpecialReverie(dewSpecialReverieItem.nextReverie, DewSave.profileMain.specialReverie.timeLimitTimestamp.ToDateTime());
		}
		else
		{
			ClearSpecialReverie();
		}
	}

	public static void ClearSpecialReverie()
	{
		DewSave.profileMain.specialReverie = new DewProfile.SpecialReverieData
		{
			type = null
		};
		DewSave.SaveProfileMain();
	}

	public static void SetSpecialReverie(Type type, DateTime endDate)
	{
		DewSpecialReverieItem dewSpecialReverieItem = (DewSpecialReverieItem)Activator.CreateInstance(type);
		dewSpecialReverieItem.OnSetupReverie();
		DewProfile.SpecialReverieData specialReverieData = new DewProfile.SpecialReverieData
		{
			type = type.Name,
			persistentVariables = new Dictionary<string, string>(),
			grantedStardust = dewSpecialReverieItem.grantedStardust,
			grantedItems = dewSpecialReverieItem.grantedItems,
			isComplete = false,
			timeLimitTimestamp = endDate.ToTimestamp()
		};
		dewSpecialReverieItem.SaveReverieStateToData(specialReverieData);
		DewSave.profileMain.specialReverie = specialReverieData;
		DewSave.SaveProfileMain();
	}

	public static void SetDailyReverie<T>(int index)
	{
		SetDailyReverie(index, typeof(T));
	}

	public static void SetDailyReverie(int index, Type type)
	{
		DewProfile.DailyReverieData newDailyReverieData = GetNewDailyReverieData(type);
		DewSave.profileMain.lastReverieTypes.Add(type.Name);
		DewSave.profileMain.reverieSlots[index] = newDailyReverieData;
		DewSave.SaveProfileMain();
	}

	public static void SetRandomDailyReverie(int index)
	{
		SetDailyReverie(index, GetRandomDailyReverieType());
	}

	private static DewProfile.DailyReverieData GetNewDailyReverieData(Type type)
	{
		DewReverieItem dewReverieItem = (DewReverieItem)Activator.CreateInstance(type);
		dewReverieItem.OnSetupReverie();
		DewProfile.DailyReverieData dailyReverieData = new DewProfile.DailyReverieData
		{
			type = type.Name,
			persistentVariables = new Dictionary<string, string>(),
			grantedStardust = dewReverieItem.grantedStardust,
			grantedItems = dewReverieItem.grantedItems,
			isComplete = false
		};
		dewReverieItem.SaveReverieStateToData(dailyReverieData);
		return dailyReverieData;
	}

	private static Type GetRandomDailyReverieType()
	{
		List<Type> list = new List<Type>();
		list.AddRange(from r in Dew.allReveries
			where !r.excludeFromPool
			select r.GetType() into t
			where !DewSave.profileMain.lastReverieTypes.Contains(t.Name)
			select t);
		if (list.Count == 0)
		{
			while (DewSave.profileMain.lastReverieTypes.Count > 3)
			{
				DewSave.profileMain.lastReverieTypes.RemoveAt(0);
			}
			list.AddRange(from r in Dew.allReveries
				where !r.excludeFromPool
				select r.GetType() into t
				where !DewSave.profileMain.lastReverieTypes.Contains(t.Name)
				select t);
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	private static bool CheckSpecialReveries_Founders()
	{
		DateTime dateTime = new DateTime(2025, 8, 1);
		DateTime dateTime2 = new DateTime(2025, 12, 31, 23, 59, 59);
		if (DateTime.Now < dateTime || DateTime.Now > dateTime2)
		{
			return false;
		}
		if (DewSave.profileMain.experienceFlags.Contains("SpecialReverie_Founders"))
		{
			return false;
		}
		DewSave.profileMain.experienceFlags.Add("SpecialReverie_Founders");
		SetSpecialReverie(typeof(Rev_Special_Founders), dateTime2);
		return true;
	}

	private static bool CheckSpecialReveries_2025WinterSale()
	{
		DateTime dateTime = new DateTime(2025, 12, 19);
		DateTime dateTime2 = new DateTime(2026, 1, 19, 23, 59, 59);
		if (DateTime.Now < dateTime || DateTime.Now > dateTime2)
		{
			return false;
		}
		if (DewSave.profileMain.experienceFlags.Contains("SpecialReverie_2025WinterSale"))
		{
			return false;
		}
		DewSave.profileMain.experienceFlags.Add("SpecialReverie_2025WinterSale");
		SetSpecialReverie(typeof(Rev_Special_2025WinterSale), dateTime2);
		return true;
	}

	private static bool CheckSpecialReveries_T1Emote()
	{
		DateTime dateTime = new DateTime(2026, 6, 1, 0, 0, 0);
		DateTime dateTime2 = new DateTime(2026, 6, 30, 23, 59, 59);
		if (DateTime.Now < dateTime || DateTime.Now > dateTime2)
		{
			return false;
		}
		if (DewSave.profileMain.experienceFlags.Contains("SpecialReverie_T1Emote"))
		{
			return false;
		}
		DewSave.profileMain.experienceFlags.Add("SpecialReverie_T1Emote");
		SetSpecialReverie(typeof(Rev_Special_T1Emote), dateTime2);
		return true;
	}

	private static bool CheckSpecialReveries_NextFest()
	{
		return false;
	}
}
