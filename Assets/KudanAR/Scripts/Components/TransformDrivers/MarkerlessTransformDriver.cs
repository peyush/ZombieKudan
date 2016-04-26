using UnityEngine;
using System.Text;
using System.Collections;

namespace Kudan.AR
{
	[AddComponentMenu("Kudan AR/Transform Drivers/Markerless Driver")]
	public class MarkerlessTransformDriver : TransformDriverBase
	{
		private TrackingMethodMarkerless _tracker;

		protected override void FindTracker()
		{
			_trackerBase = _tracker = (TrackingMethodMarkerless)Object.FindObjectOfType(typeof(TrackingMethodMarkerless));
		}

		protected override void Register()
		{
			if (_tracker != null)
			{
				_tracker._updateMarkerEvent.AddListener(OnTrackingUpdate);
			}
            this.gameObject.SetActive(false);

		}

		protected override void Unregister()
		{
			if (_tracker != null)
			{
				_tracker._updateMarkerEvent.RemoveListener(OnTrackingUpdate);
			}
		}

		public void OnTrackingUpdate(Trackable trackable)
		{
            this.transform.localPosition = trackable.position;
            this.transform.localRotation = trackable.orientation;

            this.gameObject.SetActive(trackable.isDetected);
		}
	}
};