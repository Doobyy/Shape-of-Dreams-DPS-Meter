using System;
using DG.Tweening;
using UnityEngine;

public class EntityGroundMarker : MonoBehaviour
{
	public Vector3 offset;

	public GameObject ownObject;

	public GameObject enemyObject;

	public GameObject neutralObject;

	public GameObject allyObject;

	private Entity _entity;

	private Vector3 _lastAgentPos = new Vector3(float.NaN, 0f, 0f);

	private Transform _transform;

	private Action<Entity, Entity> _onSelectedEntityChanged;

	private const int GroundYUpdateInterval = 3;

	private static int _phaseDistributor;

	private int _groundYCounter;

	private float _cachedGroundY = float.NaN;

	private void Start()
	{
		_transform = transform;
		_groundYCounter = _phaseDistributor++ % 3;
		_entity = GetComponentInParent<Entity>();
		_entity.ClientActorEvent_OnDestroyed += new Action<Actor>(ClientActorEventOnDestroyed);
		_entity.Visual.ClientEvent_OnRendererEnabledChanged += new Action<bool>(ClientEventOnRendererEnabledChanged);
		_entity.Control.ClientEvent_OnOuterRadiusChanged += new Action<float, float>(ClientEventOnOuterRadiusChanged);
		_entity.Visual.ClientEvent_OnGroundMarkerHiddenChanged += new Action<bool>(ClientEventOnGroundMarkerHiddenChanged);
		if (_entity is Hero hero)
		{
			hero.ClientHeroEvent_OnKnockedOut += new Action<EventInfoKill>(ClientHeroEventOnKnockedOut);
			hero.ClientHeroEvent_OnRevive += new Action<Hero>(ClientHeroEventOnRevive);
		}
		_onSelectedEntityChanged = OnSelectedEntityChanged;
		ControlManager instance = ManagerBase<ControlManager>.instance;
		instance.onSelectedEntityChanged = (Action<Entity, Entity>)Delegate.Combine(instance.onSelectedEntityChanged, _onSelectedEntityChanged);
		transform.position = _entity.agentPosition + offset;
		transform.localScale = Vector3.zero;
		UpdateScale();
		UpdateVisibility();
	}

	private void OnDestroy()
	{
		if (ManagerBase<ControlManager>.instance != null)
		{
			ControlManager instance = ManagerBase<ControlManager>.instance;
			instance.onSelectedEntityChanged = (Action<Entity, Entity>)Delegate.Remove(instance.onSelectedEntityChanged, _onSelectedEntityChanged);
		}
	}

	private void ClientEventOnGroundMarkerHiddenChanged(bool obj)
	{
		UpdateScale();
	}

	private void ClientEventOnRendererEnabledChanged(bool obj)
	{
		UpdateVisibility();
	}

	private void ClientActorEventOnDestroyed(Actor obj)
	{
		UpdateVisibility();
	}

	private void OnSelectedEntityChanged(Entity arg1, Entity arg2)
	{
		UpdateVisibility();
	}

	private void ClientEventOnOuterRadiusChanged(float arg1, float arg2)
	{
		UpdateScale();
	}

	private void ClientHeroEventOnRevive(Hero obj)
	{
		UpdateVisibility();
	}

	private void ClientHeroEventOnKnockedOut(EventInfoKill obj)
	{
		UpdateVisibility();
	}

	private void Update()
	{
		if (!_entity.IsNullOrInactive())
		{
			UpdatePosition();
		}
	}

	private void UpdateVisibility()
	{
		if (_entity.IsNullInactiveDeadOrKnockedOut() || _entity.Visual.isRendererOff || (UnityEngine.Object)(object)DewPlayer.local == null)
		{
			ownObject.SetActive(value: false);
			enemyObject.SetActive(value: false);
			neutralObject.SetActive(value: false);
			allyObject.SetActive(value: false);
		}
		else
		{
			TeamRelation teamRelation = DewPlayer.local.GetTeamRelation(_entity);
			ownObject.SetActive(teamRelation == TeamRelation.Own);
			enemyObject.SetActive(teamRelation == TeamRelation.Enemy);
			neutralObject.SetActive(teamRelation == TeamRelation.Neutral);
			allyObject.SetActive(teamRelation == TeamRelation.Ally);
		}
	}

	private void UpdateScale()
	{
		float spawnDuration = _entity.Visual.spawnDuration;
		ShortcutExtensions.DOKill((Component)transform, false);
		float num = _entity.Control.outerRadius;
		if (_entity is Hero)
		{
			num = num * 1.025f + 0.05f;
		}
		if (_entity.Visual.isGroundMarkerHidden)
		{
			num = 0f;
		}
		ShortcutExtensions.DOScale(transform, Vector3.one * num, (spawnDuration < 0.5f) ? 0.5f : spawnDuration);
	}

	private void UpdatePosition()
	{
		Vector3 agentPosition = _entity.Control.agentPosition;
		if (float.IsNaN(_lastAgentPos.x) || (agentPosition - _lastAgentPos).sqrMagnitude > 0.0001f)
		{
			_lastAgentPos = agentPosition;
			if (float.IsNaN(_cachedGroundY) || ++_groundYCounter >= 3)
			{
				_groundYCounter = 0;
				EntityVisual visual = _entity.Visual;
				Transform modelTransform = visual.modelTransform;
				Vector3 position = ((modelTransform != null) ? modelTransform.position : visual.GetBasePosition());
				position.x = agentPosition.x;
				position.z = agentPosition.z;
				_cachedGroundY = Dew.GetPositionOnGround(position).y;
			}
			_transform.position = new Vector3(agentPosition.x + offset.x, _cachedGroundY + offset.y, agentPosition.z + offset.z);
		}
		_transform.rotation = Quaternion.Euler(0f, (Time.time - _entity.creationTime) * 20f, 0f);
	}
}
