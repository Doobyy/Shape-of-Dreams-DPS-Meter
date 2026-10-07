using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Se_Nor_MyOwnLittleNebula : StackedStatusEffect
{
	[CompilerGenerated]
	private sealed class _003CShootMissileSequenced_003Ed__11 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Se_Nor_MyOwnLittleNebula _003C_003E4__this;

		public Entity target;

		public int count;

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
		public _003CShootMissileSequenced_003Ed__11(int _003C_003E1__state)
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
			Se_Nor_MyOwnLittleNebula se_Nor_MyOwnLittleNebula = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new SI.WaitForSeconds(se_Nor_MyOwnLittleNebula.missileFirstDelay);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				break;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				break;
			}
			if (_003Ci_003E5__2 < count)
			{
				if ((UnityEngine.Object)(object)target == null || !target.isActive)
				{
					List<Entity> entities = se_Nor_MyOwnLittleNebula.newTargetRange.GetEntities(out var handle, se_Nor_MyOwnLittleNebula.newTargetTargetable, se_Nor_MyOwnLittleNebula.victim);
					if (entities.Count == 0)
					{
						handle.Return();
						return false;
					}
					target = entities[0];
					handle.Return();
				}
				se_Nor_MyOwnLittleNebula.CreateAbilityInstance<Ai_Nor_MyOwnLittleNebula_Missile>(se_Nor_MyOwnLittleNebula.position, Quaternion.identity, new CastInfo(se_Nor_MyOwnLittleNebula.victim, target));
				_003C_003E2__current = new SI.WaitForSeconds(se_Nor_MyOwnLittleNebula.missileDelayInBetween);
				_003C_003E1__state = 2;
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

	public DewCollider newTargetRange;

	public AbilityTargetValidator newTargetTargetable;

	public int missilePerStack;

	public float missileFirstDelay;

	public float missileDelayInBetween;

	public new Hero victim
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	protected override void OnCreate()
	{
	}

	protected override void OnDestroyActor()
	{
	}

	private void SkillUsed(EventInfoSkillUse obj)
	{
	}

	private void AttackFired(EventInfoAttackFired obj)
	{
	}

	[IteratorStateMachine(typeof(_003CShootMissileSequenced_003Ed__11))]
	private IEnumerator ShootMissileSequenced(int count, Entity target)
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
	}

	private void MirrorProcessed()
	{
	}
}
