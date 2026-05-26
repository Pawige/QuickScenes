using UnityEngine;

namespace QuickScenes
{
	static class ToolbarStyles
	{
		public static readonly GUIStyle iconButtonStyle;

		static ToolbarStyles()
		{
			iconButtonStyle = new GUIStyle(GUI.skin.button)
			{
				fixedHeight = 18,
				fixedWidth = 20,
				padding = new RectOffset(3,1,2,3),
			};
		}
	}
}
