using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualBasic.Devices;
using System.Timers;
using System.Windows;

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

	[ProvideAutoLoad(UIContextGuids80.NoSolution)]
	[ProvideAutoLoad(UIContextGuids80.SolutionExists)]
	[ProvideAutoLoad(UIContextGuids80.EmptySolution)]
	[ProvideOptionPage(typeof(OptionsPage), "StatusBar Info", "General", 0, 0, true)]

	public sealed class StatusInfoPackage : Package
	{
		private Timer RefreshTimer;

		private Process IdeProcess;

		private StatusBarInjector Injector;

		private InfoControl InfoControl;

		private PerformanceCounter TotalCpuCounter;

		private PerformanceCounter TotalRamCounter;

		private OptionsPage OptionsPage;

		public StatusInfoPackage()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
		}

		EnvDTE.DTEEvents EventsObj;

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));

			base.Initialize();

			var Dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
			EventsObj = Dte.Events.DTEEvents;
			EventsObj.OnStartupComplete += this.InitExt;
			EventsObj.OnBeginShutdown += this.ShutDown;
		}

		private void InitExt()
		{
			this.RefreshTimer = new Timer(1000);
			this.RefreshTimer.Elapsed += this.RefreshTimerElapsed;
			this.RefreshTimer.Disposed += this.RefreshTimerDisposed;
			this.IdeProcess = Process.GetCurrentProcess();
			this.InfoControl = new InfoControl((long)(new ComputerInfo()).TotalPhysicalMemory);
			this.Injector = new StatusBarInjector(Application.Current.MainWindow);
			this.TotalCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
			this.TotalRamCounter = new PerformanceCounter("Memory", "Available Bytes");
			//end of construct

			this.IdeProcess.InitCpuUsage();
			this.Injector.InjectControl(this.InfoControl);
			this.OptionsPage = GetDialogPage(typeof(OptionsPage)) as OptionsPage;

			this.RefreshTimer.Enabled = true;

			if (OptionsPage != null) InfoControl.Format = OptionsPage.Format; //first trigger
		}

		private void ShutDown()
		{
			this.RefreshTimer.Stop();
		}

		public void OptionUpdated(string pName, object pValue)
		{
			if (pName != null)
			{
				switch (pName)
				{
					case "Format":
						this.InfoControl.Format = (string)pValue;
						break;
					case "Interval":
						this.RefreshTimer.Interval = (int)pValue;
						break;
					case "UseFixedWidth":
						this.InfoControl.UseFixedWidth = (bool)pValue;
						break;
					case "FixedWidth":
						this.InfoControl.FixedWidth = (int)pValue;
						break;
				}
			}
		}

		private void RefreshTimerDisposed(object sender, EventArgs e)
		{
			this.RefreshTimer.Enabled = false;
		}

		private void RefreshTimerElapsed(object sender, ElapsedEventArgs e)
		{
			UpdateInfoBar();
		}

		private void UpdateInfoBar()
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

	internal class DteInitializer : IVsShellPropertyEvents
	{
		private IVsShell ShellService;
		private uint Cookie;
		private Action Callback;

		internal DteInitializer(IVsShell shellService, Action callback)
		{
			this.ShellService = shellService;
			this.Callback = callback;

			// Set an event handler to detect when the IDE is fully initialized
			int hr = this.ShellService.AdviseShellPropertyChanges(this, out this.Cookie);

			ErrorHandler.ThrowOnFailure(hr);
		}

		int IVsShellPropertyEvents.OnShellPropertyChange(int propid, object var)
		{
			if (propid != -9053)
			{
				return 0;
			}

			// Release the event handler to detect when the IDE is fully initialized
			int hr = this.ShellService.UnadviseShellPropertyChanges(this.Cookie);

			ErrorHandler.ThrowOnFailure(hr);

			this.Cookie = 0;

			this.Callback();

			return VSConstants.S_OK;
		}
	}
}
