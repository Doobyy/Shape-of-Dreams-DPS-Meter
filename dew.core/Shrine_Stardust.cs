using System;
using UnityEngine;

public class Shrine_Stardust : Shrine, IShrineCustomAction, IInteractable
{
	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public int amount = 1;

	public Transform shakeTransform;

	private Vector3 _originalPos;

	private float _nextShakeTime;

	private float _nextPosChangeTime;

	public override bool isRegularReward => true;

	int IInteractable.priority => -1000;

	float IInteractable.focusDistance => 4.25f;

	public override void OnStartClient()
	{
		base.OnStartClient();
		_originalPos = shakeTransform.localPosition;
		shakeTransform.localRotation = Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));
		float num = UnityEngine.Random.Range(0.8f, 1.2f);
		Vector3 vector = Vector3.up * UnityEngine.Random.Range(-0.5f, 0.5f);
		availableEffect.transform.localScale *= num;
		useEffect.transform.localScale *= num;
		availableEffect.transform.localPosition += vector;
		useEffect.transform.localPosition += vector;
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (Time.time > _nextShakeTime)
		{
			_nextShakeTime = Time.time + 1f / 60f;
			shakeTransform.localPosition = _originalPos + UnityEngine.Random.onUnitSphere * 0.02f;
		}
	}

	protected override bool OnUse(Entity entity)
	{
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			gamePlayer.GiveStardust(amount);
		}
		Destroy();
		return true;
	}

	public string GetRawAction()
	{
		return DewLocalization.GetUIValue("InGame_Interact_GetStardust");
	}

	private void MirrorProcessed()
	{
	}
}
