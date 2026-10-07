using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameMod_CorruptedChaos : GameModifierBase
{
	public int minZoneIndex = 1;

	public float corruptChancePerZone = 0.25f;

	public Vector2Int corruptedRoomsRange = new Vector2Int(1, 3);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.onWorldGenerated += new Action(OnWorldGenerated);
		}
	}

	private void OnWorldGenerated()
	{
		if (NetworkedManagerBase<ZoneManager>.instance.currentZone == null || NetworkedManagerBase<ZoneManager>.instance.currentZone.useSpecialGeneration)
		{
			return;
		}
		int count = NetworkedManagerBase<ZoneManager>.instance.nodes.Count;
		if (count < 3 || UnityEngine.Random.value > corruptChancePerZone || NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex < minZoneIndex)
		{
			return;
		}
		int num = UnityEngine.Random.Range(corruptedRoomsRange.x, corruptedRoomsRange.y + 1);
		int hunterStartNodeIndex = NetworkedManagerBase<ZoneManager>.instance.hunterStartNodeIndex;
		int num2 = NetworkedManagerBase<ZoneManager>.instance.nodes.FindIndex((Predicate<WorldNodeData>)((WorldNodeData n) => n.type == WorldNodeType.ExitBoss));
		List<int> list = new List<int>();
		for (int num3 = 0; num3 < count; num3++)
		{
			if (hunterStartNodeIndex != num3)
			{
				WorldNodeData worldNodeData = NetworkedManagerBase<ZoneManager>.instance.nodes[num3];
				if (worldNodeData.type == WorldNodeType.Combat && !worldNodeData.HasMainModifier())
				{
					list.Add(num3);
				}
			}
		}
		list.Shuffle();
		List<int> list2 = new List<int>();
		Queue<int> queue = new Queue<int>();
		bool[] array = new bool[count];
		for (int num4 = 0; num4 < list.Count; num4++)
		{
			int num5 = list[num4];
			list2.Clear();
			for (int num6 = 0; num6 < count; num6++)
			{
				if (hunterStartNodeIndex != num6)
				{
					WorldNodeData worldNodeData2 = NetworkedManagerBase<ZoneManager>.instance.nodes[num6];
					if (worldNodeData2.type == WorldNodeType.Combat && !worldNodeData2.HasMainModifier() && NetworkedManagerBase<ZoneManager>.instance.GetNodeDistance(num5, num6) == 1)
					{
						list2.Add(num6);
					}
				}
			}
			if (list2.Count < num - 1)
			{
				continue;
			}
			bool flag = false;
			for (int num7 = 0; num7 < 30; num7++)
			{
				list2.Shuffle();
				Array.Clear(array, 0, array.Length);
				queue.Clear();
				array[num5] = true;
				for (int num8 = 0; num8 < num - 1; num8++)
				{
					array[list2[num8]] = true;
				}
				if (array[hunterStartNodeIndex] || array[num2])
				{
					continue;
				}
				queue.Enqueue(hunterStartNodeIndex);
				array[hunterStartNodeIndex] = true;
				while (queue.Count > 0)
				{
					int num9 = queue.Dequeue();
					if (num9 == num2)
					{
						flag = true;
						break;
					}
					for (int num10 = 0; num10 < count; num10++)
					{
						if (!array[num10] && NetworkedManagerBase<ZoneManager>.instance.IsNodeConnected(num9, num10))
						{
							array[num10] = true;
							queue.Enqueue(num10);
						}
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				NetworkedManagerBase<ZoneManager>.instance.AddModifier<RoomMod_CorruptedChaos_AuraOfPain>(num5);
				for (int num11 = 0; num11 < num - 1; num11++)
				{
					NetworkedManagerBase<ZoneManager>.instance.AddModifier<RoomMod_CorruptedChaos_AuraOfPain>(list2[num11]);
				}
				break;
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.onWorldGenerated -= new Action(OnWorldGenerated);
		}
	}

	private void MirrorProcessed()
	{
	}
}
