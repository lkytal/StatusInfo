using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using EnvDTE;
using Microsoft.VisualBasic.Devices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Process = System.Diagnostics.Process;

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
		private Timer refreshTimer;
		private Process ideProcess;
		private InfoControl infoControl;
		private StatusBarInjector injector;

		private PerformanceCounter totalCpuCounter;
		private PerformanceCounter totalRamCounter;

		private OptionsPage optionsPage;

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			Debug.WriteLine($"Entering Initialize() of: {this}");

			base.Initialize();

			var dte = (DTE)GetService(typeof(DTE));
			DTEEvents eventsObj = dte.Events.DTEEvents;
			eventsObj.OnStartupComplete += InitExt;
			eventsObj.OnBeginShutdown += ShutDown;
		}

		private void InitExt()
		{
			Debug.WriteLine("Init function loaded");

			refreshTimer = new Timer(1000);
			refreshTimer.Elapsed += RefreshTimerElapsed;

			ideProcess = Process.GetCurrentProcess();
			ideProcess.InitCpuUsage();

			totalCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
			totalRamCounter = new PerformanceCounter("Memory", "Available Bytes");

			infoControl = new InfoControl((long)(new ComputerInfo()).TotalPhysicalMemory);

			injector = new StatusBarInjector(Application.Current.MainWindow);
			injector.InjectControl(infoControl);

			optionsPage = GetDialogPage(typeof(OptionsPage)) as OptionsPage;
			if (optionsPage != null) infoControl.Format = optionsPage.Format;

			refreshTimer.Start();
		}

		private void ShutDown()
		{
			refreshTimer.Stop();
		}

		public void OptionUpdated(string pName, object pValue)
		{
			Debug.WriteLine($"Get option: {pName}");

			switch (pName)
			{
				case "Format":
					infoControl.Format = (string)pValue;
					break;
				case "Interval":
					refreshTimer.Interval = (int)pValue;
					break;
				case "UseFixedWidth":
					infoControl.UseFixedWidth = (bool)pValue;
					break;
				case "FixedWidth":
					infoControl.FixedWidth = (int)pValue;
					break;
				default:
					Debug.WriteLine($"Error nonexsist option: {pName}");
					break;
			}
		}

		private void RefreshTimerElapsed(object sender, ElapsedEventArgs e)
		{
			UpdateInfoBar();
		}

		private void UpdateInfoBar()
		{
			infoControl.Dispatcher.BeginInvoke((Action)(() =>
			{
				infoControl.CpuUsage = (int)(ideProcess.GetCpuUsage() * 100);
				infoControl.RamUsage = ideProcess.WorkingSet64;
				infoControl.TotalCpuUsage = (int)totalCpuCounter.NextValue();
				infoControl.FreeRam = totalRamCounter.NextSample().RawValue;
			}));
		}
	}
}
