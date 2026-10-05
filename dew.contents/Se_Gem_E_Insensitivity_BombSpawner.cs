using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Gem_E_Insensitivity_BombSpawner : StatusEffect
{
	public float duration = 5f;

	public float distancePerBomb = 2f;

	[NonSerialized]
	public float accumulatedDistance;

	private Vector3 _lastPosition;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		accumulatedDistance = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			ShowOnScreenTimer("Gem_E_Insensitivity");
			_lastPosition = victim.agentPosition;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (ManagerBase<TransitionManager>.instance.state != TransitionManager.StateType.Normal || ManagerBase<CameraManager>.instance.isPlayingCutscene || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			_lastPosition = victim.agentPosition;
			accumulatedDistance = 0f;
			return;
		}
		Vector3 lastPosition = _lastPosition;
		Vector3 vector = (_lastPosition = victim.agentPosition);
		float num = Vector2.Distance(lastPosition.ToXY(), vector.ToXY());
		if (num > 1000f)
		{
			return;
		}
		float num2 = num;
		while (num2 > 0f)
		{
			float num3 = distancePerBomb - accumulatedDistance;
			if (num2 < num3)
			{
				accumulatedDistance += num2;
				break;
			}
			accumulatedDistance = 0f;
			num2 -= num3;
			Vector3 pos = Vector3.Lerp(lastPosition, vector, 1f - num2 / num);
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
			IEnumerator Routine()
			{
				yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.15f));
				CreateAbilityInstance<Ai_Gem_E_Insensitivity_Bomb>(Dew.GetValidAgentPosition(Dew.GetPositionOnGround(pos + UnityEngine.Random.onUnitSphere)), null, new CastInfo(victim));
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
