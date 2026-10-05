using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_Magmadon_AtkSpawner : AbilityInstance
{
	public float maxAngle;

	public int projectilCount;

	public float projectileStartDistance;

	public float startHeight;

	public float projectileEndDistance;

	public float atkInstanceDelay;

	private Vector3 _centerPos;

	private float _baseMaxAngle;

	protected override void Awake()
	{
		base.Awake();
		_baseMaxAngle = maxAngle;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		maxAngle = _baseMaxAngle;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_centerPos = info.caster.agentPosition;
		maxAngle -= maxAngle / 2f;
		float num = maxAngle / (float)(projectilCount - 1);
		float num2 = (0f - maxAngle) / 2f;
		for (int i = 0; i < projectilCount; i++)
		{
			float num3 = num2 + num * (float)i;
			Vector3 vector = Quaternion.Euler(0f, num3 / 2f, 0f) * info.forward;
			Vector3 vector2 = Quaternion.Euler(0f, num3, 0f) * info.forward;
			Vector3 customStartPoint = _centerPos + vector * projectileStartDistance + Vector3.up * startHeight;
			Vector3 vector3 = _centerPos + vector2 * projectileEndDistance;
			vector3 = Dew.GetPositionOnGround(vector3);
			Debug.DrawRay(customStartPoint, info.forward * 5f, Color.red);
			CreateAbilityInstance(info.caster.position, null, new CastInfo(info.caster, vector3), (Ai_Mon_LavaLand_Magmadon_Projectile b) =>
			{
				b.SetCustomStartPosition(customStartPoint);
			});
		}
		yield return new SI.WaitForSeconds(atkInstanceDelay);
		CreateAbilityInstance<Ai_Mon_LavaLand_Magmadon_Atk>(info.caster.position, Quaternion.Euler(0f, info.angle, 0f), info);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
