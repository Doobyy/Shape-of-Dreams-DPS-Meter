using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Sirenix.Utilities;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

public static class DewItem
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass12_0
	{
		public List<string> itemsToSkip;

		internal bool _003CGenerateOrRefreshDLCItems_003Eb__0(string item)
		{
			return !itemsToSkip.Contains(item);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass5_0
	{
		public bool showMessages;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass5_1
	{
		public SteamTicketForWebApi ticket;

		internal UniTask<List<DecryptedItemData>> _003CGenerateItems_003Eb__1(string i)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			return RequestItemGeneration_Imp(new Dictionary<string, string> { { "item", i } }, ticket);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CGenerateItem_003Ed__4 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<bool> _003C_003Et__builder;

		public string itemName;

		public bool showMessages;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			bool result;
			try
			{
				Awaiter<bool> val;
				if (num != 0)
				{
					val = GenerateItems(new List<string> { itemName }, showMessages).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<bool>, _003CGenerateItem_003Ed__4>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
				}
				result = val.GetResult();
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CGenerateItems_003Ed__5 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<bool> _003C_003Et__builder;

		public bool showMessages;

		public List<string> itemNames;

		private _003C_003Ec__DisplayClass5_1 _003C_003E8__1;

		private _003C_003Ec__DisplayClass5_0 _003C_003E8__2;

		private List<string> _003ClocalAccs_003E5__2;

		private List<string> _003ClocalNametags_003E5__3;

		private List<string> _003ClocalEmotes_003E5__4;

		private List<string> _003ClocalSkins_003E5__5;

		private List<string> _003CserverItems_003E5__6;

		private List<string> _003CduplicateItemNames_003E5__7;

		private List<string> _003CnewItemNames_003E5__8;

		private List<DecryptedItemData> _003Cgenerated_003E5__9;

		private Awaiter<SteamTicketForWebApi> _003C_003Eu__1;

		private Awaiter<List<DecryptedItemData>[]> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_028b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0290: Unknown result type (might be due to invalid IL or missing references)
			//IL_0298: Unknown result type (might be due to invalid IL or missing references)
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0252: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_025b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0270: Unknown result type (might be due to invalid IL or missing references)
			//IL_0272: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			bool result3;
			try
			{
				if ((uint)num > 1u)
				{
					_003C_003E8__2.showMessages = showMessages;
				}
				try
				{
					Awaiter<SteamTicketForWebApi> val;
					List<string>.Enumerator enumerator;
					if (num != 0)
					{
						if (num == 1)
						{
							goto IL_020f;
						}
						if (_003C_003E8__2.showMessages)
						{
							ManagerBase<TransitionManager>.instance.SetBusy(value: true);
						}
						_003ClocalAccs_003E5__2 = new List<string>();
						_003ClocalNametags_003E5__3 = new List<string>();
						_003ClocalEmotes_003E5__4 = new List<string>();
						_003ClocalSkins_003E5__5 = new List<string>();
						_003CserverItems_003E5__6 = new List<string>();
						enumerator = itemNames.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								string current = enumerator.Current;
								if (GetItemLightPrefab(current) == null)
								{
									throw new DewException(DewExceptionType.UnknownItem, current);
								}
								if (IsItemGeneratedFromServer(current))
								{
									_003CserverItems_003E5__6.Add(current);
									continue;
								}
								if (current.StartsWith("Acc_"))
								{
									_003ClocalAccs_003E5__2.Add(current);
								}
								if (current.StartsWith("Nametag_"))
								{
									_003ClocalNametags_003E5__3.Add(current);
								}
								if (current.StartsWith("Emote_"))
								{
									_003ClocalEmotes_003E5__4.Add(current);
								}
								if (current.StartsWith("Skin_"))
								{
									_003ClocalSkins_003E5__5.Add(current);
								}
							}
						}
						finally
						{
							if (num < 0)
							{
								((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
							}
						}
						_003CduplicateItemNames_003E5__7 = new List<string>();
						_003CnewItemNames_003E5__8 = new List<string>();
						_003Cgenerated_003E5__9 = new List<DecryptedItemData>();
						if (_003CserverItems_003E5__6.Count <= 0)
						{
							goto IL_037e;
						}
						_003C_003E8__1 = new _003C_003Ec__DisplayClass5_1();
						if (_003C_003E8__2.showMessages)
						{
							ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.WaitingForSteam);
						}
						val = DewSteam.GetTicketForWebApi().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<SteamTicketForWebApi>, _003CGenerateItems_003Ed__5>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
					}
					SteamTicketForWebApi result = val.GetResult();
					_003C_003E8__1.ticket = result;
					goto IL_020f;
					IL_020f:
					try
					{
						Awaiter<List<DecryptedItemData>[]> val2;
						if (num != 1)
						{
							if (_003C_003E8__2.showMessages)
							{
								ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.RequestingItemFromServer);
							}
							val2 = UniTask.WhenAll<List<DecryptedItemData>>(EnumerableAsyncExtensions.Select<string, List<DecryptedItemData>>((IEnumerable<string>)_003CserverItems_003E5__6, (Func<string, UniTask<List<DecryptedItemData>>>)((string i) =>
							{
								//IL_0017: Unknown result type (might be due to invalid IL or missing references)
								return RequestItemGeneration_Imp(new Dictionary<string, string> { { "item", i } }, _003C_003E8__1.ticket);
							})).ToArray()).GetAwaiter();
							if (!val2.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__2 = val2;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<List<DecryptedItemData>[]>, _003CGenerateItems_003Ed__5>(ref val2, ref this);
								return;
							}
						}
						else
						{
							val2 = _003C_003Eu__2;
							_003C_003Eu__2 = default;
							num = (_003C_003E1__state = -1);
						}
						List<DecryptedItemData>[] result2 = val2.GetResult();
						foreach (List<DecryptedItemData> collection in result2)
						{
							_003Cgenerated_003E5__9.AddRange(collection);
						}
						List<DecryptedItemData>.Enumerator enumerator2 = _003Cgenerated_003E5__9.GetEnumerator();
						try
						{
							while (enumerator2.MoveNext())
							{
								DecryptedItemData current2 = enumerator2.Current;
								if (DewSave.items.ContainsUsable(current2))
								{
									_003CduplicateItemNames_003E5__7.Add(GetLocalizedNameWithType(current2.item));
								}
								else
								{
									_003CnewItemNames_003E5__8.Add(GetLocalizedNameWithType(current2.item));
								}
								DewSave.AddServerGeneratedItem(current2);
							}
						}
						finally
						{
							if (num < 0)
							{
								((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
							}
						}
					}
					finally
					{
						if (num < 0 && _003C_003E8__1.ticket != null)
						{
							((IDisposable)_003C_003E8__1.ticket).Dispose();
						}
					}
					_003C_003E8__1 = null;
					goto IL_037e;
					IL_037e:
					enumerator = _003ClocalAccs_003E5__2.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							string current3 = enumerator.Current;
							if (DewSave.profileMain.accessories.TryGetValue(current3, out var value) && value.isUnlocked)
							{
								_003CduplicateItemNames_003E5__7.Add(GetLocalizedNameWithType(current3));
								continue;
							}
							DewSave.profileMain.UnlockAccessory(current3, null);
							_003CnewItemNames_003E5__8.Add(GetLocalizedNameWithType(current3));
						}
					}
					finally
					{
						if (num < 0)
						{
							((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
						}
					}
					enumerator = _003ClocalEmotes_003E5__4.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							string current4 = enumerator.Current;
							if (DewSave.profileMain.emotes.TryGetValue(current4, out var value2) && value2.isUnlocked)
							{
								_003CduplicateItemNames_003E5__7.Add(GetLocalizedNameWithType(current4));
								continue;
							}
							DewSave.profileMain.UnlockEmote(current4, null);
							_003CnewItemNames_003E5__8.Add(GetLocalizedNameWithType(current4));
						}
					}
					finally
					{
						if (num < 0)
						{
							((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
						}
					}
					enumerator = _003ClocalNametags_003E5__3.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							string current5 = enumerator.Current;
							if (DewSave.profileMain.nametags.TryGetValue(current5, out var value3) && value3.isUnlocked)
							{
								_003CduplicateItemNames_003E5__7.Add(GetLocalizedNameWithType(current5));
								continue;
							}
							DewSave.profileMain.UnlockNametag(current5, null);
							_003CnewItemNames_003E5__8.Add(GetLocalizedNameWithType(current5));
						}
					}
					finally
					{
						if (num < 0)
						{
							((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
						}
					}
					enumerator = _003ClocalSkins_003E5__5.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							string current6 = enumerator.Current;
							if (DewSave.profileMain.skins.TryGetValue(current6, out var value4) && value4.isUnlocked)
							{
								_003CduplicateItemNames_003E5__7.Add(GetLocalizedNameWithType(current6));
								continue;
							}
							DewSave.profileMain.UnlockSkin(current6, null);
							_003CnewItemNames_003E5__8.Add(GetLocalizedNameWithType(current6));
						}
					}
					finally
					{
						if (num < 0)
						{
							((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
						}
					}
					_003CduplicateItemNames_003E5__7.Sort();
					_003CnewItemNames_003E5__8.Sort();
					if (_003CnewItemNames_003E5__8.Count > 0)
					{
						_003CGenerateItems_003Eg__PrintMessage_007C5_0(string.Format(DewLocalization.GetUIValue("Redeem_ReceivedItem"), _003CnewItemNames_003E5__8.JoinToString("\n")), ref _003C_003E8__2);
					}
					if (_003CduplicateItemNames_003E5__7.Count > 0)
					{
						_003CGenerateItems_003Eg__PrintMessage_007C5_0(string.Format(DewLocalization.GetUIValue("Redeem_YouAlreadyHaveItem"), _003CduplicateItemNames_003E5__7.JoinToString("\n")), ref _003C_003E8__2);
					}
					UnityEngine.Debug.Log($"[DewItem] {_003CnewItemNames_003E5__8.Count} new items, {_003CduplicateItemNames_003E5__7.Count} duplicate items.");
					DewSave.SaveItems();
					DewSave.SaveProfileMain();
					result3 = true;
				}
				catch (Exception e)
				{
					DewSessionError.ShowError(e, isFatal: false, isGame: false);
					goto IL_069e;
				}
				finally
				{
					if (num < 0 && _003C_003E8__2.showMessages && ManagerBase<TransitionManager>.instance != null)
					{
						ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.Empty);
						ManagerBase<TransitionManager>.instance.SetBusy(value: false);
					}
				}
				goto end_IL_0007;
				IL_069e:
				result3 = false;
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result3);
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CGenerateOrRefreshDLCItems_003Ed__12 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<bool> _003C_003Et__builder;

		private _003C_003Ec__DisplayClass12_0 _003C_003E8__1;

		private Awaiter _003C_003Eu__1;

		private Awaiter<bool> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_052f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0534: Unknown result type (might be due to invalid IL or missing references)
			//IL_053c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0502: Unknown result type (might be due to invalid IL or missing references)
			//IL_0517: Unknown result type (might be due to invalid IL or missing references)
			//IL_0519: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			bool result;
			try
			{
				_ = 1;
				try
				{
					Awaiter<bool> val;
					Awaiter val3;
					if (num != 0)
					{
						if (num == 1)
						{
							val = _003C_003Eu__2;
							_003C_003Eu__2 = default;
							num = (_003C_003E1__state = -1);
							goto IL_054b;
						}
						_003C_003E8__1 = new _003C_003Ec__DisplayClass12_0();
						UnityEngine.Debug.Log("[DewItem] Starting maintenance of DLC items");
						UniTask val2 = DewSteam.EnsureReady();
						val3 = val2.GetAwaiter();
						if (!val3.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val3;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CGenerateOrRefreshDLCItems_003Ed__12>(ref val3, ref this);
							return;
						}
					}
					else
					{
						val3 = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
					}
					val3.GetResult();
					if (DewSave.items == null)
					{
						result = true;
					}
					else
					{
						List<string> list = new List<string>();
						List<string>.Enumerator enumerator = DewSave.items.encryptedItems.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								DecryptedItemData decryptedItemData = GetDecryptedItemData(enumerator.Current);
								if (decryptedItemData != null && !(decryptedItemData.requiredDLC != "Console") && !(decryptedItemData.owner != DewSteam.steamId.m_SteamID.ToString()) && !list.Contains(decryptedItemData.item))
								{
									list.Add(decryptedItemData.item);
								}
							}
						}
						finally
						{
							if (num < 0)
							{
								((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
							}
						}
						if (DewSteam.installedDLCs.Count == 0 && list.Count == 0)
						{
							UnityEngine.Debug.Log("[DewItem] No DLCs installed and no Console items owned; exiting");
							result = true;
						}
						else
						{
							if (DewSteam.installedDLCs.Count > 0)
							{
								IEnumerator<ICosmetic> enumerator2 = Enumerable.Empty<ICosmetic>().Concat(LinqExtensions.Convert<ICosmetic>((IEnumerable)(from s in DewResources.FindAllByNameSubstring<Skin>("Skin_", ResourceLoadSettings.Light)
									where Dew.IsSkinIncludedInGame(s.name)
									select s), (Func<object, ICosmetic>)((object o) => (ICosmetic)o))).Concat(LinqExtensions.Convert<ICosmetic>((IEnumerable)(from e in DewResources.FindAllByNameSubstring<Emote>("Emote_", ResourceLoadSettings.Light)
									where Dew.IsEmoteIncludedInGame(e.name)
									select e), (Func<object, ICosmetic>)((object o) => (ICosmetic)o)))
									.Concat(LinqExtensions.Convert<ICosmetic>((IEnumerable)(from n in DewResources.FindAllByNameSubstring<Nametag>("Nametag_", ResourceLoadSettings.Light)
										where Dew.IsNametagIncludedInGame(n.name)
										select n), (Func<object, ICosmetic>)((object o) => (ICosmetic)o)))
									.Concat(LinqExtensions.Convert<ICosmetic>((IEnumerable)(from a in DewResources.FindAllByNameSubstring<Accessory>("Acc_", ResourceLoadSettings.Light)
										where Dew.IsAccessoryIncludedInGame(a.name)
										select a), (Func<object, ICosmetic>)((object o) => (ICosmetic)o)))
									.GetEnumerator();
								try
								{
									while (enumerator2.MoveNext())
									{
										ICosmetic current = enumerator2.Current;
										if (!current.generatedFromServer || current.dlcIds == null || current.dlcIds.Length == 0 || list.Contains(current.name))
										{
											continue;
										}
										enumerator = DewSteam.installedDLCs.GetEnumerator();
										try
										{
											while (enumerator.MoveNext())
											{
												string current2 = enumerator.Current;
												if (current.dlcIds.Contains(current2))
												{
													list.Add(current.name);
													break;
												}
											}
										}
										finally
										{
											if (num < 0)
											{
												((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
											}
										}
									}
								}
								finally
								{
									if (num < 0)
									{
										enumerator2?.Dispose();
									}
								}
							}
							if (list.Count == 0)
							{
								UnityEngine.Debug.Log("[DewItem] No items eligible; exiting");
								result = true;
							}
							else
							{
								int count = list.Count;
								_003C_003E8__1.itemsToSkip = new List<string>();
								enumerator = DewSave.items.encryptedItems.GetEnumerator();
								try
								{
									while (enumerator.MoveNext())
									{
										DecryptedItemData decryptedItemData2 = GetDecryptedItemData(enumerator.Current);
										if (decryptedItemData2 != null && list.Contains(decryptedItemData2.item) && decryptedItemData2.owner == DewSteam.steamId.m_SteamID.ToString() && !decryptedItemData2.NeedsRefresh())
										{
											_003C_003E8__1.itemsToSkip.Add(decryptedItemData2.item);
										}
									}
								}
								finally
								{
									if (num < 0)
									{
										((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
									}
								}
								list = list.FilterInPlace((string item) => !_003C_003E8__1.itemsToSkip.Contains(item));
								if (list.Count != 0)
								{
									UnityEngine.Debug.Log(string.Format("[DewItem] Found {0} items eligible for ownership. Skipping {1} and proceeding with generating {2}: {3}", count, _003C_003E8__1.itemsToSkip.Count, list.Count, list.JoinToString(", ")));
									val = GenerateItems(list, showMessages: false).GetAwaiter();
									if (!val.IsCompleted)
									{
										num = (_003C_003E1__state = 1);
										_003C_003Eu__2 = val;
										_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<bool>, _003CGenerateOrRefreshDLCItems_003Ed__12>(ref val, ref this);
										return;
									}
									goto IL_054b;
								}
								UnityEngine.Debug.Log(string.Format("[DewItem] Found {0} items eligible for ownership. Skipping all: {1}", count, _003C_003E8__1.itemsToSkip.JoinToString(", ")));
								result = true;
							}
						}
					}
					goto end_IL_000c;
					IL_054b:
					result = val.GetResult();
					end_IL_000c:;
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
					result = false;
				}
			}
			catch (Exception exception2)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception2);
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPostRequestAsync_003Ed__16 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<string> _003C_003Et__builder;

		public float timeoutSeconds;

		public Dictionary<string, string> postData;

		public string url;

		private CancellationTokenSource _003Ccts_003E5__2;

		private UnityWebRequest _003Crequest_003E5__3;

		private Awaiter<UnityWebRequest> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected Obj, but got Unknown
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Invalid comparison between Unknown and I4
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Invalid comparison between Unknown and I4
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Invalid comparison between Unknown and I4
			int num = _003C_003E1__state;
			string text;
			try
			{
				try
				{
					if (num != 0)
					{
						_003Ccts_003E5__2 = new CancellationTokenSource();
					}
					try
					{
						if (num != 0)
						{
							_003Ccts_003E5__2.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
							WWWForm val = new WWWForm();
							Dictionary<string, string>.Enumerator enumerator = postData.GetEnumerator();
							try
							{
								while (enumerator.MoveNext())
								{
									KeyValuePair<string, string> current = enumerator.Current;
									val.AddField(current.Key, current.Value);
								}
							}
							finally
							{
								if (num < 0)
								{
									((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
								}
							}
							_003Crequest_003E5__3 = UnityWebRequest.Post(url, val);
						}
						try
						{
							Awaiter<UnityWebRequest> val2;
							if (num != 0)
							{
								val2 = UnityAsyncExtensions.ToUniTask(_003Crequest_003E5__3.SendWebRequest(), (IProgress<float>)null, (PlayerLoopTiming)8, _003Ccts_003E5__2.Token).GetAwaiter();
								if (!val2.IsCompleted)
								{
									num = (_003C_003E1__state = 0);
									_003C_003Eu__1 = val2;
									_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<UnityWebRequest>, _003CPostRequestAsync_003Ed__16>(ref val2, ref this);
									return;
								}
							}
							else
							{
								val2 = _003C_003Eu__1;
								_003C_003Eu__1 = default;
								num = (_003C_003E1__state = -1);
							}
							val2.GetResult();
							if ((int)_003Crequest_003E5__3.result == 2 || (int)_003Crequest_003E5__3.result == 3 || (int)_003Crequest_003E5__3.result == 4)
							{
								throw new Exception("Request failed: " + _003Crequest_003E5__3.error);
							}
							text = _003Crequest_003E5__3.downloadHandler.text;
						}
						finally
						{
							if (num < 0 && _003Crequest_003E5__3 != null)
							{
								((IDisposable)_003Crequest_003E5__3).Dispose();
							}
						}
					}
					finally
					{
						if (num < 0 && _003Ccts_003E5__2 != null)
						{
							((IDisposable)_003Ccts_003E5__2).Dispose();
						}
					}
				}
				catch (OperationCanceledException)
				{
					throw new TimeoutException($"Request timed out after {timeoutSeconds} seconds");
				}
				catch (Exception ex2)
				{
					throw new Exception("Request failed: " + ex2.Message);
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(text);
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRedeemCode_003Ed__3 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<bool> _003C_003Et__builder;

		public string code;

		private List<string> _003CduplicateItemNames_003E5__2;

		private List<string> _003CnewItemNames_003E5__3;

		private SteamTicketForWebApi _003Cticket_003E5__4;

		private Awaiter<SteamTicketForWebApi> _003C_003Eu__1;

		private Awaiter<List<DecryptedItemData>> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			bool result;
			try
			{
				if ((uint)num > 1u && code.Trim().Length <= 0)
				{
					result = false;
				}
				else
				{
					try
					{
						Awaiter<SteamTicketForWebApi> val;
						if (num != 0)
						{
							if (num == 1)
							{
								goto IL_00c9;
							}
							ManagerBase<TransitionManager>.instance.SetBusy(value: true);
							ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.WaitingForSteam);
							_003CduplicateItemNames_003E5__2 = new List<string>();
							_003CnewItemNames_003E5__3 = new List<string>();
							val = DewSteam.GetTicketForWebApi().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<SteamTicketForWebApi>, _003CRedeemCode_003Ed__3>(ref val, ref this);
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
						_003Cticket_003E5__4 = result2;
						goto IL_00c9;
						IL_00c9:
						try
						{
							Awaiter<List<DecryptedItemData>> val2;
							if (num != 1)
							{
								ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.RequestingItemFromServer);
								val2 = RequestItemGeneration_Imp(new Dictionary<string, string> { { "code", code } }, _003Cticket_003E5__4).GetAwaiter();
								if (!val2.IsCompleted)
								{
									num = (_003C_003E1__state = 1);
									_003C_003Eu__2 = val2;
									_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<List<DecryptedItemData>>, _003CRedeemCode_003Ed__3>(ref val2, ref this);
									return;
								}
							}
							else
							{
								val2 = _003C_003Eu__2;
								_003C_003Eu__2 = default;
								num = (_003C_003E1__state = -1);
							}
							List<DecryptedItemData>.Enumerator enumerator = val2.GetResult().GetEnumerator();
							try
							{
								while (enumerator.MoveNext())
								{
									DecryptedItemData current = enumerator.Current;
									if (current.item.StartsWith("Stardust|"))
									{
										int num2 = int.Parse(current.item.Split("|", StringSplitOptions.None)[1], CultureInfo.InvariantCulture);
										string text = string.Format("<color=#ffa8d8>" + DewLocalization.GetUIValue("Redeem_StardustTemplate") + "</color>", $"<color=white>{num2:#,##0}</color>");
										if (DewSave.items.redeemedInGameRewards.Contains(current.item))
										{
											_003CduplicateItemNames_003E5__2.Add(text + " <color=#abb5ba>(" + DewLocalization.GetUIValue("Redeem_AlreadyReceivedWithSameCode") + ")</color>");
											continue;
										}
										_003CnewItemNames_003E5__3.Add(text);
										DewSave.profileMain.stardust += num2;
										DewSave.items.redeemedInGameRewards.Add(current.item);
									}
									else if (current.item.StartsWith("Mastery|"))
									{
										string[] array = current.item.Split("|", StringSplitOptions.None);
										string text2 = array[1];
										if (DewSave.profileStats.heroes.TryGetValue(text2, out var value))
										{
											long rewardedMasteryPoints = Dew.GetRewardedMasteryPoints(int.Parse(array[2], CultureInfo.InvariantCulture));
											string text3 = "<color=#70a7ff>" + DewLocalization.GetUIValue("TravelerMastery") + "</color>";
											string text4 = "<color=white>" + DewLocalization.GetUIValue(text2 + "_Name") + "</color>";
											if (DewSave.items.redeemedInGameRewards.Contains(current.item))
											{
												_003CduplicateItemNames_003E5__2.Add(text3 + " " + text4 + " <color=#abb5ba>(" + DewLocalization.GetUIValue("Redeem_AlreadyReceivedWithSameCode") + ")</color>");
											}
											else
											{
												int masteryLevel = value.masteryLevel;
												value.AddMasteryPoints(rewardedMasteryPoints);
												int masteryLevel2 = value.masteryLevel;
												string text5 = $"<color=#999>{masteryLevel:#,##0}<sprite=0><color=white>{masteryLevel2:#,##0} <color=#85c6ff> (+{rewardedMasteryPoints:#,##0})";
												_003CnewItemNames_003E5__3.Add(text3 + " " + text4 + " " + text5);
												DewSave.items.redeemedInGameRewards.Add(current.item);
											}
										}
									}
									else
									{
										if (DewSave.items.ContainsUsable(current))
										{
											_003CduplicateItemNames_003E5__2.Add(GetLocalizedNameWithType(current.item));
										}
										else
										{
											_003CnewItemNames_003E5__3.Add(GetLocalizedNameWithType(current.item));
										}
										DewSave.AddServerGeneratedItem(current);
									}
								}
							}
							finally
							{
								if (num < 0)
								{
									((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
								}
							}
							_003CduplicateItemNames_003E5__2.Sort();
							_003CnewItemNames_003E5__3.Sort();
							if (_003CnewItemNames_003E5__3.Count > 0)
							{
								ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
								{
									rawContent = string.Format(DewLocalization.GetUIValue("Redeem_ReceivedItem"), _003CnewItemNames_003E5__3.JoinToString("\n")),
									buttons = DewMessageSettings.ButtonType.Ok
								});
							}
							else if (code == "*")
							{
								ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
								{
									rawContent = DewLocalization.GetUIValue("Redeem_NoGiftsToRestore"),
									buttons = DewMessageSettings.ButtonType.Ok
								});
							}
							if (code != "*" && _003CduplicateItemNames_003E5__2.Count > 0)
							{
								ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
								{
									rawContent = string.Format(DewLocalization.GetUIValue("Redeem_YouAlreadyHaveItem"), _003CduplicateItemNames_003E5__2.JoinToString("\n")),
									buttons = DewMessageSettings.ButtonType.Ok
								});
							}
							UnityEngine.Debug.Log(string.Format("New items({0}): {1}", _003CnewItemNames_003E5__3.Count, _003CnewItemNames_003E5__3.JoinToString(", ")));
							UnityEngine.Debug.Log(string.Format("Duplicate items({0}): {1}", _003CduplicateItemNames_003E5__2.Count, _003CduplicateItemNames_003E5__2.JoinToString(", ")));
							DewSave.SaveItems();
							DewSave.SaveProfileMain();
							DewSave.SaveProfileStats();
							result = true;
						}
						finally
						{
							if (num < 0 && _003Cticket_003E5__4 != null)
							{
								((IDisposable)_003Cticket_003E5__4).Dispose();
							}
						}
					}
					catch (Exception e)
					{
						DewSessionError.ShowError(e, isFatal: false, isGame: false);
						goto IL_05ef;
					}
					finally
					{
						if (num < 0 && ManagerBase<TransitionManager>.instance != null)
						{
							ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.Empty);
							ManagerBase<TransitionManager>.instance.SetBusy(value: false);
						}
					}
				}
				goto end_IL_0007;
				IL_05ef:
				result = false;
				end_IL_0007:;
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRemoveExpiredOrNotOwnedItems_003Ed__11 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				try
				{
					Awaiter val2;
					if (num != 0)
					{
						UnityEngine.Debug.Log("[DewItem] Starting maintenance of expired / non-owned items");
						UniTask val = DewSteam.EnsureReady();
						val2 = val.GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CRemoveExpiredOrNotOwnedItems_003Ed__11>(ref val2, ref this);
							return;
						}
					}
					else
					{
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
					}
					val2.GetResult();
					if (DewSave.items != null)
					{
						bool flag = false;
						string[] array = DewSave.items.encryptedItems.ToArray();
						foreach (string text in array)
						{
							DecryptedItemData decryptedItemData = GetDecryptedItemData(text);
							if (decryptedItemData != null && !(decryptedItemData.owner != DewSteam.steamId.m_SteamID.ToString()) && ((decryptedItemData.IsExpired() && string.IsNullOrEmpty(decryptedItemData.requiredDLC)) || decryptedItemData.IsDLCNotOwned()))
							{
								UnityEngine.Debug.Log($"[DewItem] Removed {decryptedItemData.item}. IsExpired:{decryptedItemData.IsExpired()}, IsDLCNotOwned:{decryptedItemData.IsDLCNotOwned()}");
								DewSave.profileMain.LockServerGeneratedItem(decryptedItemData);
								DewSave.items.encryptedItems.Remove(text);
								flag = true;
							}
						}
						if (flag)
						{
							DewSave.SaveItems();
							DewSave.SaveProfileMain();
						}
						else
						{
							UnityEngine.Debug.Log("[DewItem] No items to purge; exiting");
						}
					}
				}
				catch (Exception message)
				{
					UnityEngine.Debug.Log(message);
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult();
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRequestItemGeneration_Imp_003Ed__7 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<List<DecryptedItemData>> _003C_003Et__builder;

		public Dictionary<string, string> body;

		public SteamTicketForWebApi ticket;

		private List<(string, string)> _003Cservers_003E5__2;

		private (string, string) _003Cserver_003E5__3;

		private Awaiter<string> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			List<DecryptedItemData> result;
			try
			{
				string text;
				if (num != 0)
				{
					body.Add("userId", ((object)SteamUser.GetSteamID()/*cast due to constrained. prefix*/).ToString());
					body.Add("appId", ((object)SteamUtils.GetAppID()/*cast due to constrained. prefix*/).ToString());
					body.Add("ticket", ticket.ticket);
					_003Cservers_003E5__2 = CouponServers.ToList();
					_003Cservers_003E5__2.Shuffle();
					text = null;
					goto IL_008e;
				}
				goto IL_00cc;
				IL_017a:
				_003Cserver_003E5__3 = default;
				goto IL_008e;
				IL_008e:
				_003Cserver_003E5__3 = _003Cservers_003E5__2[0];
				_003Cservers_003E5__2.RemoveAt(0);
				UnityEngine.Debug.Log("Connecting to " + _003Cserver_003E5__3.Item1 + "...");
				goto IL_00cc;
				IL_00cc:
				try
				{
					Awaiter<string> val;
					if (num != 0)
					{
						val = PostRequestAsync(_003Cserver_003E5__3.Item2, body).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<string>, _003CRequestItemGeneration_Imp_003Ed__7>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default;
						num = (_003C_003E1__state = -1);
					}
					text = val.GetResult();
				}
				catch (Exception)
				{
					UnityEngine.Debug.Log("Connection to " + _003Cserver_003E5__3.Item1 + " failed.");
					if (_003Cservers_003E5__2.Count == 0)
					{
						throw;
					}
					goto IL_017a;
				}
				if (!text.StartsWith("!"))
				{
					switch (text)
					{
					case "ITEM_NOT_ELIGIBLE":
						throw new DewException(DewExceptionType.ItemNotEligible);
					case "AUTH_FAILED":
						throw new DewException(DewExceptionType.SteamAuthFailed);
					case "INVALID_CODE":
						throw new DewException(DewExceptionType.InvalidGiftCode);
					case "USED_CODE":
						throw new DewException(DewExceptionType.UsedCode);
					case "ALREADY_HAVE":
						throw new DewException(DewExceptionType.AlreadyHaveThisGift);
					default:
						throw new DewException(DewExceptionType.FailedToGetItemGeneric, text);
					}
				}
				string[] array = text.Split("|", StringSplitOptions.None);
				List<DecryptedItemData> list = new List<DecryptedItemData>();
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					DecryptedItemData decryptedItemData = GetDecryptedItemData(array2[i]);
					if (decryptedItemData == null)
					{
						throw new DewException(DewExceptionType.InvalidResponseFromItemServer);
					}
					list.Add(decryptedItemData);
				}
				result = list;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cservers_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cservers_003E5__2 = null;
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

	private static readonly (string, string)[] CouponServers = new (string, string)[6]
	{
		("ap-east-1", "https://oc4p6dokwt3f3bwtsvxg6xjexi0ffwjb.lambda-url.ap-east-1.on.aws/"),
		("ap-southeast-1", "https://eijmsuyq3jxj7uxsf3lzvhuu7q0mtyjz.lambda-url.ap-southeast-1.on.aws/"),
		("us-west-1", "https://gv2uo22rvgskhwtkvposdbbqr40xxxbu.lambda-url.us-west-1.on.aws/"),
		("eu-west-3", "https://voolxxeuk3vq32y3guf24ythwq0raobp.lambda-url.eu-west-3.on.aws/"),
		("ca-central-1", "https://5jm3dssl3wpa7zuozscmrqgjhq0zvuea.lambda-url.ca-central-1.on.aws/"),
		("ap-northeast-1", "https://3f4iuhshq6sw5peheqr5qwrnzq0lycqt.lambda-url.ap-northeast-1.on.aws/")
	};

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void OnInit()
	{
		SpawnItemsMaintenanceDaemon();
	}

	public static MonoBehaviour GetItemLightPrefab(string itemName)
	{
		if (itemName.StartsWith("Emote_"))
		{
			return DewResources.GetByName<Emote>(itemName, ResourceLoadSettings.Light);
		}
		if (itemName.StartsWith("Acc_"))
		{
			return DewResources.GetByName<Accessory>(itemName, ResourceLoadSettings.Light);
		}
		if (itemName.StartsWith("Nametag_"))
		{
			return DewResources.GetByName<Nametag>(itemName, ResourceLoadSettings.Light);
		}
		if (itemName.StartsWith("Skin_"))
		{
			return DewResources.GetByName<Skin>(itemName, ResourceLoadSettings.Light);
		}
		return null;
	}

	public static bool IsItemGeneratedFromServer(string itemName)
	{
		MonoBehaviour itemLightPrefab = GetItemLightPrefab(itemName);
		if (itemLightPrefab is Emote emote)
		{
			return emote.generatedFromServer;
		}
		if (itemLightPrefab is Accessory accessory)
		{
			return accessory.generatedFromServer;
		}
		if (itemLightPrefab is Nametag nametag)
		{
			return nametag.generatedFromServer;
		}
		if (itemLightPrefab is Skin skin)
		{
			return skin.generatedFromServer;
		}
		return false;
	}

	[AsyncStateMachine(typeof(_003CRedeemCode_003Ed__3))]
	public static UniTask<bool> RedeemCode(string code)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CRedeemCode_003Ed__3 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder<bool>.Create();
		obj.code = code;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CRedeemCode_003Ed__3>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CGenerateItem_003Ed__4))]
	public static UniTask<bool> GenerateItem(string itemName, bool showMessages = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CGenerateItem_003Ed__4 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder<bool>.Create();
		obj.itemName = itemName;
		obj.showMessages = showMessages;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CGenerateItem_003Ed__4>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CGenerateItems_003Ed__5))]
	public static UniTask<bool> GenerateItems(List<string> itemNames, bool showMessages = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CGenerateItems_003Ed__5 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder<bool>.Create();
		obj.itemNames = itemNames;
		obj.showMessages = showMessages;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CGenerateItems_003Ed__5>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CRequestItemGeneration_Imp_003Ed__7))]
	private static UniTask<List<DecryptedItemData>> RequestItemGeneration_Imp(Dictionary<string, string> body, SteamTicketForWebApi ticket)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRequestItemGeneration_Imp_003Ed__7 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder<List<DecryptedItemData>>.Create();
		obj.body = body;
		obj.ticket = ticket;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CRequestItemGeneration_Imp_003Ed__7>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	public static DecryptedItemData GetDecryptedItemData(string ownershipKey)
	{
		try
		{
			DecryptedItemData decryptedItemData = JsonConvert.DeserializeObject<DecryptedItemData>(VerifyPgpMessage(Unshorten(ownershipKey)));
			decryptedItemData.ownershipKey = ownershipKey;
			return decryptedItemData;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static string GetLocalizedNameWithType(string name, bool colored = true)
	{
		string text = "";
		string text2 = "#fff";
		string text3 = "#fff";
		if (name.StartsWith("Acc_"))
		{
			text = DewLocalization.GetUIValue("ItemType_Souvenir");
			text2 = "#FF8EE0FF";
			text3 = "#FFCFEFFF";
		}
		if (name.StartsWith("Emote_"))
		{
			text = DewLocalization.GetUIValue("ItemType_Emote");
			text2 = "#FFE084FF";
			text3 = "#FFF0C2FF";
		}
		if (name.StartsWith("Nametag_"))
		{
			text = DewLocalization.GetUIValue("ItemType_Emblem");
			text2 = "#FF928EFF";
			text3 = "#FFD0CFFF";
		}
		if (name.StartsWith("Skin_"))
		{
			Skin byName = DewResources.GetByName<Skin>(name, ResourceLoadSettings.Light);
			Color rarityColor = Dew.GetRarityColor(byName.rarity);
			text = DewLocalization.GetUIValue($"ItemType_{byName.rarity}Costume");
			text2 = Dew.GetHex(Color.Lerp(rarityColor, Color.white, 0.25f));
			text3 = Dew.GetHex(Color.Lerp(rarityColor, Color.white, 0.75f));
		}
		if (colored)
		{
			return "<b><color=" + text2 + ">" + text + "</color></b> <color=" + text3 + ">" + DewLocalization.GetUIValue(name + "_Name") + "</color>";
		}
		return text + " " + DewLocalization.GetUIValue(name + "_Name");
	}

	public static void SpawnItemsMaintenanceDaemon()
	{
		UnityEngine.Debug.Log("[DewItem] Spawning items maintenance daemon");
		Dew.GetCoroutiner().StartCoroutine(Routine());
		static IEnumerator Routine()
		{
			yield return new WaitForSecondsRealtime(1f);
			yield return new WaitUntil(() => DewSteam.isInitialized);
			UnityEngine.Debug.Log("[DewItem] DewSteam ready. Daemon started");
			while (true)
			{
				yield return UniTaskExtensions.ToCoroutine(RemoveExpiredOrNotOwnedItems(), (Action<Exception>)null);
				yield return UniTaskExtensions.ToCoroutine<bool>(GenerateOrRefreshDLCItems(), (Action<bool>)null, (Action<Exception>)null);
				yield return new WaitForSeconds(14400f);
			}
		}
	}

	[AsyncStateMachine(typeof(_003CRemoveExpiredOrNotOwnedItems_003Ed__11))]
	public static UniTask RemoveExpiredOrNotOwnedItems()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CRemoveExpiredOrNotOwnedItems_003Ed__11 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CRemoveExpiredOrNotOwnedItems_003Ed__11>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CGenerateOrRefreshDLCItems_003Ed__12))]
	public static UniTask<bool> GenerateOrRefreshDLCItems()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CGenerateOrRefreshDLCItems_003Ed__12 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder<bool>.Create();
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CGenerateOrRefreshDLCItems_003Ed__12>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	public static string Unshorten(string shortMessage)
	{
		return "-----BEGIN PGP MESSAGE-----\n\n" + shortMessage.Substring(1) + "\n-----END PGP MESSAGE-----";
	}

	public static string VerifyPgpMessage(string armoredMessage)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected Obj, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected Obj, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected Obj, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected Obj, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected Obj, but got Unknown
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		string s = "-----BEGIN PGP PUBLIC KEY BLOCK-----\r\n\r\nxjMEZ4xvKhYJKwYBBAHaRw8BAQdA/Bv4dryZUdAZ63+svh685awHAWqMYHCj\r\ns+IrE5lMLK7NAMKMBBAWCgA+BYJnjG8qBAsJBwgJkFVJguqKN55VAxUICgQW\r\nAAIBAhkBApsDAh4BFiEEXaobhe0W0u9GValhVUmC6oo3nlUAABpDAPwPzyT9\r\noMq0pVNs64kS36NECdFELnIyYrciA9YuPIMHUgEAuEoxofkuCGtH3JcUj04r\r\nL3u/LW1FpIT/w3N6swCUpgTOOARnjG8qEgorBgEEAZdVAQUBAQdA8z7VA2DF\r\nveLUdHHOKkIuZ6j98ibpdTIETfme+Xwj4k4DAQgHwngEGBYKACoFgmeMbyoJ\r\nkFVJguqKN55VApsMFiEEXaobhe0W0u9GValhVUmC6oo3nlUAAMfvAP4joUMG\r\n6wEwCwmquuEEJ/epIBNjyL+1jOO1VZjwJIQhlwEAm3QMV/fAir/LG3Uvl/k/\r\nQz/UDkTP7cJddxRgadpt0g4=\r\n=A4fl\r\n-----END PGP PUBLIC KEY BLOCK-----";
		try
		{
			using MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(armoredMessage));
			ArmoredInputStream val = new ArmoredInputStream((Stream)memoryStream);
			try
			{
				using MemoryStream memoryStream2 = new MemoryStream(ReadFully((Stream)(object)val));
				PgpObjectFactory val2 = new PgpObjectFactory((Stream)memoryStream2);
				PgpObject val3 = val2.NextPgpObject();
				PgpPublicKey publicKey;
				using (MemoryStream memoryStream3 = new MemoryStream(Encoding.ASCII.GetBytes(s)))
				{
					ArmoredInputStream val4 = new ArmoredInputStream((Stream)memoryStream3);
					try
					{
						using MemoryStream memoryStream4 = new MemoryStream(ReadFully((Stream)(object)val4));
						publicKey = new PgpPublicKeyRing((Stream)memoryStream4).GetPublicKey();
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				if (publicKey == null)
				{
					throw new Exception("Public key not found");
				}
				PgpCompressedData val5 = (PgpCompressedData)(object)((val3 is PgpCompressedData) ? val3 : null);
				if (val5 != null)
				{
					val2 = new PgpObjectFactory(val5.GetDataStream());
					val3 = val2.NextPgpObject();
				}
				PgpOnePassSignatureList val6 = (PgpOnePassSignatureList)(object)((val3 is PgpOnePassSignatureList) ? val3 : null);
				if (val6 != null)
				{
					PgpOnePassSignature val7 = val6[0];
					PgpLiteralData val8 = (PgpLiteralData)val2.NextPgpObject();
					PgpSignature val9 = ((PgpSignatureList)val2.NextPgpObject())[0];
					val7.InitVerify(publicKey);
					StringBuilder stringBuilder = new StringBuilder();
					using (Stream stream = val8.GetInputStream())
					{
						using StreamReader streamReader = new StreamReader(stream);
						int num;
						while ((num = streamReader.Read()) >= 0)
						{
							stringBuilder.Append((char)num);
							val7.Update((byte)num);
						}
					}
					if (val7.Verify(val9))
					{
						return stringBuilder.ToString();
					}
					UnityEngine.Debug.Log("Signature verification failed");
					return null;
				}
				throw new Exception("Message is not signed or is in an unexpected format");
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.Log("Error verifying message: " + ex.Message);
			return null;
		}
	}

	private static byte[] ReadFully(Stream input)
	{
		using MemoryStream memoryStream = new MemoryStream();
		input.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	[AsyncStateMachine(typeof(_003CPostRequestAsync_003Ed__16))]
	public static UniTask<string> PostRequestAsync(string url, Dictionary<string, string> postData, float timeoutSeconds = 10f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPostRequestAsync_003Ed__16 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder<string>.Create();
		obj.url = url;
		obj.postData = postData;
		obj.timeoutSeconds = timeoutSeconds;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CPostRequestAsync_003Ed__16>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[CompilerGenerated]
	internal static void _003CGenerateItems_003Eg__PrintMessage_007C5_0(string msg, ref _003C_003Ec__DisplayClass5_0 P_1)
	{
		if (P_1.showMessages)
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				rawContent = msg,
				buttons = DewMessageSettings.ButtonType.Ok
			});
		}
		else
		{
			UnityEngine.Debug.Log("[DewItem] " + msg);
		}
	}
}
