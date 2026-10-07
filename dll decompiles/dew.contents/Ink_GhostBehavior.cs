using UnityEngine;

public class Ink_GhostBehavior : MonoBehaviour
{
	public float minAnimSpeed;

	public float maxAnimSpeed;

	private float _currentVelocity;

	private void OnEnable()
	{
		transform.localScale = Vector3.one * Random.Range(1f, 2.5f);
		GetComponentInChildren<Animator>().speed = Random.Range(minAnimSpeed, maxAnimSpeed);
	}

	private void Update()
	{
		if (!(ManagerBase<CameraManager>.instance == null))
		{
			Entity focusedEntity = ManagerBase<CameraManager>.softInstance.focusedEntity;
			if (!((Object)(object)focusedEntity == null))
			{
				Vector3 forward = focusedEntity.position - transform.position;
				forward.y = 0f;
				float y = Quaternion.LookRotation(forward).eulerAngles.y;
				Vector3 eulerAngles = transform.rotation.eulerAngles;
				eulerAngles.y = Mathf.SmoothDampAngle(eulerAngles.y, y, ref _currentVelocity, 0.75f);
				transform.rotation = Quaternion.Euler(eulerAngles);
			}
		}
	}
}
