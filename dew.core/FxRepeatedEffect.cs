using Mirror;
using UnityEngine;

[LogicUpdatePriority(500)]
public class FxRepeatedEffect : LogicBehaviour, IEffectComponent, IAttachableToEntity
{
	public bool playNew;

	public float interval = 0.5f;

	private float _lastPlayTime;

	private Entity _target;

	private NetworkIdentity _netId;

	[SerializeField]
	[HideInInspector]
	private bool _isDisabled;

	public bool isPlaying { get; private set; }

	public bool isLooping => true;

	public void Play()
	{
		if (!_isDisabled && !isPlaying)
		{
			isPlaying = true;
			_lastPlayTime = Time.time;
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (_isDisabled || !isPlaying || !(Time.time - _lastPlayTime > interval))
		{
			return;
		}
		if (_netId == null)
		{
			_netId = GetComponentInParent<NetworkIdentity>();
		}
		if (playNew)
		{
			_isDisabled = true;
			if (_target.IsNullOrInactive())
			{
				DewEffect.PlayNew(gameObject, _netId);
			}
			else
			{
				DewEffect.PlayNew(gameObject, _target, _netId);
			}
			_isDisabled = false;
		}
		else
		{
			DewEffect.Play(gameObject, _netId);
		}
		_lastPlayTime += interval;
	}

	public void Stop()
	{
		if (!_isDisabled)
		{
			isPlaying = false;
			_lastPlayTime = Time.time;
		}
	}

	public void OnAttachToEntity(Entity target)
	{
		_target = target;
	}
}
