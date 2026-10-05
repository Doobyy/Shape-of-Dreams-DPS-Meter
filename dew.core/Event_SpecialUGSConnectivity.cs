using Unity.Services.Analytics;

public class Event_SpecialUGSConnectivity : Event
{
	public bool c_success
	{
		set
		{
			((Event)this).SetParameter("c_success", value);
		}
	}

	public Event_SpecialUGSConnectivity()
		: base("SpecialUGSConnectivity")
	{
	}
}
