﻿using System;
using System.Windows.Forms;

namespace GlyssenApp.UI.Dialogs
{
	public partial class TooManyUnexpectedQuotesFoundDlg : Form
	{
		public TooManyUnexpectedQuotesFoundDlg(string caption, double percentageOfQuotesFoundThatWereUnexpected)
		{
			InitializeComponent();

			Text = caption;

			m_lblOnlyNPercentOfExpectedQuotesFound.Text = String.Format(m_lblOnlyNPercentOfExpectedQuotesFound.Text, percentageOfQuotesFoundThatWereUnexpected);

			m_lblPossibleProblemsWithFirstLevelQuotes.Text = String.Format(m_lblPossibleProblemsWithFirstLevelQuotes.Text, Glyssen.Properties.Settings.Default.MaxAcceptablePercentageOfUnknownQuotes);
		}

		public bool UserWantsToReview
		{
			get { return m_rdoReview.Checked; }
		}
	}
}
