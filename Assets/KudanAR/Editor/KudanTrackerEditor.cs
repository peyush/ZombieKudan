using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Kudan.AR
{
	[CustomEditor(typeof(KudanTracker))]
	public class KudanTrackerEditor : Editor
	{
		public Texture2D _kudanLogo;
		//private KudanTracker _target;

		void Awake()
		{
			//_target = (KudanTracker)target;
		}

		public override void OnInspectorGUI()
		{
			GUILayout.BeginVertical();


			if (_kudanLogo != null)
			{
				Rect r = GUILayoutUtility.GetRect(_kudanLogo.width, _kudanLogo.height + 32f);
				GUI.DrawTexture(r, _kudanLogo, ScaleMode.ScaleToFit);
			}

			this.DrawDefaultInspector();

			GUILayout.Space(16f);

			GUILayout.Label("App ID: " + Application.bundleIdentifier);
			if (GUILayout.Button("Set App ID"))
			{
				EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
			}

			if (GUILayout.Button("Get new API Key"))
			{
				Application.OpenURL("https://www.kudan.eu/developers/");
			}

			
			//TrackingMethodBase[] trackers = (TrackingMethodBase[])Resources.FindObjectsOfTypeAll(typeof(TrackingMethodBase));
			
			//typeof(TrackingMethodMarkerless)

			bool externalOperation = false;

			GUILayout.EndVertical();

			if (externalOperation)
			{
				// This has to be here otherwise we get strange GUI stack exceptions
				EditorGUIUtility.ExitGUI();
			}			
		}
	}
}