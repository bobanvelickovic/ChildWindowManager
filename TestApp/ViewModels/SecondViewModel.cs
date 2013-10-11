using System.ComponentModel.Composition;
using Caliburn.Micro;
using ChildWindowManager.Interfaces;

namespace TestApp.ViewModels
{
	[Export]
	public class SecondViewModel : Screen
	{
		 public void Close()
		 {
		 	IoC.Get<IChildWindowManager>().RemoveOverlayWindow(this);
		 }
	}
}