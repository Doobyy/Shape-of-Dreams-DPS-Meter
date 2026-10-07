using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_Mon_Primus_BossPrimusAeron_Force_GoldRain_Attack : InstantDamageInstance
{
	public int subInstances = 7;

	public float subInstanceGap = 3f;

	public float subInstanceInterval = 0.2f;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			DestroyOnCondition(() => ((Mon_Primus_BossPrimusAeron)info.caster).phase != Mon_Primus_BossPrimusAeron.PhaseType.Force);
			for (int i = 0; i < subInstances; i++)
			{
				yield return new WaitForSeconds(subInstanceInterval);
				Vector3 vector = position + info.rotation * new Vector3(0f, 0f, (float)(i + 1) * subInstanceGap);
				vector = Dew.GetPositionOnGround(vector);
				if ((int)Dew.GetNavMeshPathStatus(position, vector) != 0)
				{
					break;
				}
				CreateAbilityInstance<Ai_Mon_Primus_BossPrimusAeron_Force_GoldRain_Attack_SubInstance>(vector, null, new CastInfo(info.caster));
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
