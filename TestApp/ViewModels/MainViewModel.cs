using System.ComponentModel.Composition;
using Caliburn.Micro;
using ChildWindowManager.Interfaces;

namespace TestApp.ViewModels
{
	[Export]
	public class MainViewModel : Screen
	{
		public void OpenSecondWindow()
		{
			IoC.Get<IChildWindowManager>().ShowOverlayWindow(IoC.Get<SecondViewModel>());
		}

		public void OpenChildWindow()
		{
			IoC.Get<IChildWindowManager>().ShowModalChildWindow(IoC.Get<ChildViewModel>());
		}
	}
}