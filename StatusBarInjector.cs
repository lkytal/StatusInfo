using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lkytal.StatusInfo
{
	internal class StatusBarInjector
	{
		private readonly Window mainWindow;

		private FrameworkElement statusBar;

		private Panel panel;

		public StatusBarInjector(Window pMainWindow)
		{
			mainWindow = pMainWindow;
			mainWindow.Initialized += MainWindowInitialized;

			FindStatusBar();
		}

		private static DependencyObject FindChild(DependencyObject parent, string childName)
		{
			if (parent == null)
			{
				return null;
			}

			int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childrenCount; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);

				if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
				{
					return frameworkElement;
				}

				child = FindChild(child, childName);

				if (child != null)
				{
					return child;
				}
			}

			return null;
		}

		private void FindStatusBar()
		{
			statusBar = FindChild(mainWindow, "StatusBarContainer") as FrameworkElement;
			var frameworkElement = statusBar;

			if (frameworkElement != null)
			{
				panel = frameworkElement.Parent as DockPanel;
			}
		}

		private void RefindStatusBar()
		{
			if (panel == null)
			{
				FindStatusBar();
			}
		}

		public void InjectControl(FrameworkElement pControl)
		{
			RefindStatusBar();

			panel?.Dispatcher.Invoke(() => {
				pControl.SetValue(DockPanel.DockProperty, Dock.Right);
				panel.Children.Insert(1, pControl);
			});
		}

		public bool IsInjected(FrameworkElement pControl)
		{
			RefindStatusBar();

			var flag = false;

			panel?.Dispatcher.Invoke(() => {
				flag = panel.Children.Contains(pControl);
			});

			return flag;
		}

		public void UninjectControl(FrameworkElement pControl)
		{
			RefindStatusBar();

			panel?.Dispatcher.Invoke(() => panel.Children.Remove(pControl));
		}

		private void MainWindowInitialized(object sender, EventArgs e)
		{
		}
	}
}