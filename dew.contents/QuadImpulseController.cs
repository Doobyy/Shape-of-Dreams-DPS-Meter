using UnityEngine;

public class QuadImpulseController : MonoBehaviour
{
	private Camera _mainCamera;

	private void OnEnable()
	{
		if (ManagerBase<DewCamera>.instance != null)
		{
			_mainCamera = ManagerBase<DewCamera>.instance.mainCamera;
			transform.parent = _mainCamera.transform;
		}
	}
}
