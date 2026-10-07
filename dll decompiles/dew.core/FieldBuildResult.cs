using System;
using UnityEngine;

public class FieldBuildResult
{
	public Func<object> getValue;

	public Action<object> setValue;

	public SafeAction<object> onChanged = new SafeAction<object>();

	public GameObject root;
}
