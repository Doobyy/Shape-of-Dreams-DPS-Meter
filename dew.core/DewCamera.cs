using UnityEngine;

[ExecuteAlways]
public class DewCamera : ManagerBase<DewCamera>, ISettingsChangedCallback
{
	public Camera mainCamera;

	public Camera uiCamera;

	public Camera inGameUICamera;

	public bool optimize;

	private Vector2Int _lastScreenSize;

	public override bool shouldRegisterUpdates => false;

	protected override void Awake()
	{
		base.Awake();
		transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		ApplyUICameraLayout();
		_lastScreenSize = new Vector2Int(Screen.width, Screen.height);
	}

	private void Update()
	{
		Vector2Int vector2Int = new Vector2Int(Screen.width, Screen.height);
		if (!(vector2Int == _lastScreenSize))
		{
			_lastScreenSize = vector2Int;
			ApplyUICameraLayout();
		}
	}

	public void OnSettingsChanged()
	{
		RefreshUICamerasDelayed();
	}

	private async Awaitable RefreshUICamerasDelayed()
	{
		await Awaitable.NextFrameAsync();
		ApplyUICameraLayout();
		_lastScreenSize = new Vector2Int(Screen.width, Screen.height);
	}

	private void ApplyUICameraLayout()
	{
		Vector3 position = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, -100f);
		float orthographicSize = (float)Mathf.Min(Screen.width, Screen.height) * 0.5f;
		if (uiCamera != null)
		{
			uiCamera.transform.position = position;
			uiCamera.orthographicSize = orthographicSize;
		}
		if (inGameUICamera != null)
		{
			inGameUICamera.transform.position = position;
			inGameUICamera.orthographicSize = orthographicSize;
		}
	}
}
