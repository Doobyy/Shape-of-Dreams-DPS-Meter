using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Ai_MorasDomain_MorasCreation_Boss_SpawnSwarm : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__6 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_MorasDomain_MorasCreation_Boss_SpawnSwarm _003C_003E4__this;

		private List<int> _003Clist_003E5__2;

		private float _003CstartAngle_003E5__3;

		private int _003Ci_003E5__4;

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
		public _003COnCreateSequenced_003Ed__6(int _003C_003E1__state)
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
			Ai_MorasDomain_MorasCreation_Boss_SpawnSwarm ai_MorasDomain_MorasCreation_Boss_SpawnSwarm = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)ai_MorasDomain_MorasCreation_Boss_SpawnSwarm).isServer)
				{
					return false;
				}
				ai_MorasDomain_MorasCreation_Boss_SpawnSwarm.info.caster.Status.SetHealth(ai_MorasDomain_MorasCreation_Boss_SpawnSwarm.info.caster.Status.currentHealth * (1f - ai_MorasDomain_MorasCreation_Boss_SpawnSwarm.currHealthCost));
				ai_MorasDomain_MorasCreation_Boss_SpawnSwarm.DestroyOnDeath(ai_MorasDomain_MorasCreation_Boss_SpawnSwarm.info.caster);
				_003Clist_003E5__2 = new List<int>();
				for (int i = 0; i < ai_MorasDomain_MorasCreation_Boss_SpawnSwarm.spawnCount; i++)
				{
					_003Clist_003E5__2.Add(i);
				}
				_003Clist_003E5__2.Shuffle();
				_003CstartAngle_003E5__3 = UnityEngine.Random.Range(0f, 360f);
				_003Ci_003E5__4 = 0;
				break;
			}
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__4++;
				break;
			}
			if (_003Ci_003E5__4 < _003Clist_003E5__2.Count)
			{
				int num2 = _003Clist_003E5__2[_003Ci_003E5__4];
				float y = _003CstartAngle_003E5__3 + (float)num2 * 360f / (float)_003Clist_003E5__2.Count;
				Vector3 end = ai_MorasDomain_MorasCreation_Boss_SpawnSwarm.info.caster.agentPosition + Quaternion.Euler(0f, y, 0f) * Vector3.forward * UnityEngine.Random.Range(ai_MorasDomain_MorasCreation_Boss_SpawnSwarm.spawnRadiusRange.x, ai_MorasDomain_MorasCreation_Boss_SpawnSwarm.spawnRadiusRange.y);
				end = Dew.GetValidAgentDestination_LinearSweep(ai_MorasDomain_MorasCreation_Boss_SpawnSwarm.info.caster.agentPosition, end);
				Mon_MorasDomain_NightmareBlob.SpawnByProjectile(ai_MorasDomain_MorasCreation_Boss_SpawnSwarm.info.caster.agentPosition, end);
				_003C_003E2__current = new SI.WaitForSeconds(UnityEngine.Random.Range(ai_MorasDomain_MorasCreation_Boss_SpawnSwarm.spawnIntervalRange.x, ai_MorasDomain_MorasCreation_Boss_SpawnSwarm.spawnIntervalRange.y));
				_003C_003E1__state = 1;
				return true;
			}
			ai_MorasDomain_MorasCreation_Boss_SpawnSwarm.Destroy();
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

	public float currHealthCost;

	public int spawnCount;

	public Vector2 spawnRadiusRange;

	public Vector2 spawnIntervalRange;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__6))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
