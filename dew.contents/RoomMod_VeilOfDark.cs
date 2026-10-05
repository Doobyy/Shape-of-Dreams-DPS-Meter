using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomMod_VeilOfDark : RoomModifierBase
{
	public float spawnDensity;

	public float preventSpawnDistance;

	public Vector2 interval;

	public Vector2 delay;

	public Vector2 range;

	private int _maxCalCount = 10;

	public override void OnStartServer()
	{
		base.OnStartServer();
		GameManager.CallOnReady(() =>
		{
			foreach (RoomSection section in SingletonDewNetworkBehaviour<Room>.instance.sections)
			{
				((MonoBehaviour)(object)this).StartCoroutine(CreateVeilRoutine(section));
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			((MonoBehaviour)(object)this).StopAllCoroutines();
		}
	}

	private IEnumerator CreateVeilRoutine(RoomSection s)
	{
		while (true)
		{
			int spawnCount = Mathf.RoundToInt(s.area / spawnDensity);
			for (int i = 0; i < spawnCount; i++)
			{
				Vector3 vector = Vector3.zero;
				List<Vector3> list = new List<Vector3>();
				int num = 0;
				while (num < _maxCalCount)
				{
					bool flag = false;
					Vector3 anyRandomNode = s.GetAnyRandomNode();
					foreach (Vector3 item in list)
					{
						if (Vector3.Distance(anyRandomNode, item) <= preventSpawnDistance)
						{
							break;
						}
						flag = true;
					}
					num++;
					if (flag || list.Count <= 0)
					{
						list.Add(anyRandomNode);
						vector = anyRandomNode;
						break;
					}
				}
				CreateAbilityInstance(vector, null, default, (Ai_VeilOfDark b) =>
				{
					b.Networkradius = Random.Range(range.x, range.y);
				});
				yield return new WaitForSeconds(Random.Range(interval.x, interval.y));
			}
			yield return new WaitForSeconds(Random.Range(delay.x, delay.y));
		}
	}

	private void MirrorProcessed()
	{
	}
}
