using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RoomMod_GazeOfErebos : RoomModifierBase
{
	public float initDamage;

	public float tickDamage;

	public Vector2 interval;

	public Vector2 initDelay;

	public override void OnStartServer()
	{
		base.OnStartServer();
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			int b = Mathf.Max(2, Mathf.RoundToInt(SingletonDewNetworkBehaviour<Room>.instance.map.mapData.area / 150f));
			int gazes = Mathf.RoundToInt(2.01f + (float)(DewPlayer.gamePlayers.Count - 1));
			gazes = Mathf.Min(gazes, b);
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(Random.Range(initDelay.x, initDelay.y));
				while (!SingletonDewNetworkBehaviour<Room>.instance.didClearRoom && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && !this.IsNullOrInactive())
				{
					for (int i = 0; i < gazes; i++)
					{
						Vector3 vector = Vector3.zero;
						float angle = 0f;
						for (int j = 0; j < 100; j++)
						{
							RoomSection roomSection = SingletonDewNetworkBehaviour<Room>.instance.sections[Random.Range(0, SingletonDewNetworkBehaviour<Room>.instance.sections.Count)];
							Vector3 anyRandomNode = roomSection.GetAnyRandomNode();
							anyRandomNode = Dew.GetPositionOnGround(anyRandomNode);
							float num = Random.Range(0f, 360f);
							Vector3 vector2 = Quaternion.AngleAxis(num, Vector3.up) * roomSection.transform.forward;
							float num2 = 25f;
							Vector3 vector3 = anyRandomNode + vector2 * num2;
							_ = Vector3.zero;
							if (!CheckPositionValidity(vector3) || !((Dew.GetValidAgentDestination_LinearSweep(vector3, vector3 + vector2 * 30f) - vector3).sqrMagnitude > 2f))
							{
								vector3 = anyRandomNode + -vector2 * num2;
								if (!CheckPositionValidity(vector3) || !((Dew.GetValidAgentDestination_LinearSweep(vector3, vector3 + -vector2 * 30f) - vector3).sqrMagnitude > 2f))
								{
									vector = anyRandomNode;
									angle = num;
									break;
								}
							}
						}
						CreateAbilityInstance(vector, null, new CastInfo(null, angle), (Ai_Mon_Special_BossErebos_Gaze_Instance ai_Mon_Special_BossErebos_Gaze_Instance) =>
						{
							ai_Mon_Special_BossErebos_Gaze_Instance.isRoomModInstance = true;
							ai_Mon_Special_BossErebos_Gaze_Instance.initDmgHealthRatio = initDamage;
							ai_Mon_Special_BossErebos_Gaze_Instance.tickDmgHealthRatio = tickDamage;
							ai_Mon_Special_BossErebos_Gaze_Instance.duration = interval.x;
						});
						yield return new WaitForSeconds(0.5f);
					}
					yield return new WaitForSeconds(Random.Range(interval.x, interval.y));
				}
			}
		});
	}

	private bool CheckPositionValidity(Vector3 pos)
	{
		float num = 0.5f;
		NavMeshHit val = default;
		do
		{
			if (NavMesh.SamplePosition(pos, ref val, num, -1))
			{
				return true;
			}
			num += 0.5f;
		}
		while (!(num > 10f));
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
