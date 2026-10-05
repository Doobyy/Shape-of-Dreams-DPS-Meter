using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Ai_GlacialBarrier_Spawn : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__17 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_GlacialBarrier_Spawn _003C_003E4__this;

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
		public _003COnCreateSequenced_003Ed__17(int _003C_003E1__state)
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
			Ai_GlacialBarrier_Spawn ai_GlacialBarrier_Spawn = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)ai_GlacialBarrier_Spawn).isServer)
				{
					return false;
				}
				ai_GlacialBarrier_Spawn.FxPlayNetworked(ai_GlacialBarrier_Spawn.fxSpawnning);
				_003C_003E2__current = new SI.WaitForSeconds(ai_GlacialBarrier_Spawn.spawnDuration);
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				ai_GlacialBarrier_Spawn.FxPlayNetworked(ai_GlacialBarrier_Spawn.fxSpawn);
				List<Entity> entities = ai_GlacialBarrier_Spawn.range.GetEntities(out var handle, ai_GlacialBarrier_Spawn.hittable, ai_GlacialBarrier_Spawn.info.caster);
				for (int i = 0; i < entities.Count; i++)
				{
					Entity entity = entities[i];
					if (!(entity is PropEnt_GlacialBarrier))
					{
						ai_GlacialBarrier_Spawn.knockback.ApplyWithOrigin(((Component)(object)ai_GlacialBarrier_Spawn).transform.position, entity);
						ai_GlacialBarrier_Spawn.CreateBasicEffect(entity, new StunEffect(), ai_GlacialBarrier_Spawn.stunDuration, "glacialbarrier_stun");
						entity.Visual.KnockUp(2.5f, isFriendly: false);
						ai_GlacialBarrier_Spawn.ApplyElemental(ElementalType.Cold, entity);
						ai_GlacialBarrier_Spawn.FxPlayNewNetworked(ai_GlacialBarrier_Spawn.fxHit, entity);
					}
				}
				handle.Return();
				ai_GlacialBarrier_Spawn.Destroy();
				return false;
			}
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

	public GameObject fxSpawnning;

	public float spawnDuration;

	public DewCollider range;

	public AbilityTargetValidator hittable;

	public Knockback knockback;

	public GameObject fxSpawn;

	public GameObject fxHit;

	[HideInInspector]
	public float stunDuration;

	private GameObject _baseFxSpawn;

	private GameObject _baseFxSpawnning;

	private Vector3 _baseFxSpawnScale;

	private Vector3 _baseFxSpawnningScale;

	public override bool reuseInRoom
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
	}

	protected override void Awake()
	{
	}

	protected override void OnDisable()
	{
	}

	protected override void OnCreate()
	{
	}

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__17))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
