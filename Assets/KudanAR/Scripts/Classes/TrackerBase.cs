using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kudan.AR
{
	/// <summary>
	/// Base class for the tracker plugin.  This abstracts the native plugin for each operating system.
	/// </summary>
	public abstract class TrackerBase : ITracker
	{
		// The version of the plugin (scripts etc).  This is different to the version of the NATIVE plugin
		private const float PluginVersionNumber = 0.84f;

		// Copy of the markers the user has loaded
		protected List<Trackable> _trackables = new List<Trackable>(8);

		// Properties of Unity camera used for rendering
		protected float _cameraNearPlane = 0.3f;		// Unity defaults
		protected float _cameraFarPlane = 1000f;		// Unity defaults

		// Tracking thread
		protected System.Threading.Thread _trackingThread;
		protected bool _isTrackingRunning;

		// Results of tracking
		protected Texture _finalTexture;
		protected Matrix4x4 _projectionMatrix;
		protected List<Trackable> _detected = new List<Trackable>(8);

		// Frame rates of different components
		protected float _cameraRate;
		protected float _trackerRate;
		protected float _appRate;

		public float CameraFrameRate
		{
			get { return _cameraRate; }
		}

		public float TrackerFrameRate
		{
			get { return _trackerRate; }
		}

		public float AppFrameRate
		{
			get { return _appRate; }
		}

		public float GetPluginVersion()
		{
			return PluginVersionNumber;
		}

		public int GetNumTrackables()
		{
			return _trackables.Count;
		}

		public Trackable GetTrackable(int index)
		{
			return _trackables[index];
		}

		public bool IsTrackingRunning()
		{
			return _isTrackingRunning;
		}

		public void RemoveTrackable(string name)
		{
			// TODO: remove from plugin
			//_trackables.Remove();
		}

		public void ClearTrackables()
		{
			// TODO:
		}

		// Startup/shutdown
		public abstract bool InitPlugin();
		public abstract void DeinitPlugin();
		public abstract float GetNativePluginVersion();

		// Application
		public abstract void OnApplicationFocus(bool focusStatus);
		public abstract void OnApplicationPause(bool pauseStatus);

		// Cameras
		public abstract int GetNumCameras();

		// Start sources
		public abstract bool StartInputFromImage(Texture2D image);
		public abstract bool StartInputFromCamera(int deviceIndex, int targetWidth, int targetHeight);
		public abstract void StopInput();

		// Trackables
		public abstract bool AddTrackable(byte[] data, string id);

		// Fire events etc
		public abstract void UpdateTracking();

		public abstract void PostRender();
		
		// Tracking
		public abstract void StartTracking();
		public abstract void StopTracking();

		// Tracking Methods
		public abstract bool EnableTrackingMethod(int trackingMethodId);
		public abstract bool DisableTrackingMethod(int trackingMethodId);

		// Licensing
		public abstract void SetApiKey(string key, string bundleId);

		public abstract void ArbiTrackStart (Vector3 position, Quaternion orientation);
		public abstract bool ArbiTrackIsTracking ();
		public abstract void FloorPlaceGetPose (out Vector3 position, out Quaternion orientation);
		public abstract void ArbiTrackGetPose (out Vector3 position, out Quaternion orientation);


		public void SetupRenderingCamera(float cameraNearPlane, float cameraFarPlane)
		{
			_cameraNearPlane = cameraNearPlane;
			_cameraFarPlane = cameraFarPlane;
		}

		public bool AddTrackable(string path, string id)
		{
			bool result = false;
			if (System.IO.File.Exists(path))
			{
				byte[] data = System.IO.File.ReadAllBytes(path);
				result = AddTrackable(data, id);
			}
			else
			{
				Debug.LogError("[KudanAR] Missing file " + path);
			}

			return result;
		}

		public Texture GetTrackingTexture()
		{
			return _finalTexture;
		}

		public int GetNumDetectedTrackables()
		{
			return _detected.Count;
		}

		public Trackable GetDetectedTrackable(int index)
		{
			return _detected[index];
		}

		public Matrix4x4 GetProjectionMatrix()
		{
			return _projectionMatrix;
		}

		public Trackable[] GetDetectedTrackablesAsArray()
		{
			return _detected.ToArray();
		}


	}
};