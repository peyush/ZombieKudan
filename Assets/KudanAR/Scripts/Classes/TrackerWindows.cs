#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kudan.AR
{
	public class TrackerWindows : TrackerBase
	{
		private WebCamTexture _inputWebcam;
		private Texture2D _inputTexture;
		private Color32[] _rgbaPixels;
		private byte[] _monoPixels;
		private int _width, _height;
		private List<Trackable> _currentDetected = new List<Trackable>(8);

		public override bool InitPlugin()
		{
			return NativeInterface.Init();
		}

		public override void DeinitPlugin()
		{
			NativeInterface.Deinit();
		}

		public override float GetNativePluginVersion()
		{
			return NativeInterface.GetPluginVersion();
		}

		public override void OnApplicationFocus(bool focusStatus)
		{
		}

		public override void OnApplicationPause(bool pauseStatus)
		{
		}

		public override int GetNumCameras()
		{
			return WebCamTexture.devices.Length;
		}

		public override bool StartInputFromImage(Texture2D image)
		{
			// First stop existing input
			bool wasTracking = _isTrackingRunning;
			StopInput();

			// Start new input
			_inputTexture = image;
			CreateBuffersForTexture(_inputTexture);

			// Resume tracking
			_width = _inputTexture.width;
			_height = _inputTexture.height;
			_finalTexture = _inputTexture;
			if (wasTracking)
			{
				StartTracking();
			}

			return true;
		}

		public override bool StartInputFromCamera(int deviceIndex, int targetWidth, int targetHeight)
		{
			// First stop existing input
			bool wasTracking = _isTrackingRunning;
			StopInput();

			if (deviceIndex < GetNumCameras())
			{
				// Start new input
				_inputWebcam = new WebCamTexture(WebCamTexture.devices[deviceIndex].name, targetWidth, targetHeight, 30);
				if (_inputWebcam != null)
				{
					_inputWebcam.Play();
					if (_inputWebcam.isPlaying)
					{
						// Resume tracking
						_width = _inputWebcam.width;
						_height = _inputWebcam.height;
						_finalTexture = _inputWebcam;
						if (wasTracking)
						{
							StartTracking();
						}
					}
				}
			}
			return (_finalTexture != null);
		}

		private void CreateBuffersForTexture(Texture texture)
		{
			_rgbaPixels = new Color32[texture.width * texture.height];
			_monoPixels = new byte[texture.width * texture.height];
		}

		public override void StopInput()
		{
			StopTracking();

			if (_inputTexture != null)
			{
				_inputTexture = null;
			}
			if (_inputWebcam != null)
			{
				_inputWebcam.Stop();
				WebCamTexture.Destroy(_inputWebcam);
				_inputWebcam = null;
			}
			_rgbaPixels = null;
			_monoPixels = null;
			_finalTexture = null;
		}

		public override void SetApiKey(string key, string bundleId)
		{
			// Not implemented
		}

		// Trackables
		public override bool AddTrackable(byte[] data, string id)
		{
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			bool result = NativeInterface.AddTrackableSet(handle.AddrOfPinnedObject(), data.Length);
			handle.Free();

			if (result)
			{
				Trackable trackable = new Trackable();
				trackable.name = id;
				_trackables.Add(trackable);
			}

			return result;
		}

		private int _numFramesGrabbedLast;
		private int _numFramesTrackedLast;
		private int _numFramesProcessedLast;
		private int _numFramesRenderedLast;

		private int _numFramesGrabbed;
		private int _numFramesTracked;
		private int _numFramesProcessed;
		private int _numFramesRendered;
		private object _lock = new object();
		private float _rateTimer;

		private void UpdateFrameRates()
		{
			_rateTimer += Time.deltaTime;
			_numFramesRendered++;
			if (_rateTimer >= 1.0f)
			{
				_cameraRate = (float)(_numFramesGrabbed - _numFramesGrabbedLast) / _rateTimer;
				_trackerRate = (float)(_numFramesTracked - _numFramesTrackedLast) / _rateTimer;
				_appRate = (float)(_numFramesRendered - _numFramesRenderedLast) / _rateTimer;

				_numFramesGrabbedLast = _numFramesGrabbed;
				_numFramesTrackedLast = _numFramesTracked;
				_numFramesRenderedLast = _numFramesRendered;
				_rateTimer = 0f;
			}
		}

		public override void StartTracking()
		{
			if (_isTrackingRunning)
			{
				Debug.LogWarning("[KudanAR] Trying to start tracking when it's already running");
				return;
			}

			_trackingThread = new System.Threading.Thread(new System.Threading.ThreadStart(TrackingLoopThread));
			_trackingThread.Start();
			_isTrackingRunning = true;
		}

		public override void StopTracking()
		{
			if (_isTrackingRunning)
			{				
				_isTrackingRunning = false;
				if (_trackingThread != null)
				{
					// Stop the threading
					_trackingThread.Join();
				}
			}
		}

		public override bool EnableTrackingMethod(int trackingMethodId)
		{
			return NativeInterface.EnableTrackingMethod(trackingMethodId);
		}

		public override bool DisableTrackingMethod(int trackingMethodId)
		{
			return NativeInterface.DisableTrackingMethod(trackingMethodId);
		}

		public override void UpdateTracking()
		{
			// We can only update our webcam from the main thread...
			if (_inputWebcam != null && _inputWebcam.didUpdateThisFrame)
			{
				if (_rgbaPixels == null)
					CreateBuffersForTexture(_inputWebcam);

				_inputWebcam.GetPixels32(_rgbaPixels);
				_numFramesGrabbed++;
			}

			//if (_isTrackingRunning && _trackingThread == null)
			//	ThreadFunc();
			
			// Only process the markers if a new tracking has completed
			if (_numFramesProcessed != _numFramesTracked)
			{
				_numFramesProcessed = _numFramesTracked;

				// We lock here as the other thread may be trying to update the _currentDetected list
				lock(_lock)
				{
					// Copy the list of detected objects, or make a new list of it's empty
					if (_currentDetected != null)
						_detected = _currentDetected;
					else
						_detected = new List<Trackable>(8);
					_currentDetected = new List<Trackable>(8);

                    // Update projection matrix
                    float[] projectionFloats = new float[16];
                    NativeInterface.GetProjectionMatrix(_cameraNearPlane, _cameraFarPlane, projectionFloats);
                    _projectionMatrix = ConvertNativeFloatsToMatrix(projectionFloats, (float)_width / (float)_height);
				}
			}

			// Update our frame rates
			UpdateFrameRates();
		}

		public override void PostRender()
		{
		}

		private List<Trackable> GetDetected()
		{
			// Grab detected trackables from nativeland into C# Unity land
			int num = NativeInterface.GetNumberOfDetectedTrackables();
			List<Trackable> result = new List<Trackable>(num);
			for (int i = 0; i < num; i++)
			{
				Trackable trackable = new Trackable();
				StringBuilder sbName = new StringBuilder(512);
				int width = 0;
				int height = 0;
				float[] p = new float[7];
				int trackingMethod = 0;
				if (NativeInterface.GetDetectedTrackable(i, p, ref width, ref height, ref trackingMethod, sbName))
				{
					trackable.name = sbName.ToString();
					trackable.width = width;
					trackable.height = height;
					trackable.position = ConvertNativeFloatsToVector3(p[0], p[1], p[2]);
					trackable.orientation = ConvertNativeFloatsToQuaternion(p[3], p[4], p[5], p[6]);
					trackable.trackingMethod = trackingMethod;
					result.Add(trackable);
				}
			}
			return result;
		}

		void TrackingLoopThread()
		{
			while (_isTrackingRunning)
			{
				ThreadFunc();

				System.Threading.Thread.Sleep(1);
			}
		}

		private void ThreadFunc()
		{
			if (_inputWebcam != null && _numFramesGrabbed > _numFramesTracked)
			{
				// Really slow conversion to flip vertically and convert to mono
				for (int i = 0; i < _width; i++)
				{
					for (int j = 0; j < _height; j++)
					{
						int p = j * _width + i;
						byte grey = (byte)((_rgbaPixels[p].r + _rgbaPixels[p].g + _rgbaPixels[p].b) / 3);

						int p2 = ((_height - 1) - j) * _width + i;
						_monoPixels[p2] = grey;
					}
				}

				// Pin the memory so it isn't cleared by the GC
				GCHandle handle = GCHandle.Alloc(_monoPixels, GCHandleType.Pinned);

				//System.Threading.Thread.Sleep(100);

				// Wait for Kudan API to process the new frame
				NativeInterface.ProcessFrame(handle.AddrOfPinnedObject(), _width, _height, 0);

				handle.Free();

				// Process the detected markers
				// We lock here because _currentDetected may be being accessed by the UpdateTracking function
				lock (_lock)
				{
					_currentDetected = GetDetected();
				}

				_numFramesTracked++;
			}
		}

		// Utility functions for converting native data into Unity data
		public static Matrix4x4 ConvertNativeFloatsToMatrix(float[] r, float cameraAspect)
		{
			Matrix4x4 m = new Matrix4x4();
			m.SetRow(0, new Vector4(r[0], r[1], r[2], r[3]));
			m.SetRow(1, new Vector4(r[4], r[5], r[6], r[7]));
			m.SetRow(2, new Vector4(r[8], r[9], r[10], r[11]));
			m.SetRow(3, new Vector4(r[12], r[13], r[14], r[15]));

			// Scale the aspect ratio based on camera vs screen ratios
			float screenAspect = ((float)Screen.width / (float)Screen.height);
			float scale = cameraAspect / screenAspect;
			if (scale > 1f)
				m.m00 *= scale;
			else
				m.m11 /= scale;

			m = m.transpose;

			m.m02 *= -1f;
			m.m12 *= -1f;

			return m;
		}

		protected static Vector3 ConvertNativeFloatsToVector3(float x, float y, float z)
		{
			return new Vector3(-x, -y, -z);
		}

		protected static Quaternion ConvertNativeFloatsToQuaternion(float x, float y, float z, float w)
		{
			return new Quaternion(x, y, z, w) * Quaternion.AngleAxis(270f, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.left);
		}

		public override void ArbiTrackStart(Vector3 position, Quaternion orientation)
		{
			float[] f = new float[7];
			
			f[0] = position.x;
			f[1] = position.y;
			f[2] = position.z;
			
			f[3] = orientation.x;
			f[4] = orientation.y;
			f[5] = orientation.z;
			f[6] = orientation.w;
			
			NativeInterface.ArbiTrackStart(f);
		}
		
		public override bool ArbiTrackIsTracking()
		{
			return NativeInterface.ArbiTrackIsTracking();
		}
		
		
		public override void FloorPlaceGetPose(out Vector3 position, out Quaternion orientation)
		{
			float[] f = new float[7];
			
			NativeInterface.FloorPlaceGetPose(f, 200);
			
			position = new Vector3(f[0], f[1], f[2]);
			orientation = new Quaternion(f[3], f[4], f[5], f[6]);
		}
		
		public override void ArbiTrackGetPose(out Vector3 position, out Quaternion orientation)
		{
			float[] result = new float[7];
			NativeInterface.ArbiTrackGetPose(result);
			
			position = new Vector3(result[0], result[1], -result[2]);
			orientation = new Quaternion(result[3], result[4], result[5], result[6]);
		}
	}
};
#endif