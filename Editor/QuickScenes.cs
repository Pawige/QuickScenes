using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuickScenes
{
	public class QuickScenes : EditorWindow
	{
		private List<SceneFolder> _sceneFolders;

		private string[] _sceneList;
		//private SceneListSGO _sceneListObject;

		private Texture2D _buttonBg;
		List<EditorBuildSettingsScene> _editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
		private Vector2 _scrollPosition;
		public Object mainScene;
		public Object secondaryScene;
		private string _mainScene;

		[MenuItem("Window/Quick Scenes")]
		public static void ShowWindow()
		{
			GetWindow(typeof(QuickScenes));
		}

		private void OnEnable()
		{
			_sceneFolders = Utility.GenerateSceneLists();
			_sceneList = AssetDatabase.FindAssets("t:scene", new[] { "Assets/Scenes" });
		}

		private void OnGUI()
		{
			if (EditorApplication.isPlaying)
				return;


			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Refresh", GUILayout.Width(100)))
			{
				_sceneFolders = Utility.GenerateSceneLists();
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUI.skin.button.fontStyle = FontStyle.Bold;
			Color guiColor = GUI.color;
			if (GUILayout.Button("Load Main Scene", GUILayout.Height(40), GUILayout.Width(150)))
			{
				if (mainScene == null)
					ShowNotification(new GUIContent("No object selected for searching"));
				else
				{
					var newScene = mainScene as SceneAsset;
					var newPath = AssetDatabase.GetAssetPath(newScene);
					Utility.LoadScene(newPath);
				}
			}

			mainScene = EditorGUILayout.ObjectField(mainScene, typeof(SceneAsset), false);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUI.skin.button.fontStyle = FontStyle.Normal;
			if (GUILayout.Button("Load Secondary Scene", GUILayout.Height(40), GUILayout.Width(150)))
			{
				if (secondaryScene == null)
					ShowNotification(new GUIContent("No object selected for searching"));
				else
				{
					var newScene = secondaryScene as SceneAsset;
					var newPath = AssetDatabase.GetAssetPath(newScene);
					Utility.LoadScene(newPath);
				}
			}

			secondaryScene = EditorGUILayout.ObjectField(secondaryScene, typeof(SceneAsset), false);
			GUILayout.EndHorizontal();

			if (_sceneFolders == null)
				return;

			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
			foreach (SceneFolder sceneFolder in _sceneFolders)
			{
				EditorGUILayout.BeginHorizontal();
				sceneFolder.FolderOpen = EditorGUILayout.Foldout(sceneFolder.FolderOpen, sceneFolder.FolderName);
				EditorGUILayout.LabelField(sceneFolder.SceneGuids.Count.ToString(), GUILayout.Width(50));
				EditorGUILayout.EndHorizontal();
				if (!sceneFolder.FolderOpen)
					continue;

				foreach (string guid in sceneFolder.SceneGuids)
				{
					string path = AssetDatabase.GUIDToAssetPath(guid);
					string[] scenePathSplit = path.Split('/', '.');
					string sceneName = scenePathSplit[scenePathSplit.Length - 2];

					EditorGUILayout.BeginHorizontal();
					if (SceneManager.GetActiveScene().name != sceneName)
					{
						if (GUILayout.Button(sceneName))
						{
							Utility.LoadScene(path);
						}

						if (GUILayout.Button("+", GUILayout.Width(22)))
						{
							Utility.AddScene(path);
						}
					}
					else
					{
						GUI.skin.button.fontStyle = FontStyle.Bold;
						GUILayout.Button($"Current: {sceneName}");
					}

					GUI.skin.button.fontStyle = FontStyle.Normal;
					GUI.color = guiColor;
					if (GUILayout.Button("Find", GUILayout.Width(35)))
					{
						Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
					}

					EditorGUILayout.EndHorizontal();
				}
			}

			EditorGUILayout.EndScrollView();
		}
	}
}