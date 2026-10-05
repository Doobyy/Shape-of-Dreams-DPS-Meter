using System;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityController : MonoBehaviour
{
	private List<IVisibilityComponent> _components = new List<IVisibilityComponent>();

	public void AddVisbilityComponent(IVisibilityComponent c)
	{
		_components.Add(c);
	}

	public void RemoveVisbilityComponent(IVisibilityComponent c)
	{
		_components.Remove(c);
	}

	public void UpdateVisibility()
	{
		bool active = true;
		for (int i = 0; i < _components.Count; i++)
		{
			if (_components[i] is UnityEngine.Object obj && !obj)
			{
				continue;
			}
			try
			{
				if (!_components[i].shouldBeShown)
				{
					active = false;
					break;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		gameObject.SetActive(active);
	}
}
