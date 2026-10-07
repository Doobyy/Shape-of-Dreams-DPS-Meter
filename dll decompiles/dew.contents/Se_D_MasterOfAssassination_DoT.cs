using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;

public class Se_D_MasterOfAssassination_DoT : StatusEffect
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__6 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Se_D_MasterOfAssassination_DoT _003C_003E4__this;

		private int _003Ci_003E5__2;

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
			Se_D_MasterOfAssassination_DoT se_D_MasterOfAssassination_DoT = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)se_D_MasterOfAssassination_DoT).isServer)
				{
					return false;
				}
				_003Ci_003E5__2 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				se_D_MasterOfAssassination_DoT.Damage(se_D_MasterOfAssassination_DoT.totalDamage, se_D_MasterOfAssassination_DoT.procCoefficient).SetElemental(ElementalType.Dark).SetAttr(DamageAttribute.DamageOverTime)
					.SetAttr(DamageAttribute.ForceMergeNumber)
					.Dispatch(se_D_MasterOfAssassination_DoT.victim);
				_003Ci_003E5__2++;
				break;
			}
			if (_003Ci_003E5__2 < se_D_MasterOfAssassination_DoT.ticks)
			{
				_003C_003E2__current = new SI.WaitForSeconds(se_D_MasterOfAssassination_DoT.tickInterval);
				_003C_003E1__state = 1;
				return true;
			}
			se_D_MasterOfAssassination_DoT.DestroyIfActive();
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

	public ScalingValue totalDamage;

	public float procCoefficient;

	public int ticks;

	public float tickInterval;

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
