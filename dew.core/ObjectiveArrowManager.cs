using System;
using UnityEngine;

[LogicUpdatePriority(3000)]
public class ObjectiveArrowManager : ManagerBase<ObjectiveArrowManager>
{
	public Vector3? objectivePosition;

	public GameObject arrowPivot;

	private void Start()
	{
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnIsInTransitionChanged += new Action<bool>(ClientEventOnIsInTransitionChanged);
	}

	private void ClientEventOnIsInTransitionChanged(bool obj)
	{
		if (obj)
		{
			objectivePosition = null;
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		Entity focusedEntity = ManagerBase<CameraManager>.softInstance.focusedEntity;
		bool flag = !focusedEntity.IsNullInactiveDeadOrKnockedOut() && objectivePosition.HasValue;
		arrowPivot.SetActive(flag);
		if (flag)
		{
			Quaternion rotation = Quaternion.LookRotation(objectivePosition.Value - focusedEntity.position).Flattened();
			arrowPivot.transform.SetPositionAndRotation(focusedEntity.position, rotation);
		}
	}
}
