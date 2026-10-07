using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DTT.AreaOfEffectRegions;
using UnityEngine;

public class CastIndicatorManager : ManagerBase<CastIndicatorManager>
{
	public Transform playerOrigin;

	public LineRegion lineRegion;

	public CircleRegion circleRegion;

	public CircleRegion secondaryCircleRegion;

	public ArcRegion arcRegion;

	private List<Material> _instantiatedMaterials = new List<Material>();

	private bool _isAnimatingFailure;

	protected override void Awake()
	{
		base.Awake();
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			Material[] sharedMaterials = meshRenderer.sharedMaterials;
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				sharedMaterials[j] = UnityEngine.Object.Instantiate(sharedMaterials[j]);
				_instantiatedMaterials.Add(sharedMaterials[j]);
			}
			meshRenderer.sharedMaterials = sharedMaterials;
		}
	}

	private void Start()
	{
		HideAll();
	}

	private void OnDestroy()
	{
		foreach (Material instantiatedMaterial in _instantiatedMaterials)
		{
			UnityEngine.Object.Destroy(instantiatedMaterial);
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		Entity controllingEntity = ManagerBase<ControlManager>.instance.controllingEntity;
		if ((UnityEngine.Object)(object)controllingEntity == null || !ManagerBase<ControlManager>.instance.isCharacterControlEnabled)
		{
			HideAll();
			return;
		}
		if (ManagerBase<ControlManager>.instance.localSampleContext.HasValue && ManagerBase<ControlManager>.instance.localSampleContext.Value.showCastIndicator)
		{
			SampleCastInfoContext value = ManagerBase<ControlManager>.instance.localSampleContext.Value;
			CastMethodData castMethod = value.castMethod;
			DrawIndicator(castMethod, value.currentInfo, null);
			return;
		}
		if (_isAnimatingFailure)
		{
			playerOrigin.position = ManagerBase<ControlManager>.instance.controllingEntity.position;
			return;
		}
		ControlManager.ControlStateType type = ManagerBase<ControlManager>.instance.state.type;
		if (type == ControlManager.ControlStateType.AttackMove || type == ControlManager.ControlStateType.Cast)
		{
			AbilityTrigger abilityTrigger = ((type == ControlManager.ControlStateType.AttackMove) ? controllingEntity.Ability.attackAbility : ManagerBase<ControlManager>.instance.state.trigger);
			TriggerConfig currentConfig = abilityTrigger.currentConfig;
			CastMethodData castMethod2 = currentConfig.castMethod;
			DrawIndicator(castMethod2, ManagerBase<ControlManager>.instance.GetCastInfo(castMethod2, currentConfig.targetValidator), abilityTrigger);
		}
		else
		{
			HideAll();
		}
	}

	public void IndicateRangeFailure(float range)
	{
		StopAllCoroutines();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			_isAnimatingFailure = true;
			((Component)(object)lineRegion).gameObject.SetActive(value: false);
			((Component)(object)circleRegion).gameObject.SetActive(value: true);
			((Component)(object)arcRegion).gameObject.SetActive(value: false);
			((Component)(object)secondaryCircleRegion).gameObject.SetActive(value: false);
			playerOrigin.position = ManagerBase<ControlManager>.instance.controllingEntity.position;
			ShortcutExtensions.DOKill((Component)((Component)(object)circleRegion).transform, true);
			((Component)(object)circleRegion).transform.localScale = Vector3.one * 0.85f;
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)(object)circleRegion).transform, Vector3.one, 0.25f), (Ease)30);
			((CircleRegionBase)circleRegion).Radius = range;
			circleRegion.UpdateProperties();
			yield return new WaitForSeconds(0.25f);
			ShortcutExtensions.DOScale(((Component)(object)circleRegion).transform, Vector3.zero, 0.15f);
			yield return new WaitForSeconds(0.25f);
			_isAnimatingFailure = false;
		}
	}

	private void DrawIndicator(CastMethodData method, CastInfo info, AbilityTrigger trg)
	{
		Entity controllingEntity = ManagerBase<ControlManager>.instance.controllingEntity;
		switch (method.type)
		{
		case CastMethodType.None:
		{
			float effectiveRange = method.GetEffectiveRange(trg);
			if (effectiveRange > 0.1f)
			{
				DrawCircleRegion(effectiveRange);
			}
			else
			{
				HideAll();
			}
			break;
		}
		case CastMethodType.Cone:
			if (method.coneData.angle > 359f)
			{
				DrawCircleRegion(method.GetEffectiveRange(trg));
			}
			else
			{
				DrawArcRegion(method.coneData.angle, method.GetEffectiveRange(trg), info.angle);
			}
			break;
		case CastMethodType.Arrow:
			DrawLineRegion(method.GetEffectiveRange(trg), method.arrowData.width, info.angle);
			break;
		case CastMethodType.Target:
		{
			Entity target = info.target;
			if (method.targetData.radius > 0.1f && (UnityEngine.Object)(object)target != null)
			{
				DrawDoubleCircleRegion(method.GetEffectiveRange(trg), method.targetData.radius, target.position);
			}
			else
			{
				DrawCircleRegion(method.GetEffectiveRange(trg));
			}
			break;
		}
		case CastMethodType.Point:
			if (method.targetData.radius > 0.1f)
			{
				Vector3 vector = info.point;
				if (method.pointData.isClamping)
				{
					vector = Vector3.ClampMagnitude(vector - controllingEntity.position, method.pointData.range) + controllingEntity.position;
				}
				DrawDoubleCircleRegion(method.GetEffectiveRange(trg), method.pointData.radius, vector);
			}
			else
			{
				DrawCircleRegion(method.GetEffectiveRange(trg));
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void HideAll()
	{
		((Component)(object)lineRegion).gameObject.SetActive(value: false);
		((Component)(object)circleRegion).gameObject.SetActive(value: false);
		((Component)(object)arcRegion).gameObject.SetActive(value: false);
		((Component)(object)secondaryCircleRegion).gameObject.SetActive(value: false);
	}

	private void DrawCircleRegion(float range)
	{
		((Component)(object)lineRegion).gameObject.SetActive(value: false);
		((Component)(object)circleRegion).gameObject.SetActive(value: true);
		((Component)(object)arcRegion).gameObject.SetActive(value: false);
		((Component)(object)secondaryCircleRegion).gameObject.SetActive(value: false);
		playerOrigin.position = ManagerBase<ControlManager>.instance.controllingEntity.position;
		circleRegion.FillProgress = 1f;
		((CircleRegionBase)circleRegion).Radius = range;
		circleRegion.UpdateProperties();
	}

	private void DrawArcRegion(float arc, float radius, float angle)
	{
		((Component)(object)lineRegion).gameObject.SetActive(value: false);
		((Component)(object)circleRegion).gameObject.SetActive(value: false);
		((Component)(object)arcRegion).gameObject.SetActive(value: true);
		((Component)(object)secondaryCircleRegion).gameObject.SetActive(value: false);
		playerOrigin.position = ManagerBase<ControlManager>.instance.controllingEntity.position;
		((ArcRegionBase)arcRegion).Radius = radius;
		((ArcRegionBase)arcRegion).Arc = arc;
		((ArcRegionBase)arcRegion).Angle = angle;
		arcRegion.UpdateProperties();
	}

	private void DrawLineRegion(float length, float width, float angle)
	{
		((Component)(object)lineRegion).gameObject.SetActive(value: true);
		((Component)(object)circleRegion).gameObject.SetActive(value: false);
		((Component)(object)arcRegion).gameObject.SetActive(value: false);
		((Component)(object)secondaryCircleRegion).gameObject.SetActive(value: false);
		playerOrigin.position = ManagerBase<ControlManager>.instance.controllingEntity.position;
		((LineRegionBase)lineRegion).Length = length;
		((LineRegionBase)lineRegion).Width = width;
		((LineRegionBase)lineRegion).Angle = angle;
		lineRegion.UpdateProperties();
	}

	private void DrawDoubleCircleRegion(float range, float radius, Vector3 position)
	{
		((Component)(object)lineRegion).gameObject.SetActive(value: false);
		((Component)(object)circleRegion).gameObject.SetActive(value: true);
		((Component)(object)arcRegion).gameObject.SetActive(value: false);
		((Component)(object)secondaryCircleRegion).gameObject.SetActive(value: true);
		playerOrigin.position = ManagerBase<ControlManager>.instance.controllingEntity.position;
		((CircleRegionBase)circleRegion).Radius = range;
		circleRegion.UpdateProperties();
		((Component)(object)secondaryCircleRegion).transform.position = position;
		((CircleRegionBase)secondaryCircleRegion).Radius = radius;
		secondaryCircleRegion.UpdateProperties();
	}
}
