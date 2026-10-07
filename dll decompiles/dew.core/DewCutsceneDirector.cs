using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[ExecuteAlways]
[RequireComponent(typeof(PlayableDirector))]
public class DewCutsceneDirector : DewNetworkBehaviour
{
	public Action onFinishAfterFadeIn;

	public Action onFinish;

	public Action onPlay;

	public bool stopAllPlayers = true;

	public bool disableAi = true;

	public float reenableAiDelay;

	public float cutsceneStartDelay;

	public bool enableSkip = true;

	public bool endBeforeTimelineStops;

	public bool disableStartTransition;

	public bool disableEndTransition;

	private PlayableDirector _director;

	private float _lastStopTime;

	private bool _didStartStopSequence;

	private bool _isSkipping;

	public bool isPlaying
	{
		get
		{
			if ((UnityEngine.Object)(object)_director != null)
			{
				return _director.state == PlayState.Playing;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_director = ((Component)(object)this).GetComponent<PlayableDirector>();
		_director.playOnAwake = false;
	}

	private void Start()
	{
		if (Application.IsPlaying((UnityEngine.Object)(object)this))
		{
			_director.stopped += (PlayableDirector _) =>
			{
				OnStopPlaying();
			};
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += new Action<Entity>(OnEntityAdd);
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (_director.state == PlayState.Playing && !_isSkipping && !_didStartStopSequence && _director.time > _director.duration - (double)ManagerBase<CameraManager>.instance.cutsceneFadeTime)
		{
			OnStopPlaying();
		}
	}

	public void Play()
	{
		if (!Application.IsPlaying((UnityEngine.Object)(object)this))
		{
			return;
		}
		Debug.Log("Playing cutscene '" + ((UnityEngine.Object)(object)this).name + "' locally");
		_didStartStopSequence = false;
		if (((NetworkBehaviour)this).isServer)
		{
			if (stopAllPlayers)
			{
				StopAllPlayers();
			}
			if (disableAi)
			{
				EntityAI.DisableAI = true;
			}
		}
		((MonoBehaviour)(object)this).StopAllCoroutines();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			_isSkipping = false;
			onPlay?.Invoke();
			ManagerBase<CameraManager>.instance.isPlayingCutscene = true;
			ManagerBase<CameraManager>.instance.currentCutsceneDirector = this;
			if (!disableStartTransition)
			{
				ManagerBase<CameraManager>.instance.ResetLetterBoxPositions();
				ManagerBase<CameraManager>.instance.DoCutsceneFadeOut();
				yield return new WaitForSeconds(ManagerBase<CameraManager>.instance.cutsceneFadeTime + cutsceneStartDelay);
			}
			else
			{
				ManagerBase<CameraManager>.instance.DoLetterBoxFadeIn();
			}
			if (((NetworkBehaviour)this).isServer && stopAllPlayers)
			{
				StopAllPlayers();
			}
			InGameUIManager.instance.SetState("Cutscene");
			ManagerBase<CameraManager>.instance.SetActiveEntityVCam(value: false);
			if (!disableStartTransition)
			{
				ManagerBase<CameraManager>.instance.DoCutsceneFadeIn();
			}
			ManagerBase<CameraManager>.instance.cutsceneSkipButtonObject.SetActive(enableSkip);
			foreach (PlayableBinding output in ((PlayableAsset)(TimelineAsset)_director.playableAsset).outputs)
			{
				if (output.streamName == "Cinemachine Track")
				{
					_director.SetGenericBinding(output.sourceObject, (UnityEngine.Object)(object)Dew.mainCamera.GetComponent<CinemachineBrain>());
				}
				if (output.streamName.StartsWith("Activation Track"))
				{
					GameObject gameObject = _director.GetGenericBinding(output.sourceObject) as GameObject;
					if (!(gameObject == null) && Application.IsPlaying((UnityEngine.Object)(object)this))
					{
						gameObject.SetActive(value: false);
					}
				}
			}
			_director.Play();
		}
	}

	[Command(requiresAuthority = false)]
	public void CmdSkip()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendCommandInternal("System.Void DewCutsceneDirector::CmdSkip()", 895073170, (NetworkWriter)(object)val, 0, false);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcSkip()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void DewCutsceneDirector::RpcSkip()", -400979971, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void StopAllPlayers()
	{
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			allHero.Control.Stop();
			allHero.Control.CancelOngoingChannels();
		}
	}

	private void OnStopPlaying()
	{
		if (!_didStartStopSequence)
		{
			_didStartStopSequence = true;
			((MonoBehaviour)(object)this).StopAllCoroutines();
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			if (!disableEndTransition)
			{
				if (!_isSkipping)
				{
					ManagerBase<CameraManager>.instance.DoCutsceneFadeOut();
					yield return new WaitForSeconds(ManagerBase<CameraManager>.instance.cutsceneFadeTime);
				}
				ManagerBase<CameraManager>.instance.DoCutsceneFadeIn();
			}
			else if (!_isSkipping)
			{
				ManagerBase<CameraManager>.instance.DoLetterBoxFadeOut();
				yield return new WaitForSeconds(ManagerBase<CameraManager>.instance.cutsceneFadeTime);
			}
			ManagerBase<CameraManager>.instance.SetActiveEntityVCam(value: true);
			if (InGameUIManager.instance.IsState("Cutscene"))
			{
				InGameUIManager.instance.SetState("Playing");
			}
			ManagerBase<CameraManager>.instance.isPlayingCutscene = false;
			ManagerBase<CameraManager>.instance.currentCutsceneDirector = null;
			onFinish?.Invoke();
			yield return new WaitForSeconds(ManagerBase<CameraManager>.instance.cutsceneFadeTime);
			yield return new WaitForSeconds(reenableAiDelay);
			if (((NetworkBehaviour)this).isServer && disableAi)
			{
				EntityAI.DisableAI = false;
			}
			onFinishAfterFadeIn?.Invoke();
		}
	}

	[ClientRpc]
	public void PlayNetworked()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void DewCutsceneDirector::PlayNetworked()", 1583074866, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (ManagerBase<CameraManager>.instance != null && (UnityEngine.Object)(object)ManagerBase<CameraManager>.instance.currentCutsceneDirector == (UnityEngine.Object)(object)this)
		{
			ManagerBase<CameraManager>.instance.isPlayingCutscene = false;
			ManagerBase<CameraManager>.instance.currentCutsceneDirector = null;
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd -= new Action<Entity>(OnEntityAdd);
		}
	}

	private void OnEntityAdd(Entity obj)
	{
		if (((NetworkBehaviour)this).isServer && obj is BossMonster)
		{
			obj.Visual.SkipSpawning();
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdSkip()
	{
		RpcSkip();
	}

	protected static void InvokeUserCode_CmdSkip(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSkip called on client.");
		}
		else
		{
			((DewCutsceneDirector)(object)obj).UserCode_CmdSkip();
		}
	}

	protected void UserCode_RpcSkip()
	{
		if (!ManagerBase<CameraManager>.instance.isPlayingCutscene || (UnityEngine.Object)(object)ManagerBase<CameraManager>.instance.currentCutsceneDirector != (UnityEngine.Object)(object)this || _director.state != PlayState.Playing)
		{
			Debug.Log("Skipping cutscene '" + ((UnityEngine.Object)(object)this).name + "' failed. Cutscene is not playing");
			return;
		}
		if (_director.time >= _director.duration - 0.10000000149011612 - (double)ManagerBase<CameraManager>.instance.cutsceneFadeTime)
		{
			Debug.Log("Skipping cutscene '" + ((UnityEngine.Object)(object)this).name + "' failed. Too late to skip");
			return;
		}
		Debug.Log("Skipping cutscene '" + ((UnityEngine.Object)(object)this).name + "'");
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			_isSkipping = true;
			if (!disableEndTransition)
			{
				ManagerBase<CameraManager>.instance.DoCutsceneFadeOut();
			}
			else
			{
				ManagerBase<CameraManager>.instance.DoLetterBoxFadeOut();
			}
			yield return new WaitForSeconds(ManagerBase<CameraManager>.instance.cutsceneFadeTime);
			if (((NetworkBehaviour)this).isServer)
			{
				foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
				{
					if (allEntity.Visual.isSpawning)
					{
						allEntity.Visual.SkipSpawning();
					}
				}
				foreach (KeyValuePair<SpawnMonsterSettings, Coroutine> ongoingSpawn in SingletonDewNetworkBehaviour<Room>.instance.monsters.ongoingSpawns)
				{
					ongoingSpawn.Key.isCutsceneSkipped = true;
				}
			}
			if (_director.state == PlayState.Playing)
			{
				_director.time = _director.duration - 0.009999999776482582;
			}
		}
	}

	protected static void InvokeUserCode_RpcSkip(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSkip called on server.");
		}
		else
		{
			((DewCutsceneDirector)(object)obj).UserCode_RpcSkip();
		}
	}

	protected void UserCode_PlayNetworked()
	{
		Debug.Log("Playing cutscene '" + ((UnityEngine.Object)(object)this).name + "' networked");
		Play();
	}

	protected static void InvokeUserCode_PlayNetworked(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC PlayNetworked called on server.");
		}
		else
		{
			((DewCutsceneDirector)(object)obj).UserCode_PlayNetworked();
		}
	}

	static DewCutsceneDirector()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterCommand(typeof(DewCutsceneDirector), "System.Void DewCutsceneDirector::CmdSkip()", (RemoteCallDelegate)InvokeUserCode_CmdSkip, false);
		RemoteProcedureCalls.RegisterRpc(typeof(DewCutsceneDirector), "System.Void DewCutsceneDirector::RpcSkip()", (RemoteCallDelegate)InvokeUserCode_RpcSkip);
		RemoteProcedureCalls.RegisterRpc(typeof(DewCutsceneDirector), "System.Void DewCutsceneDirector::PlayNetworked()", (RemoteCallDelegate)InvokeUserCode_PlayNetworked);
	}
}
