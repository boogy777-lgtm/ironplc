using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using _3S.CoDeSys.Controls.Collections;
using _3S.CoDeSys.Controls.Controls;
using _3S.CoDeSys.Core.OnlineHelp;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	[AssociatedOnlineHelpTopic("codesys.chm::/_cds_dlg_project_settings_compile_options.html")]
	public class ProjectGlobalDefinesDialog : Form, IProjectGlobalDefinesModelListener
	{
		private IProjectGlobalDefinesViewListener _viewListener;

		private ColumnWidthAdjuster _columnWidthAdjuster;

		private IContainer components;

		private Button _btnCancel;

		private Button _btnOK;

		private TreeTableView _ttvProjectDefines;

		private Label _lblWarningContainsInvalidDefines;

		private ToolStrip _toolStripProjectDefines;

		private ToolStripButton _addProjectDefine;

		public IList<string> DialogAnswer { get; set; } = new List<string>();


		internal ProjectGlobalDefinesDialog(IList<string> startMessage, IAPEnvironmentFacade apEnvironmentFacade)
		{
			InitializeComponent();
			_ttvProjectDefines.add_SelectionChanged((EventHandler)_ttvProjectDefines_SelectionChanged);
			_viewListener = new ProjectGlobalDefinesController(this);
			_viewListener.Initialize(apEnvironmentFacade, startMessage);
		}

		public void SetTableModel(ProjectGlobalDefinesTableModel tableModel)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected O, but got Unknown
			_ttvProjectDefines.set_Model((ITreeTableModel)(object)tableModel);
			_columnWidthAdjuster = new ColumnWidthAdjuster(_ttvProjectDefines, new int[1] { 100 });
			_columnWidthAdjuster.SetColumnsWidths();
		}

		public void SetEnabled(EProjectGlobalDefinesDialogPosition position, bool enabled)
		{
			if (position == EProjectGlobalDefinesDialogPosition.EndDialogOK)
			{
				_btnOK.Enabled = enabled;
			}
		}

		private void _btnOK_Click(object sender, EventArgs e)
		{
			_viewListener.ApplyChanges();
		}

		private void _addProjectDefine_Click(object sender, EventArgs e)
		{
			_viewListener.AddDefine();
		}

		private void _ttvProjectDefines_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				_viewListener.RemoveDefine();
			}
		}

		private void _ttvProjectDefines_SelectionChanged(object sender, EventArgs e)
		{
			int iIndex = -1;
			if (1 == ((TreeTableViewNodeCollection)_ttvProjectDefines.get_SelectedNodes()).get_Count())
			{
				iIndex = ((TreeTableViewNodeCollection)_ttvProjectDefines.get_SelectedNodes()).get_Item(0).get_Index();
			}
			_viewListener.SelectionChanged(iIndex);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(_3S.CoDeSys.LanguageModelManagerConfigurators.ProjectGlobalDefinesDialog));
			this._btnCancel = new System.Windows.Forms.Button();
			this._btnOK = new System.Windows.Forms.Button();
			this._ttvProjectDefines = new TreeTableView();
			this._lblWarningContainsInvalidDefines = new System.Windows.Forms.Label();
			this._toolStripProjectDefines = new System.Windows.Forms.ToolStrip();
			this._addProjectDefine = new System.Windows.Forms.ToolStripButton();
			this._toolStripProjectDefines.SuspendLayout();
			base.SuspendLayout();
			resources.ApplyResources(this._btnCancel, "_btnCancel");
			this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._btnCancel.Name = "_btnCancel";
			this._btnCancel.UseVisualStyleBackColor = true;
			resources.ApplyResources(this._btnOK, "_btnOK");
			this._btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._btnOK.Name = "_btnOK";
			this._btnOK.UseVisualStyleBackColor = true;
			this._btnOK.Click += new System.EventHandler(_btnOK_Click);
			this._ttvProjectDefines.set_AllowColumnReorder(false);
			resources.ApplyResources(this._ttvProjectDefines, "_ttvProjectDefines");
			this._ttvProjectDefines.set_AutoRestoreSelection(false);
			((System.Windows.Forms.Control)(object)this._ttvProjectDefines).BackColor = System.Drawing.SystemColors.Window;
			this._ttvProjectDefines.set_BorderStyle(System.Windows.Forms.BorderStyle.Fixed3D);
			this._ttvProjectDefines.set_DoNotShrinkColumnsAutomatically(false);
			this._ttvProjectDefines.set_ForceFocusOnClick(false);
			this._ttvProjectDefines.set_GridLines(true);
			this._ttvProjectDefines.set_HeaderStyle(System.Windows.Forms.ColumnHeaderStyle.None);
			this._ttvProjectDefines.set_HideSelection(false);
			this._ttvProjectDefines.set_ImmediateEdit(true);
			this._ttvProjectDefines.set_Indent(20);
			this._ttvProjectDefines.set_KeepColumnWidthsAdjusted(false);
			this._ttvProjectDefines.set_Model((ITreeTableModel)null);
			this._ttvProjectDefines.set_MultiSelect(false);
			((System.Windows.Forms.Control)(object)this._ttvProjectDefines).Name = "_ttvProjectDefines";
			this._ttvProjectDefines.set_NoSearchStrings(false);
			this._ttvProjectDefines.set_OnlyWhenFocused(false);
			this._ttvProjectDefines.set_OpenEditOnDblClk(true);
			this._ttvProjectDefines.set_ReadOnly(false);
			this._ttvProjectDefines.set_Scrollable(true);
			this._ttvProjectDefines.set_ShowLines(false);
			this._ttvProjectDefines.set_ShowPlusMinus(false);
			this._ttvProjectDefines.set_ShowRootLines(false);
			this._ttvProjectDefines.set_ToggleOnDblClk(false);
			((System.Windows.Forms.Control)(object)this._ttvProjectDefines).KeyDown += new System.Windows.Forms.KeyEventHandler(_ttvProjectDefines_KeyDown);
			resources.ApplyResources(this._lblWarningContainsInvalidDefines, "_lblWarningContainsInvalidDefines");
			this._lblWarningContainsInvalidDefines.Name = "_lblWarningContainsInvalidDefines";
			this._toolStripProjectDefines.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this._toolStripProjectDefines.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this._addProjectDefine });
			resources.ApplyResources(this._toolStripProjectDefines, "_toolStripProjectDefines");
			this._toolStripProjectDefines.Name = "_toolStripProjectDefines";
			resources.ApplyResources(this._addProjectDefine, "_addProjectDefine");
			this._addProjectDefine.Name = "_addProjectDefine";
			this._addProjectDefine.Click += new System.EventHandler(_addProjectDefine_Click);
			base.AcceptButton = this._btnOK;
			resources.ApplyResources(this, "$this");
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this._btnCancel;
			base.Controls.Add(this._toolStripProjectDefines);
			base.Controls.Add(this._lblWarningContainsInvalidDefines);
			base.Controls.Add((System.Windows.Forms.Control)(object)this._ttvProjectDefines);
			base.Controls.Add(this._btnOK);
			base.Controls.Add(this._btnCancel);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "ProjectGlobalDefinesDialog";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			this._toolStripProjectDefines.ResumeLayout(false);
			this._toolStripProjectDefines.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
