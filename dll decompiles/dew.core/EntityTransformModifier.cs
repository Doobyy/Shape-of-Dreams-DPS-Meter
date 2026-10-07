using UnityEngine;

public class EntityTransformModifier
{
	private Vector3 _localOffset = Vector3.zero;

	private Vector3 _worldOffset = Vector3.zero;

	private Vector3 _scaleMultiplier = Vector3.one;

	private Quaternion _rotation = Quaternion.identity;

	internal Entity _parent;

	public Vector3 localOffset
	{
		get
		{
			return _localOffset;
		}
		set
		{
			_localOffset = value;
			if ((Object)(object)_parent != null)
			{
				_parent.Visual.DirtyTransformModifiers();
			}
		}
	}

	public Vector3 worldOffset
	{
		get
		{
			return _worldOffset;
		}
		set
		{
			_worldOffset = value;
			if ((Object)(object)_parent != null)
			{
				_parent.Visual.DirtyTransformModifiers();
			}
		}
	}

	public Vector3 scaleMultiplier
	{
		get
		{
			return _scaleMultiplier;
		}
		set
		{
			_scaleMultiplier = value;
			if ((Object)(object)_parent != null)
			{
				_parent.Visual.DirtyTransformModifiers();
			}
		}
	}

	public Quaternion rotation
	{
		get
		{
			return _rotation;
		}
		set
		{
			_rotation = value;
			if ((Object)(object)_parent != null)
			{
				_parent.Visual.DirtyTransformModifiers();
			}
		}
	}

	public void Stop()
	{
		if (!((Object)(object)_parent == null) && !((Object)(object)_parent.Visual == null))
		{
			_parent.Visual.RemoveTransformModifier(this);
			_parent = null;
		}
	}

	internal void ResetForReuse()
	{
		_localOffset = Vector3.zero;
		_worldOffset = Vector3.zero;
		_scaleMultiplier = Vector3.one;
		_rotation = Quaternion.identity;
		_parent = null;
	}
}
