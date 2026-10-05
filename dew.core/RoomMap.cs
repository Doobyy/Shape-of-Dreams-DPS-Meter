using Unity.Collections;
using UnityEngine;

public class RoomMap : RoomComponent
{
	private const int MaxMapSteps = 1024;

	private const int MapTextureMaxSize = 2048;

	private const float FogOfWarRevealRadius = 24f;

	private const float FogOfWarTickInterval = 0.25f;

	private const float FogOfWarRevealDecaySpeed = 0.4f;

	private const float FogOfWarVisitedAreaVisibility = 0.25f;

	private const float FogOfWarCellSize = 3f;

	public Texture2D mapTexture;

	public float density = 0.5f;

	[HideInInspector]
	public MapData mapData;

	private NativeArray<float> _fowRaw;

	private int _radius;

	private int _fowWidth;

	private int _fowHeight;

	private float _nextFowTickTime;

	public Texture2D fowTexture { get; private set; }

	public override void OnRoomStart()
	{
		base.OnRoomStart();
		_fowWidth = Mathf.RoundToInt(mapData.cells.size.x / 3f);
		_fowHeight = Mathf.RoundToInt(mapData.cells.size.y / 3f);
		fowTexture = new Texture2D(_fowWidth, _fowHeight, TextureFormat.RFloat, mipChain: false);
		Color[] array = new Color[_fowWidth * _fowHeight];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (isRevisit ? Color.red : Color.black);
		}
		fowTexture.SetPixels(array);
		fowTexture.Apply();
		_fowRaw = fowTexture.GetRawTextureData<float>();
		_radius = Mathf.RoundToInt(8f);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!(Time.time > _nextFowTickTime))
		{
			return;
		}
		_nextFowTickTime = Time.time + 0.25f;
		for (int i = 0; i < _fowRaw.Length; i++)
		{
			if (_fowRaw[i] > 0.25f)
			{
				_fowRaw[i] = Mathf.MoveTowards(_fowRaw[i], 0.25f, 0.1f);
			}
		}
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			Vector2 foWPosFromWorldPos = GetFoWPosFromWorldPos(allHero.position.ToXY());
			RevealFog(foWPosFromWorldPos.x, foWPosFromWorldPos.y);
		}
		fowTexture.Apply();
	}

	public bool IsWorldPosVisible(Vector3 worldPos)
	{
		if (!_fowRaw.IsCreated)
		{
			return false;
		}
		Vector2Int foWPosIntFromWorldPos = GetFoWPosIntFromWorldPos(worldPos.ToXY());
		int foWIndex = GetFoWIndex(foWPosIntFromWorldPos.x, foWPosIntFromWorldPos.y);
		return _fowRaw[foWIndex] > 0.3f;
	}

	public bool IsWorldPosVisited(Vector3 worldPos)
	{
		if (!_fowRaw.IsCreated)
		{
			return false;
		}
		Vector2Int foWPosIntFromWorldPos = GetFoWPosIntFromWorldPos(worldPos.ToXY());
		int foWIndex = GetFoWIndex(foWPosIntFromWorldPos.x, foWPosIntFromWorldPos.y);
		return _fowRaw[foWIndex] > 0.2f;
	}

	private Vector2 GetFoWPosFromWorldPos(Vector2 worldPos)
	{
		Vector2 normalizedPos = mapData.cells.GetNormalizedPos(worldPos);
		return new Vector2(normalizedPos.x * (float)_fowWidth, normalizedPos.y * (float)_fowHeight);
	}

	private Vector2Int GetFoWPosIntFromWorldPos(Vector2 worldPos)
	{
		Vector2 normalizedPos = mapData.cells.GetNormalizedPos(worldPos);
		return new Vector2Int(Mathf.RoundToInt(normalizedPos.x * (float)_fowWidth), Mathf.RoundToInt(normalizedPos.y * (float)_fowHeight));
	}

	private void RevealFog(float cenX, float cenY)
	{
		Vector2Int vector2Int = new Vector2Int(Mathf.RoundToInt(cenX), Mathf.RoundToInt(cenY));
		int num = (_radius - 2) * (_radius - 2);
		int num2 = _radius * _radius;
		int num3 = num2 - num;
		int num4 = _radius + 1;
		for (int i = vector2Int.y - num4; i <= vector2Int.y + num4; i++)
		{
			float num5 = (float)i - cenY;
			float num6 = num5 * num5;
			for (int j = vector2Int.x - num4; j <= vector2Int.x + num4; j++)
			{
				float num7 = (float)j - cenX;
				float num8 = num7 * num7 + num6;
				if (!(num8 > (float)num2))
				{
					int foWIndex = GetFoWIndex(j, i);
					if (num8 > (float)num)
					{
						float b = 1f - (num8 - (float)num) / (float)num3;
						_fowRaw[foWIndex] = Mathf.Max(_fowRaw[foWIndex], b);
					}
					else
					{
						_fowRaw[foWIndex] = 1f;
					}
				}
			}
		}
	}

	private int GetFoWIndex(int x, int y)
	{
		x = Mathf.Clamp(x, 0, _fowWidth - 1);
		y = Mathf.Clamp(y, 0, _fowHeight - 1);
		return x + y * _fowWidth;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (fowTexture != null)
		{
			Object.Destroy(fowTexture);
			fowTexture = null;
		}
		if (_fowRaw.IsCreated)
		{
			_fowRaw.Dispose();
		}
	}

	private void MirrorProcessed()
	{
	}
}
