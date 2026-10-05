using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

public class RoomMod_FireDevil : RoomModifierBase
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			int b = Mathf.Max(2, Mathf.RoundToInt(SingletonDewNetworkBehaviour<Room>.instance.map.mapData.area / 150f));
			int a = Mathf.RoundToInt(2.01f + 1f * (float)DewPlayer.gamePlayers.Count);
			a = Mathf.Min(a, b);
			for (int i = 0; i < a; i++)
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			}
		});
		static void GetStartEndPos(out Vector3 start, out Vector3 end)
		{
			start = Vector3.zero;
			end = Vector3.zero;
			for (int i = 0; i < 100; i++)
			{
				Vector3 randomWorldPosition = SingletonDewNetworkBehaviour<Room>.instance.sections[Random.Range(0, SingletonDewNetworkBehaviour<Room>.instance.sections.Count)].GetRandomWorldPosition();
				float closestHeroDistance = Dew.GetClosestHeroDistance(randomWorldPosition);
				if (!(closestHeroDistance < 4f) && !(closestHeroDistance > 20f))
				{
					start = Dew.GetPositionOnGround(randomWorldPosition);
					end = start + Random.insideUnitSphere.Flattened().normalized * 20f;
					end = Dew.GetPositionOnGround(end);
					end = Dew.GetValidAgentDestination_LinearSweep(start, end);
					if (!(Vector3.Distance(end, start) < 5f) && (SingletonDewNetworkBehaviour<Room>.instance.didClearRoom || !(Dew.GetClosestHeroDistance(end) > 8f)) && (SingletonDewNetworkBehaviour<Room>.instance.didClearRoom || !((Object)(object)LavaLand_Lava.instance != null) || (!LavaLand_Lava.instance.IsPositionOnLava(start) && !LavaLand_Lava.instance.IsPositionOnLava(end))))
					{
						return;
					}
				}
			}
			Debug.Log("Using sub-optimal pos for tornado");
		}
		IEnumerator Routine()
		{
			while (true)
			{
				yield return new WaitForSeconds(Random.Range(0.5f, 2f));
				if (SingletonDewNetworkBehaviour<Room>.instance.didClearRoom || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition || this.IsNullOrInactive())
				{
					break;
				}
				GetStartEndPos(out var start, out var end);
				Ai_RoomMod_FireDevil_Tornado instance = CreateAbilityInstance(start, null, default, (Ai_RoomMod_FireDevil_Tornado ai) =>
				{
					ai.NetworkendPos = end;
				});
				yield return new WaitWhile(() => !instance.IsNullOrInactive());
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Actor[] array = NetworkedManagerBase<ActorManager>.instance.allActors.ToArray();
		foreach (Actor actor in array)
		{
			if (actor is Ai_RoomMod_FireDevil_Tornado)
			{
				actor.Destroy();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
