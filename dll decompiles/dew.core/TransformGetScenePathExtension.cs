using UnityEngine;

public static class TransformGetScenePathExtension
{
	public static string GetScenePath(this Transform t)
	{
		string text = t.name;
		while (t.parent != null)
		{
			t = t.parent;
			text = t.name + "/" + text;
		}
		return text;
	}
}
