using System.Windows.Controls;
using Caliburn.Micro;
using ChildWindowManager.Utils;
using Xceed.Wpf.Toolkit;

namespace ChildWindowManager.Interfaces
{
	public interface IChildWindowManager : IWindowManager
	{
		ChildWindowAnimation WindowAnimationType { get; set; }
		ChildWindowAnimation OverlayAnimationType { get; set; }

		void ShowChildWindow(Screen rootModel);
		void ShowModalChildWindow(Screen rootModel);
		void RemoveChildWindow(ChildWindow view);
		void RemoveChildWindow(Screen rootModel);

		void ShowOverlayWindow(Screen rootModel);
		void RemoveOverlayWindow(UserControl view);
		void RemoveOverlayWindow(Screen rootModel);
		void RemoveAllWindows();
	}
}