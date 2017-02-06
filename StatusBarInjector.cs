using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Lkytal.StatusInfo
{
	internal class StatusBarInjector
	{
		private Window window;

		private FrameworkElement statusBar;

		private Panel panel;

		public StatusBarInjector(Window pWindow)
		{
			this.window = pWindow;
			this.window.Initialized += this.window_Initialized;

			this.FindStatusBar();
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
			this.statusBar = FindChild(this.window, "StatusBarContainer") as FrameworkElement;
			var frameworkElement = this.statusBar;
			if (frameworkElement != null) this.panel = frameworkElement.Parent as DockPanel;
		}

		public void InjectControl(FrameworkElement pControl)
		{
			this.panel.Dispatcher.Invoke(() => {
				pControl.SetValue(DockPanel.DockProperty, Dock.Right);
				this.panel.Children.Insert(1, pControl);
			});
		}

		public bool IsInjected(FrameworkElement pControl)
		{
			bool flag2 = false;
			this.panel.Dispatcher.Invoke(() => {
				bool flag = this.panel.Children.Contains(pControl);
				bool flag1 = flag;
				flag2 = flag;
				return flag1;
			});
			return flag2;
		}

		public void UninjectControl(FrameworkElement pControl)
		{
			this.panel.Dispatcher.Invoke(() => this.panel.Children.Remove(pControl));
		}

		private void window_Initialized(object sender, EventArgs e)
		{
		}
	}
}