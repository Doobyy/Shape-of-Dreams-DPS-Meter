using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Mirror;
using UnityEngine;

public class Shrine_MorasDomain_HerPresence : Shrine, ICustomInteractable
{
	[CompilerGenerated]
	private sealed class _003CWaveRoutine_003Ed__44 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public int waveIndex;

		public Shrine_MorasDomain_HerPresence _003C_003E4__this;

		private bool _003CisBossWave_003E5__2;

		private int _003CmonsterTypeCount_003E5__3;

		private float _003CpopMultiplier_003E5__4;

		private List<MonsterPool.SpawnRuleEntry> _003CbaseMonsterTypes_003E5__5;

		private int _003Ci_003E5__6;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CWaveRoutine_003Ed__44(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			//IL_0278: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bf: Expected Obj, but got Unknown
			//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			Shrine_MorasDomain_HerPresence CS_0024_003C_003E8__locals40 = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CisBossWave_003E5__2 = waveIndex == CS_0024_003C_003E8__locals40.waveCount - 1;
				_003CmonsterTypeCount_003E5__3 = CS_0024_003C_003E8__locals40.monsterTypeCountsByWave[waveIndex];
				_003CpopMultiplier_003E5__4 = CS_0024_003C_003E8__locals40.populationMultipliersByWave[waveIndex];
				SingletonDewNetworkBehaviour<Room>.instance.monsters.FinishAllOngoingSpawns();
				if (CS_0024_003C_003E8__locals40._currentRule != null)
				{
					if (CS_0024_003C_003E8__locals40._currentRule.pool != null)
					{
						UnityEngine.Object.Destroy(CS_0024_003C_003E8__locals40._currentRule.pool);
					}
					UnityEngine.Object.Destroy(CS_0024_003C_003E8__locals40._currentRule);
				}
				CS_0024_003C_003E8__locals40.MakeUnavailable();
				if (_003CisBossWave_003E5__2)
				{
					CS_0024_003C_003E8__locals40.FxPlayNetworked(CS_0024_003C_003E8__locals40.fxSilence);
				}
				_003C_003E2__current = new WaitForSeconds(1f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				CS_0024_003C_003E8__locals40._currentRule = UnityEngine.Object.Instantiate(CS_0024_003C_003E8__locals40.singleWaveRule);
				CS_0024_003C_003E8__locals40._currentRule.pool = UnityEngine.Object.Instantiate(CS_0024_003C_003E8__locals40._allMonsters);
				_003CbaseMonsterTypes_003E5__5 = CS_0024_003C_003E8__locals40.GetRandomEntries(_003CmonsterTypeCount_003E5__3);
				_003C_003E2__current = new WaitForSeconds(1f);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__6 = 0;
				goto IL_02c8;
			case 3:
				_003C_003E1__state = -1;
				CS_0024_003C_003E8__locals40._currentRule.pool.entries = new List<MonsterPool.SpawnRuleEntry>(_003CbaseMonsterTypes_003E5__5.Take(_003Ci_003E5__6 + 1));
				_003C_003E2__current = UniTaskExtensions.ToCoroutine(SingletonDewNetworkBehaviour<Room>.instance.monsters.SpawnMonstersAsync(new SpawnMonsterSettings
				{
					rule = CS_0024_003C_003E8__locals40._currentRule,
					section = CS_0024_003C_003E8__locals40.mainSection,
					spawnPopulationMultiplier = _003CpopMultiplier_003E5__4 * CS_0024_003C_003E8__locals40.populationMultipliersBySubWave.GetClamped(_003Ci_003E5__6),
					ignoreTurnPopMultiplier = true,
					beforeSpawn = (Entity e) =>
					{
						e.Visual.invisibleByDefault = true;
						e.Visual.skipSpawning = true;
					},
					spawnPosGetter = () =>
					{
						/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
					},
					afterSpawn = (Entity e) =>
					{
					}
				}), (Action<Exception>)null);
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(1.25f);
				_003C_003E1__state = 5;
				return true;
			case 5:
				_003C_003E1__state = -1;
				_003Ci_003E5__6++;
				goto IL_02c8;
			case 6:
				_003C_003E1__state = -1;
				CS_0024_003C_003E8__locals40.FxStopNetworked(CS_0024_003C_003E8__locals40.fxBossMusic);
				_003C_003E2__current = new WaitForSeconds(2f);
				_003C_003E1__state = 7;
				return true;
			case 7:
				_003C_003E1__state = -1;
				goto IL_0430;
			case 8:
				{
					_003C_003E1__state = -1;
					if (CS_0024_003C_003E8__locals40.nextWaveIndex == CS_0024_003C_003E8__locals40.waveCount)
					{
						CS_0024_003C_003E8__locals40.Break();
					}
					else
					{
						CS_0024_003C_003E8__locals40.MakeAvailable();
					}
					return false;
				}
				IL_02c8:
				if (_003Ci_003E5__6 < _003CmonsterTypeCount_003E5__3 && !(CS_0024_003C_003E8__locals40._currentRule == null))
				{
					if (_003Ci_003E5__6 == 0)
					{
						CS_0024_003C_003E8__locals40.FxPlayNetworked(CS_0024_003C_003E8__locals40.fxWaveStart);
					}
					else
					{
						CS_0024_003C_003E8__locals40.FxPlayNetworked(CS_0024_003C_003E8__locals40.fxSubWaveStart);
					}
					_003C_003E2__current = new WaitForSeconds(1.25f);
					_003C_003E1__state = 3;
					return true;
				}
				if (_003CisBossWave_003E5__2)
				{
					CS_0024_003C_003E8__locals40.FxPlayNetworked(CS_0024_003C_003E8__locals40.fxBossMusic);
					Monster monster = Dew.SpawnEntity(CS_0024_003C_003E8__locals40.availableBosses[(CS_0024_003C_003E8__locals40.bossIndexOverride < 0) ? UnityEngine.Random.Range(0, CS_0024_003C_003E8__locals40.availableBosses.Length) : CS_0024_003C_003E8__locals40.bossIndexOverride], CS_0024_003C_003E8__locals40.position, null, null, DewPlayer.creep, NetworkedManagerBase<GameManager>.instance.ambientLevel, (Monster b) =>
					{
						if (b is BossMonster bossMonster)
						{
							bossMonster.skipBossSoulFlow = true;
						}
						b.Visual.skipSpawning = true;
						b.Visual.invisibleByDefault = true;
						b.Status.AddStatBonus(new StatBonus
						{
							maxHealthPercentage = -35f,
							attackDamagePercentage = 15f,
							abilityPowerPercentage = 15f,
							attackSpeedPercentage = 15f,
							abilityHastePercentage = 50f
						});
					});
					monster.Visual.EnableRenderers();
					CS_0024_003C_003E8__locals40.CreateStatusEffect<Se_MorasDomain_MorasCreation_Boss>(monster, default);
					CS_0024_003C_003E8__locals40.CreateStatusEffect(monster, default, (Se_MorasDomain_AppearFromGround s) =>
					{
						s.NetworkskipDisappearAnim = true;
						s.onDisappearAnimFinish += new Action(s.Appear);
					});
					UniTaskCompletionSource completionSource = new UniTaskCompletionSource();
					monster.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
					{
						completionSource.TrySetResult();
					});
					_003C_003E2__current = UniTaskExtensions.ToCoroutine(completionSource.Task, (Action<Exception>)null);
					_003C_003E1__state = 6;
					return true;
				}
				goto IL_0430;
				IL_0430:
				CS_0024_003C_003E8__locals40.SpawnRewards(waveIndex);
				_003C_003E2__current = new WaitForSeconds(2.5f);
				_003C_003E1__state = 8;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	public GameObject fxBreakTap;

	public GameObject fxBreak;

	public GameObject fxSilence;

	public GameObject fxBossMusic;

	[SyncVar]
	private float _breakProgress;

	private float _lastBreakTapTime;

	private Coroutine _breakResetRoutine;

	public MonsterSpawnRule singleWaveRule;

	public RoomSection mainSection;

	public int waveCount;

	public int[] monsterTypeCountsByWave;

	public float[] populationMultipliersByWave;

	public Rarity[] rewardRarityByWave;

	public RoomRewardFlowItemType[] scrambledRewardPoolOnWaves;

	public float[] populationMultipliersBySubWave;

	public Monster[] availableBosses;

	public GameObject fxWaveStart;

	public GameObject fxSubWaveStart;

	public GameObject fxSpawnRewards;

	public int nextWaveIndex;

	public int bossIndexOverride;

	private MonsterPool _allMonsters;

	private MonsterSpawnRule _currentRule;

	private RoomRewardFlowItemType[] _rewardPoolByWave;

	public string nameRawText
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	public string interactActionRawText
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	public string interactAltActionRawText
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	public bool canAltInteract
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	public float? altInteractProgress
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	public Cost cost
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	public float Network_breakProgress
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
		[param: In]
		set
		{
		}
	}

	protected override void OnCreate()
	{
	}

	protected override void OnDestroyActor()
	{
	}

	public override void OnInteract(Entity entity, bool alt)
	{
	}

	private void DoWaveStart()
	{
	}

	[Server]
	private void DoBreakTap()
	{
	}

	[Server]
	private void Break()
	{
	}

	protected override bool OnUse(Entity entity)
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private List<MonsterPool.SpawnRuleEntry> GetRandomEntries(int count)
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	[IteratorStateMachine(typeof(_003CWaveRoutine_003Ed__44))]
	private IEnumerator WaveRoutine(int waveIndex)
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	[Server]
	private void SpawnRewards(int waveIndex)
	{
		// ILSpy could not decompile this. Please report the exception below,
		// along with the assembly it came from, at https://github.com/icsharpcode/ILSpy/issues/new
		// System.IndexOutOfRangeException: Index was outside the bounds of the array.
		//    at ICSharpCode.Decompiler.Util.BitSet.Set(Int32 index) in /_/ICSharpCode.Decompiler/Util/BitSet.cs:line 196
		//    at ICSharpCode.Decompiler.Util.BitSet.set_Item(Int32 index, Boolean value) in /_/ICSharpCode.Decompiler/Util/BitSet.cs:line 69
		//    at ICSharpCode.Decompiler.IL.ILReader.PrepareBranchTargetsAndStacksForExceptionHandlers() in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 613
		//    at ICSharpCode.Decompiler.IL.ILReader.ReadInstructions(CancellationToken cancellationToken) in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 499
		//    at ICSharpCode.Decompiler.IL.ILReader.ReadIL(MethodDefinitionHandle method, MethodBodyBlock body, GenericContext genericContext, ILFunctionKind kind, CancellationToken cancellationToken) in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 726
		//    at ICSharpCode.Decompiler.CSharp.CSharpDecompiler.DecompileBody(IMethod method, EntityDeclaration entityDecl, DecompileRun decompileRun, ITypeResolveContext decompilationContext, ExtensionInfo extensionInfo) in /_/ICSharpCode.Decompiler/CSharp/CSharpDecompiler.cs:line 2391
	}

	private void OnCreate_Waves()
	{
		// ILSpy could not decompile this. Please report the exception below,
		// along with the assembly it came from, at https://github.com/icsharpcode/ILSpy/issues/new
		// System.IndexOutOfRangeException: Index was outside the bounds of the array.
		//    at ICSharpCode.Decompiler.Util.BitSet.Set(Int32 index) in /_/ICSharpCode.Decompiler/Util/BitSet.cs:line 196
		//    at ICSharpCode.Decompiler.Util.BitSet.set_Item(Int32 index, Boolean value) in /_/ICSharpCode.Decompiler/Util/BitSet.cs:line 69
		//    at ICSharpCode.Decompiler.IL.ILReader.PrepareBranchTargetsAndStacksForExceptionHandlers() in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 613
		//    at ICSharpCode.Decompiler.IL.ILReader.ReadInstructions(CancellationToken cancellationToken) in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 499
		//    at ICSharpCode.Decompiler.IL.ILReader.ReadIL(MethodDefinitionHandle method, MethodBodyBlock body, GenericContext genericContext, ILFunctionKind kind, CancellationToken cancellationToken) in /_/ICSharpCode.Decompiler/IL/ILReader.cs:line 726
		//    at ICSharpCode.Decompiler.CSharp.CSharpDecompiler.DecompileBody(IMethod method, EntityDeclaration entityDecl, DecompileRun decompileRun, ITypeResolveContext decompilationContext, ExtensionInfo extensionInfo) in /_/ICSharpCode.Decompiler/CSharp/CSharpDecompiler.cs:line 2391
	}

	private void OnDestroyActor_Waves()
	{
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
	}
}
