using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Monster_Smite_Spawner : AbilityInstance
{
	public int smiteCount = 8;

	public float startDistance = 4f;

	public float distancePerInstance = 3f;

	public float interval = 0.15f;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			switch (Random.Range(0, 3))
			{
			case 0:
				((MonoBehaviour)(object)this).StartCoroutine(LineRoutine(0f, new Vector3(-9f, 0f, 5f)));
				yield return new SI.WaitForSeconds(interval / 3f);
				((MonoBehaviour)(object)this).StartCoroutine(LineRoutine(0f, new Vector3(0f, 0f, 0f)));
				yield return new SI.WaitForSeconds(interval / 3f);
				((MonoBehaviour)(object)this).StartCoroutine(LineRoutine(0f, new Vector3(9f, 0f, 5f)));
				break;
			case 1:
				((MonoBehaviour)(object)this).StartCoroutine(LineRoutine(0f, new Vector3(0f, 0f, 0f)));
				yield return new SI.WaitForSeconds(interval / 3f);
				((MonoBehaviour)(object)this).StartCoroutine(LineRoutine(45f, new Vector3(4f, 0f, 4f)));
				yield return new SI.WaitForSeconds(interval / 3f);
				((MonoBehaviour)(object)this).StartCoroutine(LineRoutine(-45f, new Vector3(-4f, 0f, 4f)));
				break;
			default:
				((MonoBehaviour)(object)this).StartCoroutine(LineRoutine(0f, default));
				((MonoBehaviour)(object)this).StartCoroutine(LineRoutine(90f, default));
				((MonoBehaviour)(object)this).StartCoroutine(LineRoutine(-90f, default));
				((MonoBehaviour)(object)this).StartCoroutine(LineRoutine(180f, default));
				break;
			}
			Destroy();
		}
		IEnumerator LineRoutine(float offsetAngle, Vector3 offsetPosition)
		{
			LockDestroy();
			for (int i = 0; i < smiteCount; i++)
			{
				if (i != 0)
				{
					yield return new WaitForSeconds(interval);
				}
				Vector3 vector = position + info.rotation * offsetPosition + Quaternion.Euler(0f, offsetAngle, 0f) * info.forward * (startDistance + distancePerInstance * (float)i);
				vector = Dew.GetPositionOnGround(vector);
				if ((int)Dew.GetNavMeshPathStatus(info.caster.agentPosition, vector) == 0)
				{
					CreateAbilityInstance<Ai_Mon_Special_BossPolaris_Monster_Smite_Instance>(vector, null, new CastInfo(info.caster));
				}
			}
			UnlockDestroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
