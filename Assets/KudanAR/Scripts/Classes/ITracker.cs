using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kudan.AR
{
	public interface ITracker
	{
		// Startup/shutdown
		bool InitPlugin();
		void DeinitPlugin();
		float GetPluginVersion();
		float GetNativePluginVersion();

		// Cameras
		int GetNumCameras();

		// Rendering camera properties
		void SetupRenderingCamera(float cameraNearPlane, float cameraFarPlane);

		// Start sources
		bool StartInputFromImage(Texture2D image);
		bool StartInputFromCamera(int deviceIndex, int targetWidth, int targetHeight);
		void StopInput();

		// Trackables
		bool AddTrackable(string path, string id);
		bool AddTrackable(byte[] data, string id);
		int GetNumTrackables();
		Trackable GetTrackable(int index);
		void RemoveTrackable(string name);
		void ClearTrackables();

		// Fire events etc
		void UpdateTracking();
		
		// Tracking
		void StartTracking();
		bool IsTrackingRunning();
		void StopTracking();

		// Tracking Methods
		bool EnableTrackingMethod(int trackingMethodId);
		bool DisableTrackingMethod(int trackingMethodId);
		
		// Texture
		Texture GetTrackingTexture();
		
		// Detected results
		int GetNumDetectedTrackables();
		Trackable GetDetectedTrackable(int index);
		Matrix4x4 GetProjectionMatrix();
	}
};