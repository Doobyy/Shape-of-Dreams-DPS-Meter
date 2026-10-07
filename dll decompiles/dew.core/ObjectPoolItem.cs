using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolItem
{
	public List<GameObject> inactiveInstances = new List<GameObject>();

	public int activeInstances;

	public int peakActiveInstances;

	public int peakActiveThisRoom;

	public bool isMonster;

	public bool isPickup;

	public float lastInstantiateUnscaledTime;

	public float lastReuseUnscaledTime;

	public float lastDestroyUnscaledTime;

	public bool reuseInRoom;

	public int roomsSinceUsed;

	public int maxActiveObserved;
}
