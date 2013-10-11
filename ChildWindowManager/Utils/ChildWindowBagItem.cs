using System;
using Caliburn.Micro;

namespace ChildWindowManager.Utils
{
	public class ChildWindowBagItem
	{
		public ChildWindowBagItem()
		{
			Id = Guid.NewGuid();
		}

		public ChildWindowBagItem(ChildWindowType windowType, Screen viewModel) : this()
		{
			WindowType = windowType;
			ViewModel = viewModel;
		}

		public Guid Id { get; private set; }
		public ChildWindowType WindowType { get; set; }
		public Screen ViewModel { get; set; }
	}
}