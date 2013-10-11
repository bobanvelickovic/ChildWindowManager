using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using ChildWindowManager.Interfaces;
using ChildWindowManager.Utils;
using Xceed.Wpf.Toolkit;
using WindowStartupLocation = Xceed.Wpf.Toolkit.WindowStartupLocation;
using WindowState = Xceed.Wpf.Toolkit.WindowState;

namespace ChildWindowManager
{
	/// <summary>
	/// Custom child window manager based on Caliburn.Micro's WindowManager
	/// </summary>
	public class ChildWindowManager : WindowManager, IChildWindowManager
	{
		private readonly TimeSpan defaultAnimationTime = new TimeSpan(0, 0, 0, 0, 125);

		public ChildWindowManager()
		{
			ChildWindowBag = new List<ChildWindowBagItem>();
		}

		public ChildWindowManager(ChildWindowAnimation windowAninmationType, ChildWindowAnimation overlayAnimationType)
		{
			ChildWindowBag = new List<ChildWindowBagItem>();
			WindowAnimationType = windowAninmationType;
			OverlayAnimationType = overlayAnimationType;
		}

		public ChildWindowAnimation WindowAnimationType { get; set; }
		public ChildWindowAnimation OverlayAnimationType { get; set; }

		public List<ChildWindowBagItem> ChildWindowBag { get; private set; }

		public void RemoveAllWindows()
		{
			while (ChildWindowBag.Count > 0)
			{
				var bagItem = ChildWindowBag.First();
				switch (bagItem.WindowType)
				{
					case ChildWindowType.Window:
					case ChildWindowType.Modal:
						RemoveChildWindow(bagItem.ViewModel);
						break;
					default:
						RemoveOverlayWindow(bagItem.ViewModel);
						break;
				}
			}
		}

		/// <summary>
		/// Shows the overlay window.
		/// </summary>
		/// <param name="rootModel">The root model.</param>
		public void ShowOverlayWindow(Screen rootModel)
		{
			PrepareWindow<UserControl>(rootModel, ChildWindowType.Overlay);
		}

		/// <summary>
		/// Removes the overlay window.
		/// </summary>
		/// <param name="view">The view.</param>
		public void RemoveOverlayWindow(UserControl view)
		{
			RemoveWindow(view, ChildWindowType.Overlay);
		}

		/// <summary>
		/// Removes the overlay window.
		/// </summary>
		/// <param name="rootModel">The viewmodel of the child window.</param>
		public void RemoveOverlayWindow(Screen rootModel)
		{
			var childWindowBagItem = ChildWindowBag.Find(ci => ci.ViewModel.Equals(rootModel) && ci.WindowType == ChildWindowType.Overlay);
			ChildWindowBag.Remove(childWindowBagItem);
			RemoveWindow<UserControl>(rootModel, ChildWindowType.Overlay);
		}

		/// <summary>
		/// Shows the modal child window.
		/// </summary>
		/// <param name="rootModel">The viewmodel of the child window.</param>
		public void ShowModalChildWindow(Screen rootModel)
		{
			PrepareWindow<ChildWindow>(rootModel, ChildWindowType.Modal, true);
		}

		/// <summary>
		/// Shows the child window.
		/// </summary>
		/// <param name="rootModel">The viewmodel of the child window.</param>
		public void ShowChildWindow(Screen rootModel)
		{
			PrepareWindow<ChildWindow>(rootModel, ChildWindowType.Window);
		}

		/// <summary>
		/// Removes the child window.
		/// </summary>
		/// <param name="view">The view (ChildWindow of WPFToolKit.Extended).</param>
		public void RemoveChildWindow(ChildWindow view)
		{
			RemoveWindow(view, ChildWindowType.Window);
		}

		/// <summary>
		/// Removes the child window.
		/// </summary>
		/// <param name="rootModel">The viewmodel of the child window.</param>
		public void RemoveChildWindow(Screen rootModel)
		{
			RemoveWindow<ChildWindow>(rootModel, ChildWindowType.Window);
		}

		protected void PrepareWindow<T>(Screen rootModel, ChildWindowType windowType, bool isModal = false) where T : ContentControl
		{
			var content = Application.Current.MainWindow.Content as Grid;
			if (content == null)
				return;

			var rowCount = content.RowDefinitions.Count;
			var columnCount = content.ColumnDefinitions.Count;

			var view = ViewLocator.LocateForModel(rootModel, null, null);
			if (view == null)
				return;

			if (isModal && content.Children.Contains(view))
				return;

			if (typeof(T) == typeof(ChildWindow))
			{
				((ChildWindow)view).WindowState = WindowState.Open;
				((ChildWindow)view).WindowStartupLocation = WindowStartupLocation.Center;
				((ChildWindow)view).IsModal = isModal;
			}

			Grid.SetColumn(view, 0);
			Grid.SetColumnSpan(view, columnCount == 0 ? 1 : columnCount);
			Grid.SetRow(view, 0);
			Grid.SetRowSpan(view, rowCount == 0 ? 1 : rowCount);

			ViewModelBinder.Bind(rootModel, view, null);
			content.Children.Add(view);

			ChildWindowBag.Add(new ChildWindowBagItem(windowType, rootModel));

			var animationType = typeof(T) == typeof(ChildWindow) ? WindowAnimationType : OverlayAnimationType;
			PrepareAnimation(view as ContentControl, content.ActualHeight.Equals(0) ? content.Height : content.ActualHeight, animationType);
		}

		protected void RemoveWindow<T>(T view, ChildWindowType windowType) where T : ContentControl
		{
			var rootModel = ViewModelLocator.LocateForView(view);
			var childWindowBagItem = ChildWindowBag.Find(ci => ci.ViewModel.Equals(rootModel) && ci.WindowType == windowType);
			ChildWindowBag.Remove(childWindowBagItem);

			var mainWindow = Application.Current.MainWindow;
			var content = mainWindow.Content as Grid;
			if (content == null)
				return;

			var animationType = typeof(T) == typeof(ChildWindow) ? WindowAnimationType : OverlayAnimationType;
			RemoveAnimation(view, content, animationType);
		}

		protected void RemoveWindow<T>(Screen rootModel, ChildWindowType windowType) where T : ContentControl
		{
			var view = rootModel.GetView() as T;
			if (view == null)
				return;
			RemoveWindow(view, windowType);
		}

		#region Animations
		protected void PrepareAnimation(ContentControl view, double contentHeight, ChildWindowAnimation animationType = ChildWindowAnimation.None)
		{
			switch (animationType)
			{
				case ChildWindowAnimation.Fade:
					var fadeDa = new DoubleAnimation(0, 1, new Duration(defaultAnimationTime));
					Storyboard.SetTarget(fadeDa, view);
					Storyboard.SetTargetProperty(fadeDa, new PropertyPath("Opacity"));

					var fadeSb = new Storyboard();
					fadeSb.Children.Add(fadeDa);
					fadeSb.Begin();
					break;
				case ChildWindowAnimation.Grown:
					var scale = new ScaleTransform(0.1, 0.1);
					var transformGroup = new TransformGroup();
					transformGroup.Children.Add(scale);
					view.RenderTransform = transformGroup;
					view.RenderTransformOrigin = new Point(0.5, 0.5);

					var xScaleAnimation = new DoubleAnimationUsingKeyFrames();
					var xKeyFrame = new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(defaultAnimationTime));
					xScaleAnimation.KeyFrames = new DoubleKeyFrameCollection { xKeyFrame };
					Storyboard.SetTarget(xScaleAnimation, view);
					Storyboard.SetTargetProperty(xScaleAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));

					var yScaleAnimation = new DoubleAnimationUsingKeyFrames();
					var yKeyFrame = new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(defaultAnimationTime));
					yScaleAnimation.KeyFrames = new DoubleKeyFrameCollection { yKeyFrame };
					Storyboard.SetTarget(yScaleAnimation, view);
					Storyboard.SetTargetProperty(yScaleAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

					var grownSb = new Storyboard();
					grownSb.Children.Add(xScaleAnimation);
					grownSb.Children.Add(yScaleAnimation);
					grownSb.Begin();
					break;
				case ChildWindowAnimation.Fall:
					view.Margin = new Thickness(0, (-1) * contentHeight, 0, contentHeight);

					var fallAnimation = new ThicknessAnimationUsingKeyFrames();
					var fallKeyFrame = new EasingThicknessKeyFrame(new Thickness(0), KeyTime.FromTimeSpan(defaultAnimationTime));
					fallAnimation.KeyFrames = new ThicknessKeyFrameCollection { fallKeyFrame };
					Storyboard.SetTarget(fallAnimation, view);
					Storyboard.SetTargetProperty(fallAnimation, new PropertyPath("(FrameworkElement.Margin)"));
					var fallSb = new Storyboard();
					fallSb.Children.Add(fallAnimation);
					fallSb.Begin();
					break;
			}
		}

		protected void RemoveAnimation(ContentControl view, Grid content, ChildWindowAnimation animationType = ChildWindowAnimation.None)
		{
			switch (animationType)
			{
				case ChildWindowAnimation.Fade:
					var fadeDa = new DoubleAnimation(1, 0, new Duration(defaultAnimationTime));
					Storyboard.SetTarget(fadeDa, view);
					Storyboard.SetTargetProperty(fadeDa, new PropertyPath("Opacity"));

					var fadeSb = new Storyboard();
					fadeSb.Children.Add(fadeDa);
					fadeSb.Completed += (s, e) => content.Children.Remove(view);
					fadeSb.Begin();
					break;
				case ChildWindowAnimation.Grown:
					var scale = new ScaleTransform(1, 1);
					var transformGroup = new TransformGroup();
					transformGroup.Children.Add(scale);
					view.RenderTransform = transformGroup;
					view.RenderTransformOrigin = new Point(0.5, 0.5);

					var xScaleAnimation = new DoubleAnimationUsingKeyFrames();
					var xKeyFrame = new EasingDoubleKeyFrame(0.1, KeyTime.FromTimeSpan(defaultAnimationTime));
					xScaleAnimation.KeyFrames = new DoubleKeyFrameCollection { xKeyFrame };
					Storyboard.SetTarget(xScaleAnimation, view);
					Storyboard.SetTargetProperty(xScaleAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));

					var yScaleAnimation = new DoubleAnimationUsingKeyFrames();
					var yKeyFrame = new EasingDoubleKeyFrame(0.1, KeyTime.FromTimeSpan(defaultAnimationTime));
					yScaleAnimation.KeyFrames = new DoubleKeyFrameCollection { yKeyFrame };
					Storyboard.SetTarget(yScaleAnimation, view);
					Storyboard.SetTargetProperty(yScaleAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

					var grownSb = new Storyboard();
					grownSb.Children.Add(xScaleAnimation);
					grownSb.Children.Add(yScaleAnimation);
					grownSb.Completed += (s, e) => content.Children.Remove(view);
					grownSb.Begin();
					break;
				case ChildWindowAnimation.Fall:
					var contentHeight = content.ActualHeight.Equals(0) ? content.Height : content.ActualHeight;

					var fallAnimation = new ThicknessAnimationUsingKeyFrames();
					var fallKeyFrame = new EasingThicknessKeyFrame(new Thickness(0, (-1) * contentHeight, 0, contentHeight), KeyTime.FromTimeSpan(defaultAnimationTime));
					fallAnimation.KeyFrames = new ThicknessKeyFrameCollection { fallKeyFrame };
					Storyboard.SetTarget(fallAnimation, view);
					Storyboard.SetTargetProperty(fallAnimation, new PropertyPath("(FrameworkElement.Margin)"));
					var fallSb = new Storyboard();
					fallSb.Children.Add(fallAnimation);
					fallSb.Completed += (s, e) => content.Children.Remove(view);
					fallSb.Begin();
					break;
				default:
					content.Children.Remove(view);
					break;
			}
		}
		#endregion
	}
}
