using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__8 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk _003C_003E4__this;

		private CastInfo _003CnewInfo_003E5__2;

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
			Ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				if (!((NetworkBehaviour)ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk).isServer)
				{
					return false;
				}
				Ai_Mon_Special_BossObliviax_DashAtk instance = ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.CreateAbilityInstance<Ai_Mon_Special_BossObliviax_DashAtk>(ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info.caster.agentPosition, null, ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info);
				_003C_003E2__current = new SI.WaitForCondition(() => instance.isDestroyed);
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = new SI.WaitForSeconds(0.05f);
				_003C_003E1__state = 2;
				return true;
			case 2:
			{
				_003C_003E1__state = -1;
				ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info.caster.Control.StartDaze(ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.castDuration + 0.1f);
				Hero closestAliveHero = Dew.GetClosestAliveHero(ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info.caster.agentPosition, fallbackToDead: true, ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info.caster);
				float num2 = AbilityTrigger.PredictAngle_Simple(ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info.caster, 1f, closestAliveHero, ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info.caster.position, ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.castDuration);
				Quaternion quaternion = Quaternion.Euler(0f, num2, 0f);
				Vector3 vector = quaternion * Vector3.forward;
				ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info.caster.Control.Rotate(quaternion, immediately: false, 0.6f);
				ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info.caster.Animation.PlayAbilityAnimation(ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.animationClip);
				ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.FxPlayNetworked(ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.fxTelegraph, ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info.caster.agentPosition + vector * (ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk._telegraphController.height / 2f), quaternion);
				_003CnewInfo_003E5__2 = new CastInfo(ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info.caster, num2);
				if ((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null && NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier() >= 0.45f && ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.enableDoubleExtraAtk && UnityEngine.Random.value <= ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.doubleExtraAtkChance)
				{
					ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.FxPlayNetworked(ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.fxCast, ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info.caster);
					_003C_003E2__current = new SI.WaitForSeconds(ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.castDuration);
					_003C_003E1__state = 3;
					return true;
				}
				ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info.caster.Control.StartDaze(ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.castDuration + 0.1f);
				_003C_003E2__current = new SI.WaitForSeconds(ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.castDuration);
				_003C_003E1__state = 5;
				return true;
			}
			case 3:
				_003C_003E1__state = -1;
				ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.CreateAbilityInstance(ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info.caster.agentPosition, null, _003CnewInfo_003E5__2, (Ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk b) =>
				{
					b.enableDoubleExtraAtk = false;
				});
				_003C_003E2__current = new SI.WaitForSeconds(1f);
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.Destroy();
				return false;
			case 5:
				_003C_003E1__state = -1;
				ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.CreateAbilityInstance<Ai_Mon_Special_BossObliviax_DashAtk>(ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.info.caster.agentPosition, null, _003CnewInfo_003E5__2);
				_003C_003E2__current = new SI.WaitForSeconds(3f);
				_003C_003E1__state = 6;
				return true;
			case 6:
				_003C_003E1__state = -1;
				ai_Mon_Special_BossObliviax_DashAtk_ExtraAtk.Destroy();
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

	public float castDuration;

	public DewAnimationClip animationClip;

	public GameObject fxTelegraph;

	public GameObject fxCast;

	public float doubleExtraAtkChance;

	[HideInInspector]
	public bool enableDoubleExtraAtk;

	private BoxTelegraphController _telegraphController;

	protected override void OnPrepare()
	{
	}

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__8))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
