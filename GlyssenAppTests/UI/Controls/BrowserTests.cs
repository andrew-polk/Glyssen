﻿using System;
using System.Windows.Forms;
using GlyssenApp.UI;
using NUnit.Framework;

namespace GlyssenAppTests.UI.Controls
{
	class BrowserTests
	{
		[Test, Ignore("By hand only")]
		[STAThread]
		public void Navigate()
		{
			using (var form = new Form())
			{
				using (var browser = new Browser())
				{
					form.Controls.Add(browser);
					browser.Navigate("www.google.com");
					form.ShowDialog();
				}
			}
		}

		[Test, Ignore("By hand only")]
		[STAThread]
		public void DisplayHtml()
		{
			using (var form = new Form())
			{
				using (var browser = new Browser())
				{
					form.Controls.Add(browser);
					browser.DisplayHtml("normal <em>italics</em> <strong>bold</strong>");
					form.ShowDialog();
				}
			}
		}
	}
}
