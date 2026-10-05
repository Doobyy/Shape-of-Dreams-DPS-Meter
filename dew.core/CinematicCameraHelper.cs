using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[LogicUpdatePriority(500)]
public class CinematicCameraHelper : LogicBehaviour
{
	public CanvasGroup entireUICanvasGroup;

	public GameObject castIndicatorObject;

	public GameObject[] cinematicCameras;

	public CinemachineTargetGroup groupMyHero;

	public CinemachineTargetGroup groupAllHeroes;

	public CinemachineTargetGroup groupHeroesAndMonsters;

	public int maxTargets;

	public float weightGainPerSecond;

	public float radius = 3f;

	public int freeCamIndex;

	public Transform freeCamTargetTransform;

	public Transform freeCamTransform;

	public float freeCamSmoothTime;

	public float freeCamRotSmoothTime;

	public float freeCamFastSpeed;

	public float freeCamSpeed;

	public float freeCamSlowSpeed;

	public float freeCamMouseSensitivity;

	private int _currentCameraIndex = -1;

	private Hero _hiddenHero;

	private bool _didDisableControls;

	private Vector3 _cvPos;

	private float _cvRotX;

	private float _cvRotY;

	private float _cvRotZ;

	private List<Transform> _allEntities = new List<Transform>();

	public bool isCinematicHelperEnabled { get; set; }

	private void Awake()
	{
		Target[] array = new Target[maxTargets];
		for (int i = 0; i < array.Length; i++)
		{
			array[i].radius = radius;
			array[i].weight = 0f;
		}
		groupAllHeroes.m_Targets = array;
		Target[] array2 = new Target[maxTargets];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].radius = radius;
			array2[j].weight = 0f;
		}
		groupHeroesAndMonsters.m_Targets = array2;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!isCinematicHelperEnabled || ControlManager.IsInputFieldFocused())
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Keypad1))
		{
			ManagerBase<CursorManager>.instance.disableSoftwareCursor = !ManagerBase<CursorManager>.instance.disableSoftwareCursor;
		}
		if (Input.GetKeyDown(KeyCode.Keypad2))
		{
			entireUICanvasGroup.alpha = ((entireUICanvasGroup.alpha > 0.1f) ? 0f : 1f);
			castIndicatorObject.SetActive(entireUICanvasGroup.alpha > 0.1f);
		}
		if (Input.GetKeyDown(KeyCode.Keypad3))
		{
			_currentCameraIndex++;
			if (_currentCameraIndex >= cinematicCameras.Length)
			{
				_currentCameraIndex = -1;
			}
			for (int i = 0; i < cinematicCameras.Length; i++)
			{
				cinematicCameras[i].SetActive(_currentCameraIndex == i);
			}
			if (_currentCameraIndex == freeCamIndex)
			{
				freeCamTargetTransform.transform.SetPositionAndRotation(Dew.mainCamera.transform.position, Dew.mainCamera.transform.rotation);
				freeCamTransform.transform.SetPositionAndRotation(freeCamTargetTransform.position, freeCamTargetTransform.rotation);
				ManagerBase<ControlManager>.instance.DisableCharacterControls();
				_didDisableControls = true;
				ManagerBase<CameraManager>.instance.disableSeeThrough = true;
			}
			else if (_didDisableControls)
			{
				ManagerBase<ControlManager>.instance.EnableCharacterControls();
				_didDisableControls = false;
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				ManagerBase<CameraManager>.instance.disableSeeThrough = false;
			}
		}
		if (Input.GetKeyDown(KeyCode.Keypad4))
		{
			if ((Object)(object)_hiddenHero != null)
			{
				_hiddenHero.Visual.EnableRenderersLocal();
				_hiddenHero = null;
			}
			else
			{
				DewPlayer.local.hero.Visual.DisableRenderersLocal();
				_hiddenHero = DewPlayer.local.hero;
			}
		}
		if (_currentCameraIndex == freeCamIndex)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			float num;
			if (Input.GetKey(KeyCode.LeftShift))
			{
				num = freeCamFastSpeed;
			}
			else
			{
				num = (Input.GetKey(KeyCode.LeftControl) ? freeCamSlowSpeed : freeCamSpeed);
			}
			if (Input.GetKey(KeyCode.W))
			{
				freeCamTargetTransform.position += Time.unscaledDeltaTime * num * freeCamTransform.forward;
			}
			if (Input.GetKey(KeyCode.A))
			{
				freeCamTargetTransform.position += Time.unscaledDeltaTime * num * -freeCamTransform.right;
			}
			if (Input.GetKey(KeyCode.S))
			{
				freeCamTargetTransform.position += Time.unscaledDeltaTime * num * -freeCamTransform.forward;
			}
			if (Input.GetKey(KeyCode.D))
			{
				freeCamTargetTransform.position += Time.unscaledDeltaTime * num * freeCamTransform.right;
			}
			if (Input.GetKey(KeyCode.Space))
			{
				freeCamTargetTransform.position += Time.unscaledDeltaTime * num * Vector3.up;
			}
			if (Input.GetKey(KeyCode.C))
			{
				freeCamTargetTransform.position += Time.unscaledDeltaTime * num * Vector3.down;
			}
			Vector3 eulerAngles = freeCamTargetTransform.rotation.eulerAngles;
			float num2 = eulerAngles.x - Input.GetAxis("Mouse Y") * freeCamMouseSensitivity;
			if (eulerAngles.x < 90f && num2 > 89f)
			{
				num2 = 89f;
			}
			if (eulerAngles.x > 90f && num2 < 271f)
			{
				num2 = 271f;
			}
			float num3 = eulerAngles.y + Input.GetAxis("Mouse X") * freeCamMouseSensitivity;
			int num4 = 0;
			freeCamTargetTransform.rotation = Quaternion.Euler(num2, num3, num4);
			Vector3 eulerAngles2 = freeCamTransform.rotation.eulerAngles;
			freeCamTransform.position = Vector3.SmoothDamp(freeCamTransform.position, freeCamTargetTransform.position, ref _cvPos, freeCamSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime);
			freeCamTransform.rotation = Quaternion.Euler(Mathf.SmoothDampAngle(eulerAngles2.x, num2, ref _cvRotX, freeCamRotSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime), Mathf.SmoothDampAngle(eulerAngles2.y, num3, ref _cvRotY, freeCamRotSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime), Mathf.SmoothDampAngle(eulerAngles2.z, num4, ref _cvRotZ, freeCamRotSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime));
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (_currentCameraIndex < 0)
		{
			return;
		}
		if ((Object)(object)DewPlayer.local != null && (Object)(object)DewPlayer.local.hero != null)
		{
			groupMyHero.m_Targets[0].target = ((Component)(object)DewPlayer.local.hero).transform;
		}
		_allEntities.Clear();
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (allEntity is Hero || allEntity is Monster)
			{
				_allEntities.Add(((Component)(object)allEntity).transform);
			}
		}
		CopyList(_allEntities, groupHeroesAndMonsters);
		CopyList((IReadOnlyCollection<Component>)NetworkedManagerBase<ActorManager>.instance.allHeroes, groupAllHeroes);
	}

	private void CopyList(IReadOnlyCollection<Component> from, CinemachineTargetGroup to)
	{
		to.m_Targets[0].target = null;
		int num = 0;
		foreach (Component item in from)
		{
			if (num >= to.m_Targets.Length)
			{
				break;
			}
			to.m_Targets[num].radius = radius;
			to.m_Targets[num].target = ((num < from.Count) ? item.transform : to.m_Targets[0].target);
			if (num >= from.Count)
			{
				to.m_Targets[num].weight = 0f;
			}
			else
			{
				to.m_Targets[num].weight = Mathf.MoveTowards(to.m_Targets[num].weight, 1f, weightGainPerSecond * Time.deltaTime);
			}
			num++;
		}
	}
}
