using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_StellaMatter_Atk : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__8 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Mon_Sky_StellaMatter_Atk _003C_003E4__this;

		private int _003Ccount_003E5__2;

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
			Ai_Mon_Sky_StellaMatter_Atk CS_0024_003C_003E8__locals27 = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)CS_0024_003C_003E8__locals27).isServer)
				{
					return false;
				}
				CS_0024_003C_003E8__locals27.DestroyOnDeath(CS_0024_003C_003E8__locals27.info.caster);
				CS_0024_003C_003E8__locals27._added = new Vector3[CS_0024_003C_003E8__locals27.atkCount];
				_003C_003E2__current = new SI.WaitForSeconds(CS_0024_003C_003E8__locals27.channelDelay);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003Ccount_003E5__2 = 0;
				break;
			case 2:
				_003C_003E1__state = -1;
				break;
			case 3:
				_003C_003E1__state = -1;
				break;
			}
			if (_003Ccount_003E5__2 < CS_0024_003C_003E8__locals27.atkCount)
			{
				Vector3 end = Vector3.zero;
				bool flag = true;
				if (_003Ccount_003E5__2 == 0)
				{
					end = CS_0024_003C_003E8__locals27.info.target.agentPosition;
					flag = false;
				}
				if (_003Ccount_003E5__2 == 1)
				{
					end = AbilityTrigger.PredictPoint_Simple(CS_0024_003C_003E8__locals27.info.caster, UnityEngine.Random.Range(0.7f, 1f), CS_0024_003C_003E8__locals27.info.target, CS_0024_003C_003E8__locals27.atkDelay);
					flag = false;
				}
				if (!flag)
				{
					end = Dew.GetValidAgentDestination_Closest(CS_0024_003C_003E8__locals27.info.target.agentPosition, end);
					end = Dew.GetPositionOnGround(end);
					CS_0024_003C_003E8__locals27._added[_003Ccount_003E5__2] = end;
					CS_0024_003C_003E8__locals27.CreateAbilityInstance(end, null, new CastInfo(CS_0024_003C_003E8__locals27.info.caster, end), (Ai_Mon_Sky_StellaMatter_AtkSub b) =>
					{
					});
					_003Ccount_003E5__2++;
					_003C_003E2__current = new SI.WaitForSeconds(CS_0024_003C_003E8__locals27.atkInterval);
					_003C_003E1__state = 2;
					return true;
				}
				int num2 = 0;
				while (num2 < CS_0024_003C_003E8__locals27.maxTryCount)
				{
					num2++;
					bool flag2 = true;
					end = CS_0024_003C_003E8__locals27.info.target.agentPosition + UnityEngine.Random.insideUnitCircle.ToXZ() * CS_0024_003C_003E8__locals27.atkRadius;
					end = Dew.GetValidAgentDestination_Closest(CS_0024_003C_003E8__locals27.info.target.agentPosition, end);
					end = Dew.GetPositionOnGround(end);
					for (int num3 = 0; num3 < CS_0024_003C_003E8__locals27._added.Length; num3++)
					{
						if (!(Vector3.SqrMagnitude(end - CS_0024_003C_003E8__locals27._added[num3]) < CS_0024_003C_003E8__locals27.sqrMinDistance))
						{
							flag2 = false;
						}
					}
					if (!flag2)
					{
						CS_0024_003C_003E8__locals27._added[_003Ccount_003E5__2] = end;
						CS_0024_003C_003E8__locals27.CreateAbilityInstance(end, null, new CastInfo(CS_0024_003C_003E8__locals27.info.caster, end), (Ai_Mon_Sky_StellaMatter_AtkSub b) =>
						{
						});
						break;
					}
				}
				_003Ccount_003E5__2++;
				_003C_003E2__current = new SI.WaitForSeconds(CS_0024_003C_003E8__locals27.atkInterval);
				_003C_003E1__state = 3;
				return true;
			}
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

	public float atkRadius;

	public int atkCount;

	public float sqrMinDistance;

	public float channelDelay;

	public float atkDelay;

	public float atkInterval;

	public int maxTryCount;

	private Vector3[] _added;

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__8))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
