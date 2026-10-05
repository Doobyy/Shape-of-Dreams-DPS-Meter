using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Ai_R_PhantomBlade : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__18 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_R_PhantomBlade _003C_003E4__this;

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
		public _003COnCreateSequenced_003Ed__18(int _003C_003E1__state)
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
			Ai_R_PhantomBlade ai_R_PhantomBlade = _003C_003E4__this;
			ListReturnHandle<Entity> handle;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)ai_R_PhantomBlade).isServer)
				{
					return false;
				}
				ai_R_PhantomBlade.FxPlayNetworked(ai_R_PhantomBlade.firstAtkEffect);
				List<Entity> entities = ai_R_PhantomBlade.range.GetEntities(out handle, ai_R_PhantomBlade.tvDefaultHarmfulEffectTargets);
				for (int j = 0; j < entities.Count; j++)
				{
					Entity entity2 = entities[j];
					ai_R_PhantomBlade.OnHit(entity2, isSecondAtk: false);
				}
				handle.Return();
				ai_R_PhantomBlade.FxPlayNetworked(ai_R_PhantomBlade.secondAtkDelayEffect);
				_003C_003E2__current = new SI.WaitForSeconds(ai_R_PhantomBlade.secondAtkDelay);
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
			{
				_003C_003E1__state = -1;
				ai_R_PhantomBlade.FxPlayNetworked(ai_R_PhantomBlade.secondAtkEffect);
				List<Entity> entities = ai_R_PhantomBlade.range.GetEntities(out handle, ai_R_PhantomBlade.tvDefaultHarmfulEffectTargets);
				for (int i = 0; i < entities.Count; i++)
				{
					Entity entity = entities[i];
					ai_R_PhantomBlade.OnHit(entity, isSecondAtk: true);
				}
				handle.Return();
				_003C_003E2__current = new SI.WaitForSeconds(ai_R_PhantomBlade.destroyDelay);
				_003C_003E1__state = 2;
				return true;
			}
			case 2:
				_003C_003E1__state = -1;
				ai_R_PhantomBlade.FxPlayNetworked(ai_R_PhantomBlade.destroyEffect);
				ai_R_PhantomBlade.Destroy();
				return false;
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

	public DewCollider range;

	public ScalingValue firstDmgFactor;

	public ScalingValue secondDmgFactor;

	public float procCoefficient;

	public Knockback Knockback;

	public int atkCount;

	public float atkDuration;

	public float elementalChance;

	public float secondAtkDelay;

	public float destroyDelay;

	public GameObject firstAtkEffect;

	public GameObject firstHitEffect;

	public GameObject secondAtkEffect;

	public GameObject secondHitEffect;

	public GameObject secondAtkDelayEffect;

	public GameObject destroyEffect;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__18))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void OnHit(Entity entity, bool isSecondAtk)
	{
	}

	private void MirrorProcessed()
	{
	}
}
