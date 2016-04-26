using UnityEngine;
using System.Text;
using System.Collections;

namespace Kudan.AR
{
	[AddComponentMenu("Kudan AR/Transform Drivers/Marker Based Driver")]
	public class MarkerTransformDriver : TransformDriverBase
	{
		private const float UnityScaleFactor = 10f;

		[Tooltip("Optional ID")]
		public string _expectedId;
		public bool _applyMarkerScale;

		[Header("Plane Drawing")]
		public bool _alwaysDrawMarkerPlane = true;
		public int _markerPlaneWidth;
		public int _markerPlaneHeight;

		private string _trackableId;

		private TrackingMethodMarker _tracker;

		protected override void FindTracker()
		{
			_trackerBase = _tracker = (TrackingMethodMarker)Object.FindObjectOfType(typeof(TrackingMethodMarker));
		}

		protected override void Register()
		{
			if (_tracker != null)
			{
				_tracker._foundMarkerEvent.AddListener(OnTrackingFound);
				_tracker._lostMarkerEvent.AddListener(OnTrackingLost);
				_tracker._updateMarkerEvent.AddListener(OnTrackingUpdate);

				this.gameObject.SetActive(false);
			}
		}

		protected override void Unregister()
		{
			if (_tracker != null)
			{
				_tracker._foundMarkerEvent.RemoveListener(OnTrackingFound);
				_tracker._lostMarkerEvent.RemoveListener(OnTrackingLost);
				_tracker._updateMarkerEvent.RemoveListener(OnTrackingUpdate);
			}
		}

		public void OnTrackingFound(Trackable trackable)
		{
			bool matches = false;
			if (string.IsNullOrEmpty(_expectedId) || (_expectedId == trackable.name))
			{
				matches = true;
			}

			if (matches)
			{
				_trackableId = trackable.name;
				this.gameObject.SetActive(true);
			}
		}

		public void OnTrackingLost(Trackable trackable)
		{
			if (_trackableId == trackable.name)
			{
				this.gameObject.SetActive(false);
				_trackableId = string.Empty;
			}
		}

		public void OnTrackingUpdate(Trackable trackable)
		{
			if (_trackableId == trackable.name)
			{
				this.transform.localPosition = trackable.position;
				this.transform.localRotation = trackable.orientation;

				if (_applyMarkerScale)
				{
					this.transform.localScale = new Vector3(trackable.height / UnityScaleFactor, 1f, trackable.width / UnityScaleFactor);
				}
			}
		}

#if UNITY_EDITOR

		public void SetScaleFromMarkerSize()
		{
			if (_markerPlaneWidth > 0 && _markerPlaneHeight > 0)
			{
				this.transform.localScale = new Vector3(_markerPlaneHeight / UnityScaleFactor, 1f, _markerPlaneWidth / UnityScaleFactor);
			}
		}

		void OnDrawGizmosSelected()
		{
			DrawPlane();
		}

		void OnDrawGizmos()
		{
			if (_alwaysDrawMarkerPlane)
			{
				DrawPlane();
			}
		}

		private void DrawPlane()
		{
			Gizmos.matrix = this.transform.localToWorldMatrix;

			Vector3 size = Vector3.one * UnityScaleFactor;

			// In the editor mode use the user entered size values
			if (!Application.isPlaying)
			{
				if (_markerPlaneWidth > 0 && _markerPlaneHeight > 0)
				{
					size = new Vector3(_markerPlaneHeight, 0.01f, _markerPlaneWidth);
				}
				else
				{
					return;
				}
			}

			// Draw a flat cube to represent the area the marker would take up
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(Vector3.zero, size);
		}

#endif
	}
};