using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_AltTeleport : AbilityInstance
{
	public float postDelay;

	public float awayDis;

	public float teleportDelay;

	public float teleportDuration;

	public GameObject fxTeleportStart;

	public GameObject fxTeleportEnd;

	[Space(15f)]
	public int lineAtkCount;

	public float lineAtkAngle;

	public float lineAtkChance;

	public float pullAtkChance;

	private int _maxTeleportCount;

	private float _baseTeleportDuration;

	private Channel _channel;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseTeleportDuration = teleportDuration;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		teleportDuration = _baseTeleportDuration;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_channel = info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = float.PositiveInfinity,
			onCancel = DestroyIfActive
		});
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		_maxTeleportCount = ((At_Mon_Sky_BossNyx_AltTeleport)firstTrigger).maxTeleportCount;
		int teleportCount = Random.Range(1, _maxTeleportCount + 1);
		for (int i = 0; i < teleportCount; i++)
		{
			info.caster.Visual.DisableRenderers();
			Hero hero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
			Vector3 aIAgentPosition = hero.GetAIAgentPosition(info.caster);
			Vector3 normalized = (aIAgentPosition - info.caster.agentPosition).normalized;
			Vector3 vector = aIAgentPosition + normalized * awayDis;
			vector = Dew.GetPositionOnGround(vector);
			vector = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, vector);
			Teleport(info.caster, vector);
			info.caster.Control.RotateTowards(hero, immediately: true, 0.5f);
			info.caster.AI.Aggro(hero);
			yield return new SI.WaitForSeconds(teleportDuration);
			FxPlayNetworked(fxTeleportEnd, info.caster);
			info.caster.Visual.EnableRenderers();
			if (i != teleportCount - 1)
			{
				FxPlayNetworked(fxTeleportStart, info.caster);
				yield return new SI.WaitForSeconds(teleportDelay);
			}
		}
		if (Random.value < lineAtkChance)
		{
			float num = lineAtkAngle / (float)lineAtkCount;
			float num2 = (0f - lineAtkAngle) / 2f;
			int j;
			for (j = 0; j < lineAtkCount; j++)
			{
				CreateAbilityInstance(info.caster.agentPosition, info.caster.rotation * Quaternion.Euler(0f, num2 + num * (float)j, 0f), new CastInfo(info.caster), (Ai_Mon_Sky_BossNyx_LineAtk b) =>
				{
					b.disableAnimations = j >= 2;
					b.NetworkdisableAudioAndShake = j >= 2;
				});
			}
		}
		else if (Random.value < pullAtkChance)
		{
			Hero closestAliveHero = Dew.GetClosestAliveHero(info.caster.agentPosition, fallbackToDead: true, info.caster);
			CreateAbilityInstance<Ai_Mon_Sky_BossNyx_PullAtk>(info.caster.agentPosition, null, new CastInfo(info.caster, closestAliveHero));
		}
		else
		{
			CreateAbilityInstance<Ai_Mon_Sky_BossNyx_PullAtk_AfterAtk>(info.caster.agentPosition, null, new CastInfo(info.caster));
		}
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
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
