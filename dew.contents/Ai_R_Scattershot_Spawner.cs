using Mirror;
using UnityEngine;

public class Ai_R_Scattershot_Spawner : AbilityInstance
{
	public float maxAngle = 30f;

	public float frontDistance = 0.5f;

	public ScalingValue shootCount;

	public ScalingValue empowerChance;

	public int addedShootCount;

	public GameObject fxEmpoweredCast;

	private bool _isEmpowered;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		_isEmpowered = Random.value * 100f < GetValue(empowerChance);
		int num = Mathf.RoundToInt(GetValue(shootCount));
		if (_isEmpowered)
		{
			num += addedShootCount;
			FxPlayNewNetworked(fxEmpoweredCast);
		}
		float a = 0f - maxAngle;
		float b = maxAngle;
		for (int i = 0; i < num; i++)
		{
			float num2 = Mathf.Lerp(a, b, (float)i / (float)(num - 1));
			CreateAbilityInstance(info.caster.Visual.GetCenterPosition() + info.forward * frontDistance, Quaternion.identity, new CastInfo(info.caster, info.angle + num2), (Ai_R_Scattershot_Projectile ai) =>
			{
				ai.isEmpoweredByCrit = _isEmpowered;
			});
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
