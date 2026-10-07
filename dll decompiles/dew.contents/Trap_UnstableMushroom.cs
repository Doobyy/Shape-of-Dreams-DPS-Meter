using UnityEngine;

[DewResourceLink(ResourceLinkBy.None)]
public class Trap_UnstableMushroom : Actor, IActivatableTrap, IBanRoomNodesNearby, IBanCampsNearby
{
	public GameObject fxEffect;

	public Color greenColor;

	public Color purpleColor;

	public GameObject[] greens;

	public GameObject[] purples;

	private bool _isGreen;

	public override void OnStartClient()
	{
		base.OnStartClient();
		FxPlay(fxEffect);
		_isGreen = Random.value > 0.5f;
		GameObject[] array = greens;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		array = purples;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
		GameObject[] array2 = (_isGreen ? greens : purples);
		array2[Random.Range(0, array2.Length)].SetActive(value: true);
	}

	public void ActivateTrap()
	{
		if (isActive)
		{
			CreateAbilityInstance(((Component)(object)this).transform.position, null, default, (Ai_Trap_UnstableMushroom m) =>
			{
				m.NetworkmainColor = (_isGreen ? greenColor : purpleColor);
			});
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(fxEffect);
	}

	private void MirrorProcessed()
	{
	}
}
