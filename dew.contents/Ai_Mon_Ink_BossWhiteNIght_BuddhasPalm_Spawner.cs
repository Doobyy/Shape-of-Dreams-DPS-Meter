using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNIght_BuddhasPalm_Spawner : AbilityInstance
{
	public float castDuration;

	public float postDelay;

	public float interval;

	public Vector2 deviationRadius;

	public DewAnimationClip readyClip;

	public DewAnimationClip atkClip;

	public GameObject fxStart;

	public GameObject fxTelegraph;

	public int rageAtkCount;

	public float rageAtkMultiplier;

	private int _spawnCount = 1;

	private bool _isRage;

	private float _pristineCastDuration;

	private float _pristineRageAtkMultiplier;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_pristineCastDuration = castDuration;
		_pristineRageAtkMultiplier = rageAtkMultiplier;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		castDuration = _pristineCastDuration;
		rageAtkMultiplier = _pristineRageAtkMultiplier;
		_spawnCount = 1;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_isRage = ((Mon_Ink_BossWhiteNight)info.caster)._isRage;
		if (_isRage)
		{
			_spawnCount = rageAtkCount;
			castDuration *= rageAtkMultiplier;
		}
		else
		{
			rageAtkMultiplier = 1f;
		}
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = castDuration + postDelay,
			isAttack = false,
			onCancel = DestroyIfActive
		});
		info.caster.Animation.PlayAbilityAnimation(readyClip, 1f / rageAtkMultiplier);
		FxPlayNetworked(fxStart, info.caster);
		Vector3 point = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), info.target, castDuration);
		point = Dew.GetPositionOnGround(point);
		for (int i = 0; i < _spawnCount; i++)
		{
			if (i == 0)
			{
				Quaternion value = Quaternion.LookRotation(point - info.caster.agentPosition);
				CreateAbilityInstance(point, value, new CastInfo(info.caster), (Ai_Mon_Ink_BossWhiteNight_BuddhasPalm_Instance b) =>
				{
					b.startDelay = castDuration;
				});
				FxPlayNewNetworked(fxTelegraph, point, value);
				continue;
			}
			yield return new SI.WaitForSeconds(interval);
			point += Random.insideUnitCircle.ToXZ() * Random.Range(deviationRadius.x, deviationRadius.y);
			Quaternion value2 = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
			CreateAbilityInstance(point, value2, new CastInfo(info.caster), (Ai_Mon_Ink_BossWhiteNight_BuddhasPalm_Instance b) =>
			{
				b.startDelay = castDuration;
			});
			FxPlayNewNetworked(fxTelegraph, point, value2);
		}
		yield return new SI.WaitForSeconds(castDuration - interval * (float)(_spawnCount - 1));
		info.caster.Animation.PlayAbilityAnimation(atkClip);
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
