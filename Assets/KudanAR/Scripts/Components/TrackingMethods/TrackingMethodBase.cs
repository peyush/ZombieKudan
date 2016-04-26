using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kudan.AR
{
	public abstract class TrackingMethodBase : MonoBehaviour
	{
		public KudanTracker _kudanTracker;

		public abstract string Name { get; }
		public abstract int TrackingMethodId { get; }

		protected bool _isTrackingEnabled;

		public TrackerBase Plugin
		{
			get { return _kudanTracker.Interface; }
		}

		public bool TrackingEnabled
		{
			get { return _isTrackingEnabled; }
		}

		void Awake()
		{
			if (_kudanTracker == null)
			{
				_kudanTracker = FindObjectOfType<KudanTracker>();
			}
			if (_kudanTracker == null)
			{
				Debug.LogWarning("[KudanAR] Cannot find KudanTracker in scene", this);
			}
		}

		public virtual void Init()
		{
		}

		public virtual void StartTracking()
		{
			if (Plugin != null)
			{
				if (Plugin.EnableTrackingMethod(TrackingMethodId))
				{
					_isTrackingEnabled = true;
				}
				else
				{
					Debug.LogError(string.Format("[KudanAR] Tracking method {0} not supported", TrackingMethodId));
				}
			}
		}

		public virtual void StopTracking()
		{
			if (Plugin != null)
			{
				if (Plugin.DisableTrackingMethod(TrackingMethodId))
				{
					_isTrackingEnabled = false;
				}
				else
				{
					Debug.LogError(string.Format("[KudanAR] Tracking method {0} not supported", TrackingMethodId));
				}
			}
		}

		public virtual void ProcessFrame()
		{
		}

		public virtual void DebugGUI(int uiScale)
		{
		}
	}
}