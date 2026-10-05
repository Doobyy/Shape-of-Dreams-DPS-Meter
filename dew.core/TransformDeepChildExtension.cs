using System.Collections.Generic;
using UnityEngine;

public static class TransformDeepChildExtension
{
	public static Transform FindDeepChild(this Transform aParent, string aName)
	{
		Queue<Transform> queue = new Queue<Transform>();
		queue.Enqueue(aParent);
		while (queue.Count > 0)
		{
			Transform transform = queue.Dequeue();
			if (transform.name == aName)
			{
				return transform;
			}
			foreach (Transform item in transform)
			{
				queue.Enqueue(item);
			}
		}
		return null;
	}

	public static IEnumerable<Transform> FindDeepChildren(this Transform aParent, string aName)
	{
		Queue<Transform> queue = new Queue<Transform>();
		queue.Enqueue(aParent);
		while (queue.Count > 0)
		{
			Transform c = queue.Dequeue();
			if (c.name == aName)
			{
				yield return c;
			}
			foreach (Transform item in c)
			{
				queue.Enqueue(item);
			}
		}
	}
}
