using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_MirageSkin_Delusion_Delusional : StackedStatusEffect
{
	private class Ad_HealPreventedText
	{
		public float lastShowTime;
	}

	public FxVolume delusionalVolume;

	public Formula duration;

	[NonSerialized]
	public bool isEternal;

	public GameObject fxStackUpdated;

	private OnScreenTimerHandle _timer;

	private Formula _initialDuration;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_initialDuration = duration;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		Entity v = victim;
		InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
		{
			rawText = DewLocalization.GetUIValue("Se_MirageSkin_Delusion_Delusional_Popup"),
			color = new Color(1f, 0.5f, 0.7f),
			worldPosGetter = () => (!((UnityEngine.Object)(object)v != null)) ? Vector3.zero : v.Visual.GetCenterPosition()
		});
		if (((NetworkBehaviour)victim).isOwned)
		{
			_timer = ShowOnScreenTimerLocally(new OnScreenTimerHandle
			{
				rawTextGetter = () => string.Format("{0} <b><alpha=999>({1}%)</b>", DewLocalization.GetUIValue("Se_MirageSkin_Delusion_Delusional_Name"), stack),
				color = new Color(1f, 0.4f, 0.7f),
				fillAmountGetter = () => normalizedDuration ?? 1f
			});
		}
		if (((NetworkBehaviour)this).isServer)
		{
			victim.takenHealProcessor.Add(VictimOntakenHealProcessor, 10);
			UpdateDuration();
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if (((NetworkBehaviour)this).isServer)
		{
			UpdateDuration();
		}
	}

	private void UpdateDuration()
	{
		if (!isEternal)
		{
			SetTimer(duration.Evaluate(stack));
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		float target = (float)stack / (float)maxStack * 0.5f;
		delusionalVolume.targetStrength = Mathf.MoveTowards(delusionalVolume.targetStrength, target, 0.3f * Time.deltaTime);
		delusionalVolume.UpdateVolume();
		if (((NetworkBehaviour)this).isServer && victim.IsNullInactiveDeadOrKnockedOut())
		{
			DestroyIfActive();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_timer != null)
		{
			HideOnScreenTimerLocally(_timer);
			_timer = null;
		}
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.takenHealProcessor.Remove(VictimOntakenHealProcessor);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		isEternal = false;
		duration = _initialDuration;
		if (delusionalVolume != null)
		{
			delusionalVolume.targetStrength = 0f;
			delusionalVolume.UpdateVolume();
		}
	}

	private void VictimOntakenHealProcessor(ref HealData data, Actor actor, Entity target)
	{
		data.ApplyReduction((float)stack / 100f);
		if (!victim.TryGetData<Ad_HealPreventedText>(out var data2))
		{
			data2 = new Ad_HealPreventedText();
			victim.AddData(data2);
		}
		if (!(Time.time - data2.lastShowTime < 0.35f))
		{
			data2.lastShowTime = Time.time;
			RpcShowHealPrevented();
		}
	}

	[ClientRpc]
	private void RpcShowHealPrevented()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Se_MirageSkin_Delusion_Delusional::RpcShowHealPrevented()", 1645532168, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcShowHealPrevented()
	{
		if (!victim.IsNullInactiveDeadOrKnockedOut())
		{
			Entity v = victim;
			InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
			{
				rawText = DewLocalization.GetUIValue("Se_MirageSkin_Delusion_Delusional_HealReduced"),
				color = new Color(0.85f, 0.3f, 0.2f),
				worldPosGetter = () => (!((UnityEngine.Object)(object)v != null)) ? Vector3.zero : v.Visual.GetCenterPosition()
			});
		}
	}

	protected static void InvokeUserCode_RpcShowHealPrevented(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowHealPrevented called on server.");
		}
		else
		{
			((Se_MirageSkin_Delusion_Delusional)(object)obj).UserCode_RpcShowHealPrevented();
		}
	}

	static Se_MirageSkin_Delusion_Delusional()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Se_MirageSkin_Delusion_Delusional), "System.Void Se_MirageSkin_Delusion_Delusional::RpcShowHealPrevented()", (RemoteCallDelegate)InvokeUserCode_RpcShowHealPrevented);
	}
}
