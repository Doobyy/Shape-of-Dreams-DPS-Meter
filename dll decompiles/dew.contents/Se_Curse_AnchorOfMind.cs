using Mirror;

public class Se_Curse_AnchorOfMind : CurseStatusEffect
{
	private ActorRef<Ai_Curse_AnchorOfMind_Anchor> _anchor;

	private float _graceTime;

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (!_anchor.IsNullOrInactive() || ManagerBase<CameraManager>.instance.isPlayingCutscene || ManagerBase<TransitionManager>.instance.state == TransitionManager.StateType.Loading || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			_graceTime = 1.25f;
			return;
		}
		_graceTime -= dt;
		if (!(_graceTime > 0f) && !victim.IsNullInactiveDeadOrKnockedOut())
		{
			_anchor = CreateAbilityInstance<Ai_Curse_AnchorOfMind_Anchor>(victim.agentPosition, null, new CastInfo(victim));
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !_anchor.IsNullOrInactive())
		{
			_anchor.Get().Destroy();
			_anchor = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
