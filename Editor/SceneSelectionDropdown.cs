using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly HashSet<string> _hiddenSceneGuids;
		
		private const int MAXIMUM_VIEW_COUNT = 50;

		public SceneSelectionDropdown(AdvancedDropdownState state, string title, List<SceneFolder> folderList) : base(state)
		{
			_folderList = folderList;
			_cachedData = Utility.GetSavedData();
			_hiddenSceneGuids = Utility.GetHiddenSceneGuids();
			int favoritesCount = _cachedData.FavoriteScenes.Count;
			int largestViewCount = Mathf.Max(_folderList.Count + 2 + favoritesCount);
			foreach (SceneFolder sceneFolder in _folderList)
			{
				largestViewCount = Mathf.Max(largestViewCount, sceneFolder.SceneGuids.Count + 2);
			}
			largestViewCount = Mathf.Min(largestViewCount, MAXIMUM_VIEW_COUNT);
			minimumSize = new Vector2(EditorGUIUtility.singleLineHeight * 14, CalculateNeededHeight(largestViewCount));
			_dropdownTitle = title;
		}

		private static float CalculateNeededHeight(int longestList)
		{
			return EditorGUIUtility.singleLineHeight * longestList;
		}

		protected override AdvancedDropdownItem BuildRoot()
		{
			var root = new AdvancedDropdownItem(_dropdownTitle);
			_scenes = new Dictionary<AdvancedDropdownItem, string>();

			AddFavoriteScenes(root);
			AddSceneFolders(root);
			AddEmptyState(root);

			return root;
		}
		
		protected override void ItemSelected(AdvancedDropdownItem item)
		{
			if (_scenes != null && _scenes.TryGetValue(item, out string sceneGuid))
			{
				SelectionMade?.Invoke(sceneGuid);
			}
		}

		private void AddFavoriteScenes(AdvancedDropdownItem root)
		{
			foreach (SceneData favoriteScene in _cachedData.FavoriteScenes)
			{
				var sceneItem = new AdvancedDropdownItem(favoriteScene.SceneName);
				root.AddChild(sceneItem);
				_scenes[sceneItem] = favoriteScene.SceneGuid;
			}

			if (_cachedData.FavoriteScenes.Count > 0 && _folderList.Count > 0)
			{
				root.AddSeparator();
			}
		}

		private void AddSceneFolders(AdvancedDropdownItem root)
		{
			foreach (SceneFolder sceneFolder in _folderList)
			{
				if (Utility.AreAllScenesInFolderHidden(_hiddenSceneGuids, sceneFolder))
					continue;

				int childCount = 0;
				var folderItem = new AdvancedDropdownItem(sceneFolder.FolderName);
				for (var i = 0; i < sceneFolder.SceneGuids.Count; i++)
				{
					string sceneGuid = sceneFolder.SceneGuids[i];
					if (_hiddenSceneGuids.Contains(sceneGuid))
						continue;

					string sceneName = sceneFolder.SceneNames[i];

					var sceneItem = new AdvancedDropdownItem(sceneName);
					folderItem.AddChild(sceneItem);
					childCount++;
					_scenes[sceneItem] = sceneGuid;
				}

				if (childCount > 0)
				{
					root.AddChild(folderItem);
				}
			}
		}

		private static void AddEmptyState(AdvancedDropdownItem root)
		{
#if UNITY_6000_5_OR_NEWER
			if (root.childList.Any())
				return;
#else 
			if (root.children.Any())
				return;
#endif
			var emptyItem = new AdvancedDropdownItem("No visible scenes found")
			{
				enabled = false
			};
			root.AddChild(emptyItem);
		}
	}
}
