//#define KUDAN_DEVELOPMENT
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kudan.AR
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Kudan AR/Kudan Tracker")]
	public class KudanTracker : MonoBehaviour
	{
		private const int DefaultCameraWidth = 640;
		private const int DefaultCameraHeight = 480;
		
		protected TrackerBase _trackerPlugin;
		protected Trackable[] _lastDetectedTrackables;

		[Tooltip("The API key issued by Kudan")]
		public string _apiKey = string.Empty;

		public TrackingMethodBase _defaultTrackingMethod;
		public TrackingMethodBase[] _trackingMethods;

		[Tooltip("Don't destroy between level loads")]
		public bool _makePersistent = true;
		public bool _startOnEnable = true;
		public bool _applyProjection = true;

		[Tooltip("The camera to apply the projection matrix to.  If left blank this will use the main camera.")]
		public Camera _renderingCamera;

		[Tooltip("The renderer draw the tracking texture to.")]
		public Renderer _background;
		public bool _displayDebugGUI = true;

		[Range(1, 4)]
		public int _debugGuiScale = 1;

		[HideInInspector]
		public Shader _debugFlatShader;

		// Interface exposing the Kudan API for those that need scripting control
		public TrackerBase Interface
		{
			get { return _trackerPlugin; }
		}

		private TrackingMethodBase _currentTrackingMethod;
		public TrackingMethodBase CurrentTrackingMethod
		{
			get { return _currentTrackingMethod; }
		}

#if UNITY_EDITOR
		private int _toolbarIndex;

		public void EditorGUI(ref bool externalOperation)
		{
			/*string[] toolbarStrings = new string[_internalTrackingMethods.Length];
			for (int i = 0; i < _internalTrackingMethods.Length; i++)
			{
				toolbarStrings[i] = _internalTrackingMethods[i].Name;
			}

			GUILayout.BeginVertical("box");

			_toolbarIndex = GUILayout.Toolbar(_toolbarIndex, toolbarStrings);
			_internalTrackingMethods[_toolbarIndex].EditorGUI(ref externalOperation);

			GUILayout.EndVertical();*/
		}
#endif

		void Start()
		{
			// Check there is only a single instance of this component
			if (FindObjectsOfType<KudanTracker>().Length > 1)
			{
				Debug.LogError("[KudanAR] There should only be one instance of KudanTracker active at a time");
				return;
			}
 			
			CreateDebugLineMaterial();

			// Create the platform specific plugin interface
#if KUDAN_DEVELOPMENT && (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
			_trackerPlugin = new TrackerWindows();
#elif UNITY_ANDROID
			_trackerPlugin = new TrackerAndroid();
#elif UNITY_IOS
			_trackerPlugin = new TrackeriOS(_background);
			Application.targetFrameRate = 60;
#else
			Debug.LogWarning("[KudanAR] not supported on this platform");
#endif

			if (_trackerPlugin == null)
			{
				Debug.LogError("[KudanAR] Failed to initialise");
				this.enabled = false;
				return;
			}

			// Check licensing
			this.StartCoroutine("RunLicensing");

			// Initialise plugin
			if (!_trackerPlugin.InitPlugin())
			{
				Debug.LogError("[KudanAR] Error initialising plugin");
				this.enabled = false;
			}
			else
			{
				// Set the API key
				if (!string.IsNullOrEmpty(_apiKey))
					_trackerPlugin.SetApiKey(_apiKey, Application.bundleIdentifier);
				else
					Debug.LogWarning("[KudanAR] No API key specified");

				// Print plugin version
				float version = _trackerPlugin.GetPluginVersion();
				float nativeVersion = _trackerPlugin.GetNativePluginVersion();
				Debug.Log(string.Format("[KudanAR] Initialising v{0} (native v{1})", version, nativeVersion));

				// Don't destroy this component between level loads
				if (_makePersistent)
				{
					GameObject.DontDestroyOnLoad(this.gameObject);
				}

				foreach (TrackingMethodBase method in _trackingMethods)
				{
					method.Init();
				}

				ChangeTrackingMethod(_defaultTrackingMethod);

				// Start the camera
				if (_trackerPlugin.StartInputFromCamera(0, DefaultCameraWidth, DefaultCameraHeight))
				{
					// Start tracking
					if (_startOnEnable)
					{
						_trackerPlugin.StartTracking();
					}
				}
				else
				{
					Debug.LogError("[KudanAR] Failed to start camera, is it already in use?");
				}
			}
		}

		IEnumerator RunLicensing()
		{
			const string licenseUrl = "https://api.kudan.eu/licensing/check/{0}/status.txt";
			string bundleID = Application.bundleIdentifier;
			
			string url = string.Format(licenseUrl, bundleID);
			WWW www = new WWW(url);
			yield return www;

			if (www.text == "ASTLA_LAVISTA")
			{
				Debug.LogError("[KudanAR] Invalid license, please register your application and set your bundle/package Id in Player Settings");
				this.enabled = false;
			}
		}

		void OnEnable()
		{
			if (_startOnEnable)
			{
				StartTracking();
			}
		}

		void OnDisable()
		{
			StopTracking();
		}

		void OnApplicationFocus(bool focusStatus)
		{
			if (_trackerPlugin != null)
			{
				_trackerPlugin.OnApplicationFocus(focusStatus);
			}
		}

		void OnApplicationPause(bool pauseStatus)
		{
			if (_trackerPlugin != null)
			{
				_trackerPlugin.OnApplicationPause(pauseStatus);
			}
		}

		public void StartTracking()
		{
			if (_trackerPlugin != null)
			{
				_trackerPlugin.StartTracking();
			}
		}
		
		public void StopTracking()
		{
			if (_trackerPlugin != null)
			{
				_trackerPlugin.StopTracking();
			}
			
			// Restore projection matrix
			Camera camera = _renderingCamera;
			if (camera == null)
				camera = Camera.main;
			if (camera != null)
			{
				camera.ResetProjectionMatrix();
			}
		}
		
		public void ChangeTrackingMethod(TrackingMethodBase newTrackingMethod)
		{
			if (newTrackingMethod != null && _currentTrackingMethod != newTrackingMethod)
			{
				if (_currentTrackingMethod != null)
				{
					_currentTrackingMethod.StopTracking();
				}

				_currentTrackingMethod = newTrackingMethod;
				_currentTrackingMethod.StartTracking();
			}
		}

        public void ArbiTrackStart(Vector3 position, Quaternion orientation)
        {
            _trackerPlugin.ArbiTrackStart(position, orientation);
        }

        public bool ArbiTrackIsTracking()
        {
            return _trackerPlugin.ArbiTrackIsTracking();
        }


        public void FloorPlaceGetPose(out Vector3 position, out Quaternion orientation)
        {
            _trackerPlugin.FloorPlaceGetPose(out position, out orientation);
        }

        public void ArbiTrackGetPose(out Vector3 position, out Quaternion orientation)
        {
            _trackerPlugin.ArbiTrackGetPose(out position, out orientation);
        }

		void OnDestroy()
		{
			if (_trackerPlugin != null)
			{
				StopTracking();
				_trackerPlugin.StopInput();
				_trackerPlugin.DeinitPlugin();
				_trackerPlugin = null;
			}

			if (_lineMaterial != null)
			{
				Material.Destroy(_lineMaterial);
				_lineMaterial = null;
			}
		}

		void Update()
		{
			if (_trackerPlugin != null)
			{
				Camera renderingCamera = _renderingCamera;
				if (renderingCamera == null)
					renderingCamera = Camera.main;
				_trackerPlugin.SetupRenderingCamera(renderingCamera.nearClipPlane, renderingCamera.farClipPlane);
				
				// Update tracking
				_trackerPlugin.UpdateTracking();
				
				// Apply projection matrix
				if (_applyProjection)
					renderingCamera.projectionMatrix = _trackerPlugin.GetProjectionMatrix();
				else
					renderingCamera.ResetProjectionMatrix();

				// Take a copy of the detected trackables
				ProcessNewTrackables();

				_currentTrackingMethod.ProcessFrame();

				// Apply texture to background renderer
				Texture texture = _trackerPlugin.GetTrackingTexture();
				if (_background != null && texture != null)
				{
					_background.material.mainTexture = texture;
				}
			}
		}

#if UNITY_ANDROID
		void OnPostRender()
		{
			if (_trackerPlugin != null)
			{
				_trackerPlugin.PostRender();
			}

			if (_displayDebugGUI)
			{
				RenderAxes();
			}
		}
#else
		void OnPostRender()
		{
			if (_displayDebugGUI)
			{
				RenderAxes();
			}
		}
#endif
		private void ProcessNewTrackables()
		{
			_lastDetectedTrackables = _trackerPlugin.GetDetectedTrackablesAsArray();
		}

		public bool HasActiveTrackingData()
		{
			return (_trackerPlugin != null && _trackerPlugin.IsTrackingRunning() && _lastDetectedTrackables != null && _lastDetectedTrackables.Length > 0);
		}

		void OnDrawGizmos()
		{
			// Draw useful debug rendering in Editor
			if (HasActiveTrackingData())
			{
				foreach (Trackable t in _lastDetectedTrackables)
				{
					// Draw circle
					Gizmos.color = Color.cyan;
					Gizmos.DrawSphere(t.position, 10f);

					// Draw line from origin to point (useful if object is offscreen)
					Gizmos.color = Color.cyan;
					Gizmos.DrawLine(Vector3.zero, t.position);
					
					// Draw axes
					Matrix4x4 xform = Matrix4x4.TRS(t.position, t.orientation, Vector3.one * 250f);
					Gizmos.matrix = xform;

					Gizmos.color = Color.red;
					Gizmos.DrawLine(Vector3.zero, Vector3.right);

					Gizmos.color = Color.green;
					Gizmos.DrawLine(Vector3.zero, Vector3.up);

					Gizmos.color = Color.blue;
					Gizmos.DrawLine(Vector3.zero, Vector3.forward);
				}
			}
		}

		public bool StartLineRendering()
		{
			bool result = false;
			if (_lineMaterial != null)
			{
				_lineMaterial.SetPass(0);
				result = true;
			}
			return result;
		}

		private void RenderAxes()
		{
			if (HasActiveTrackingData() && StartLineRendering())
			{			
				foreach (Trackable t in _lastDetectedTrackables)
				{
					Matrix4x4 xform = Matrix4x4.TRS(t.position, t.orientation, Vector3.one * 250f);

					GL.PushMatrix();

					Matrix4x4 m = GL.GetGPUProjectionMatrix(_trackerPlugin.GetProjectionMatrix(), false);
					m = _trackerPlugin.GetProjectionMatrix();
					GL.LoadProjectionMatrix(m);

					// Draw line from origin to point (useful if object is offscreen)
					GL.Color(Color.cyan);
					GL.Vertex(Vector3.zero);
					GL.Vertex(t.position);		

					GL.Begin(GL.LINES);
					GL.MultMatrix(xform);
					GL.Color(Color.red);
					GL.Vertex(Vector3.zero);
					GL.Vertex(Vector3.right);

					GL.Color(Color.green);
					GL.Vertex(Vector3.zero);
					GL.Vertex(Vector3.up);

					GL.Color(Color.blue);
					GL.Vertex(Vector3.zero);
					GL.Vertex(Vector3.forward);

					GL.End();
					GL.PopMatrix();
				}
			}
		}
		
		void OnGUI()
		{
			// Display debug GUI with tracking information
			if (_displayDebugGUI)
			{
				GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(_debugGuiScale, _debugGuiScale, 1f));
				GUILayout.BeginVertical("box");
#if UNITY_EDITOR
				GUILayout.Label("KUDAN AR", UnityEditor.EditorStyles.boldLabel);
#else
				GUILayout.Label("KUDAN AR");
#endif
				// Tracking status
				if (_trackerPlugin != null && _trackerPlugin.IsTrackingRunning())
				{
					GUI.color = Color.green;
					GUILayout.Label("Tracker is running");
				}
				else
				{
					GUI.color = Color.red;
					GUILayout.Label("Tracker NOT running");
				}
				GUI.color = Color.white;

				// Screen resolution
				GUILayout.Label("Screen: " + Screen.width + "x" + Screen.height);

				// Frame rates
				if (_trackerPlugin != null)
				{
					GUILayout.Label("Camera rate:  " + _trackerPlugin.CameraFrameRate.ToString("F2") + "hz");
					GUILayout.Label("Tracker rate: " + _trackerPlugin.TrackerFrameRate.ToString("F2") + "hz");
					GUILayout.Label("App rate: " + _trackerPlugin.AppFrameRate.ToString("F2") + "hz");
				}

				if (_trackerPlugin != null && _trackerPlugin.IsTrackingRunning())
				{
					// Texture image and resolution
					Texture finalTexture = _trackerPlugin.GetTrackingTexture();
					if (finalTexture != null)
					{
						GUILayout.Label("Texture: " + finalTexture.width + "x" + finalTexture.height);
						Rect r = GUILayoutUtility.GetRect(finalTexture.width / 4f, finalTexture.height / 4f);
						GUI.DrawTexture(r, finalTexture, ScaleMode.ScaleToFit);
					}

					if (_currentTrackingMethod != null)
					{
						GUILayout.Label("Method: " + _currentTrackingMethod.Name);
						_currentTrackingMethod.DebugGUI(_debugGuiScale);
					}
				}
			}
		}

		private Material _lineMaterial;

		private void CreateDebugLineMaterial()
		{
			if (!_lineMaterial && _debugFlatShader != null)
			{
				_lineMaterial = new Material(_debugFlatShader);
				_lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
		}
	}
};