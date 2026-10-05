using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using UnityEngine;

public class Ai_E_ClutchesOfMalice_ReachAndPull : AbilityInstance
{
	[HideInInspector]
	public float endSyncTime;

	public DewBeamRenderer line;

	public float lineShootDuration;

	public DewEase pullEase;

	public float pullDuration;

	public GameObject fxStart;

	public GameObject fxHit;

	public ScalingValue firstDmgFactor;

	public ScalingValue secondDmgFactor;

	public float procCoefficient;

	private bool _sustainEnable;

	private Quaternion _fxStartBaseLocalRotation;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_fxStartBaseLocalRotation = fxStart.transform.localRotation;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_sustainEnable = false;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		Vector3 startPos = ((Component)(object)this).transform.position;
		line.SetPoints(startPos, startPos);
		line.enabled = true;
		Quaternion quaternion = Quaternion.LookRotation(info.target.position - startPos);
		FxPlay(fxStart, startPos, quaternion * _fxStartBaseLocalRotation);
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		yield return new SI.WaitForCondition(() => _sustainEnable);
		yield return new SI.WaitForSeconds(endSyncTime - Time.time);
		if (((NetworkBehaviour)this).isServer && !info.target.IsNullInactiveDeadOrKnockedOut() && !info.target.Status.hasUnstoppable)
		{
			Vector3 end = Dew.GetPositionOnGround(startPos) + Random.insideUnitSphere * 1.25f;
			Vector3 validAgentDestination_Closest = Dew.GetValidAgentDestination_Closest(info.target.agentPosition, end);
			validAgentDestination_Closest = Dew.GetPositionOnGround(validAgentDestination_Closest);
			info.target.Control.StartDaze(pullDuration);
			info.target.Control.StartDisplacement(new DispByDestination
			{
				destination = validAgentDestination_Closest,
				canGoOverTerrain = true,
				duration = pullDuration,
				ease = pullEase,
				isCanceledByCC = false,
				isFriendly = false,
				onCancel = Destroy,
				rotateForward = false
			});
			yield return new SI.WaitForSeconds(pullDuration);
			FxPlayNewNetworked(fxHit, info.target);
			Damage(secondDmgFactor, procCoefficient).Dispatch(info.target);
			DestroyIfActive();
		}
		IEnumerator Routine()
		{
			Vector3 endPos = Vector3.zero;
			float elapsedTime = 0f;
			Vector3 targetPos = info.target.Visual.GetCenterPosition();
			Vector3 pos = ((Component)(object)this).transform.position;
			while (elapsedTime < lineShootDuration)
			{
				pos = Vector3.Lerp(pos, targetPos, elapsedTime / lineShootDuration);
				line.SetEndPoint(pos);
				endPos = targetPos;
				elapsedTime += Time.deltaTime;
				yield return null;
			}
			line.SetEndPoint(endPos);
			_sustainEnable = true;
			if (((NetworkBehaviour)this).isServer)
			{
				Damage(firstDmgFactor, procCoefficient).Dispatch(info.target);
			}
		}
	}

	protected override void ActiveFrameUpdate()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected Obj, but got Unknown
		base.ActiveFrameUpdate();
		if (!_sustainEnable || line == null)
		{
			return;
		}
		if (info.target.IsNullInactiveDeadOrKnockedOut() || info.target.Status.hasUnstoppable)
		{
			Vector3 pos = line.lineRenderer.GetPosition(1);
			TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(DOTween.To((DOGetter<Vector3>)(() => pos), (DOSetter<Vector3>)((Vector3 x) =>
			{
				pos = x;
			}), ((Component)(object)this).transform.position, 0.1f), (TweenCallback)(() =>
			{
				if (((NetworkBehaviour)this).isServer)
				{
					DestroyIfActive();
				}
			}));
			line.SetEndPoint(pos);
		}
		else
		{
			line.lineRenderer.SetPosition(1, info.target.Visual.GetCenterPosition());
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		line.enabled = false;
	}

	private void MirrorProcessed()
	{
	}
}
