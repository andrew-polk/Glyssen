﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using L10NSharp;
using L10NSharp.UI;
using Paratext;
using ProtoScript.Controls;
using ProtoScript.Quote;
using SIL.ObjectModel;
using SIL.WritingSystems;

namespace ProtoScript.Dialogs
{
	public partial class QuotationMarksDialog : Form
	{
		private readonly Project m_project;
		private readonly BlockNavigatorViewModel m_navigatorViewModel;
		private string m_xOfYFmt;

		internal QuotationMarksDialog(Project project, BlockNavigatorViewModel navigatorViewModel, bool readOnly)
		{
			InitializeComponent();

			m_project = project;
			m_navigatorViewModel = navigatorViewModel;
	
			if (Properties.Settings.Default.QuoteMarksDialogShowGridView)
				m_toolStripButtonGridView.Checked = true;

			var books = new BookSet();
			foreach (var bookId in m_navigatorViewModel.IncludedBooks)
				books.Add(bookId);
			m_scriptureReference.VerseControl.BooksPresentSet = books;
			m_scriptureReference.VerseControl.ShowEmptyBooks = false;

			m_scriptureReference.VerseControl.AllowVerseSegments = false;
			m_scriptureReference.VerseControl.Versification = m_navigatorViewModel.Versification;
			m_scriptureReference.VerseControl.VerseRefChanged += m_scriptureReference_VerseRefChanged;

			m_blocksViewer.Initialize(m_navigatorViewModel);
			m_navigatorViewModel.CurrentBlockChanged += (sender, args) => LoadBlock();

			SetupQuoteMarksComboBoxes(m_project.QuoteSystem);

			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;

			m_blocksViewer.VisibleChanged += (sender, args) => this.SafeInvoke(() =>
			{
				if (m_blocksViewer.Visible)
					LoadBlock();
			}, true);

			SetFilterControlsFromMode();

			if (readOnly)
				MakeReadOnly();
		}

		private void HandleStringsLocalized()
		{
			SetupQuoteMarksComboBoxes(CurrentQuoteSystem);
			m_xOfYFmt = m_labelXofY.Text;
		}

		private void SetupQuoteMarksComboBoxes(QuoteSystem currentSystem)
		{
//			m_comboQuoteMarks.Items.AddRange(QuoteSystem.AllUniqueFirstLevelSystems.ToArray());
//			m_comboQuoteMarks.SelectedItem = currentSystem.GetCorrespondingFirstLevelQuoteSystem();

			foreach (var control in m_pnlLevels.Controls)
			{
				var cb = control as ComboBox;
				if (cb != null)
				{
					cb.Items.Clear();
					cb.Items.AddRange(QuoteUtils.AllDefaultSymbols());
				}
			}

			foreach (var level in m_project.QuoteSystem.NormalLevels)
			{
				switch (level.Level)
				{
					case 1:
						m_cbLevel1Begin.Text = BlankBecomesNone(level.Open);
						m_cbLevel1Continue.Text = BlankBecomesNone(level.Continue);
						m_cbLevel1End.Text = BlankBecomesNone(level.Close);
						break;
					case 2:
						m_cbLevel2Begin.Text = BlankBecomesNone(level.Open);
						m_cbLevel2Continue.Text = BlankBecomesNone(level.Continue);
						m_cbLevel2End.Text = BlankBecomesNone(level.Close);
						break;
					case 3:
						m_cbLevel3Begin.Text = BlankBecomesNone(level.Open);
						m_cbLevel3Continue.Text = BlankBecomesNone(level.Continue);
						m_cbLevel3End.Text = BlankBecomesNone(level.Close);
						break;
				}
			}

			var quotationDashMarker = currentSystem.QuotationDashMarker;
			m_chkDialogueQuotations.Checked = !String.IsNullOrEmpty(quotationDashMarker);
			m_cboQuotationDash.Items.Clear();
			m_cboQuotationDash.Items.Add(string.Format(LocalizationManager.GetString("QuotationMarksDialog.QuotationDash", "Quotation dash ({0})"), "U+2015"));
			m_cboQuotationDash.Items.Add(string.Format(LocalizationManager.GetString("QuotationMarksDialog.EmDash", "Em-dash ({0})"), "U+2014"));
			switch (quotationDashMarker)
			{
				case "\u2015": m_cboQuotationDash.SelectedIndex = 0; break;
				case "\u2014": m_cboQuotationDash.SelectedIndex = 1; break;
				default: m_cboQuotationDash.Text = quotationDashMarker; break;
			}

			m_cboEndQuotationDash.Items.Clear();
			m_cboEndQuotationDash.Items.Add(LocalizationManager.GetString("QuotationMarksDialog.EndQuotationDashWithParagraphOnly", "End of paragraph (only)"));
			m_cboEndQuotationDash.Items.Add(SameAsStartDashText);

			var quotationDashEndMarker = currentSystem.QuotationDashEndMarker;
			if (string.IsNullOrEmpty(quotationDashEndMarker))
				m_cboEndQuotationDash.SelectedIndex = 0;
			else if (quotationDashEndMarker == quotationDashMarker)
				m_cboEndQuotationDash.SelectedIndex = 1;
			else
				m_cboEndQuotationDash.Text = quotationDashEndMarker;
		}

		private string BlankBecomesNone(string text)
		{
			return string.IsNullOrEmpty(text) ? QuoteUtils.None : text;
		}

		private string NoneBecomesBlank(string text)
		{
			return text == QuoteUtils.None ? null : text;
		}

		private void MakeReadOnly()
		{
			//Review: Do we need to disable the controls?
			m_btnOk.Enabled = false;
		}

		private string SameAsStartDashText
		{
			get
			{
				string quotationDashMarker;
				switch (m_cboQuotationDash.SelectedIndex)
				{
					case 0: quotationDashMarker = "U+2015"; break;
					case 1: quotationDashMarker = "U+2014"; break;
					default: quotationDashMarker = m_cboQuotationDash.Text; break;
				}

				if (String.IsNullOrWhiteSpace(quotationDashMarker))
					return LocalizationManager.GetString("QuotationMarksDialog.EndQuotationDashWithStartDash",
						"Same as start quotation dash");
					
				return string.Format(LocalizationManager.GetString("QuotationMarksDialog.EndQuotationDashWithStartDash",
					"Same as start quotation dash ({0})"), quotationDashMarker);
			}
		}

		private bool ValidateQuoteSystem(QuoteSystem quoteSystem, out string validationMessage)
		{
			var level1 = quoteSystem.FirstLevel;
			if (level1 == null || string.IsNullOrEmpty(level1.Open) || string.IsNullOrEmpty(level1.Close))
			{
				validationMessage = LocalizationManager.GetString("QuotationMarksDialog.Level1OpenCloseRequired", "Level 1 Open and Close are required.");
				return false;
			}
			validationMessage = null;
			return true;
		}

		private void HandlecomboQuoteMarksDrawItem(object sender, DrawItemEventArgs e)
		{
			e.DrawBackground();
			if (e.Index < 0)
				TextRenderer.DrawText(e.Graphics, string.Empty, m_comboQuoteMarks.Font, e.Bounds, m_comboQuoteMarks.ForeColor,
					TextFormatFlags.Left);
			else
			{
				var selectedQuoteSystem = (QuoteSystem)m_comboQuoteMarks.Items[e.Index];
				string text = selectedQuoteSystem.ToString();
				var color = ((e.State & DrawItemState.Selected) > 0) ? SystemColors.HighlightText : m_comboQuoteMarks.ForeColor;

				TextRenderer.DrawText(e.Graphics, text, m_comboQuoteMarks.Font, e.Bounds, color,
					TextFormatFlags.Left);
				var quotesWidth = TextRenderer.MeasureText(e.Graphics, text, m_comboQuoteMarks.Font).Width;

				string majorLanguage = LocalizationManager.GetDynamicString("ProtoscriptGenerator",
					"QuotationMarks.MajorLanguage" + selectedQuoteSystem.MajorLanguage, selectedQuoteSystem.MajorLanguage);

				text = string.Format(LocalizationManager.GetString("QuotationMarksDialog.QuoteUsageFormat", "(commonly used in {0})",
				"Parameter is the name of a language and/or country"), majorLanguage);

				var bounds = new Rectangle(e.Bounds.Left + quotesWidth, e.Bounds.Top, e.Bounds.Width - quotesWidth, e.Bounds.Height);

				TextRenderer.DrawText(e.Graphics, text, m_cboQuotationDash.Font, bounds, color,
					TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
			}

			if (e.State != DrawItemState.ComboBoxEdit)
				e.Graphics.DrawLine(Pens.Black, new Point(e.Bounds.Left, e.Bounds.Bottom - 1), new Point(e.Bounds.Right, e.Bounds.Bottom - 1));
		}

		private void m_btnOk_Click(object sender, EventArgs e)
		{
			QuoteSystem currentQuoteSystem = CurrentQuoteSystem;
			string validationMessage;
			if (!ValidateQuoteSystem(currentQuoteSystem, out validationMessage))
			{
				MessageBox.Show(validationMessage, LocalizationManager.GetString("QuotationMarksDialog.QuoteSystemInvalid", "Quote System Invalid"));
				DialogResult = DialogResult.None;
				return;
			}
			if (m_project.IsQuoteSystemUserConfirmed && m_project.QuoteSystem != null && m_project.QuoteSystem != currentQuoteSystem)
			{
				string msg = LocalizationManager.GetString("ProjectSettingsDialog.ConfirmReparseMessage", "Changing the quote system will require a reparse of the text.  An attempt will be made to preserve the work you have already completed, but some character assignments might be lost.  A backup of your project will be created before this occurs." + Environment.NewLine + Environment.NewLine + "Are you sure you want to change the quote system?");
				string title = LocalizationManager.GetString("ProjectSettingsDialog.ConfirmReparse", "Confirm Reparse");
				if (MessageBox.Show(msg, title, MessageBoxButtons.YesNo) != DialogResult.Yes)
				{
					SetupQuoteMarksComboBoxes(m_project.QuoteSystem);
					DialogResult = DialogResult.None;
					return;
				}
			}
			m_project.IsQuoteSystemUserConfirmed = true;
			m_project.QuoteSystem = currentQuoteSystem;
		}

		public QuoteSystem CurrentQuoteSystem
		{
			get
			{
				var levels = new BulkObservableList<QuotationMark>();
				levels.Add(new QuotationMark(NoneBecomesBlank(m_cbLevel1Begin.Text), NoneBecomesBlank(m_cbLevel1End.Text), NoneBecomesBlank(m_cbLevel1Continue.Text), 1, QuotationMarkingSystemType.Normal));
				if (!string.IsNullOrEmpty(m_cbLevel2Begin.Text))
					levels.Add(new QuotationMark(NoneBecomesBlank(m_cbLevel2Begin.Text), NoneBecomesBlank(m_cbLevel2End.Text), NoneBecomesBlank(m_cbLevel2Continue.Text), 2, QuotationMarkingSystemType.Normal));
				if (!string.IsNullOrEmpty(m_cbLevel3Begin.Text))
					levels.Add(new QuotationMark(NoneBecomesBlank(m_cbLevel3Begin.Text), NoneBecomesBlank(m_cbLevel3End.Text), NoneBecomesBlank(m_cbLevel3Continue.Text), 3, QuotationMarkingSystemType.Normal));

				if (m_chkDialogueQuotations.Checked)
				{
					string quotationDashMarker = null;
					string quotationDashEndMarker = null;
					switch (m_cboQuotationDash.SelectedIndex)
					{
						case 0: quotationDashMarker = "\u2015"; break;
						case 1: quotationDashMarker = "\u2014"; break;
						default:
							if (!String.IsNullOrWhiteSpace(m_cboQuotationDash.Text))
								quotationDashMarker = m_cboQuotationDash.Text;
							break;
					}

					switch (m_cboEndQuotationDash.SelectedIndex)
					{
						case 0: break;
						case 1: quotationDashEndMarker = quotationDashMarker; break;
						default:
							if (!String.IsNullOrWhiteSpace(m_cboEndQuotationDash.Text))
								quotationDashEndMarker = m_cboEndQuotationDash.Text;
							break;
					}

					if (quotationDashMarker != null)
						levels.Add(new QuotationMark(quotationDashMarker, quotationDashEndMarker, null, 1, QuotationMarkingSystemType.Narrative));
				}

				return new QuoteSystem(levels);
			}
		}

		#region Form events
		private void m_btnNext_Click(object sender, EventArgs e)
		{
			m_navigatorViewModel.LoadNextRelevantBlock();
		}

		private void m_btnPrevious_Click(object sender, EventArgs e)
		{
			m_navigatorViewModel.LoadPreviousRelevantBlock();
		}

		private void m_chkDialogueQuotations_CheckedChanged(object sender, EventArgs e)
		{
			m_cboQuotationDash.Enabled = m_chkDialogueQuotations.Checked;
			m_cboEndQuotationDash.Enabled = m_chkDialogueQuotations.Checked;
			m_lblStartDialogueQuote.Enabled = m_chkDialogueQuotations.Checked;
			m_lblEndDialogueQuote.Enabled = m_chkDialogueQuotations.Checked;
			m_chkAlternateSpeakersInFirstLevelQuotes.Enabled = m_chkDialogueQuotations.Checked;
		}

		private void m_cboQuotationDash_TextChanged(object sender, EventArgs e)
		{
			m_cboEndQuotationDash.Items[1] = SameAsStartDashText;
		}
		private void HandleFilterChanged(object sender, EventArgs e)
		{
			if (!IsHandleCreated)
				return;

			BlocksToDisplay mode;

			switch (m_toolStripComboBoxFilter.SelectedIndex)
			{
				case 0: mode = BlocksToDisplay.AllExpectedQuotes; break;
				default: mode = BlocksToDisplay.AllScripture; break;
			}

			m_navigatorViewModel.Mode = mode;

			if (m_navigatorViewModel.RelevantBlockCount > 0)
			{
				LoadBlock();
			}
			else
			{
				m_blocksViewer.Clear();
				m_labelXofY.Visible = false;
				
				string msg = LocalizationManager.GetString("DialogBoxes.AssignCharacterDialog.NoMatches", "Nothing matches your current filter.");
				MessageBox.Show(this, msg, ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void SetFilterControlsFromMode()
		{
			var mode = m_navigatorViewModel.Mode;
			if ((mode & BlocksToDisplay.AllExpectedQuotes) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 0;
			else if ((mode & BlocksToDisplay.AllScripture) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 1;
			else
				throw new InvalidEnumArgumentException("mode", (int)mode, typeof(BlocksToDisplay));
		}

		public void LoadBlock()
		{
			UpdateDisplay();
			UpdateNavigationButtonState();
		}

		private void UpdateDisplay()
		{
			var blockRef = m_navigatorViewModel.GetBlockReference(m_navigatorViewModel.CurrentBlock);
			var versesInBlock = m_navigatorViewModel.CurrentBlock.LastVerse - blockRef.Verse;
			var displayedRefMinusBlockStartRef = m_scriptureReference.VerseControl.VerseRef.BBBCCCVVV - blockRef.BBCCCVVV;
			if (displayedRefMinusBlockStartRef < 0 || displayedRefMinusBlockStartRef > versesInBlock)
				m_scriptureReference.VerseControl.VerseRef = new VerseRef(m_navigatorViewModel.GetBlockReference(m_navigatorViewModel.CurrentBlock), ScrVers.English);
			m_labelXofY.Visible = m_navigatorViewModel.IsCurrentBlockRelevant;
			Debug.Assert(m_navigatorViewModel.RelevantBlockCount >= m_navigatorViewModel.CurrentBlockDisplayIndex);
			m_labelXofY.Text = string.Format(m_xOfYFmt, m_navigatorViewModel.CurrentBlockDisplayIndex, m_navigatorViewModel.RelevantBlockCount);
		}

		private void UpdateNavigationButtonState()
		{
			m_btnNext.Enabled = m_navigatorViewModel.CanNavigateToNextRelevantBlock;
			m_btnPrevious.Enabled = m_navigatorViewModel.CanNavigateToPreviousRelevantBlock;
		}

		private void HandleHtmlViewCheckChanged(object sender, EventArgs e)
		{
			if (m_toolStripButtonHtmlView.Checked == m_toolStripButtonGridView.Checked)
			{
				m_toolStripButtonGridView.Checked = !m_toolStripButtonHtmlView.Checked;

				Debug.Assert(!m_toolStripButtonGridView.Checked);

				m_blocksViewer.ViewType = ScriptBlocksViewType.Html;
				Properties.Settings.Default.QuoteMarksDialogShowGridView = false;
			}
		}

		private void HandleDataGridViewCheckChanged(object sender, EventArgs e)
		{
			if (m_toolStripButtonHtmlView.Checked == m_toolStripButtonGridView.Checked)
			{
				m_toolStripButtonHtmlView.Checked = !m_toolStripButtonGridView.Checked;

				Debug.Assert(!m_toolStripButtonHtmlView.Checked);

				m_blocksViewer.ViewType = ScriptBlocksViewType.Grid;
				Properties.Settings.Default.QuoteMarksDialogShowGridView = true;
			}
		}

		private void HandleViewTypeToolStripButtonClick(object sender, EventArgs e)
		{
			var button = (ToolStripButton)sender;
			if (!button.Checked)
				button.Checked = true;
		}

		private void IncreaseFont(object sender, EventArgs e)
		{
			m_blocksViewer.IncreaseFont();
		}

		private void DecreaseFont(object sender, EventArgs e)
		{
			m_blocksViewer.DecreaseFont();
		}

		private void m_scriptureReference_VerseRefChanged(object sender, PropertyChangedEventArgs e)
		{
			m_navigatorViewModel.TryLoadBlock(m_scriptureReference.VerseControl.VerseRef);
		}
		#endregion

		//#region Block Navigation event handlers
		//private void OnDocumentCompleted(object sender, GeckoDocumentCompletedEventArgs e)
		//{
		//	m_blocksDisplayBrowser.ScrollElementIntoView(BlockNavigatorViewModel.kMainQuoteElementId, -225);
		//}
		//#endregion
	}
}
