using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Kudan.AR
{
	[CustomEditor(typeof(TrackableData))]
	public class TrackableDataEditor : Editor
	{
		private TrackableData db;

		void Awake()
		{
			db = (TrackableData)target;
		}

		public override void OnInspectorGUI()
		{
			GUILayout.BeginVertical();

			EditorGUILayout.LabelField("ID:", db.id);
			int dataLength = 0;
			if (db.Data != null)
				dataLength = db.Data.Length;
			EditorGUILayout.LabelField("Size:", (dataLength / 1024) + " KB");
			/*if (db.image)
			{
				Rect r = GUILayoutUtility.GetRect(256f, 256f);
				EditorGUI.DrawPreviewTexture(r, db.image, null, ScaleMode.ScaleToFit);
			}*/

			this.DrawDefaultInspector();

			if (GUILayout.Button("Browse for KARMarker File"))
			{
				string path = UnityEditor.EditorUtility.OpenFilePanel("Kudan AR", "", "KARMarker");
				if (!string.IsNullOrEmpty(path))
				{
					db.Set(System.IO.Path.GetFileNameWithoutExtension(path), System.IO.File.ReadAllBytes(path));
					UnityEditor.EditorUtility.SetDirty(db);
				}
			}
			GUILayout.EndVertical();
		}
		
		public override bool HasPreviewGUI()
		{
			return true;
		}

		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			db = (TrackableData)target;

			Texture2D result = null;
			if (db != null && db.image != null)
				result = db.image;

			if (result != null)
				GUI.DrawTexture(r, result, ScaleMode.ScaleToFit);
		}
		/*
		public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			db = (TrackableData)target;

			if (_iconTexture)
				return _iconTexture;
			//if (db != null && db.image != null)
//				return db.image;
			return Texture2D.blackTexture;
		}*/

	}
}