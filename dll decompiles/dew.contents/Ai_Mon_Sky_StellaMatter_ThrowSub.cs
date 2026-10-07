using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_StellaMatter_ThrowSub : AbilityInstance
{
	public struct Ad_CheckThrowSubDuplication
	{
		public float LastHitTime;
	}

	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__16 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Mon_Sky_StellaMatter_ThrowSub _003C_003E4__this;

		private float _003CtickCount_003E5__2;

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
		public _003COnCreateSequenced_003Ed__16(int _003C_003E1__state)
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
			Ai_Mon_Sky_StellaMatter_ThrowSub ai_Mon_Sky_StellaMatter_ThrowSub = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)ai_Mon_Sky_StellaMatter_ThrowSub).isServer)
				{
					return false;
				}
				_003C_003E2__current = new SI.WaitForSeconds(ai_Mon_Sky_StellaMatter_ThrowSub.delay);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				ai_Mon_Sky_StellaMatter_ThrowSub.FxPlayNetworked(ai_Mon_Sky_StellaMatter_ThrowSub.fxStart);
				ai_Mon_Sky_StellaMatter_ThrowSub.FxPlayNetworked(ai_Mon_Sky_StellaMatter_ThrowSub.fxSustain);
				_003CtickCount_003E5__2 = ai_Mon_Sky_StellaMatter_ThrowSub.sustainTime / ai_Mon_Sky_StellaMatter_ThrowSub.tickInterval;
				_003Ci_003E5__3 = 0;
				break;
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__3++;
				break;
			}
			if ((float)_003Ci_003E5__3 < _003CtickCount_003E5__2)
			{
				List<Entity> entities = ai_Mon_Sky_StellaMatter_ThrowSub.range.GetEntities(out var handle, ai_Mon_Sky_StellaMatter_ThrowSub.hittable, ai_Mon_Sky_StellaMatter_ThrowSub.info.caster);
				for (int i = 0; i < entities.Count; i++)
				{
					Entity entity = entities[i];
					if (entity.HasData<Ad_CheckThrowSubDuplication>())
					{
						float num2 = ai_Mon_Sky_StellaMatter_ThrowSub.tickInterval / 2f;
						if (Time.time - entity.GetData<Ad_CheckThrowSubDuplication>().LastHitTime < num2)
						{
							continue;
						}
						entity.RemoveData<Ad_CheckThrowSubDuplication>();
					}
					if (!entity.HasData<Ad_CheckThrowSubDuplication>())
					{
						entity.AddData(new Ad_CheckThrowSubDuplication
						{
							LastHitTime = Time.time
						});
					}
					ai_Mon_Sky_StellaMatter_ThrowSub.CreateDamage(DamageData.SourceType.Default, ai_Mon_Sky_StellaMatter_ThrowSub.tickDmgFactor).Dispatch(entity);
					ai_Mon_Sky_StellaMatter_ThrowSub.CreateBasicEffect(entity, new SlowEffect
					{
						decay = ai_Mon_Sky_StellaMatter_ThrowSub.isSlowDecay,
						strength = ai_Mon_Sky_StellaMatter_ThrowSub.slowStrength
					}, ai_Mon_Sky_StellaMatter_ThrowSub.slowDuration, "UniverseMatter_Slow");
					ai_Mon_Sky_StellaMatter_ThrowSub.FxPlayNewNetworked(ai_Mon_Sky_StellaMatter_ThrowSub.fxHit, entity);
				}
				handle.Return();
				_003C_003E2__current = new SI.WaitForSeconds(ai_Mon_Sky_StellaMatter_ThrowSub.tickInterval);
				_003C_003E1__state = 2;
				return true;
			}
			ai_Mon_Sky_StellaMatter_ThrowSub.FxStopNetworked(ai_Mon_Sky_StellaMatter_ThrowSub.fxSustain);
			ai_Mon_Sky_StellaMatter_ThrowSub.FxPlayNetworked(ai_Mon_Sky_StellaMatter_ThrowSub.fxStop);
			ai_Mon_Sky_StellaMatter_ThrowSub.Destroy();
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

	public float delay;

	public float tickInterval;

	public float sustainTime;

	public float slowStrength;

	public float slowDuration;

	public bool isSlowDecay;

	public DewCollider range;

	public ScalingValue tickDmgFactor;

	public AbilityTargetValidator hittable;

	public GameObject fxStart;

	public GameObject fxSustain;

	public GameObject fxStop;

	public GameObject fxHit;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__16))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
