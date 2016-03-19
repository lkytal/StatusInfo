using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Lkytal.StatusInfo
{
	public partial class InfoControl : UserControl
	{
		public readonly static string[] Formats;

		public readonly static string[] FormatDescriptions;

		private Dictionary<string, TextBlockList> textBlockLists;

		private long totalRam;

		private int fixedWidth = 150;

		private bool useFixedWidth;

		public int CpuUsage
		{
			set
			{
				string str = string.Format("{0}%", value);
				this.textBlockLists["<CPU>"].Text = str;
				this.textBlockLists["<#CPU>"].Text = str;
				this.textBlockLists["<#CPU>"].Foreground = this.GetCpuColor(value);
			}
		}

		public int FixedWidth
		{
			get
			{
				return this.fixedWidth;
			}
			set
			{
				this.fixedWidth = value;
				this.UseFixedWidth = this.UseFixedWidth;
			}
		}

		public string Format
		{
			set
			{
				TextBlockList textBlockList;
				this.stackPanel.Children.Clear();
				this.textBlockLists.Clear();
				this.InitTextBlockLists();
				string str = value;
				while (str != "")
				{
					TextBlock nextTextBlock = this.GetNextTextBlock(ref str, out textBlockList);
					if (textBlockList != null)
					{
						textBlockList.Add(nextTextBlock);
					}
					this.stackPanel.Children.Add(nextTextBlock);
				}
				foreach (TextBlockList textBlockList1 in this.textBlockLists.Values)
				{
					textBlockList1.Text = "N/A";
				}
			}
		}

		public long FreeRam
		{
			set
			{
				long num = this.totalRam - value;
				string readableByteSize = num.ToReadableByteSize();
				this.textBlockLists["<TOTAL_USE_RAM>"].Text = readableByteSize;
				this.textBlockLists["<#TOTAL_USE_RAM>"].Text = readableByteSize;
				this.textBlockLists["<#TOTAL_USE_RAM>"].Foreground = this.GetRamColor(num);
				int num1 = (int)(num * (long)100 / this.totalRam);
				readableByteSize = string.Format("{0}%", num1);
				this.textBlockLists["<TOTAL_USE_RAM%>"].Text = readableByteSize;
				this.textBlockLists["<#TOTAL_USE_RAM%>"].Text = readableByteSize;
				this.textBlockLists["<#TOTAL_USE_RAM%>"].Foreground = this.GetRamColor(num);
				readableByteSize = value.ToReadableByteSize();
				this.textBlockLists["<FREE_RAM>"].Text = readableByteSize;
				this.textBlockLists["<#FREE_RAM>"].Text = readableByteSize;
				this.textBlockLists["<#FREE_RAM>"].Foreground = this.GetRamColor(num);
				num1 = (int)(value * (long)100 / this.totalRam);
				readableByteSize = string.Format("{0}%", num1);
				this.textBlockLists["<FREE_RAM%>"].Text = readableByteSize;
				this.textBlockLists["<#FREE_RAM%>"].Text = readableByteSize;
				this.textBlockLists["<#FREE_RAM%>"].Foreground = this.GetRamColor(num);
			}
		}

		public long RamUsage
		{
			set
			{
				string readableByteSize = value.ToReadableByteSize();
				this.textBlockLists["<RAM>"].Text = readableByteSize;
				this.textBlockLists["<#RAM>"].Text = readableByteSize;
				this.textBlockLists["<#RAM>"].Foreground = this.GetRamColor(value);
				int num = (int)(value * (long)100 / this.totalRam);
				readableByteSize = string.Format("{0}%", num);
				this.textBlockLists["<RAM%>"].Text = readableByteSize;
				this.textBlockLists["<#RAM%>"].Text = readableByteSize;
				this.textBlockLists["<#RAM%>"].Foreground = this.GetCpuColor(num);
			}
		}

		public int TotalCpuUsage
		{
			set
			{
				string str = string.Format("{0}%", value);
				this.textBlockLists["<TOTAL_CPU>"].Text = str;
				this.textBlockLists["<#TOTAL_CPU>"].Text = str;
				this.textBlockLists["<#TOTAL_CPU>"].Foreground = this.GetCpuColor(value);
			}
		}

		public bool UseFixedWidth
		{
			get
			{
				return this.useFixedWidth;
			}
			set
			{
				this.useFixedWidth = value;
				if (!this.useFixedWidth)
				{
					base.Width = double.NaN;
					return;
				}
				base.Width = (double)this.FixedWidth;
			}
		}

		static InfoControl()
		{
			string[] strArrays = new string[] { "CPU", "TOTAL_CPU", "RAM", "FREE_RAM", "TOTAL_USE_RAM", "RAM%", "FREE_RAM%", "TOTAL_USE_RAM%" };
			InfoControl.Formats = strArrays;
			string[] strArrays1 = new string[] { "Cpu usage of Visual Studio", "Cpu usage of computer", "Ram usage of Visual Studio", "Free ram of computer", "Ram usage of computer", "Ram usage of Visual Studio in percent", "Free ram of computer in percent", "Ram usage of computer in percent" };
			InfoControl.FormatDescriptions = strArrays1;
		}

		public InfoControl(long pTotalRam)
		{
			this.totalRam = pTotalRam;
			this.InitializeComponent();
			this.textBlockLists = new Dictionary<string, TextBlockList>();
			this.Format = "IDEStatusBarInfos is loading...";
		}

		private Brush GetCpuColor(int cpu)
		{
			Color color;
			if (cpu > 50)
			{
				Color yellow = Colors.Yellow;
				color = yellow.FadeTo(Colors.Red, (float)(cpu - 50) / 50f);
			}
			else
			{
				Color white = Colors.White;
				color = white.FadeTo(Colors.Yellow, (float)cpu / 50f);
			}
			return new SolidColorBrush(color);
		}

		private TextBlock GetNextTextBlock(ref string format, out TextBlockList textBlockList)
		{
			string str;
			TextBlock textBlock = new TextBlock()
			{
				Foreground = new SolidColorBrush(Colors.White)
			};
			TextBlock textBlock1 = textBlock;
			int num = format.IndexOfAny(this.textBlockLists.Keys.ToArray<string>(), out str);
			if (num == -1)
			{
				textBlock1.Text = format;
				format = "";
				textBlockList = null;
			}
			else if (num != 0)
			{
				textBlock1.Text = format.Substring(0, num);
				format = format.Substring(num);
				textBlockList = null;
			}
			else
			{
				textBlock1.Text = "";
				format = format.Substring(str.Length);
				textBlockList = this.textBlockLists[str];
			}
			return textBlock1;
		}

		private Brush GetRamColor(long ram)
		{
			int num = (int)(ram * (long)100 / this.totalRam);
			return this.GetCpuColor(num);
		}

		private void InitTextBlockLists()
		{
			string[] formats = InfoControl.Formats;
			for (int i = 0; i < (int)formats.Length; i++)
			{
				string textBlockList = formats[i];
				this.textBlockLists[string.Format("<{0}>", textBlockList)] = new TextBlockList();
				this.textBlockLists[string.Format("<#{0}>", textBlockList)] = new TextBlockList();
			}
		}
	}
}