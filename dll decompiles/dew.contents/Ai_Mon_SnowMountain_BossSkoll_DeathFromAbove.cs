using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_Mon_SnowMountain_BossSkoll_DeathFromAbove : AbilityInstance
{
	[Serializable]
	public class Pattern
	{
		public Vector3[] positions;
	}

	public float ascendTime;

	public float ascendHeight;

	public DewAnimationClip animAscend;

	public float delay0;

	public int patternSwordWaves = 3;

	public Pattern[] patterns;

	public Vector2Int countPerWave;

	public int atHeroSwordWavesSolo = 2;

	public int atHeroSwordWavesMultiplayerPerPlayer = 1;

	public float atHeroRandomMag;

	public Vector2 intervalPerSword;

	public float perWaveDelay;

	public float delay1;

	public GameObject fxFollowTelegraph;

	public float followDuration;

	public float followStartRandomMag;

	public float followSpeed;

	public DewAnimationClip animDescendFalling;

	public DewAnimationClip animLand;

	public float descendTime;

	public bool resetWhirlwind;

	public float postDaze;

	private EntityTransformModifier _entTransform;

	[SyncVar]
	private Vector3 _followPos;

	public override bool reuseInRoom => true;

	public Vector3 Network_followPos
	{
		get
		{
			return _followPos;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<Vector3>(value, ref _followPos, 64uL, (Action<Vector3, Vector3>)null);
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		_entTransform = info.caster.Visual.GetNewTransformModifier();
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		BossMonster.RevealStealthedBeforeSpecialAttack();
		CreateStatusEffect<Se_Mon_SnowMountain_BossSkoll_DeathFromAbove_Invulnerable>(info.caster).DestroyOnDestroy(this);
		info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
		RpcAscend();
		yield return new SI.WaitForSeconds(ascendTime + delay0);
		RefValue<int> lastWave = new RefValue<int>(-1);
		RefValue<Pattern> selectedPattern = new RefValue<Pattern>();
		RefValue<Quaternion> patternRot = new RefValue<Quaternion>();
		int patternOffsetIndex = UnityEngine.Random.Range(0, patterns.Length);
		RefValue<Vector3> pivot = new RefValue<Vector3>();
		yield return SwordRoutine(patternSwordWaves, (int waveIndex, int swordIndex) =>
		{
			if ((int)lastWave != waveIndex)
			{
				lastWave.value = waveIndex;
				selectedPattern.value = patterns[(patternOffsetIndex + waveIndex) % patterns.Length];
				patternRot.value = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
				Hero hero2 = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
				pivot.value = hero2.GetAIAgentPosition(info.caster);
			}
			Vector3[] positions = selectedPattern.value.positions;
			return pivot.value + positions[swordIndex % positions.Length];
		});
		int playerCount = DewPlayer.gamePlayers.Count;
		DewPlayer[] array = DewPlayer.gamePlayers.ToArray();
		foreach (DewPlayer h in array)
		{
			if (!h.hero.IsNullInactiveDeadOrKnockedOut())
			{
				int waves = ((playerCount > 1) ? atHeroSwordWavesMultiplayerPerPlayer : atHeroSwordWavesSolo);
				yield return SwordRoutine(waves, (int waveIndex, int swordIndex) => h.hero.GetAIAgentPosition(info.caster) + UnityEngine.Random.onUnitSphere * atHeroRandomMag);
			}
		}
		yield return new SI.WaitForSeconds(delay1);
		Hero hero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
		Network_followPos = Dew.GetPositionOnGround(hero.GetAIAgentPosition(info.caster) + UnityEngine.Random.onUnitSphere * followStartRandomMag);
		Network_followPos = Dew.GetValidAgentPosition(_followPos);
		FxPlayNetworked(fxFollowTelegraph);
		for (float t = 0f; t < followDuration; t += LogicUpdateManager.logicDeltaTime)
		{
			hero = Dew.GetClosestAliveHero(_followPos, fallbackToDead: true, info.caster);
			Network_followPos = Dew.GetPositionOnGround(Vector3.MoveTowards(_followPos, hero.GetAIAgentPosition(info.caster), followSpeed * LogicUpdateManager.logicDeltaTime));
			Teleport(info.caster, _followPos);
			yield return null;
		}
		RpcDescendAndFinish();
	}

	private IEnumerator SwordRoutine(int waves, Func<int, int, Vector3> posGetter)
	{
		for (int i = 0; i < waves; i++)
		{
			int swordCount = UnityEngine.Random.Range(countPerWave.x, countPerWave.y + 1);
			for (int j = 0; j < swordCount; j++)
			{
				try
				{
					Vector3 vector = FilterSwordPosition(posGetter(i, j));
					CreateAbilityInstance<Ai_Mon_SnowMountain_BossSkoll_Sword>(vector, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), new CastInfo(info.caster));
				}
				catch (Exception)
				{
				}
				yield return new SI.WaitForSeconds(UnityEngine.Random.Range(intervalPerSword.x, intervalPerSword.y));
			}
			yield return new SI.WaitForSeconds(perWaveDelay);
		}
	}

	private Vector3 FilterSwordPosition(Vector3 input)
	{
		input = Dew.GetPositionOnGround(input);
		input = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, input);
		return input;
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		fxFollowTelegraph.transform.position = _followPos;
	}

	protected override void OnDestroyActor()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (_entTransform != null)
		{
			_entTransform.Stop();
		}
		if (((NetworkBehaviour)this).isServer && !info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
			info.caster.Control.StartDaze(postDaze);
			if (resetWhirlwind && info.caster.Ability.TryGetAbility<At_Mon_SnowMountain_BossSkoll_Whirlwind>(out var trigger))
			{
				ResetCooldown(trigger);
			}
		}
	}

	[ClientRpc]
	private void RpcAscend()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_SnowMountain_BossSkoll_DeathFromAbove::RpcAscend()", 1514687603, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	[ClientRpc]
	private void RpcDescendAndFinish()
	{
		NetworkWriterPooled val = NetworkWriterPool.Get();
		((NetworkBehaviour)this).SendRPCInternal("System.Void Ai_Mon_SnowMountain_BossSkoll_DeathFromAbove::RpcDescendAndFinish()", 323676137, (NetworkWriter)(object)val, 0, true);
		NetworkWriterPool.Return(val);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcAscend()
	{
		StartSequence(Routine());
		IEnumerator Routine()
		{
			if (_entTransform != null)
			{
				if (((NetworkBehaviour)this).isServer)
				{
					info.caster.Animation.PlayAbilityAnimation(animAscend);
				}
				for (float t = 0f; t < ascendTime; t += LogicUpdateManager.logicDeltaTime)
				{
					float num = t / ascendTime;
					_entTransform.worldOffset = Vector3.up * num * ascendHeight;
					_entTransform.scaleMultiplier = Vector3.one * (1f - num);
					yield return null;
				}
				_entTransform.worldOffset = Vector3.up * ascendHeight;
				_entTransform.scaleMultiplier = Vector3.zero;
				info.caster.Visual.DisableRenderersLocal();
			}
		}
	}

	protected static void InvokeUserCode_RpcAscend(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAscend called on server.");
		}
		else
		{
			((Ai_Mon_SnowMountain_BossSkoll_DeathFromAbove)(object)obj).UserCode_RpcAscend();
		}
	}

	protected void UserCode_RpcDescendAndFinish()
	{
		StartSequence(Routine());
		IEnumerator Routine()
		{
			if (_entTransform != null)
			{
				if (((NetworkBehaviour)this).isServer)
				{
					info.caster.Control.Teleport(_followPos);
					info.caster.Animation.PlayAbilityAnimation(animDescendFalling);
					info.caster.Control.RotateTowards(Dew.GetClosestAliveHero(_followPos, fallbackToDead: true, info.caster), immediately: true);
				}
				info.caster.Visual.EnableRenderersLocal();
				for (float t = 0f; t < descendTime; t += LogicUpdateManager.logicDeltaTime)
				{
					float num = t / descendTime;
					_entTransform.worldOffset = Vector3.up * ((1f - num) * ascendHeight);
					_entTransform.scaleMultiplier = Vector3.one * num;
					yield return null;
				}
				_entTransform.worldOffset = Vector3.zero;
				_entTransform.scaleMultiplier = Vector3.one;
				if (((NetworkBehaviour)this).isServer)
				{
					info.caster.Animation.PlayAbilityAnimation(animLand);
					CreateAbilityInstance<Ai_Mon_SnowMountain_BossSkoll_DeathFromAbove_Land>(info.caster.agentPosition, null, new CastInfo(info.caster));
					info.caster.Control.StartDaze(postDaze);
					Destroy();
				}
			}
		}
	}

	protected static void InvokeUserCode_RpcDescendAndFinish(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDescendAndFinish called on server.");
		}
		else
		{
			((Ai_Mon_SnowMountain_BossSkoll_DeathFromAbove)(object)obj).UserCode_RpcDescendAndFinish();
		}
	}

	static Ai_Mon_SnowMountain_BossSkoll_DeathFromAbove()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_SnowMountain_BossSkoll_DeathFromAbove), "System.Void Ai_Mon_SnowMountain_BossSkoll_DeathFromAbove::RpcAscend()", (RemoteCallDelegate)InvokeUserCode_RpcAscend);
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_Mon_SnowMountain_BossSkoll_DeathFromAbove), "System.Void Ai_Mon_SnowMountain_BossSkoll_DeathFromAbove::RpcDescendAndFinish()", (RemoteCallDelegate)InvokeUserCode_RpcDescendAndFinish);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteVector3(writer, _followPos);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteVector3(writer, _followPos);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _followPos, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<Vector3>(ref _followPos, (Action<Vector3, Vector3>)null, NetworkReaderExtensions.ReadVector3(reader));
		}
	}
}
