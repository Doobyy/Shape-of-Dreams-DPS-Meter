using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	public bool useFacingInsteadOfBillboard;

	private void LateUpdate()
	{
		Camera mainCamera = Dew.mainCamera;
		if (!(mainCamera == null))
		{
			Transform transform = mainCamera.transform;
			Transform transform2 = base.transform;
			if (useFacingInsteadOfBillboard)
			{
				transform2.rotation = Quaternion.LookRotation(transform2.position - transform.position);
			}
			else
			{
				transform2.rotation = transform.rotation;
			}
		}
	}
}
