using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Newtonsoft.Json;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

public static class DewEula
{
	private class EulaRequest
	{
		public string user_id;

		public string eula_id;

		public string ticket;
	}

	private class CheckEulaResponse
	{
		public bool ok;

		public bool agreed;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CAgreeEula_003Ed__5 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		private SteamTicketForWebApi _003Cticket_003E5__2;

		private Awaiter<SteamTicketForWebApi> _003C_003Eu__1;

		private Awaiter _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter<SteamTicketForWebApi> val;
				if (num != 0)
				{
					if (num == 1)
					{
						goto IL_007f;
					}
					needsToAgree = false;
					val = DewSteam.GetTicketForWebApi().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<SteamTicketForWebApi>, _003CAgreeEula_003Ed__5>(ref val, ref this);
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
				_003Cticket_003E5__2 = result;
				goto IL_007f;
				IL_007f:
				try
				{
					Awaiter val3;
					if (num != 1)
					{
						UniTask val2 = AgreeEula(_003Cticket_003E5__2);
						val3 = val2.GetAwaiter();
						if (!val3.IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__2 = val3;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CAgreeEula_003Ed__5>(ref val3, ref this);
							return;
						}
					}
					else
					{
						val3 = _003C_003Eu__2;
						_003C_003Eu__2 = default;
						num = (_003C_003E1__state = -1);
					}
					val3.GetResult();
				}
				finally
				{
					if (num < 0 && _003Cticket_003E5__2 != null)
					{
						((IDisposable)_003Cticket_003E5__2).Dispose();
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cticket_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cticket_003E5__2 = null;
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
	private struct _003CAgreeEula_003Ed__6 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public SteamTicketForWebApi ticket;

		private UnityWebRequest _003Crequest_003E5__2;

		private UnityWebRequestAsyncOperationAwaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected Obj, but got Unknown
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Expected Obj, but got Unknown
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Expected Obj, but got Unknown
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				string text = default;
				if (num != 0)
				{
					needsToAgree = false;
					text = "https://ntxojqrachcxqpwezfzw.supabase.co/functions/v1/agree-eula";
				}
				try
				{
					string s = default;
					if (num != 0)
					{
						string user_id = SteamUser.GetSteamID().m_SteamID.ToString();
						s = JsonConvert.SerializeObject((object)new EulaRequest
						{
							user_id = user_id,
							eula_id = "EULA20251210",
							ticket = ticket.ticket
						});
						_003Crequest_003E5__2 = new UnityWebRequest(text, "POST");
					}
					try
					{
						UnityWebRequestAsyncOperationAwaiter val;
						if (num != 0)
						{
							byte[] bytes = Encoding.UTF8.GetBytes(s);
							_003Crequest_003E5__2.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
							_003Crequest_003E5__2.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
							_003Crequest_003E5__2.SetRequestHeader("Content-Type", "application/json");
							val = UnityAsyncExtensions.GetAwaiter(_003Crequest_003E5__2.SendWebRequest());
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<UnityWebRequestAsyncOperationAwaiter, _003CAgreeEula_003Ed__6>(ref val, ref this);
								return;
							}
						}
						else
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default;
							num = (_003C_003E1__state = -1);
						}
						val.GetResult();
					}
					finally
					{
						if (num < 0 && _003Crequest_003E5__2 != null)
						{
							((IDisposable)_003Crequest_003E5__2).Dispose();
						}
					}
					_003Crequest_003E5__2 = null;
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
	private struct _003CCheckEula_003Ed__7 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CheckEulaResponse> _003C_003Et__builder;

		public SteamTicketForWebApi ticket;

		private UnityWebRequest _003Crequest_003E5__2;

		private UnityWebRequestAsyncOperationAwaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Expected Obj, but got Unknown
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Expected Obj, but got Unknown
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Expected Obj, but got Unknown
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Invalid comparison between Unknown and I4
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CheckEulaResponse result;
			try
			{
				string text = default;
				if (num != 0)
				{
					text = "https://ntxojqrachcxqpwezfzw.supabase.co/functions/v1/check-eula";
				}
				try
				{
					string s = default;
					if (num != 0)
					{
						string user_id = SteamUser.GetSteamID().m_SteamID.ToString();
						s = JsonConvert.SerializeObject((object)new EulaRequest
						{
							user_id = user_id,
							eula_id = "EULA20251210",
							ticket = ticket.ticket
						});
						_003Crequest_003E5__2 = new UnityWebRequest(text, "POST");
					}
					try
					{
						UnityWebRequestAsyncOperationAwaiter val;
						if (num != 0)
						{
							byte[] bytes = Encoding.UTF8.GetBytes(s);
							_003Crequest_003E5__2.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
							_003Crequest_003E5__2.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
							_003Crequest_003E5__2.SetRequestHeader("Content-Type", "application/json");
							val = UnityAsyncExtensions.GetAwaiter(_003Crequest_003E5__2.SendWebRequest());
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<UnityWebRequestAsyncOperationAwaiter, _003CCheckEula_003Ed__7>(ref val, ref this);
								return;
							}
						}
						else
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default;
							num = (_003C_003E1__state = -1);
						}
						val.GetResult();
						result = (((int)_003Crequest_003E5__2.result == 1) ? (JsonConvert.DeserializeObject<CheckEulaResponse>(_003Crequest_003E5__2.downloadHandler.text) ?? new CheckEulaResponse
						{
							ok = false,
							agreed = false
						}) : new CheckEulaResponse
						{
							ok = false,
							agreed = false
						});
					}
					finally
					{
						if (num < 0 && _003Crequest_003E5__2 != null)
						{
							((IDisposable)_003Crequest_003E5__2).Dispose();
						}
					}
				}
				catch (Exception message)
				{
					UnityEngine.Debug.Log(message);
					result = new CheckEulaResponse
					{
						ok = false,
						agreed = false
					};
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

	public const string CurrentEulaSetKey = "EULA20251210";

	public static bool needsToAgree;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static async void OnInit()
	{
		if (string.IsNullOrEmpty("EULA20251210") || DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			needsToAgree = false;
			return;
		}
		bool didAgreeLocal = DewSave.platformSettings.lastAgreedEulaKeyLocal == "EULA20251210";
		needsToAgree = !didAgreeLocal;
		try
		{
			if (!DewSteam.isInitialized)
			{
				return;
			}
			using SteamTicketForWebApi ticket = await DewSteam.GetTicketForWebApi();
			bool isIndeterminate = false;
			bool didAgreeOnline = true;
			UnityEngine.Debug.Log("[EULA] Checking EULA20251210");
			CheckEulaResponse checkEulaResponse = await CheckEula(ticket);
			if (checkEulaResponse.ok)
			{
				UnityEngine.Debug.Log(string.Format("[EULA] Check Result for {0}: {1}", "EULA20251210", checkEulaResponse.agreed));
				didAgreeOnline &= checkEulaResponse.agreed;
			}
			else
			{
				UnityEngine.Debug.Log("[EULA] Check Result for EULA20251210: ???");
				isIndeterminate = true;
			}
			if (isIndeterminate)
			{
				UnityEngine.Debug.Log("[EULA] Check has failed. Falling back to local");
				return;
			}
			if (didAgreeLocal && !didAgreeOnline)
			{
				UnityEngine.Debug.Log("[EULA] Local has agreed EULA. Uploading agree data");
				await AgreeEula(ticket);
				return;
			}
			if (didAgreeOnline)
			{
				DewSave.platformSettings.lastAgreedEulaKeyLocal = "EULA20251210";
			}
			needsToAgree = !didAgreeOnline;
		}
		catch (Exception message)
		{
			UnityEngine.Debug.Log("[EULA] Check failed due to below exception. Falling back to local");
			UnityEngine.Debug.Log(message);
		}
	}

	[AsyncStateMachine(typeof(_003CAgreeEula_003Ed__5))]
	public static UniTask AgreeEula()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CAgreeEula_003Ed__5 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CAgreeEula_003Ed__5>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CAgreeEula_003Ed__6))]
	private static UniTask AgreeEula(SteamTicketForWebApi ticket)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CAgreeEula_003Ed__6 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj.ticket = ticket;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CAgreeEula_003Ed__6>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CCheckEula_003Ed__7))]
	private static UniTask<CheckEulaResponse> CheckEula(SteamTicketForWebApi ticket)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CCheckEula_003Ed__7 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder<CheckEulaResponse>.Create();
		obj.ticket = ticket;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CCheckEula_003Ed__7>(ref obj);
		return obj._003C_003Et__builder.Task;
	}
}
