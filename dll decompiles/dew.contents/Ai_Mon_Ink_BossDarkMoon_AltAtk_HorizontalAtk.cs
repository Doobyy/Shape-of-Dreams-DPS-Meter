using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_AltAtk_HorizontalAtk : InstantDamageInstance
{
	public float postDelay;

	public float backDashDistance;

	public float backDashDuration;

	public DewEase ease;

	public int additionalSpawnCount;

	public float additionalSpawnDelay;

	public float AdditionalSpawnInterval;

	public float telegraphDuration;

	public GameObject fxTelegraph;

	[NonSerialized]
	public bool isMainAtk = true;

	[NonSerialized]
	public bool isRage;

	private GameObject _baseStartEffect;

	private GameObject _baseStartEffectNoStop;

	private bool _baseDestroyWhenDone;

	protected override void Awake()
	{
		base.Awake();
		_baseStartEffect = startEffect;
		_baseStartEffectNoStop = startEffectNoStop;
		_baseDestroyWhenDone = destroyWhenDone;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		startEffect = _baseStartEffect;
		startEffectNoStop = _baseStartEffectNoStop;
		destroyWhenDone = _baseDestroyWhenDone;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer && isRage && !isMainAtk)
		{
			destroyWhenDone = true;
			FxPlayNetworked(fxTelegraph, position, rotation);
			yield return new SI.WaitForSeconds(telegraphDuration);
		}
		yield return base.OnCreateSequenced();
		if (((NetworkBehaviour)this).isServer && isMainAtk)
		{
			if (isRage)
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
			Vector3 end = info.caster.agentPosition - info.forward * backDashDistance;
			end = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, end);
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = false,
				destination = end,
				ease = ease,
				duration = backDashDuration,
				isCanceledByCC = false,
				isFriendly = true,
				onCancel = DestroyIfActive,
				onFinish = () =>
				{
					info.caster.Control.Rotate(info.forward, immediately: true);
					info.caster.Control.StartDaze(postDelay);
					DestroyIfActive();
				}
			});
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(additionalSpawnDelay);
			for (int i = 0; i < additionalSpawnCount; i++)
			{
				Vector3 point = position + UnityEngine.Random.insideUnitCircle.ToXZ() * 5f;
				Quaternion value = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f);
				CreateAbilityInstance(point, value, new CastInfo(info.caster, point), (Ai_Mon_Ink_BossDarkMoon_AltAtk_HorizontalAtk b) =>
				{
					b.isRage = true;
					b.isMainAtk = false;
					b.startEffect = null;
					b.startEffectNoStop = null;
				});
				yield return new WaitForSeconds(AdditionalSpawnInterval);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
