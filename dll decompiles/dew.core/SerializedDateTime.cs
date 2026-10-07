using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public struct SerializedDateTime(DateTime dt) : ISerializationCallbackReceiver, IEquatable<SerializedDateTime>
{
	[NonSerialized]
	public DateTime dateTime = dt;

	[HideInInspector]
	[SerializeField]
	private string _dateTime = null;

	public static implicit operator DateTime(SerializedDateTime udt)
	{
		return udt.dateTime;
	}

	public static implicit operator SerializedDateTime(DateTime dt)
	{
		return new SerializedDateTime(dt);
	}

	public void OnBeforeSerialize()
	{
		_dateTime = dateTime.ToString("o", CultureInfo.InvariantCulture);
	}

	public void OnAfterDeserialize()
	{
		if (string.IsNullOrEmpty(_dateTime))
		{
			dateTime = default;
		}
		else if (!DateTime.TryParseExact(_dateTime, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTime))
		{
			DateTime.TryParse(_dateTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTime);
		}
	}

	public bool Equals(SerializedDateTime other)
	{
		return dateTime == other.dateTime;
	}

	public override bool Equals(object obj)
	{
		if (obj is SerializedDateTime other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return dateTime.GetHashCode();
	}

	public override string ToString()
	{
		return dateTime.ToString("o", CultureInfo.InvariantCulture);
	}

	public static bool operator ==(SerializedDateTime a, SerializedDateTime b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(SerializedDateTime a, SerializedDateTime b)
	{
		return !a.Equals(b);
	}
}
