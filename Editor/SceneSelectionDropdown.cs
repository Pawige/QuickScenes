using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace QuickScenes
{
	class SceneSelectionDropdown : AdvancedDropdown
	{
		public Action<string> SelectionMade;

		private readonly List<SceneFolder> _folderList;
		private Dictionary<AdvancedDropdownItem, string> _scenes;
		private readonly string _dropdownTitle;
		private readonly SavedData _cachedData;
		
		private const int MAXIMUM_VIEW_COUNT = 50;

		public SceneSelectionDropdown(AdvancedDropdownState state, string title, List<SceneFolder> folderList) : base(state)
		{
			_folderList = folderList;
			_cachedData = Utility.GetSavedData();
			int favoritesCount = _cachedData.FavoriteScenes.Count;
			int largestViewCount = Mathf.Max(_folderList.Count + 3 + favoritesCount);
			foreach (SceneFolder sceneFolder in _folderList)
			{
				largestViewCount = Mathf.Max(largestViewCount, sceneFolder.SceneGuids.Count + 3);
			}
			largestViewCount = Mathf.Min(largestViewCount, MAXIMUM_VIEW_COUNT);
			minimumSize = new Vector2(minimumSize.x, CalculateNeededHeight(largestViewCount));
			_dropdownTitle = title;
		}

		private float CalculateNeededHeight(int longestList)
		{
			return EditorGUIUtility.singleLineHeight * longestList;
		}

		protected override AdvancedDropdownItem BuildRoot()
		{
			var root = new AdvancedDropdownItem(_dropdownTitle);

			_scenes = new Dictionary<AdvancedDropdownItem, string>();
			foreach (SceneData favoriteScene in _cachedData.FavoriteScenes)
			{
				var sceneItem = new AdvancedDropdownItem(favoriteScene.SceneName);
				root.AddChild(sceneItem);	
				_scenes.TryAdd(sceneItem, favoriteScene.SceneGuid);
			}
			root.AddSeparator();
			foreach (SceneFolder sceneFolder in _folderList)
			{
				if (Utility.AreAllScenesInFolderHidden(_cachedData, sceneFolder))
					continue;
				
				var folderItem = new AdvancedDropdownItem(sceneFolder.FolderName);
				root.AddChild(folderItem);
				foreach (string sceneGuid in sceneFolder.SceneGuids)
				{
					if (_cachedData.HiddenScenes.Exists(sceneData => sceneData.SceneGuid == sceneGuid))
						continue;
					
					string path = AssetDatabase.GUIDToAssetPath(sceneGuid);
					string[] scenePathSplit = path.Split('/', '.');
					// Reminder: [^2] is the same as writing [scenePathSplit.Length - 2];
					string sceneName = scenePathSplit[^2];
					var sceneItem = new AdvancedDropdownItem(sceneName);
					folderItem.AddChild(sceneItem);	
					_scenes.TryAdd(sceneItem, sceneGuid);
				}
			}

			return root;
		}

		protected override void ItemSelected(AdvancedDropdownItem item)
		{
			SelectionMade.Invoke(_scenes[item]);
		}
	}
}