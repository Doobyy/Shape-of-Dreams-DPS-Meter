using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[LogicUpdatePriority(400)]
public class DewCanvas : LogicBehaviour, ISettingsChangedCallback
{
	private CanvasScaler _scaler;

	private Canvas _canvas;

	private float _originalScale;

	private Vector2 _lastScreenSize;

	private float _lastAssignedFactor;

	private void Awake()
	{
		DoInit();
	}

	private void DoInit()
	{
		_scaler = GetComponent<CanvasScaler>();
		_canvas = GetComponent<Canvas>();
		if ((bool)(Object)(object)_scaler)
		{
			_originalScale = _scaler.scaleFactor;
		}
	}

	private void Start()
	{
		Vector2 lastScreenSize = new Vector2(Screen.width, Screen.height);
		_lastScreenSize = lastScreenSize;
		UpdateScale();
		if (ManagerBase<DewCamera>.instance != null)
		{
			_canvas.worldCamera = ManagerBase<DewCamera>.instance.uiCamera;
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (_canvas.worldCamera == null && ManagerBase<DewCamera>.softInstance != null)
		{
			_canvas.worldCamera = ManagerBase<DewCamera>.softInstance.uiCamera;
		}
		Vector2 vector = new Vector2(Screen.width, Screen.height);
		if (_lastScreenSize != vector)
		{
			_lastScreenSize = vector;
			UpdateScaleMultiple();
		}
	}

	private void UpdateScaleMultiple()
	{
		if (isActiveAndEnabled)
		{
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			for (int i = 0; i < 5; i++)
			{
				yield return null;
				UpdateScale();
			}
		}
	}

	private void UpdateScale()
	{
		if (!(Object)(object)_canvas)
		{
			DoInit();
		}
		float num = Mathf.Clamp((float)Screen.height / 1440f, 0.1f, float.PositiveInfinity);
		num = _originalScale * DewSave.profileMain.gameplay.uiScale * num;
		if ((bool)(Object)(object)_scaler && !Mathf.Approximately(_lastAssignedFactor, num))
		{
			_scaler.scaleFactor = num;
			_lastAssignedFactor = num;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateScaleMultiple();
	}

	public void OnSettingsChanged()
	{
		UpdateScaleMultiple();
	}
}
