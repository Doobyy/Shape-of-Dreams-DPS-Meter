using Mirror;
using UnityEngine;

public class Se_U_BeamOfBalance : StatusEffect
{
	public float postDelay = 0.5f;

	public float beamDuration = 5f;

	public DewAnimationClip endAnim;

	public ChargingChannelData channel;

	private ChargingChannel _channel;

	private ActorRef<Ai_U_BeamOfBalance_Beam> _beam;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoInvulnerable();
			SetTimer(beamDuration);
			ShowOnScreenTimer();
			info.caster.Control.IncrementBlockCounters(Channel.BlockedAction.Everything);
			_channel = channel.Get(this).SetInitialInfo(info).OnCancel((ChargingChannel _) =>
			{
				DestroyIfActive();
			})
				.Dispatch(info.caster, firstTrigger);
			_beam = CreateAbilityInstance(info.point, null, info, (Ai_U_BeamOfBalance_Beam beam) =>
			{
				beam.targetPosition = info.point;
				beam.hitEndTime = Time.time + beamDuration;
			});
			_beam.Get().DestroyOnDestroy(this);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !_beam.IsNullOrInactive() && _channel.isActive)
		{
			Ai_U_BeamOfBalance_Beam ai_U_BeamOfBalance_Beam = _beam.Get();
			ai_U_BeamOfBalance_Beam.targetPosition = info.caster.agentPosition + Vector2.ClampMagnitude(_channel.castInfo.point.ToXY() - info.caster.agentPosition.ToXY(), _channel.castMethod._range).ToXZ();
			info.caster.Control.RotateTowards(ai_U_BeamOfBalance_Beam.position, immediately: false, 0.25f);
		}
	}

	protected override void OnDestroyActor()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			_beam = null;
			if (_channel != null && _channel.isActive)
			{
				_channel.Cancel();
			}
			if ((Object)(object)info.caster != null)
			{
				info.caster.Control.DecrementBlockCounters(Channel.BlockedAction.Everything);
			}
			if (!info.caster.IsNullInactiveDeadOrKnockedOut())
			{
				info.caster.Control.StartDaze(postDelay);
				info.caster.Animation.PlayAbilityAnimation(endAnim);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
