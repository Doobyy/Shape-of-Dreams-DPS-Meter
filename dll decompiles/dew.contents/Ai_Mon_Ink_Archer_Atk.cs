using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_Archer_Atk : AbilityInstance
{
	public ScalingValue dmgFactor;

	public GameObject fxTelegraph;

	public GameObject fxInstance;

	public GameObject fxHit;

	public float range;

	public float duration;

	public float interval;

	public float delay;

	[Space(15f)]
	public GameObject fxTelegraphMiniboss;

	public GameObject fxInstanceMiniboss;

	public float minibossMultiplier;

	private bool _aoeEnabled;

	private float _time;

	private bool _hasBaseSnapshot;

	private float _baseRange;

	private float _baseDelay;

	private GameObject _baseFxTelegraph;

	private GameObject _baseFxInstance;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			_aoeEnabled = false;
			if (!_hasBaseSnapshot)
			{
				_hasBaseSnapshot = true;
				_baseRange = range;
				_baseDelay = delay;
				_baseFxTelegraph = fxTelegraph;
				_baseFxInstance = fxInstance;
			}
			if (((At_Mon_Ink_Archer_Atk)firstTrigger).isMiniboss)
			{
				fxTelegraph = fxTelegraphMiniboss;
				fxInstance = fxInstanceMiniboss;
				range = _baseRange * (1f + minibossMultiplier);
				delay = _baseDelay * minibossMultiplier;
			}
			else
			{
				fxTelegraph = _baseFxTelegraph;
				fxInstance = _baseFxInstance;
				range = _baseRange;
				delay = _baseDelay;
			}
			_time = Time.time;
			FxApplySpeedMultiplierNetworked(fxTelegraph, 1f / delay);
			FxPlayNetworked(fxTelegraph, info.point, null);
			info.caster.Control.StartDaze(delay);
			yield return new SI.WaitForSeconds(delay);
			FxPlayNetworked(fxInstance, info.point, null);
			_aoeEnabled = true;
			yield return new SI.WaitForSeconds(duration);
			_aoeEnabled = false;
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxInstance);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !_aoeEnabled || Time.time - _time <= interval)
		{
			return;
		}
		_time = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, info.point, range, tvDefaultHarmfulEffectTargets))
		{
			if (!item.IsNullInactiveDeadOrKnockedOut())
			{
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.point).SetElemental(ElementalType.Dark).Dispatch(item);
				FxPlayNewNetworked(fxHit, item);
			}
		}
		handle.Return();
	}

	private void MirrorProcessed()
	{
	}
}
