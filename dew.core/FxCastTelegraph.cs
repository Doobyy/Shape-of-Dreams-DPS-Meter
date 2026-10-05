using System;
using UnityEngine;

[LogicUpdatePriority(1000)]
public class FxCastTelegraph : LogicBehaviour
{
	public enum PositionType
	{
		None = 2,
		Caster = 0,
		Target = 1
	}

	public enum OffsetType
	{
		WorldSpace,
		CastDirection
	}

	public enum RotationType
	{
		None = 3,
		AlwaysIdentity = 0,
		LocalRotationOnWorldSpace = 1,
		LocalRotationOnCastDirection = 2
	}

	public PositionType position = PositionType.None;

	public OffsetType offset;

	public RotationType rotation = RotationType.None;

	public bool adjustSimulationSpeed = true;

	private Vector3 _initPosition;

	private Quaternion _initRotation;

	private bool _isInitPosSet;

	public CastMethodType castMethod { get; private set; }

	public CastInfo castInfo { get; private set; }

	private void Awake()
	{
		CheckInitialPositionData();
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		UpdatePosition();
	}

	public void Setup(CastMethodType method, CastInfo info, float duration)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		castInfo = info;
		castMethod = method;
		if (duration > 0f && adjustSimulationSpeed)
		{
			ListReturnHandle<ParticleSystem> handle;
			foreach (ParticleSystem item in ((Component)this).GetComponentsInChildrenNonAlloc(out handle))
			{
				MainModule main = item.main;
				main.simulationSpeed = 1f / duration;
			}
			ListReturnHandle<BoxTelegraphController> handle2;
			foreach (BoxTelegraphController item2 in ((Component)this).GetComponentsInChildrenNonAlloc(out handle2))
			{
				item2.duration = duration;
			}
			ListReturnHandle<ArcTelegraphController> handle3;
			foreach (ArcTelegraphController item3 in ((Component)this).GetComponentsInChildrenNonAlloc(out handle3))
			{
				item3.duration = duration;
			}
			handle.Return();
			handle2.Return();
			handle3.Return();
		}
		UpdatePosition();
	}

	private void CheckInitialPositionData()
	{
		if (!_isInitPosSet)
		{
			_isInitPosSet = true;
			_initPosition = transform.localPosition;
			_initRotation = transform.localRotation;
		}
	}

	private void UpdatePosition()
	{
		try
		{
			if (position == PositionType.None && rotation == RotationType.None)
			{
				return;
			}
			CheckInitialPositionData();
			Vector3 vector = default;
			switch (position)
			{
			case PositionType.Caster:
				if ((UnityEngine.Object)(object)castInfo.caster == null)
				{
					return;
				}
				vector = castInfo.caster.position;
				break;
			case PositionType.Target:
				if (castMethod == CastMethodType.Target)
				{
					vector = castInfo.target.position;
					break;
				}
				if (castMethod == CastMethodType.Point)
				{
					vector = castInfo.point;
					break;
				}
				return;
			default:
				throw new ArgumentOutOfRangeException();
			case PositionType.None:
				break;
			}
			Quaternion quaternion;
			switch (castMethod)
			{
			case CastMethodType.None:
				if ((UnityEngine.Object)(object)castInfo.caster == null)
				{
					return;
				}
				quaternion = castInfo.caster.Control.desiredRotation;
				break;
			case CastMethodType.Cone:
			case CastMethodType.Arrow:
				quaternion = castInfo.rotation;
				break;
			case CastMethodType.Target:
				quaternion = (((UnityEngine.Object)(object)castInfo.target == null) ? castInfo.rotation : Quaternion.LookRotation(castInfo.target.position - castInfo.caster.position).Flattened());
				break;
			case CastMethodType.Point:
			{
				if ((UnityEngine.Object)(object)castInfo.caster == null)
				{
					return;
				}
				Vector3 forward = castInfo.point - castInfo.caster.position;
				quaternion = ((!(forward.sqrMagnitude < 0.001f)) ? Quaternion.LookRotation(forward).Flattened() : castInfo.caster.rotation);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
			Vector3 vector2 = offset switch
			{
				OffsetType.WorldSpace => vector + _initPosition, 
				OffsetType.CastDirection => vector + quaternion * _initPosition, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			Quaternion quaternion2 = default;
			switch (rotation)
			{
			case RotationType.AlwaysIdentity:
				quaternion2 = Quaternion.identity;
				break;
			case RotationType.LocalRotationOnCastDirection:
				quaternion2 = quaternion * _initRotation;
				break;
			case RotationType.LocalRotationOnWorldSpace:
				quaternion2 = _initRotation;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case RotationType.None:
				break;
			}
			if (position != PositionType.None)
			{
				transform.position = vector2;
			}
			if (rotation != RotationType.None)
			{
				transform.rotation = quaternion2;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
