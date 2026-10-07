using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class Ink_AutoCraneSpawner : MonoBehaviour
{
	public GameObject craneObject;

	public Ease ease;

	public float duration;

	public float distance;

	public float startDelay;

	public float spawnInterval;

	public float failedInterval;

	public int maxTryCount;

	public Vector2 camZOffset;

	public float circleRadius;

	private void OnEnable()
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(Random.Range(0f, startDelay));
			while (true)
			{
				if (!SpawnCrane())
				{
					yield return new WaitForSeconds(failedInterval);
				}
				else
				{
					yield return new WaitForSeconds(spawnInterval);
				}
			}
		}
	}

	private bool SpawnCrane()
	{
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Expected Obj, but got Unknown
		Vector3 randomCraneSpawnPoint = GetRandomCraneSpawnPoint();
		if (randomCraneSpawnPoint.sqrMagnitude == 0f)
		{
			return false;
		}
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		int i = 0;
		bool flag = false;
		for (; i < maxTryCount; i++)
		{
			vector = Random.insideUnitCircle.ToXZ().normalized;
			vector2 = randomCraneSpawnPoint - vector * distance / 2f;
			if (!Physics.Raycast(vector2, vector, distance, LayerMask.GetMask("Ground")))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		Vector3 vector3 = vector2 + vector * distance;
		GameObject crane = Object.Instantiate(craneObject, vector2, Quaternion.LookRotation(vector));
		Vector3 localScale = crane.transform.localScale;
		crane.transform.localScale = Vector3.zero;
		TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(DOTween.Sequence(), (Tween)(object)ShortcutExtensions.DOScale(crane.transform, localScale, 0.5f)), (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(crane.transform, 0.01f, duration), ease));
		TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.Append(DOTween.Sequence(), (Tween)(object)ShortcutExtensions.DOMove(crane.transform, vector3, duration, false)), (TweenCallback)(() =>
		{
			Object.Destroy(crane);
		}));
		return true;
	}

	private Vector3 GetRandomCraneSpawnPoint()
	{
		if (ManagerBase<DewCamera>.softInstance == null)
		{
			return Vector3.zero;
		}
		Transform transform = ManagerBase<DewCamera>.softInstance.mainCamera.transform;
		Vector3 forward = transform.forward;
		Vector3 center = transform.position + forward * Random.Range(camZOffset.x, camZOffset.y);
		return GetRandomPointInCircle(center, forward, circleRadius);
	}

	private Vector3 GetRandomPointInCircle(Vector3 center, Vector3 normal, float radius)
	{
		Vector2 insideUnitCircle = Random.insideUnitCircle;
		Vector3 normalized = Vector3.Cross(normal, Vector3.up).normalized;
		Vector3 normalized2 = Vector3.Cross(normal, normalized).normalized;
		return center + (normalized * insideUnitCircle.x + normalized2 * insideUnitCircle.y) * radius;
	}
}
