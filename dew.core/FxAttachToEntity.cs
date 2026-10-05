using System;
using UnityEngine;

[LogicUpdatePriority(1000)]
public class FxAttachToEntity : LogicBehaviour, IAttachableToEntity
{
	public enum PositionType
	{
		Base,
		Center,
		Above,
		Head,
		LeftHand,
		RightHand,
		LeftFoot,
		RightFoot,
		Weapon,
		Muzzle,
		Conversation
	}

	public enum OffsetType
	{
		None,
		WorldSpace,
		LocalSpace
	}

	public enum RotationType
	{
		DoNothing,
		AlwaysIdentity,
		PreserveRotation,
		PreserveLocalRotation,
		CameraRotation
	}

	public enum ScaleType
	{
		DoNothing,
		EntityRadiusHeight,
		EntityXYZ,
		EntityRadius,
		EntityOuterRadius
	}

	public PositionType position = PositionType.Center;

	public OffsetType offset;

	public RotationType rotation = RotationType.AlwaysIdentity;

	public bool useFlatRotation;

	public ScaleType scale;

	private Transform _entityTransform;

	[SerializeField]
	[HideInInspector]
	private Vector3 _localPosition;

	[SerializeField]
	[HideInInspector]
	private Quaternion _localRotation;

	[SerializeField]
	[HideInInspector]
	private Vector3 _localScale;

	[SerializeField]
	[HideInInspector]
	private bool _isLocalPositionSet;

	private const float kUpdateInterval = 1f / 160f;

	private float _lastUpdateTime = float.NegativeInfinity;

	private Transform _tr;

	public Entity targetEntity { get; private set; }

	private void Awake()
	{
		MakeSureLocalPositionSet();
		_tr = transform;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		float unscaledTime = Time.unscaledTime;
		if (!(unscaledTime - _lastUpdateTime < 1f / 160f))
		{
			_lastUpdateTime = unscaledTime;
			UpdatePosition();
		}
	}

	public void OnAttachToEntity(Entity target)
	{
		if ((UnityEngine.Object)(object)targetEntity != (UnityEngine.Object)(object)target)
		{
			NotifyDetach();
		}
		targetEntity = target;
		if ((bool)(UnityEngine.Object)(object)targetEntity)
		{
			_entityTransform = ((Component)(object)target).transform;
		}
		if (isActiveAndEnabled)
		{
			NotifyAttach();
		}
		if (_tr == null)
		{
			_tr = transform;
		}
		UpdatePosition();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		NotifyAttach();
		UpdatePosition();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		NotifyDetach();
	}

	private void NotifyAttach()
	{
		if ((UnityEngine.Object)(object)targetEntity != null && (UnityEngine.Object)(object)targetEntity.Visual != null && targetEntity.Visual._attachedEffects != null && !targetEntity.Visual._attachedEffects.Contains(this))
		{
			targetEntity.Visual._attachedEffects.Add(this);
		}
	}

	private void NotifyDetach()
	{
		if ((UnityEngine.Object)(object)targetEntity != null && (UnityEngine.Object)(object)targetEntity.Visual != null && targetEntity.Visual._attachedEffects != null)
		{
			targetEntity.Visual._attachedEffects.Remove(this);
		}
	}

	private void OnDestroy()
	{
		NotifyDetach();
		targetEntity = null;
	}

	private void MakeSureLocalPositionSet()
	{
		if (!_isLocalPositionSet)
		{
			_isLocalPositionSet = true;
			_localPosition = transform.localPosition;
			_localRotation = transform.localRotation;
			_localScale = transform.localScale;
		}
	}

	private void UpdatePosition()
	{
		CameraManager softInstance = ManagerBase<CameraManager>.softInstance;
		Entity entity = targetEntity;
		if (softInstance == null || (UnityEngine.Object)(object)entity == null)
		{
			return;
		}
		if (_tr == null)
		{
			_tr = transform;
		}
		MakeSureLocalPositionSet();
		EntityVisual visual = entity.Visual;
		bool flag = offset == OffsetType.LocalSpace || rotation == RotationType.PreserveLocalRotation;
		Quaternion quaternion = default;
		Vector3 vector;
		switch (position)
		{
		case PositionType.Base:
			vector = visual.GetBasePosition();
			if (flag)
			{
				quaternion = _entityTransform.rotation;
			}
			break;
		case PositionType.Center:
			vector = visual.GetCenterPosition();
			if (flag)
			{
				quaternion = _entityTransform.rotation;
			}
			break;
		case PositionType.Above:
			vector = visual.GetAbovePosition();
			if (flag)
			{
				quaternion = _entityTransform.rotation;
			}
			break;
		case PositionType.Muzzle:
			vector = visual.GetMuzzlePosition();
			if (flag)
			{
				quaternion = visual.GetMuzzleRotation();
			}
			break;
		case PositionType.Weapon:
			vector = visual.GetWeaponPosition();
			if (flag)
			{
				quaternion = visual.GetWeaponRotation();
			}
			break;
		case PositionType.Head:
			vector = visual.GetBonePosition((HumanBodyBones)10);
			if (flag)
			{
				quaternion = visual.GetBoneRotation((HumanBodyBones)10);
			}
			break;
		case PositionType.LeftHand:
			vector = visual.GetBonePosition((HumanBodyBones)17);
			if (flag)
			{
				quaternion = visual.GetBoneRotation((HumanBodyBones)17);
			}
			break;
		case PositionType.RightHand:
			vector = visual.GetBonePosition((HumanBodyBones)18);
			if (flag)
			{
				quaternion = visual.GetBoneRotation((HumanBodyBones)18);
			}
			break;
		case PositionType.LeftFoot:
			vector = visual.GetBonePosition((HumanBodyBones)5);
			if (flag)
			{
				quaternion = visual.GetBoneRotation((HumanBodyBones)5);
			}
			break;
		case PositionType.RightFoot:
			vector = visual.GetBonePosition((HumanBodyBones)6);
			if (flag)
			{
				quaternion = visual.GetBoneRotation((HumanBodyBones)6);
			}
			break;
		case PositionType.Conversation:
			vector = visual.GetConversationPivotPosition();
			if (flag)
			{
				quaternion = _entityTransform.rotation;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		switch (offset)
		{
		case OffsetType.LocalSpace:
			vector += quaternion * _localPosition;
			break;
		case OffsetType.WorldSpace:
			vector += softInstance.entityCamAngleRotation * _localPosition;
			break;
		}
		switch (scale)
		{
		case ScaleType.EntityRadiusHeight:
		{
			Vector3 size3 = visual.GetBodyBounds().size;
			float num2 = Mathf.Min(size3.x, size3.z);
			_tr.localScale = new Vector3(_localScale.x * num2, _localScale.y * size3.y, _localScale.z * num2);
			break;
		}
		case ScaleType.EntityXYZ:
		{
			Vector3 size2 = visual.GetBodyBounds().size;
			_tr.localScale = new Vector3(_localScale.x * size2.x, _localScale.y * size2.y, _localScale.z * size2.z);
			break;
		}
		case ScaleType.EntityRadius:
		{
			Vector3 size = visual.GetBodyBounds().size;
			float num = Mathf.Min(size.x, size.z);
			_tr.localScale = new Vector3(_localScale.x * num, _localScale.y * num, _localScale.z * num);
			break;
		}
		case ScaleType.EntityOuterRadius:
		{
			float outerRadius = entity.Control.outerRadius;
			_tr.localScale = new Vector3(_localScale.x * outerRadius, _localScale.y * outerRadius, _localScale.z * outerRadius);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case ScaleType.DoNothing:
			break;
		}
		if (rotation == RotationType.DoNothing && !useFlatRotation)
		{
			_tr.position = vector;
			return;
		}
		Quaternion quaternion2 = rotation switch
		{
			RotationType.AlwaysIdentity => softInstance.entityCamAngleRotation, 
			RotationType.PreserveRotation => _localRotation, 
			RotationType.PreserveLocalRotation => quaternion * _localRotation, 
			RotationType.DoNothing => _tr.rotation, 
			RotationType.CameraRotation => Dew.mainCamera.transform.rotation, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (useFlatRotation)
		{
			quaternion2 = Quaternion.Euler(0f, quaternion2.eulerAngles.y, 0f);
		}
		_tr.SetPositionAndRotation(vector, quaternion2);
	}
}
