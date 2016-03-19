using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualBasic.Devices;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace Lkytal.StatusInfo
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the
	/// IVsPackage interface and uses the registration attributes defined in the framework to
	/// register itself and its components with the shell.
	/// </summary>
	// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
	// a package.
	[PackageRegistration(UseManagedResourcesOnly = true)]
	// This attribute is used to register the information needed to show this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	[Guid(GuidList.guidStatusInfoPkgString)]

	[ProvideAutoLoad("{10534154-102D-46E2-ABA8-A6BFA25BA0BE}")]
	[ProvideOptionPage(typeof(OptionsPage), "StatusBar Info", "General", 0, 0, true)]

	public sealed class StatusInfoPackage : Package
	{
		/// <summary>
		/// Default constructor of the package.
		/// Inside this method you can place any initialization code that does not require
		/// any Visual Studio service because at this point the package object is created but
		/// not sited yet inside Visual Studio environment. The place to do all the other
		/// initialization is the Initialize method.
		/// </summary>

		private readonly Timer RefreshTimer;

		private readonly Process IdeProcess;

		private readonly StatusBarInjector Injector;

		private readonly InfoControl InfoControl;

		private readonly PerformanceCounter TotalCpuCounter;

		private readonly PerformanceCounter TotalRamCounter;

		private OptionsPage OptionsPage;

		public StatusInfoPackage()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));

			this.RefreshTimer = new Timer(1000);
			this.RefreshTimer.Elapsed += new ElapsedEventHandler(this.RefreshTimerElapsed);
			this.RefreshTimer.Disposed += new EventHandler(this.RefreshTimerDisposed);
			this.IdeProcess = Process.GetCurrentProcess();
			this.InfoControl = new InfoControl((long)(new ComputerInfo()).TotalPhysicalMemory);
			this.Injector = new StatusBarInjector(Application.Current.MainWindow);
			this.TotalCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
			this.TotalRamCounter = new PerformanceCounter("Memory", "Available Bytes");
		}

		/////////////////////////////////////////////////////////////////////////////
		// Overridden Package Implementation
		#region Package Members

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));

			base.Initialize();

			this.IdeProcess.InitCpuUsage();
			this.Injector.InjectControl(this.InfoControl);
			this.OptionsPage = base.GetDialogPage(typeof(OptionsPage)) as OptionsPage;

			this.RefreshTimer.Enabled = true;
		}
		#endregion

		public void OptionUpdated(string pName, object pValue)
		{
			if (pName != null)
			{
				if (pName == "Format")
				{
					this.InfoControl.Format = (string)pValue;
				}
				else if (pName == "Interval")
				{
					this.RefreshTimer.Interval = (int)pValue;
				}
				else if (pName == "UseFixedWidth")
				{
					this.InfoControl.UseFixedWidth = (bool)pValue;
				}
				else if (pName == "FixedWidth")
				{
					this.InfoControl.FixedWidth = (int)pValue;
				}
			}
		}

		private void RefreshTimerDisposed(object sender, EventArgs e)
		{
			this.RefreshTimer.Enabled = false;
		}

		private void RefreshTimerElapsed(object sender, ElapsedEventArgs e)
		{
			this.InfoControl.Dispatcher.Invoke(() =>
			{
				this.InfoControl.CpuUsage = (int)(this.IdeProcess.GetCpuUsage() * 100);
				this.InfoControl.RamUsage = this.IdeProcess.WorkingSet64;
				this.InfoControl.TotalCpuUsage = (int)this.TotalCpuCounter.NextValue();
				this.InfoControl.FreeRam = this.TotalRamCounter.NextSample().RawValue;
			});
		}
	}
}
