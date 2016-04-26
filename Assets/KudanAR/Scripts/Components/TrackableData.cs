using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kudan.AR
{
	[System.Serializable]
	public class TrackableData : ScriptableObject
	{
		public string id;

		[Header("Optional")]

		public Texture2D image;		// optional
		[HideInInspector]
		public byte[] data;

		public string ID
		{
			get { return id; }
		}

		public byte[] Data
		{
			get { return data; }
		}

		public void Set(string id, byte[] data)
		{
			this.id = id;
			this.data = data;
		}

#if UNITY_EDITOR
		[Multiline(8)]
		public string notes;		// optional
#endif

#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/Kudan AR Trackable Data")]
		public static void EditorCreateIssue()
		{
			string path = UnityEditor.EditorUtility.OpenFilePanel("Kudan AR", "", "KARMarker");
			if (!string.IsNullOrEmpty(path))
			{
				TrackableData obj = ScriptableObject.CreateInstance<TrackableData>();
				UnityEditor.AssetDatabase.CreateAsset(obj, "Assets/NewKudanTrackable.asset");
				UnityEditor.AssetDatabase.SaveAssets();

				obj.id = System.IO.Path.GetFileNameWithoutExtension(path);
				obj.data = System.IO.File.ReadAllBytes(path);
				UnityEditor.EditorUtility.SetDirty(obj);

				UnityEditor.EditorUtility.FocusProjectWindow();
				UnityEditor.Selection.activeObject = obj;
			}
		}
#endif
	}
}