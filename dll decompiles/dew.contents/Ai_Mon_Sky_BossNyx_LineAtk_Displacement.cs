using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_LineAtk_Displacement : AbilityInstance
{
	public float postDelay;

	public float duration;

	public float magnitude;

	public float maxDistance;

	public float radius;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public DewEase ease;

	public GameObject fxOnDisplacementEnd;

	public GameObject fxTelegraph;

	public GameObject fxHit;

	public float nearAttackForwardDist = 3.5f;

	public float nearAttackInstanceAngle = 30f;

	private Channel _channel;

	private int _spawnCount;

	private int _generation;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_generation++;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		int gen = _generation;
		DestroyOnDeath(info.caster);
		_spawnCount = ((At_Mon_Sky_BossNyx_LineAtk)firstTrigger).spawnCount;
		float delay = DewResources.GetByType<Ai_Mon_Sky_BossNyx_LineAtk>(default(ResourceLoadSettings)).damageDelay;
		DewEffect.ApplySpeedMultiplierNetworked(((NetworkBehaviour)this).netIdentity, fxTelegraph, 1f / delay);
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = float.PositiveInfinity,
			onCancel = DestroyIfActive
		});
		if ((Object)(object)info.target == null)
		{
			DestroyIfActive();
		}
		Vector3 dest = AbilityTrigger.PredictPoint_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength() * 2f, info.target, duration) + Random.insideUnitCircle.ToXZ().normalized * Random.Range(0f, magnitude);
		dest = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, dest);
		Vector3 vector = dest - info.caster.agentPosition;
		Vector3 normalized = vector.normalized;
		if (vector.sqrMagnitude > maxDistance * maxDistance)
		{
			dest = info.caster.agentPosition + normalized * maxDistance;
			dest = Dew.GetValidAgentDestination_LinearSweep(info.caster.agentPosition, dest);
		}
		info.caster.Control.RotateTowards(dest, immediately: false, duration);
		info.caster.Control.StartDisplacement(new DispByDestination
		{
			affectedByMovementSpeed = true,
			canGoOverTerrain = false,
			destination = dest,
			ease = ease,
			duration = duration,
			isCanceledByCC = false,
			isFriendly = true,
			onCancel = DestroyIfActive,
			onFinish = () =>
			{
				if (gen == _generation && isActive)
				{
					((MonoBehaviour)(object)this).StartCoroutine(Routine());
					info.caster.Control.RotateTowards(dest, immediately: true);
					float num = Random.Range(0f, 360f);
					int i;
					for (i = 0; i < _spawnCount; i++)
					{
						float num2 = num + 360f / (float)_spawnCount * (float)i;
						Vector3 vector2 = Quaternion.Euler(0f, num2, 0f) * Vector3.forward;
						Vector3 point = info.caster.agentPosition + vector2 * nearAttackForwardDist;
						CreateAbilityInstance(point, Quaternion.Euler(0f, num2 + nearAttackInstanceAngle, 0f), new CastInfo(info.caster, point), (Ai_Mon_Sky_BossNyx_LineAtk b) =>
						{
							b.disableAnimations = true;
							b.NetworkdisableAudioAndShake = i >= 2;
						});
					}
				}
			},
			rotateForward = false
		});
		yield return new SI.WaitForSeconds(duration + delay);
		info.caster.Control.StartDaze(postDelay);
		DestroyIfActive();
		IEnumerator Routine()
		{
			FxPlayNetworked(fxOnDisplacementEnd, info.caster);
			FxPlayNetworked(fxTelegraph, info.caster.agentPosition, null);
			yield return new WaitForSeconds(delay);
			ListReturnHandle<Entity> handle;
			foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, info.caster.agentPosition, radius, tvDefaultHarmfulEffectTargets))
			{
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.caster.agentPosition).SetElemental(ElementalType.Light).Dispatch(item);
				FxPlayNewNetworked(fxHit, item);
				knockback.ApplyWithOrigin(info.caster.agentPosition, item);
			}
			handle.Return();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _channel != null)
		{
			_channel.Cancel();
			_channel = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
