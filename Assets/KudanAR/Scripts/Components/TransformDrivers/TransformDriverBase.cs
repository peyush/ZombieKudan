using UnityEngine;
using System.Text;
using System.Collections;

namespace Kudan.AR
{
	public abstract class TransformDriverBase : MonoBehaviour
	{
		protected TrackingMethodBase _trackerBase;

		public virtual void Start()
		{
			FindTracker();
			if (_trackerBase != null)
			{
				Register();
			}
		}

		public virtual void OnDestroy()
		{
			Unregister();
		}

		public virtual void Update()
		{
			if (_trackerBase == null)
			{
				FindTracker();
				if (_trackerBase != null)
				{
					Register();
				}
			}
		}

		protected virtual void Register()
		{
			// Register with the Tracking Method class to handle any events etc
		}

		protected virtual void Unregister()
		{
			// Unregister with the Tracking Method class from any events etc
		}

		protected abstract void FindTracker();
	}
};