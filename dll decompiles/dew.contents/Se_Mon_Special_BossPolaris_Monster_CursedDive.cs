using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Se_Mon_Special_BossPolaris_Monster_CursedDive : StatusEffect
{
	[CompilerGenerated]
	private sealed class _003C_003COnCreateSequenced_003Eg__Routine_007C8_0_003Ed : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Se_Mon_Special_BossPolaris_Monster_CursedDive _003C_003E4__this;

		private List<Vector3> _003Cpositions_003E5__2;

		private int _003Ci_003E5__3;

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
		public _003C_003COnCreateSequenced_003Eg__Routine_007C8_0_003Ed(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			Se_Mon_Special_BossPolaris_Monster_CursedDive se_Mon_Special_BossPolaris_Monster_CursedDive = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				se_Mon_Special_BossPolaris_Monster_CursedDive.LockDestroy();
				_003Cpositions_003E5__2 = new List<Vector3>();
				for (int i = 0; i < se_Mon_Special_BossPolaris_Monster_CursedDive.stoneCount; i++)
				{
					_003Cpositions_003E5__2.Add(SingletonBehaviour<Room_BossArena>.instance.GetRandomPathablePosition());
				}
				for (int j = 0; j < 10; j++)
				{
					for (int k = 0; k < _003Cpositions_003E5__2.Count; k++)
					{
						Vector3 vector = se_Mon_Special_BossPolaris_Monster_CursedDive.info.caster.agentPosition;
						float num2 = Vector3.Distance(_003Cpositions_003E5__2[k], se_Mon_Special_BossPolaris_Monster_CursedDive.info.caster.agentPosition);
						for (int l = 0; l < _003Cpositions_003E5__2.Count; l++)
						{
							if (k != l)
							{
								float num3 = Vector3.Distance(_003Cpositions_003E5__2[k], _003Cpositions_003E5__2[l]);
								if (!(num3 >= num2))
								{
									num2 = num3;
									vector = _003Cpositions_003E5__2[l];
								}
							}
						}
						if (!(num2 > 5f))
						{
							_003Cpositions_003E5__2[k] = Dew.GetValidAgentDestination_Closest(_003Cpositions_003E5__2[k], _003Cpositions_003E5__2[k] + (_003Cpositions_003E5__2[k] - vector).normalized * 1.5f);
						}
					}
				}
				_003Ci_003E5__3 = 0;
				break;
			}
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__3++;
				break;
			}
			if (_003Ci_003E5__3 < _003Cpositions_003E5__2.Count)
			{
				se_Mon_Special_BossPolaris_Monster_CursedDive.CreateAbilityInstance<Ai_Mon_Special_BossPolaris_Monster_CursedDive_SpawnStone>(_003Cpositions_003E5__2[_003Ci_003E5__3], null, new CastInfo(se_Mon_Special_BossPolaris_Monster_CursedDive.info.caster));
				_003C_003E2__current = new WaitForSeconds(0.1f);
				_003C_003E1__state = 1;
				return true;
			}
			se_Mon_Special_BossPolaris_Monster_CursedDive.UnlockDestroy();
			return false;
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

	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__8 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Se_Mon_Special_BossPolaris_Monster_CursedDive _003C_003E4__this;

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
		public _003COnCreateSequenced_003Ed__8(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			Se_Mon_Special_BossPolaris_Monster_CursedDive CS_0024_003C_003E8__locals30 = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)CS_0024_003C_003E8__locals30).isServer)
				{
					return false;
				}
				CS_0024_003C_003E8__locals30.DoInvulnerable();
				CS_0024_003C_003E8__locals30.DoInvisible();
				CS_0024_003C_003E8__locals30.DoUncollidable();
				CS_0024_003C_003E8__locals30.DoUntargetable();
				CS_0024_003C_003E8__locals30.info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
				CS_0024_003C_003E8__locals30.FxPlayNetworked(CS_0024_003C_003E8__locals30.fxAscendLoop, CS_0024_003C_003E8__locals30.info.caster);
				_003C_003E2__current = new SI.WaitForSeconds(CS_0024_003C_003E8__locals30.initialDelayBeforeTelegraph);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				CS_0024_003C_003E8__locals30.Teleport(CS_0024_003C_003E8__locals30.info.caster, SingletonBehaviour<Room_BossArena>.instance.center);
				CS_0024_003C_003E8__locals30.info.caster.Control.Rotate(ManagerBase<CameraManager>.instance.entityCamAngle + 180f, immediately: true, 1f);
				CS_0024_003C_003E8__locals30.FxPlayNetworked(CS_0024_003C_003E8__locals30.fxTelegraph, CS_0024_003C_003E8__locals30.info.caster, SingletonBehaviour<Room_BossArena>.instance.center, null);
				_003C_003E2__current = new SI.WaitForSeconds(CS_0024_003C_003E8__locals30.telegraphTime - CS_0024_003C_003E8__locals30.descendTime);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				CS_0024_003C_003E8__locals30.FxStopNetworked(CS_0024_003C_003E8__locals30.fxAscendLoop);
				CS_0024_003C_003E8__locals30.FxPlayNetworked(CS_0024_003C_003E8__locals30.fxDescendLoop, CS_0024_003C_003E8__locals30.info.caster);
				_003C_003E2__current = new SI.WaitForSeconds(CS_0024_003C_003E8__locals30.descendTime);
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				CS_0024_003C_003E8__locals30.CreateAbilityInstance<Ai_Mon_Special_BossPolaris_Monster_CursedDive_Stomp>(SingletonBehaviour<Room_BossArena>.instance.center, null, new CastInfo(CS_0024_003C_003E8__locals30.info.caster));
				((MonoBehaviour)(object)CS_0024_003C_003E8__locals30).StartCoroutine(Routine());
				CS_0024_003C_003E8__locals30.info.caster.Control.StartDaze(CS_0024_003C_003E8__locals30.postDaze);
				CS_0024_003C_003E8__locals30.Destroy();
				return false;
			}
			[IteratorStateMachine(typeof(_003C_003COnCreateSequenced_003Eg__Routine_007C8_0_003Ed))]
			IEnumerator Routine()
			{
				/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
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

	public GameObject fxAscendLoop;

	public GameObject fxTelegraph;

	public GameObject fxDescendLoop;

	public float initialDelayBeforeTelegraph;

	public float telegraphTime;

	public float descendTime;

	public float postDaze;

	public int stoneCount;

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__8))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	protected override void OnDestroyActor()
	{
	}

	private void MirrorProcessed()
	{
	}
}
