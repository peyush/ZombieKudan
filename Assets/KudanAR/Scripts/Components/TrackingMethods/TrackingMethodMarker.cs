using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kudan.AR
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Kudan AR/Tracking Methods/Marker Tracking")]
	public class TrackingMethodMarker : TrackingMethodBase
	{
		public TrackableData[] _markers;

		public MarkerFoundEvent _foundMarkerEvent;
		public MarkerLostEvent _lostMarkerEvent;
		public MarkerUpdateEvent _updateMarkerEvent;

		private Trackable[] _lastDetectedTrackables;
	
		public override string Name
		{
			get { return "Marker"; }
		}

		public override int TrackingMethodId
		{
			get { return 0; }
		}

		public override void Init()
		{
			LoadMarkers();
		}

		private void LoadMarkers()
		{
			foreach (TrackableData marker in _markers)
			{
				if (marker != null)
				{
					if (marker.Data == null || marker.Data.Length == 0)
					{
						Debug.LogWarning("[KudanAR] Marker has no data assigned");
					}
					else if (!Plugin.AddTrackable(marker.Data, marker.id))
					{
						Debug.LogError("[KudanAR] Error adding trackable " + marker.id);
					}
				}
				else
				{
					Debug.LogWarning("[KudanAR] Null marker in list");
				}
			}
		}

		public override void ProcessFrame()
		{
			ProcessNewTrackables();
		}

        public override void StopTracking()
        {
            base.StopTracking();

            Trackable[] oldtrackables = _lastDetectedTrackables;

            if (oldtrackables != null)
            {
                for (int i = 0; i < oldtrackables.Length; i++)
                {
                    _lostMarkerEvent.Invoke(oldtrackables[i]);
                }
            }

            _lastDetectedTrackables = null;
        }

        private void ProcessNewTrackables()
		{
			Trackable[] newTrackables = Plugin.GetDetectedTrackablesAsArray();
			Trackable[] oldtrackables = _lastDetectedTrackables;

			// Find lost markers
			if (oldtrackables != null)
			{
				for (int i = 0; i < oldtrackables.Length; i++)
				{
					bool found = false;
					for (int j = 0; j < newTrackables.Length; j++)
					{
						if (oldtrackables[i].name == newTrackables[j].name)
						{
							found = true;
							break;
						}
					}

					if (!found)
					{
						_lostMarkerEvent.Invoke(oldtrackables[i]);
					}
				}
			}

			if (newTrackables != null)
			{
				// Find new markers
				for (int j = 0; j < newTrackables.Length; j++)
				{
					bool found = false;
					if (oldtrackables != null)
					{
						for (int i = 0; i < oldtrackables.Length; i++)
						{
							if (oldtrackables[i].name == newTrackables[j].name)
							{
								found = true;
								break;
							}
						}
					}

					if (!found)
					{
						_foundMarkerEvent.Invoke(newTrackables[j]);
					}
				}

				// Find updated markers
				for (int j = 0; j < newTrackables.Length; j++)
				{
					_updateMarkerEvent.Invoke(newTrackables[j]);
				}
			}

			// Point to the new markers
			_lastDetectedTrackables = newTrackables;
		}

		public override void DebugGUI(int uiScale)
		{
			// Each actively tracked object
			GUILayout.Label("Loaded: " + Plugin.GetNumTrackables());
			int numDetected = 0;
			if (_lastDetectedTrackables != null)
				numDetected = _lastDetectedTrackables.Length;
			GUILayout.Label("Detected: " + numDetected);

			if (_kudanTracker.HasActiveTrackingData())
			{
				foreach (Trackable t in _lastDetectedTrackables)
				{
					GUILayout.Label("Found: " + t.name);

					if (Camera.current != null)
					{
						// Draw a label in camera-space at the point of the detected marker
						Vector3 sp = Camera.current.WorldToScreenPoint(t.position);
						sp.y = Screen.height - sp.y;
						GUIContent content = new GUIContent(t.name + "\n" + t.position.ToString() + "\n" + t.width + "x" + t.height);
						Vector2 size = GUI.skin.box.CalcSize(content);
						GUI.Label(new Rect(sp.x / uiScale, sp.y / uiScale, size.x, size.y), content, "box");
					}
				}

				GUILayout.EndVertical();
			}

		}
	}
}