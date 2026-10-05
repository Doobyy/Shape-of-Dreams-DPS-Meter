using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_StellaMatter_ThrowMagazine : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__1 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Mon_Sky_StellaMatter_ThrowMagazine _003C_003E4__this;

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
		public _003COnCreateSequenced_003Ed__1(int _003C_003E1__state)
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
			Ai_Mon_Sky_StellaMatter_ThrowMagazine ai_Mon_Sky_StellaMatter_ThrowMagazine = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)ai_Mon_Sky_StellaMatter_ThrowMagazine).isServer)
				{
					return false;
				}
				ai_Mon_Sky_StellaMatter_ThrowMagazine.DestroyOnDeath(ai_Mon_Sky_StellaMatter_ThrowMagazine.info.caster);
				_003C_003E2__current = new SI.WaitForSeconds(ai_Mon_Sky_StellaMatter_ThrowMagazine.delay);
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				for (int i = 0; i < NetworkedManagerBase<ActorManager>.instance.allHeroes.Count; i++)
				{
					Hero hero = NetworkedManagerBase<ActorManager>.instance.allHeroes[i];
					if (!hero.IsNullInactiveDeadOrKnockedOut())
					{
						Vector3 agentPosition = hero.agentPosition;
						ai_Mon_Sky_StellaMatter_ThrowMagazine.CreateAbilityInstance<Ai_Mon_Sky_StellaMatter_Throw>(ai_Mon_Sky_StellaMatter_ThrowMagazine.info.caster.position, null, new CastInfo(ai_Mon_Sky_StellaMatter_ThrowMagazine.info.caster, agentPosition));
					}
				}
				ai_Mon_Sky_StellaMatter_ThrowMagazine.Destroy();
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

	public float delay;

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__1))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
