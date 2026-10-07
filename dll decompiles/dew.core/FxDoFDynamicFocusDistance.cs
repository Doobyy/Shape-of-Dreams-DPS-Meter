using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FxDoFDynamicFocusDistance : MonoBehaviour
{
	private VolumeProfile _profile;

	private DepthOfField _dof;

	private float _originalValue;

	private void Start()
	{
		Volume component = GetComponent<Volume>();
		if ((Object)(object)component == null)
		{
			Debug.LogWarning(name + " does not have a Volume component");
			return;
		}
		if ((Object)(object)component.sharedProfile == null)
		{
			Debug.LogWarning(name + "'s Volume component does not have a profile");
			return;
		}
		_profile = Object.Instantiate<VolumeProfile>(component.sharedProfile);
		component.sharedProfile = _profile;
		VolumeComponent val = _profile.components.Find((VolumeComponent v) => v is DepthOfField);
		_dof = (DepthOfField)(object)((val is DepthOfField) ? val : null);
		if ((Object)(object)_dof == null || !((VolumeParameter)_dof.focusDistance).overrideState)
		{
			Debug.LogWarning(name + "'s " + ((Object)(object)component.sharedProfile).name + " profile does not have a DoF component with focusDistance overriden");
			return;
		}
		int index = _profile.components.IndexOf((VolumeComponent)(object)_dof);
		_dof = Object.Instantiate<DepthOfField>(_dof);
		_profile.components[index] = (VolumeComponent)(object)_dof;
		_originalValue = ((VolumeParameter<float>)(object)_dof.focusDistance).value;
	}

	private void Update()
	{
		if (!((Object)(object)_dof == null))
		{
			if (ManagerBase<CameraManager>.softInstance == null || ManagerBase<CameraManager>.softInstance.isPlayingCutscene || (Object)(object)ManagerBase<CameraManager>.softInstance.focusedEntity == null || ManagerBase<DewCamera>.softInstance == null)
			{
				((VolumeParameter<float>)(object)_dof.focusDistance).value = _originalValue;
			}
			else
			{
				((VolumeParameter<float>)(object)_dof.focusDistance).value = Vector3.Distance(ManagerBase<DewCamera>.softInstance.mainCamera.transform.position, ManagerBase<CameraManager>.softInstance.focusedEntity.Visual.GetCenterPosition());
			}
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)_dof != null)
		{
			Object.Destroy((Object)(object)_dof);
		}
		if ((Object)(object)_profile != null)
		{
			Object.Destroy((Object)(object)_profile);
		}
	}
}
