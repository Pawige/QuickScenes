using UnityEngine;

namespace QuickScenes
{
	static class ToolbarStyles
	{
		public static readonly GUIStyle commandButtonStyle;
		public static readonly GUIStyle iconButtonStyle;

		static ToolbarStyles()
		{
			commandButtonStyle = new GUIStyle("DropDown")
			{
				fixedHeight = 20,
				fixedWidth = 100
			};
			iconButtonStyle = new GUIStyle(GUI.skin.button)
			{
				fixedHeight = 20,
				fixedWidth = 21,
				padding = new RectOffset(4,3,4,4),
			};
		}
	}
}