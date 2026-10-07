using UnityEngine;

public static class GameObjectExtensions
{
	public static void SetActiveAll(this GameObject[] gobjs, bool value)
	{
		foreach (GameObject gameObject in gobjs)
		{
			if (!(gameObject == null))
			{
				gameObject.SetActive(value);
			}
		}
	}
}
