using System;
using System.Collections;
using HighlightPlus;
using UnityEngine;

public class EntityHighlightProvider : MeshHighlightProvider
{
	public const float HitEffectDuration = 0.1f;

	public bool disableHighlight;

	private const bool AlwaysDisplayEnemyOutline = false;

	private Entity _entity;

	private float _hitValue;

	protected override void Awake()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		_entity = GetComponent<Entity>();
		if (!((UnityEngine.Object)(object)_entity == null))
		{
			_entity.EntityEvent_OnDeath += (Action<EventInfoKill>)((EventInfoKill _) =>
			{
				meshHighlight.highlighted = false;
			});
			meshHighlight.effectGroup = (TargetOptions)5;
		}
	}

	protected override void Start()
	{
		base.Start();
		UpdateStyle();
		OnCursorStatusUpdated();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return null;
			if (!((UnityEngine.Object)(object)meshHighlight == null) && !((UnityEngine.Object)(object)_entity == null))
			{
				meshHighlight.SetTargets(((Component)(object)_entity).transform, _entity.Visual.solidRenderers.ToArray());
				meshHighlight.highlighted = true;
			}
		}
	}

	protected override void OnCursorStatusUpdated()
	{
		switch (cursorStatus)
		{
		case CursorStatus.None:
			meshHighlight.outline = 0f;
			break;
		case CursorStatus.Hover:
			meshHighlight.outline = 0.4f;
			break;
		case CursorStatus.Active:
			meshHighlight.outline = 0.6f;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (disableHighlight)
		{
			meshHighlight.outline = 0f;
		}
		meshHighlight.outlineWidth = 1.25f;
		meshHighlight.UpdateMaterialProperties();
		UpdateStyle();
	}

	protected override void OnClickTimeUpdated()
	{
		base.OnClickTimeUpdated();
		UpdateStyle();
	}

	public void ShowHit()
	{
		_hitValue = 1f;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		_hitValue = Mathf.MoveTowards(_hitValue, 0f, Time.deltaTime / 0.1f);
		meshHighlight.innerGlow = _hitValue;
	}

	private void UpdateStyle()
	{
		if (!((UnityEngine.Object)(object)_entity == null))
		{
			switch (((UnityEngine.Object)(object)DewPlayer.local == null) ? TeamRelation.Neutral : DewPlayer.local.GetTeamRelation(_entity))
			{
			case TeamRelation.Own:
				meshHighlight.outlineColor = ManagerBase<ObjectHighlightManager>.instance.own.color;
				break;
			case TeamRelation.Neutral:
				meshHighlight.outlineColor = ManagerBase<ObjectHighlightManager>.instance.neutral.color;
				break;
			case TeamRelation.Enemy:
				meshHighlight.outlineColor = ManagerBase<ObjectHighlightManager>.instance.enemy.color;
				break;
			case TeamRelation.Ally:
				meshHighlight.outlineColor = ManagerBase<ObjectHighlightManager>.instance.ally.color;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			meshHighlight.UpdateMaterialProperties();
		}
	}
}
