using UnityEngine;

namespace Kudan.AR
{
	[System.Serializable]
	public class MarkerLostEvent : UnityEngine.Events.UnityEvent<Trackable>
	{
	}

	[System.Serializable]
	public class MarkerFoundEvent : UnityEngine.Events.UnityEvent<Trackable>
	{
	}

	[System.Serializable]
	public class MarkerUpdateEvent : UnityEngine.Events.UnityEvent<Trackable>
	{
	}
}
