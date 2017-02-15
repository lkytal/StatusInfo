using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lkytal.StatusInfo
{
	internal class TextBlockList : List<TextBlock>
	{
		public Brush Foreground
		{
			set
			{
				foreach (TextBlock textBlock in this)
				{
					textBlock.Foreground = value;
				}
			}
		}

		public string Text
		{
			set
			{
				foreach (TextBlock textBlock in this)
				{
					textBlock.Text = value;
				}
			}
		}
	}
}