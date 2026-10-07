using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Q_DeathMark_Marked : StatusEffect
{
	public float lingerDuration = 1.5f;

	public float dashSpeed = 30f;

	public DewBeamRenderer beam;

	public GameObject fxBlinkOnCaster;

	private AbilityTrigger.ChangedConfigHandle _handle;

	private Vector3 _lastTargetPosition;

	private float _baseLingerDuration;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseLingerDuration = lingerDuration;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		lingerDuration = _baseLingerDuration;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		_lastTargetPosition = info.target.agentPosition;
		_handle = firstTrigger.ChangeConfigTimedOnce(1, lingerDuration, (EventInfoAbilityInstance ai) =>
		{
			FxPlayNewNetworked(fxBlinkOnCaster, info.caster);
			CreateBasicEffect(info.caster, new UncollidableEffect(), 0.25f);
			CreateBasicEffect(info.caster, new UnstoppableEffect(), 0.25f);
			Vector3 end = _lastTargetPosition + (_lastTargetPosition - info.caster.agentPosition).normalized * 1.5f;
			end = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, end);
			Vector3 vector = (end - info.caster.agentPosition).Flattened();
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				destination = end,
				duration = Mathf.Max(vector.magnitude / dashSpeed, 0.01f),
				ease = DewEase.Linear,
				isFriendly = true,
				rotateForward = false,
				canGoOverTerrain = true,
				isCanceledByCC = false
			});
			if (info.target.IsNullOrInactive())
			{
				info.caster.Control.RotateTowards(_lastTargetPosition, immediately: false, 1f);
			}
			else
			{
				info.caster.Control.RotateTowards(info.target, immediately: false, 1f);
			}
			DestroyIfActive();
		}, DestroyIfActive);
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (((NetworkBehaviour)info.caster.owner).isLocalPlayer)
		{
			beam.SetPoints(info.caster.Visual.GetCenterPosition(), victim.Visual.GetCenterPosition());
			beam.enabled = ((NetworkBehaviour)info.caster.owner).isLocalPlayer;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !info.target.IsNullInactiveDeadOrKnockedOut())
		{
			_lastTargetPosition = info.target.agentPosition;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		beam.enabled = false;
		if (((NetworkBehaviour)this).isServer && _handle != null && _handle.isActive)
		{
			_handle.Stop();
			_handle = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
