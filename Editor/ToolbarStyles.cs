using UnityEngine;

namespace QuickScenes
{
	static class ToolbarStyles
	{
		public static readonly GUIStyle commandButtonStyle;
		public static readonly GUIStyle toolbarButtonStyle;
		public static readonly GUIStyle iconButtonStyle;

		static ToolbarStyles()
		{
			commandButtonStyle = new GUIStyle("DropDown")
			{
				fixedHeight = 20,
				fixedWidth = 100
			};
			toolbarButtonStyle = new GUIStyle(GUI.skin.button)
			{
				fixedHeight = 20,
				fixedWidth = 21,
				padding = new RectOffset(3,1,2,3),
			};
			iconButtonStyle = new GUIStyle(GUI.skin.button)
			{
				fixedHeight = 18,
				fixedWidth = 20,
				padding = new RectOffset(3,1,2,3),
			};
		}
	}
}