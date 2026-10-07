using System.Collections;
using UnityEngine;

public class Despair_FogPositionController : DewNetworkBehaviour
{
	[SerializeField]
	private Transform[] fogs;

	private bool _positionCheckEnable;

	private int _count;

	private float[] _fogBaseY;

	private bool _isTeleporting;

	private float _targetBaseY;

	public override void OnStart()
	{
		base.OnStart();
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitUntil(() => (Object)(object)NetworkedManagerBase<GameManager>.instance != null);
			GameManager.CallOnReady(() =>
			{
				_fogBaseY = new float[fogs.Length];
				for (int i = 0; i < fogs.Length; i++)
				{
					_fogBaseY[i] = fogs[i].position.y;
				}
				Entity focusedEntity = ManagerBase<CameraManager>.instance.focusedEntity;
				_targetBaseY = focusedEntity.position.y;
				_positionCheckEnable = true;
			});
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!_positionCheckEnable)
		{
			return;
		}
		float num = 0f;
		if (!_isTeleporting)
		{
			num = ManagerBase<CameraManager>.instance.focusedEntity.agentPosition.y - _targetBaseY;
			for (int i = 0; i < fogs.Length; i++)
			{
				fogs[i].position = new Vector3(fogs[i].position.x, _fogBaseY[i] + num, fogs[i].position.z);
			}
		}
		else
		{
			num = ManagerBase<CameraManager>.instance.focusedEntity.position.y - _targetBaseY;
			for (int j = 0; j < fogs.Length; j++)
			{
				fogs[j].position = new Vector3(fogs[j].position.x, _fogBaseY[j] + num, fogs[j].position.z);
			}
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (_positionCheckEnable)
		{
			Entity focusedEntity = ManagerBase<CameraManager>.instance.focusedEntity;
			_isTeleporting = focusedEntity.Status.HasStatusEffect<Se_Shrine_Despair_Teleport>();
		}
	}

	private void MirrorProcessed()
	{
	}
}
