using System.Collections.Generic;
using UnityEngine;

public class DarkCave_BlinkPosition : MonoBehaviour
{
	public static List<DarkCave_BlinkPosition> instances = new List<DarkCave_BlinkPosition>();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInit()
	{
		instances = new List<DarkCave_BlinkPosition>();
	}

	private void Awake()
	{
		instances.Add(this);
	}

	private void OnDestroy()
	{
		instances.Remove(this);
	}
}
