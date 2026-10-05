using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class UGSManager : ManagerBase<UGSManager>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CEnsureReady_003Ed__9 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public UGSManager _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UGSManager uGSManager = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default;
					num = (_003C_003E1__state = -1);
					goto IL_00c2;
				}
				UGSManager uGSManager2 = _003C_003E4__this;
				if (uGSManager.status != ServiceStatus.Ready)
				{
					UnityEngine.Debug.Log("Ensuring ready");
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.WaitingForService);
					float startTime = Time.unscaledTime;
					UniTask val2 = UniTask.WaitWhile((Func<bool>)(() => (Time.unscaledTime - startTime < 1f || uGSManager2.status == ServiceStatus.Loading) && Time.unscaledTime - startTime < 5f), (PlayerLoopTiming)8, default(CancellationToken));
					val = val2.GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CEnsureReady_003Ed__9>(ref val, ref this);
						return;
					}
					goto IL_00c2;
				}
				goto end_IL_000e;
				IL_00c2:
				val.GetResult();
				if (uGSManager.status == ServiceStatus.Loading)
				{
					throw new Exception("Unity Game Services Timeout");
				}
				if (uGSManager.status == ServiceStatus.Error)
				{
					throw new Exception("Unity Game Services Unavailable");
				}
				UnityEngine.Debug.Log("UGS is ready");
				end_IL_000e:;
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
	private struct _003CWaitUntilLoad_003Ed__10 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public UGSManager _003C_003E4__this;

		public float timeout;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					UGSManager uGSManager = _003C_003E4__this;
					float timeout = this.timeout;
					float startTime = Time.unscaledTime;
					UniTask val = UniTask.WaitWhile((Func<bool>)(() => uGSManager.status == ServiceStatus.Loading && Time.unscaledTime - startTime < timeout), (PlayerLoopTiming)8, default(CancellationToken));
					val2 = val.GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CWaitUntilLoad_003Ed__10>(ref val2, ref this);
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
	private struct _003CWaitUntilLoad_003Ed__11 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public UGSManager _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UGSManager uGSManager = _003C_003E4__this;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					UniTask val = uGSManager.WaitUntilLoad(5f);
					val2 = val.GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CWaitUntilLoad_003Ed__11>(ref val2, ref this);
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

	public override bool shouldRegisterUpdates => false;

	public ServiceStatus status { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		TryInit();
	}

	public async void TryInit()
	{
		if ((int)UnityServices.State == 2)
		{
			status = ServiceStatus.Ready;
		}
		else
		{
			if ((int)UnityServices.State == 1)
			{
				return;
			}
			status = ServiceStatus.Loading;
			try
			{
				InitializationOptions val = new InitializationOptions();
				string text = "production";
				UnityEngine.Debug.Log("Unity Services environment is: " + text);
				EnvironmentsOptionsExtensions.SetEnvironmentName(val, text);
				await UnityServices.InitializeAsync(val);
				if (GlobalAnalyticsManager.IsAnalyticsEnabled())
				{
					AnalyticsService.Instance.StartDataCollection();
				}
				RegisterHandlers();
				await AuthenticationService.Instance.SignInAnonymouslyAsync((SignInOptions)null);
				status = ServiceStatus.Ready;
			}
			catch (Exception message)
			{
				UnityEngine.Debug.Log(message);
				status = ServiceStatus.Error;
			}
		}
	}

	private void RegisterHandlers()
	{
		AuthenticationService.Instance.SignedIn += () =>
		{
			UnityEngine.Debug.Log("PlayerID: " + AuthenticationService.Instance.PlayerId);
		};
		AuthenticationService.Instance.SignInFailed += (RequestFailedException err) =>
		{
			UnityEngine.Debug.Log(err);
		};
		AuthenticationService.Instance.SignedOut += () =>
		{
			UnityEngine.Debug.Log("Player signed out.");
		};
		AuthenticationService.Instance.Expired += () =>
		{
			UnityEngine.Debug.Log("Player session could not be refreshed and expired.");
		};
	}

	[AsyncStateMachine(typeof(_003CEnsureReady_003Ed__9))]
	public UniTask EnsureReady()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CEnsureReady_003Ed__9 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CEnsureReady_003Ed__9>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CWaitUntilLoad_003Ed__10))]
	public UniTask WaitUntilLoad(float timeout)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitUntilLoad_003Ed__10 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj.timeout = timeout;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CWaitUntilLoad_003Ed__10>(ref obj);
		return obj._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CWaitUntilLoad_003Ed__11))]
	public UniTask WaitUntilLoad()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitUntilLoad_003Ed__11 obj = default;
		obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		obj._003C_003E4__this = this;
		obj._003C_003E1__state = -1;
		obj._003C_003Et__builder.Start<_003CWaitUntilLoad_003Ed__11>(ref obj);
		return obj._003C_003Et__builder.Task;
	}
}
