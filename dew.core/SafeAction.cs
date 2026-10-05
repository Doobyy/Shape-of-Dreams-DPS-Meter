using System;
using System.Collections.Generic;
using UnityEngine;

public class SafeAction : IPoolClearable
{
	private readonly List<Action> _handlers = new List<Action>();

	public int Count => _handlers.Count;

	public void Invoke()
	{
		switch (_handlers.Count)
		{
		case 0:
			return;
		case 1:
			try
			{
				_handlers[0]?.Invoke();
				return;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return;
			}
		}
		List<Action> list = DewPool.GetList(out ListReturnHandle<Action> handle);
		list.AddRange(_handlers);
		foreach (Action item in list)
		{
			try
			{
				item?.Invoke();
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
		handle.Return();
	}

	public SafeAction Add(Action handler)
	{
		if (handler != null)
		{
			_handlers.Add(handler);
		}
		return this;
	}

	public SafeAction Remove(Action handler)
	{
		if (handler != null)
		{
			_handlers.Remove(handler);
		}
		return this;
	}

	public void Clear()
	{
		_handlers.Clear();
	}

	public static SafeAction operator +(SafeAction e, Action handler)
	{
		if (e == null)
		{
			e = new SafeAction();
		}
		if (handler != null)
		{
			e._handlers.Add(handler);
		}
		return e;
	}

	public static SafeAction operator -(SafeAction e, Action handler)
	{
		if (e == null)
		{
			return null;
		}
		if (handler != null)
		{
			e._handlers.Remove(handler);
		}
		return e;
	}

	public bool Contains(Action handler)
	{
		return _handlers.Contains(handler);
	}
}
public class SafeAction<T> : IPoolClearable
{
	private readonly List<Action<T>> _handlers = new List<Action<T>>();

	public int Count => _handlers.Count;

	public void Invoke(T arg)
	{
		switch (_handlers.Count)
		{
		case 0:
			return;
		case 1:
			try
			{
				_handlers[0]?.Invoke(arg);
				return;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return;
			}
		}
		List<Action<T>> list = DewPool.GetList(out ListReturnHandle<Action<T>> handle);
		list.AddRange(_handlers);
		foreach (Action<T> item in list)
		{
			try
			{
				item?.Invoke(arg);
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
		handle.Return();
	}

	public SafeAction<T> Add(Action<T> handler)
	{
		if (handler != null)
		{
			_handlers.Add(handler);
		}
		return this;
	}

	public SafeAction<T> Remove(Action<T> handler)
	{
		if (handler != null)
		{
			_handlers.Remove(handler);
		}
		return this;
	}

	public void Clear()
	{
		_handlers.Clear();
	}

	public static SafeAction<T> operator +(SafeAction<T> e, Action<T> handler)
	{
		if (e == null)
		{
			e = new SafeAction<T>();
		}
		if (handler != null)
		{
			e._handlers.Add(handler);
		}
		return e;
	}

	public static SafeAction<T> operator -(SafeAction<T> e, Action<T> handler)
	{
		if (e == null)
		{
			return null;
		}
		if (handler != null)
		{
			e._handlers.Remove(handler);
		}
		return e;
	}

	public bool Contains(Action<T> handler)
	{
		return _handlers.Contains(handler);
	}
}
public class SafeAction<T1, T2> : IPoolClearable
{
	private readonly List<Action<T1, T2>> _handlers = new List<Action<T1, T2>>();

	public void Invoke(T1 a, T2 b)
	{
		switch (_handlers.Count)
		{
		case 0:
			return;
		case 1:
			try
			{
				_handlers[0]?.Invoke(a, b);
				return;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return;
			}
		}
		List<Action<T1, T2>> list = DewPool.GetList(out ListReturnHandle<Action<T1, T2>> handle);
		list.AddRange(_handlers);
		foreach (Action<T1, T2> item in list)
		{
			try
			{
				item?.Invoke(a, b);
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
		handle.Return();
	}

	public SafeAction<T1, T2> Add(Action<T1, T2> handler)
	{
		if (handler != null)
		{
			_handlers.Add(handler);
		}
		return this;
	}

	public SafeAction<T1, T2> Remove(Action<T1, T2> handler)
	{
		if (handler != null)
		{
			_handlers.Remove(handler);
		}
		return this;
	}

	public void Clear()
	{
		_handlers.Clear();
	}

	public static SafeAction<T1, T2> operator +(SafeAction<T1, T2> e, Action<T1, T2> handler)
	{
		if (e == null)
		{
			e = new SafeAction<T1, T2>();
		}
		if (handler != null)
		{
			e._handlers.Add(handler);
		}
		return e;
	}

	public static SafeAction<T1, T2> operator -(SafeAction<T1, T2> e, Action<T1, T2> handler)
	{
		if (e == null)
		{
			return null;
		}
		if (handler != null)
		{
			e._handlers.Remove(handler);
		}
		return e;
	}

	public bool Contains(Action<T1, T2> handler)
	{
		return _handlers.Contains(handler);
	}
}
public class SafeAction<T1, T2, T3> : IPoolClearable
{
	private readonly List<Action<T1, T2, T3>> _handlers = new List<Action<T1, T2, T3>>();

	public void Invoke(T1 a, T2 b, T3 c)
	{
		switch (_handlers.Count)
		{
		case 0:
			return;
		case 1:
			try
			{
				_handlers[0]?.Invoke(a, b, c);
				return;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return;
			}
		}
		List<Action<T1, T2, T3>> list = DewPool.GetList(out ListReturnHandle<Action<T1, T2, T3>> handle);
		list.AddRange(_handlers);
		foreach (Action<T1, T2, T3> item in list)
		{
			try
			{
				item?.Invoke(a, b, c);
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
		handle.Return();
	}

	public SafeAction<T1, T2, T3> Add(Action<T1, T2, T3> handler)
	{
		if (handler != null)
		{
			_handlers.Add(handler);
		}
		return this;
	}

	public SafeAction<T1, T2, T3> Remove(Action<T1, T2, T3> handler)
	{
		if (handler != null)
		{
			_handlers.Remove(handler);
		}
		return this;
	}

	public void Clear()
	{
		_handlers.Clear();
	}

	public static SafeAction<T1, T2, T3> operator +(SafeAction<T1, T2, T3> e, Action<T1, T2, T3> handler)
	{
		if (e == null)
		{
			e = new SafeAction<T1, T2, T3>();
		}
		if (handler != null)
		{
			e._handlers.Add(handler);
		}
		return e;
	}

	public static SafeAction<T1, T2, T3> operator -(SafeAction<T1, T2, T3> e, Action<T1, T2, T3> handler)
	{
		if (e == null)
		{
			return null;
		}
		if (handler != null)
		{
			e._handlers.Remove(handler);
		}
		return e;
	}

	public bool Contains(Action<T1, T2, T3> handler)
	{
		return _handlers.Contains(handler);
	}
}
public class SafeAction<T1, T2, T3, T4> : IPoolClearable
{
	private readonly List<Action<T1, T2, T3, T4>> _handlers = new List<Action<T1, T2, T3, T4>>();

	public void Invoke(T1 a, T2 b, T3 c, T4 d)
	{
		switch (_handlers.Count)
		{
		case 0:
			return;
		case 1:
			try
			{
				_handlers[0]?.Invoke(a, b, c, d);
				return;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return;
			}
		}
		List<Action<T1, T2, T3, T4>> list = DewPool.GetList(out ListReturnHandle<Action<T1, T2, T3, T4>> handle);
		list.AddRange(_handlers);
		foreach (Action<T1, T2, T3, T4> item in list)
		{
			try
			{
				item?.Invoke(a, b, c, d);
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
		handle.Return();
	}

	public SafeAction<T1, T2, T3, T4> Add(Action<T1, T2, T3, T4> handler)
	{
		if (handler != null)
		{
			_handlers.Add(handler);
		}
		return this;
	}

	public SafeAction<T1, T2, T3, T4> Remove(Action<T1, T2, T3, T4> handler)
	{
		if (handler != null)
		{
			_handlers.Remove(handler);
		}
		return this;
	}

	public void Clear()
	{
		_handlers.Clear();
	}

	public static SafeAction<T1, T2, T3, T4> operator +(SafeAction<T1, T2, T3, T4> e, Action<T1, T2, T3, T4> handler)
	{
		if (e == null)
		{
			e = new SafeAction<T1, T2, T3, T4>();
		}
		if (handler != null)
		{
			e._handlers.Add(handler);
		}
		return e;
	}

	public static SafeAction<T1, T2, T3, T4> operator -(SafeAction<T1, T2, T3, T4> e, Action<T1, T2, T3, T4> handler)
	{
		if (e == null)
		{
			return null;
		}
		if (handler != null)
		{
			e._handlers.Remove(handler);
		}
		return e;
	}

	public bool Contains(Action<T1, T2, T3, T4> handler)
	{
		return _handlers.Contains(handler);
	}
}
