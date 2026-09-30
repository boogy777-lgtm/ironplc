using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _3S.CoDeSys.Controls.Controls;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.OnlineHelp;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	[AssociatedOnlineHelpTopic("codesys.chm::/_cds_dlg_project_settings_compile_warnings.htm")]
	public class WarningOptions : UserControl
	{
		private readonly WarningModel _warningModel;

		private readonly List<Warning> _alWarnings = new List<Warning>();

		private IContainer components;

		private TreeTableView _ttvWarnings;

		private Label label2;

		private PictureBox pictureBox2;

		private Label label1;

		private PictureBox pictureBox1;

		private static Guid ActiveApplicationGuid
		{
			get
			{
				try
				{
					return (APEnvironment.Engine.Projects.PrimaryProject == null) ? Guid.Empty : APEnvironment.Engine.Projects.PrimaryProject.ActiveApplication;
				}
				catch
				{
					return Guid.Empty;
				}
			}
		}

		public WarningOptions()
		{
			InitializeComponent();
			ILMWarningConfiguration warningConfiguration = APEnvironment.LMServiceProvider.ConfigurationService.WarningConfiguration;
			IEnumerable<int> disabledWarningsSet = warningConfiguration.DisabledWarningsSet;
			IEnumerable<int> enumerable = null;
			if (warningConfiguration is ILMWarningConfiguration2 iLMWarningConfiguration)
			{
				enumerable = iLMWarningConfiguration.WarningAsErrorSet;
			}
			HashSet<int> hashSet = null;
			if (disabledWarningsSet != null)
			{
				hashSet = new HashSet<int>(disabledWarningsSet);
			}
			HashSet<int> hashSet2 = ((enumerable != null) ? new HashSet<int>(enumerable) : new HashSet<int>());
			foreach (int item in warningConfiguration.WarningsSet)
			{
				bool bActive = hashSet == null || !hashSet.Contains(item);
				bool bAsError = hashSet2.Contains(item);
				Warning warning = new Warning(item, bActive, bAsError);
				if (!string.IsNullOrEmpty(warning.Message))
				{
					_alWarnings.Add(warning);
				}
			}
			_warningModel = new WarningModel(_ttvWarnings);
			_ttvWarnings.set_Model((ITreeTableModel)(object)_warningModel);
			_warningModel.Refill(_alWarnings);
		}

		internal bool Save()
		{
			HashSet<int> hashSet = new HashSet<int>();
			HashSet<int> hashSet2 = new HashSet<int>();
			_warningModel.Save(hashSet, hashSet2);
			DetectChanges(hashSet, hashSet2, out var bDisabledWarningsChanged, out var bWarningAsErrorChanged);
			APEnvironment.LMServiceProvider.ConfigurationService.WarningConfiguration.DisabledWarningsSet = hashSet;
			if (APEnvironment.LMServiceProvider.ConfigurationService.WarningConfiguration is ILMWarningConfiguration2 iLMWarningConfiguration)
			{
				iLMWarningConfiguration.WarningAsErrorSet = hashSet2;
			}
			UpdateLanguageModel(bDisabledWarningsChanged, bWarningAsErrorChanged);
			return true;
		}

		private void DetectChanges(HashSet<int> hsDisabledWarningIdsCurr, HashSet<int> hsWarningsAsErrorsIdsCurr, out bool bDisabledWarningsChanged, out bool bWarningAsErrorChanged)
		{
			bDisabledWarningsChanged = DetectChanges(hsDisabledWarningIdsCurr, APEnvironment.LMServiceProvider.ConfigurationService.WarningConfiguration.DisabledWarningsSet);
			bWarningAsErrorChanged = false;
			if (APEnvironment.LMServiceProvider.ConfigurationService.WarningConfiguration is ILMWarningConfiguration2 iLMWarningConfiguration)
			{
				bWarningAsErrorChanged = DetectChanges(hsWarningsAsErrorsIdsCurr, iLMWarningConfiguration.WarningAsErrorSet);
			}
		}

		private bool DetectChanges(HashSet<int> hsIdsCurr, IEnumerable<int> idsPrev)
		{
			bool result = false;
			if (idsPrev == null)
			{
				return true;
			}
			HashSet<int> hashSet = new HashSet<int>(hsIdsCurr);
			foreach (int item in idsPrev)
			{
				if (hashSet.Contains(item))
				{
					hashSet.Remove(item);
				}
				else
				{
					result = true;
				}
			}
			if (hashSet.Any())
			{
				result = true;
			}
			return result;
		}

		private void UpdateLanguageModel(bool bDisabledWarningsChanged, bool bWarningAsErrorChanged)
		{
			if ((!bDisabledWarningsChanged && !bWarningAsErrorChanged) || APEnvironment.Engine.Projects.PrimaryProject == null)
			{
				return;
			}
			try
			{
				APEnvironment.Engine.BeginCleanAll();
				APEnvironment.Engine.UpdateLanguageModel(APEnvironment.Engine.Projects.PrimaryProject.Handle);
				Guid activeApplicationGuid = ActiveApplicationGuid;
				APEnvironment.LMServiceProvider.CommandService.ForceRebuildAll(activeApplicationGuid);
				APEnvironment.LanguageModelMgr.ClearDownloadContext(activeApplicationGuid);
			}
			finally
			{
				APEnvironment.Engine.EndCleanAll();
			}
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
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Expected O, but got Unknown
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager componentResourceManager = new System.ComponentModel.ComponentResourceManager(typeof(_3S.CoDeSys.LanguageModelManagerConfigurators.WarningOptions));
			this._ttvWarnings = new TreeTableView();
			this.label2 = new System.Windows.Forms.Label();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)this.pictureBox2).BeginInit();
			((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
			base.SuspendLayout();
			this._ttvWarnings.set_AllowColumnReorder(false);
			componentResourceManager.ApplyResources(this._ttvWarnings, "_ttvWarnings");
			this._ttvWarnings.set_AutoRestoreSelection(false);
			((System.Windows.Forms.Control)(object)this._ttvWarnings).BackColor = System.Drawing.SystemColors.Window;
			this._ttvWarnings.set_BorderStyle(System.Windows.Forms.BorderStyle.FixedSingle);
			this._ttvWarnings.set_DoNotShrinkColumnsAutomatically(false);
			this._ttvWarnings.set_ForceFocusOnClick(false);
			this._ttvWarnings.set_GridLines(true);
			this._ttvWarnings.set_HeaderStyle(System.Windows.Forms.ColumnHeaderStyle.Nonclickable);
			this._ttvWarnings.set_HideSelection(false);
			this._ttvWarnings.set_ImmediateEdit(false);
			this._ttvWarnings.set_Indent(20);
			this._ttvWarnings.set_KeepColumnWidthsAdjusted(false);
			this._ttvWarnings.set_Model((ITreeTableModel)null);
			this._ttvWarnings.set_MultiSelect(false);
			((System.Windows.Forms.Control)(object)this._ttvWarnings).Name = "_ttvWarnings";
			this._ttvWarnings.set_NoSearchStrings(false);
			this._ttvWarnings.set_OnlyWhenFocused(false);
			this._ttvWarnings.set_OpenEditOnDblClk(false);
			this._ttvWarnings.set_ReadOnly(false);
			this._ttvWarnings.set_Scrollable(true);
			this._ttvWarnings.set_ShowLines(true);
			this._ttvWarnings.set_ShowPlusMinus(true);
			this._ttvWarnings.set_ShowRootLines(true);
			this._ttvWarnings.set_ToggleOnDblClk(false);
			componentResourceManager.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			componentResourceManager.ApplyResources(this.pictureBox2, "pictureBox2");
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.TabStop = false;
			componentResourceManager.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			componentResourceManager.ApplyResources(this.pictureBox1, "pictureBox1");
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.TabStop = false;
			componentResourceManager.ApplyResources(this, "$this");
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(this.label2);
			base.Controls.Add(this.pictureBox2);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.pictureBox1);
			base.Controls.Add((System.Windows.Forms.Control)(object)this._ttvWarnings);
			base.Name = "WarningOptions";
			((System.ComponentModel.ISupportInitialize)this.pictureBox2).EndInit();
			((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
