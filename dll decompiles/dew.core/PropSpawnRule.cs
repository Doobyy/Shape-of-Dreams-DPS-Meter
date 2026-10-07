using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Prop Spawn Rule", menuName = "Prop Spawn Rule")]
public class PropSpawnRule : ScriptableObject
{
	[Serializable]
	public class PropEntry
	{
		public AssetRef<GameObject> prop;

		public float chance = 1f;

		public Vector2Int count = new Vector2Int(1, 1);

		public Vector2Int roomIndexRange = new Vector2Int(0, int.MaxValue);
	}

	public List<PropEntry> entries;
}
