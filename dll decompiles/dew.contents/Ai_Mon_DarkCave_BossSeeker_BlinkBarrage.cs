using System.Collections;
using Mirror;

public class Ai_Mon_DarkCave_BossSeeker_BlinkBarrage : AbilityInstance
{
	public int blinkCount;

	public float blinkInterval;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			info.caster.Control.StartDaze(blinkInterval * (float)blinkCount);
			for (int i = 0; i < blinkCount; i++)
			{
				CreateStatusEffect<Se_Mon_DarkCave_BossSeeker_Blink>(info.caster, new CastInfo(info.caster));
				yield return new SI.WaitForSeconds(blinkInterval);
			}
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
