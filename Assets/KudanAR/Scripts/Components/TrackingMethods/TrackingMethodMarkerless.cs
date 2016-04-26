using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kudan.AR
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Kudan AR/Tracking Methods/Markerless Tracking")]
	public class TrackingMethodMarkerless : TrackingMethodBase
	{
		public override string Name
		{
			get { return "Markerless"; }
		}

		public override int TrackingMethodId
		{
			get { return 1; }
		}

		public MarkerUpdateEvent _updateMarkerEvent;

		public override void ProcessFrame()
		{
            Vector3 position;
            Quaternion orientation;

            _kudanTracker.ArbiTrackGetPose(out position, out orientation);

            Trackable trackable = new Trackable();
            trackable.position = position;
            trackable.orientation = orientation;

            trackable.isDetected = _kudanTracker.ArbiTrackIsTracking();

            _updateMarkerEvent.Invoke(trackable);
		}

        public override void StopTracking()
        {
            base.StopTracking();
            Trackable trackable = new Trackable();
            trackable.isDetected = false;

            _updateMarkerEvent.Invoke(trackable);
        }
	}
}