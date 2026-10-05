using Unity.Services.Analytics;

public class Event_SpecialEOSConnectivity : Event
{
	public bool c_success
	{
		set
		{
			((Event)this).SetParameter("c_success", value);
		}
	}

	public Event_SpecialEOSConnectivity()
		: base("SpecialEOSConnectivity")
	{
	}
}
