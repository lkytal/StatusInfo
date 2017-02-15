using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lkytal.StatusInfo
{
	internal class StatusBarInjector
	{
		private readonly Window MainWindow;

		private FrameworkElement statusBar;

		private Panel panel;

		public StatusBarInjector(Window pMainWindow)
		{
			MainWindow = pMainWindow;
			MainWindow.Initialized += MainWindowInitialized;

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
				FrameworkElement frameworkElement = child as FrameworkElement;
				if (frameworkElement != null && frameworkElement.Name == childName)
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
			statusBar = FindChild(MainWindow, "StatusBarContainer") as FrameworkElement;
			var frameworkElement = statusBar;
			if (frameworkElement != null) panel = frameworkElement.Parent as DockPanel;
		}

		public void InjectControl(FrameworkElement pControl)
		{
			panel.Dispatcher.Invoke(() => {
				pControl.SetValue(DockPanel.DockProperty, Dock.Right);
				panel.Children.Insert(1, pControl);
			});
		}

		public bool IsInjected(FrameworkElement pControl)
		{
			bool flag2 = false;
			panel.Dispatcher.Invoke(() => {
				bool flag = panel.Children.Contains(pControl);
				bool flag1 = flag;
				flag2 = flag;
				return flag1;
			});
			return flag2;
		}

		public void UninjectControl(FrameworkElement pControl)
		{
			panel.Dispatcher.Invoke(() => panel.Children.Remove(pControl));
		}

		private void MainWindowInitialized(object sender, EventArgs e)
		{
		}
	}
}