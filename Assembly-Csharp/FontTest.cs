using UnityEngine;

public class FontTest : MonoBehaviour
{
	public GameObject flickerObject;

	private void Update()
	{
		flickerObject.SetActive(value: false);
		flickerObject.SetActive(value: true);
	}
}
