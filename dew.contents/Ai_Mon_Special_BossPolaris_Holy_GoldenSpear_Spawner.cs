using System.Collections;
using System.Globalization;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Holy_GoldenSpear_Spawner : AbilityInstance
{
	public Transform patternParent;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		if (Vector3.Distance(info.caster.agentPosition, info.point) < 8f)
		{
			CastInfo castInfo = info;
			castInfo.point = info.caster.agentPosition + (info.point - info.caster.agentPosition).normalized * 8f;
			info = castInfo;
		}
		patternParent.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		patternParent.gameObject.SetActive(value: false);
		Transform selectedPattern = patternParent.GetChild(Random.Range(0, patternParent.childCount));
		int maxSequenceNumber = 0;
		for (int i = 0; i < selectedPattern.childCount; i++)
		{
			maxSequenceNumber = Mathf.Max(maxSequenceNumber, int.Parse(selectedPattern.GetChild(i).name, CultureInfo.InvariantCulture));
		}
		float num = 0.55f;
		float perSequenceInterval = num / (float)maxSequenceNumber;
		Quaternion rot = Quaternion.LookRotation((info.point - info.caster.agentPosition).Flattened());
		for (int j = 0; j <= maxSequenceNumber; j++)
		{
			for (int k = 0; k < selectedPattern.childCount; k++)
			{
				Transform child = selectedPattern.GetChild(k);
				Vector3 pos;
				if (!(child.name != j.ToString(CultureInfo.InvariantCulture)))
				{
					pos = info.point + rot * child.position;
					((MonoBehaviour)(object)this).StartCoroutine(Routine());
				}
				IEnumerator Routine()
				{
					yield return new WaitForSeconds(Random.Range(0f, 0.125f));
					CreateAbilityInstance(position, null, new CastInfo(info.caster, pos), (Ai_Mon_Special_BossPolaris_Holy_GoldenSpear_Projectile ai) =>
					{
						Vector3 normalized = (pos - info.caster.agentPosition).Flattened().normalized;
						ai.SetCustomStartPosition(info.caster.Visual.GetCenterPosition() + normalized * -5f + Random.onUnitSphere.Flattened() * 2f);
					});
				}
			}
			yield return new SI.WaitForSeconds(perSequenceInterval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
