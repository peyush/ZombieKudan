//#define NULL_PLUGIN
using UnityEngine;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;

namespace Kudan.AR
{
#if UNITY_STANDALONE_WIN || UNITY_ANDROID || UNITY_IOS
	public class NativeInterface
	{
#if UNITY_STANDALONE_WIN || UNITY_ANDROID
		private const string PLUGIN_FILE = "KudanPlugin";
#elif UNITY_IOS
		private const string PLUGIN_FILE = "__Internal";
#endif

#if !NULL_PLUGIN
		[DllImport(PLUGIN_FILE)]
		public static extern bool Init();
		
		[DllImport(PLUGIN_FILE)]
		public static extern void Deinit();
		
		[DllImport(PLUGIN_FILE)]
		public static extern float GetPluginVersion();

		[DllImport(PLUGIN_FILE)]
		public static extern bool AddTrackableSet(System.IntPtr dataPointer, int dataLength);

		[DllImport(PLUGIN_FILE)]
		public static extern void ProcessFrame(System.IntPtr dataPointer, int width, int height, int padding);

		[DllImport(PLUGIN_FILE)]
		public static extern int GetNumberOfTrackables();

		[DllImport(PLUGIN_FILE)]
		public static extern int GetNumberOfDetectedTrackables();

		[DllImport(PLUGIN_FILE)]
		public static extern bool GetProjectionMatrix(float nearPlane, float farPlane, float[] result);

		[DllImport(PLUGIN_FILE)]
		public static extern bool GetDetectedTrackable(int index, float[] result, ref int width, ref int height, ref int trackingMethod, StringBuilder name);

		[DllImport(PLUGIN_FILE)]
		public static extern bool EnableTrackingMethod(int trackingMethodId);

		[DllImport(PLUGIN_FILE)]
		public static extern bool DisableTrackingMethod(int trackingMethodId);

        [DllImport(PLUGIN_FILE)]
        public static extern void ArbiTrackGetPose(float[] result);

        [DllImport(PLUGIN_FILE)]
        public static extern void ArbiTrackStart(float[] pose);

        [DllImport(PLUGIN_FILE)]
        public static extern bool ArbiTrackIsTracking();

        [DllImport(PLUGIN_FILE)]
        public static extern void FloorPlaceGetPose(float[] pose, float depth);


#elif NULL_PLUGIN

		public static bool Init()
		{
			return true; 
		}
		
		public static void Deinit()
		{ 
		}
		
		public static float GetPluginVersion()
		{
			return 0.0f;
		}

		public static bool AddTrackableSet(System.IntPtr dataPointer, int dataLength)
		{
			return true;
		}

		public static void ProcessFrame(System.IntPtr dataPointer, int width, int height, int padding)
		{
		}

		public static int GetNumberOfTrackables()
		{
			return 0;
		}

		public static int GetNumberOfDetectedTrackables()
		{
			return 0;
		}

		public static bool GetProjectionMatrix(float nearPlane, float farPlane, float[] result)
		{
			return false;
		}

		public static bool GetDetectedTrackable(float[] result, StringBuilder name, ref int width, ref int height)
		{
		}
#endif
	}
#endif
};