using Caliburn.Micro;
using ChildWindowManager.Interfaces;

namespace TestApp.Views
{
	/// <summary>
	/// Interaction logic for ChildView.xaml
	/// </summary>
	public partial class ChildView
	{
		public ChildView()
		{
			InitializeComponent();
		}

		private void ChildWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			IoC.Get<IChildWindowManager>().RemoveChildWindow(this);
		}
	}
}
