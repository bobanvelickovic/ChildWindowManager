using System.ComponentModel.Composition;
using Caliburn.Micro;
using ChildWindowManager.Interfaces;

namespace TestApp.ViewModels
{
	[Export]
	public class ChildViewModel : Screen
	{
		public void Close()
		{
			IoC.Get<IChildWindowManager>().RemoveChildWindow(this);
		}
	}
}