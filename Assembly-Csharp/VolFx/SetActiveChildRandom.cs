using UnityEngine;

namespace VolFx;

[AddComponentMenu("")]
public class SetActiveChildRandom : MonoBehaviour
{
	public void Invoke()
	{
		int num = Random.Range(0, transform.childCount);
		while (transform.GetChild(num).gameObject.activeSelf)
		{
			num = Random.Range(0, transform.childCount);
		}
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.SetActive(i == num);
		}
	}
}
