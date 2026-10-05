using Mirror;

public class Ai_Mon_Despair_BossAzurak_Atk_InitDamage : InstantDamageInstance
{
	public float[] spawnAngles = new float[2];

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		for (int i = 0; i < spawnAngles.Length; i++)
		{
			float angle = spawnAngles[i];
			int i2 = i;
			CreateAbilityInstance(range.transform.position, null, new CastInfo(info.caster), (Ai_Mon_Despair_BossAzurak_Atk_SubSpawner ai) =>
			{
				ai.startAngle = info.angle + angle;
				ai.playSounds = i2 == spawnAngles.Length / 2;
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
