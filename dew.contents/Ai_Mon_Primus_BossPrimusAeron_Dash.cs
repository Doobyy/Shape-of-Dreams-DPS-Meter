using System;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Dash : AbilityInstance
{
	public GameObject[] fxByPhase;

	public Dash dash;

	[NonSerialized]
	public Vector3? customPoint;

	private bool _dashCached;

	private float _origDashDistance;

	private float _origDashDuration;

	private DewEase _origDashEase;

	private bool _origDashRotateForward;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (!_dashCached)
		{
			_dashCached = true;
			_origDashDistance = dash.distance;
			_origDashDuration = dash.duration;
			_origDashEase = dash.ease;
			_origDashRotateForward = dash.rotateForward;
		}
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity).DestroyOnDestroy(this);
		Mon_Primus_BossPrimusAeron mon_Primus_BossPrimusAeron = (Mon_Primus_BossPrimusAeron)info.caster;
		if (mon_Primus_BossPrimusAeron.phase == Mon_Primus_BossPrimusAeron.PhaseType.Rage)
		{
			dash.ease = DewEase.Linear;
			dash.duration *= 0.4f;
		}
		Vector3 normalized = (info.caster.AI.context.targetEnemy.GetAIAgentPosition(info.caster) - info.caster.agentPosition).normalized;
		if (customPoint.HasValue)
		{
			normalized = (customPoint.Value - info.caster.agentPosition).normalized;
		}
		else
		{
			normalized = ((mon_Primus_BossPrimusAeron.phase != Mon_Primus_BossPrimusAeron.PhaseType.Adapt) ? (Quaternion.Euler(0f, UnityEngine.Random.Range(-30f, 30f), 0f) * normalized) : (Quaternion.Euler(0f, UnityEngine.Random.Range(-90f, 90f), 0f) * normalized));
		}
		bool flag = mon_Primus_BossPrimusAeron.phase != Mon_Primus_BossPrimusAeron.PhaseType.Force;
		dash.rotateForward = !flag;
		if (flag && (UnityEngine.Object)(object)mon_Primus_BossPrimusAeron.AI.context.targetEnemy != null)
		{
			mon_Primus_BossPrimusAeron.Control.RotateTowards(mon_Primus_BossPrimusAeron.AI.context.targetEnemy, immediately: false, 1f);
		}
		if (customPoint.HasValue)
		{
			dash.distance = (customPoint.Value - info.caster.agentPosition).magnitude;
			dash.ApplyByDestination(info.caster, customPoint.Value, (DispByDestination d) =>
			{
				d.onCancel = DestroyIfActive;
				d.onFinish = DestroyIfActive;
			});
		}
		else
		{
			dash.ApplyByDirection(info.caster, normalized, (DispByDestination d) =>
			{
				d.onCancel = DestroyIfActive;
				d.onFinish = DestroyIfActive;
			});
		}
		FxPlayNetworked(fxByPhase.GetClamped((int)mon_Primus_BossPrimusAeron.phase), mon_Primus_BossPrimusAeron, position, Quaternion.LookRotation(normalized));
		ResetCooldown(mon_Primus_BossPrimusAeron.Ability.attackAbility);
		ResetCooldown(mon_Primus_BossPrimusAeron.Ability.GetAbility<At_Mon_Primus_BossPrimusAeron_Force_Swipe>());
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		customPoint = null;
		if (_dashCached)
		{
			dash.distance = _origDashDistance;
			dash.duration = _origDashDuration;
			dash.ease = _origDashEase;
			dash.rotateForward = _origDashRotateForward;
		}
	}

	private void MirrorProcessed()
	{
	}
}
