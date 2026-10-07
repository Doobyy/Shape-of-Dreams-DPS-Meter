using System;
using System.Collections;
using UnityEngine;

public class Sky_NightWaterEffect : MonoBehaviour
{
	public GameObject footstepEffect;

	public Sky_NightWaterEffect_RippleCreator rippleCreatorPrefab;

	public GameObject rippleEffect;

	public bool onlyWhenWalking;

	public Vector2 interval;

	public bool excludeLesser;

	private void Start()
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitWhile(() => (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.softInstance == null || (UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.softInstance == null || (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.softInstance.currentRoom == null);
			NetworkedManagerBase<ZoneManager>.instance.currentRoom.ClientEvent_OnFootstep += new Action<Entity>(ClientEventOnFootstep);
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += new Action<Entity>(OnEntityAdd);
			foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
			{
				if (!excludeLesser || !(allEntity is Monster { type: Monster.MonsterType.Lesser }))
				{
					Sky_NightWaterEffect_RippleCreator sky_NightWaterEffect_RippleCreator = UnityEngine.Object.Instantiate(rippleCreatorPrefab);
					sky_NightWaterEffect_RippleCreator._entity = allEntity;
					sky_NightWaterEffect_RippleCreator._parent = this;
				}
			}
		}
	}

	private void OnEntityAdd(Entity e)
	{
		if ((!excludeLesser || !(e is Monster { type: Monster.MonsterType.Lesser })) && !(rippleCreatorPrefab == null))
		{
			Sky_NightWaterEffect_RippleCreator sky_NightWaterEffect_RippleCreator = UnityEngine.Object.Instantiate(rippleCreatorPrefab);
			sky_NightWaterEffect_RippleCreator._entity = e;
			sky_NightWaterEffect_RippleCreator._parent = this;
		}
	}

	private void OnDestroy()
	{
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance.currentRoom != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.currentRoom.ClientEvent_OnFootstep -= new Action<Entity>(ClientEventOnFootstep);
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd -= new Action<Entity>(OnEntityAdd);
		}
	}

	private void ClientEventOnFootstep(Entity obj)
	{
		Vector3 bonePosition = obj.Visual.GetBonePosition((HumanBodyBones)5);
		Vector3 bonePosition2 = obj.Visual.GetBonePosition((HumanBodyBones)6);
		DewEffect.PlayNew(footstepEffect, (bonePosition.y < bonePosition2.y) ? bonePosition : bonePosition2, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
	}
}
