using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_E_SearingCharge : AbilityInstance
{
	public GameObject firstHitEffect;

	public GameObject markingEffect;

	public GameObject secondHitEffect;

	public GameObject secondHitSound;

	public Knockback knockback;

	public GameObject resetEffect;

	public ScalingValue firstHitDmgAmount;

	public ScalingValue secondHitDmgAmount;

	public float chargeDuration;

	public float secondDmgDelay;

	public float checkKillGracePeriod = 0.5f;

	public float radius;

	public int maxSecondHitSound = 2;

	private List<Entity> _entsHit;

	private bool _enableCheckTimer;

	private bool _shouldDoCollisionCheck;

	private Vector3 _previousPos;

	private Vector3 _currentPos;

	private float _multiplier = 1f;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_enableCheckTimer = false;
		_shouldDoCollisionCheck = false;
		_multiplier = 1f;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		_entsHit = new List<Entity>();
		_previousPos = info.caster.position;
		Vector3 delta = info.forward * ((St_E_SearingCharge)firstTrigger).currentConfig.castMethod._length;
		info.caster.Control.StartDaze(chargeDuration);
		yield return new SI.WaitForSeconds(0.1f);
		info.caster.Status.CalculateStats();
		_multiplier = Mathf.Max(1f, info.caster.Status.movementSpeedMultiplier);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			destination = info.caster.position + delta * _multiplier,
			duration = chargeDuration,
			ease = DewEase.EaseOutQuad,
			isFriendly = true,
			rotateForward = true,
			canGoOverTerrain = false
		});
		_shouldDoCollisionCheck = true;
		yield return new SI.WaitForSeconds(chargeDuration);
		_shouldDoCollisionCheck = false;
		_enableCheckTimer = true;
		yield return new SI.WaitForSeconds(secondDmgDelay);
		for (int i = 0; i < _entsHit.Count; i++)
		{
			if (!((Object)(object)_entsHit[i] == null) && ((NetworkBehaviour)_entsHit[i]).isServer)
			{
				if (i < maxSecondHitSound)
				{
					FxPlayNewNetworked(secondHitSound, _entsHit[i]);
				}
				FxPlayNewNetworked(secondHitEffect, _entsHit[i]);
				Damage(secondHitDmgAmount).SetElemental(ElementalType.Fire).SetDirection(rotation).ApplyAmplification(_multiplier - 1f)
					.Dispatch(_entsHit[i]);
			}
		}
		yield return new SI.WaitForSeconds(checkKillGracePeriod);
		Destroy();
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!((NetworkBehaviour)this).isServer || !_shouldDoCollisionCheck)
		{
			return;
		}
		_currentPos = info.caster.position;
		float maxDistance = Vector3.Distance(_previousPos, _currentPos);
		List<Entity> list = DewPhysics.SphereCastAllEntities(out var handle, _previousPos, radius, info.forward, maxDistance, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < list.Count; i++)
		{
			Entity item = list[i];
			if (!_entsHit.Contains(item))
			{
				FxPlayNewNetworked(firstHitEffect, list[i]);
				FxPlayNewNetworked(markingEffect, list[i]);
				knockback.ApplyWithOrigin(info.caster.position, list[i]);
				Damage(firstHitDmgAmount).SetElemental(ElementalType.Fire).SetDirection(rotation).ApplyAmplification(Mathf.Max(_multiplier - 1f, 0f))
					.Dispatch(list[i]);
				_entsHit.Add(item);
			}
		}
		_previousPos = info.caster.position;
		handle.Return();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !_enableCheckTimer)
		{
			return;
		}
		for (int i = 0; i < _entsHit.Count && !((Object)(object)_entsHit[i] == null); i++)
		{
			if (!_entsHit[i].isAlive)
			{
				if ((Object)(object)firstTrigger != null)
				{
					ResetCooldown(firstTrigger);
					FxPlayNetworked(resetEffect, info.caster);
				}
				_enableCheckTimer = false;
				break;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
