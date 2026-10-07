using System.Collections;
using UnityEngine;

public class Extras_PlayX4VisualArtShowcase : MonoBehaviour
{
	public Transform[] characters;

	private Vector3 _ogPosition;

	private Quaternion _ogRotation;

	private void Awake()
	{
		_ogPosition = ManagerBase<DewCamera>.instance.mainCamera.transform.position;
		_ogRotation = ManagerBase<DewCamera>.instance.mainCamera.transform.rotation;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			for (int i = 0; i < characters.Length; i++)
			{
				characters[i].transform.position += i * Vector3.right * 5f;
			}
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			StopAllCoroutines();
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			Transform[] array = characters;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].gameObject.SetActive(value: false);
			}
			Transform[] array2 = characters;
			foreach (Transform current in array2)
			{
				current.gameObject.SetActive(value: true);
				ManagerBase<DewCamera>.instance.mainCamera.transform.position = _ogPosition;
				ManagerBase<DewCamera>.instance.mainCamera.transform.rotation = _ogRotation;
				Time.timeScale = 5f;
				yield return new WaitForSecondsRealtime(1f);
				float startTime = Time.unscaledTime;
				Time.timeScale = 0f;
				while (Time.unscaledTime - startTime < 60f && !Input.GetKeyDown(KeyCode.S))
				{
					ManagerBase<DewCamera>.instance.mainCamera.transform.RotateAround(Vector3.zero, Vector3.up, (Input.GetKey(KeyCode.M) ? 1000f : 360f) / 30f * Time.unscaledDeltaTime);
					yield return null;
				}
				current.gameObject.SetActive(value: false);
			}
		}
	}
}
