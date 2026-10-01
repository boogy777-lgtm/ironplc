using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using _3S.CoDeSys.Core.OnlineHelp;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	[AssociatedOnlineHelpTopic("codesys.chm::/_cds_dlg_project_settings_compile_options.htm")]
	public class CompileProperties : UserControl, ICompilePropertiesModelListener
	{
		private Dictionary<ECompilePropertiesEditorPosition, Control> PositionToControlForErrors;

		private ICompilePropertiesViewListener _viewListener;

		private IContainer components;

		private GroupBox _groupCompilerVersion;

		private ComboBox _cbVersions;

		private Label _errorLabel;

		private ErrorProvider _errNotAvailable;

		private Label _fixVersionLabel;

		private GroupBox _groupSettings;

		private CheckBox _cbReplaceConstants;

		private GroupBox _groupMessages;

		private Label label1;

		private ComboBox _cbMaxWarnings;

		private CheckBox _cbUnicodeIdentifier;

		private CheckBox _cbEnableBreakpointLogging;

		private CheckBox _cbutf8Encoding;

		private Button _btnEditProjectDefines;

		private CheckBox _cbReportCompiledPousDuringIncrementalCompile;

		public CompileProperties()
		{
			InitializeComponent();
			_viewListener = new CompilePropertiesController(this, new APEnvironmentFacade());
			_viewListener.Initialize(APEnvironment.Engine.CommandLineManager.HasSwitch("debug"));
			PositionToControlForErrors = new Dictionary<ECompilePropertiesEditorPosition, Control>
			{
				{
					ECompilePropertiesEditorPosition.CompilerVersionSelection,
					_cbVersions
				},
				{
					ECompilePropertiesEditorPosition.ProjectDefinesTrigger,
					_btnEditProjectDefines
				},
				{
					ECompilePropertiesEditorPosition.MaxNumberOfWarnings,
					_cbMaxWarnings
				}
			};
		}

		public void InitializeView()
		{
			_errNotAvailable.SetIconAlignment(_errorLabel, ErrorIconAlignment.MiddleLeft);
			_errNotAvailable.SetIconPadding(_errorLabel, 2);
		}

		public void SetEnabled(ECompilePropertiesEditorPosition position, bool enabled)
		{
			switch (position)
			{
			case ECompilePropertiesEditorPosition.CompilerVersionSelection:
				_cbVersions.Enabled = enabled;
				break;
			case ECompilePropertiesEditorPosition.ProjectDefinesTrigger:
				_btnEditProjectDefines.Enabled = enabled;
				break;
			case ECompilePropertiesEditorPosition.AllowUnicodeInIdentifiers:
				_cbUnicodeIdentifier.Enabled = enabled;
				break;
			case ECompilePropertiesEditorPosition.ReplaceConstants:
				_cbReplaceConstants.Enabled = enabled;
				break;
			case ECompilePropertiesEditorPosition.EnableLoggingInBreakpoints:
				_cbEnableBreakpointLogging.Enabled = enabled;
				break;
			case ECompilePropertiesEditorPosition.Utf8EncodedStrings:
				_cbutf8Encoding.Enabled = enabled;
				break;
			case ECompilePropertiesEditorPosition.ReportCompiledPousDuringIncrementalCompile:
				_cbReportCompiledPousDuringIncrementalCompile.Enabled = enabled;
				break;
			case ECompilePropertiesEditorPosition.MaxNumberOfWarnings:
				_cbMaxWarnings.Enabled = enabled;
				break;
			case ECompilePropertiesEditorPosition.CompilerVersionWarningDisplay:
			case ECompilePropertiesEditorPosition.ProjectDefinesDialog:
				break;
			}
		}

		public void SetOptionState(ECompilePropertiesEditorPosition position, bool selected)
		{
			switch (position)
			{
			case ECompilePropertiesEditorPosition.AllowUnicodeInIdentifiers:
				_cbUnicodeIdentifier.Checked = selected;
				break;
			case ECompilePropertiesEditorPosition.ReplaceConstants:
				_cbReplaceConstants.Checked = selected;
				break;
			case ECompilePropertiesEditorPosition.EnableLoggingInBreakpoints:
				_cbEnableBreakpointLogging.Checked = selected;
				break;
			case ECompilePropertiesEditorPosition.Utf8EncodedStrings:
				_cbutf8Encoding.Checked = selected;
				break;
			case ECompilePropertiesEditorPosition.ReportCompiledPousDuringIncrementalCompile:
				_cbReportCompiledPousDuringIncrementalCompile.Checked = selected;
				break;
			}
		}

		public bool GetOption(ECompilePropertiesEditorPosition position)
		{
			bool result = false;
			switch (position)
			{
			case ECompilePropertiesEditorPosition.AllowUnicodeInIdentifiers:
				result = _cbUnicodeIdentifier.Checked;
				break;
			case ECompilePropertiesEditorPosition.ReplaceConstants:
				result = _cbReplaceConstants.Checked;
				break;
			case ECompilePropertiesEditorPosition.EnableLoggingInBreakpoints:
				result = _cbEnableBreakpointLogging.Checked;
				break;
			case ECompilePropertiesEditorPosition.Utf8EncodedStrings:
				result = _cbutf8Encoding.Checked;
				break;
			case ECompilePropertiesEditorPosition.ReportCompiledPousDuringIncrementalCompile:
				result = _cbReportCompiledPousDuringIncrementalCompile.Checked;
				break;
			}
			return result;
		}

		public string GetText(ECompilePropertiesEditorPosition position)
		{
			string empty = string.Empty;
			switch (position)
			{
			case ECompilePropertiesEditorPosition.CompilerVersionSelection:
				empty = _cbVersions.Text;
				break;
			case ECompilePropertiesEditorPosition.MaxNumberOfWarnings:
				empty = _cbMaxWarnings.Text;
				break;
			}
			return empty;
		}

		public void SetMessage(ECompilePropertiesEditorPosition position, string message, string iconName)
		{
			_errNotAvailable.Icon = APEnvironment.Engine.ResourceManager.GetIcon(GetType(), "_3S.CoDeSys.LanguageModelManagerConfigurators.Resources." + iconName + ".ico");
			_errNotAvailable.SetError(_errorLabel, message);
			_errorLabel.Text = message;
			_errorLabel.Show();
		}

		public void HideMessage()
		{
			_errNotAvailable.SetError(_errorLabel, string.Empty);
			_errorLabel.Hide();
		}

		public void ShowCompilerversions(IEnumerable<string> compilerversions)
		{
			foreach (string compilerversion in compilerversions)
			{
				_cbVersions.Items.Add(compilerversion);
			}
		}

		public void ShowCompilerversion(string stCompilerversion)
		{
			_cbVersions.Text = stCompilerversion;
			_cbVersions.SelectedItem = stCompilerversion;
		}

		public void ShowMaxNumbersOfWarnings(IEnumerable<string> maxNumbersOfWarnings)
		{
			foreach (string maxNumbersOfWarning in maxNumbersOfWarnings)
			{
				_cbMaxWarnings.Items.Add(maxNumbersOfWarning);
			}
		}

		public void ShowMaxNumberOfWarnings(string stMaxNumberOfWarnings)
		{
			_cbMaxWarnings.Text = stMaxNumberOfWarnings;
			_cbMaxWarnings.SelectedItem = stMaxNumberOfWarnings;
		}

		public object GetSelected(ECompilePropertiesEditorPosition position)
		{
			object result = null;
			switch (position)
			{
			case ECompilePropertiesEditorPosition.CompilerVersionSelection:
				result = _cbVersions.SelectedItem;
				break;
			case ECompilePropertiesEditorPosition.MaxNumberOfWarnings:
				result = _cbMaxWarnings.SelectedItem;
				break;
			}
			return result;
		}

		public void DisableCustomTextInput(ECompilePropertiesEditorPosition position)
		{
			switch (position)
			{
			case ECompilePropertiesEditorPosition.CompilerVersionSelection:
				_cbVersions.DropDownStyle = ComboBoxStyle.DropDownList;
				break;
			case ECompilePropertiesEditorPosition.MaxNumberOfWarnings:
				_cbMaxWarnings.DropDownStyle = ComboBoxStyle.DropDownList;
				break;
			}
		}

		public ValidationResult ValidateAndSave()
		{
			return _viewListener.ValidateAndSaveModelFromView();
		}

		public Control GetControlFromPosition(ECompilePropertiesEditorPosition position)
		{
			return PositionToControlForErrors[position];
		}

		public IList<string> StartDialog(ECompilePropertiesEditorPosition position, IList<string> startMessage)
		{
			if (position != ECompilePropertiesEditorPosition.ProjectDefinesDialog)
			{
				return new List<string>();
			}
			ProjectGlobalDefinesDialog projectGlobalDefinesDialog = new ProjectGlobalDefinesDialog(startMessage, new APEnvironmentFacade());
			if (projectGlobalDefinesDialog.ShowDialog(this) == DialogResult.OK)
			{
				return projectGlobalDefinesDialog.DialogAnswer;
			}
			return startMessage;
		}

		private void _cbVersions_SelectedIndexChanged(object sender, EventArgs e)
		{
			_viewListener.CompilerVersionChanged(_cbVersions.SelectedItem as string);
		}

		private void _btnEditProjectDefines_Click(object sender, EventArgs e)
		{
			_viewListener.Triggered(ECompilePropertiesEditorPosition.ProjectDefinesTrigger);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			base.OnHandleDestroyed(e);
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager componentResourceManager = new System.ComponentModel.ComponentResourceManager(typeof(_3S.CoDeSys.LanguageModelManagerConfigurators.CompileProperties));
			this._groupCompilerVersion = new System.Windows.Forms.GroupBox();
			this._fixVersionLabel = new System.Windows.Forms.Label();
			this._errorLabel = new System.Windows.Forms.Label();
			this._cbVersions = new System.Windows.Forms.ComboBox();
			this._errNotAvailable = new System.Windows.Forms.ErrorProvider(this.components);
			this._groupSettings = new System.Windows.Forms.GroupBox();
			this._btnEditProjectDefines = new System.Windows.Forms.Button();
			this._cbutf8Encoding = new System.Windows.Forms.CheckBox();
			this._cbEnableBreakpointLogging = new System.Windows.Forms.CheckBox();
			this._cbUnicodeIdentifier = new System.Windows.Forms.CheckBox();
			this._cbReplaceConstants = new System.Windows.Forms.CheckBox();
			this._cbReportCompiledPousDuringIncrementalCompile = new System.Windows.Forms.CheckBox();
			this._groupMessages = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this._cbMaxWarnings = new System.Windows.Forms.ComboBox();
			this._groupCompilerVersion.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)this._errNotAvailable).BeginInit();
			this._groupSettings.SuspendLayout();
			this._groupMessages.SuspendLayout();
			base.SuspendLayout();
			this._groupCompilerVersion.Controls.Add(this._fixVersionLabel);
			this._groupCompilerVersion.Controls.Add(this._errorLabel);
			this._groupCompilerVersion.Controls.Add(this._cbVersions);
			componentResourceManager.ApplyResources(this._groupCompilerVersion, "_groupCompilerVersion");
			this._groupCompilerVersion.Name = "_groupCompilerVersion";
			this._groupCompilerVersion.TabStop = false;
			componentResourceManager.ApplyResources(this._fixVersionLabel, "_fixVersionLabel");
			this._fixVersionLabel.Name = "_fixVersionLabel";
			componentResourceManager.ApplyResources(this._errorLabel, "_errorLabel");
			this._errorLabel.Name = "_errorLabel";
			this._cbVersions.FormattingEnabled = true;
			componentResourceManager.ApplyResources(this._cbVersions, "_cbVersions");
			this._cbVersions.Name = "_cbVersions";
			this._cbVersions.SelectedIndexChanged += new System.EventHandler(_cbVersions_SelectedIndexChanged);
			this._errNotAvailable.ContainerControl = this;
			this._groupSettings.Controls.Add(this._btnEditProjectDefines);
			this._groupSettings.Controls.Add(this._cbutf8Encoding);
			this._groupSettings.Controls.Add(this._cbEnableBreakpointLogging);
			this._groupSettings.Controls.Add(this._cbUnicodeIdentifier);
			this._groupSettings.Controls.Add(this._cbReplaceConstants);
			componentResourceManager.ApplyResources(this._groupSettings, "_groupSettings");
			this._groupSettings.Name = "_groupSettings";
			this._groupSettings.TabStop = false;
			componentResourceManager.ApplyResources(this._btnEditProjectDefines, "_btnEditProjectDefines");
			this._btnEditProjectDefines.Name = "_btnEditProjectDefines";
			this._btnEditProjectDefines.UseVisualStyleBackColor = true;
			this._btnEditProjectDefines.Click += new System.EventHandler(_btnEditProjectDefines_Click);
			componentResourceManager.ApplyResources(this._cbutf8Encoding, "_cbutf8Encoding");
			this._cbutf8Encoding.Name = "_cbutf8Encoding";
			this._cbutf8Encoding.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this._cbEnableBreakpointLogging, "_cbEnableBreakpointLogging");
			this._cbEnableBreakpointLogging.Name = "_cbEnableBreakpointLogging";
			this._cbEnableBreakpointLogging.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this._cbUnicodeIdentifier, "_cbUnicodeIdentifier");
			this._cbUnicodeIdentifier.Name = "_cbUnicodeIdentifier";
			this._cbUnicodeIdentifier.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this._cbReplaceConstants, "_cbReplaceConstants");
			this._cbReplaceConstants.Name = "_cbReplaceConstants";
			this._cbReplaceConstants.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this._cbReportCompiledPousDuringIncrementalCompile, "_cbReportCompiledPousDuringIncrementalCompile");
			this._cbReportCompiledPousDuringIncrementalCompile.Name = "_cbReportCompiledPousDuringIncrementalCompile";
			this._cbReportCompiledPousDuringIncrementalCompile.UseVisualStyleBackColor = true;
			this._groupMessages.Controls.Add(this._cbReportCompiledPousDuringIncrementalCompile);
			this._groupMessages.Controls.Add(this.label1);
			this._groupMessages.Controls.Add(this._cbMaxWarnings);
			componentResourceManager.ApplyResources(this._groupMessages, "_groupMessages");
			this._groupMessages.Name = "_groupMessages";
			this._groupMessages.TabStop = false;
			componentResourceManager.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			this._cbMaxWarnings.FormattingEnabled = true;
			componentResourceManager.ApplyResources(this._cbMaxWarnings, "_cbMaxWarnings");
			this._cbMaxWarnings.Name = "_cbMaxWarnings";
			componentResourceManager.ApplyResources(this, "$this");
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(this._groupMessages);
			base.Controls.Add(this._groupSettings);
			base.Controls.Add(this._groupCompilerVersion);
			base.Name = "CompileProperties";
			this._groupCompilerVersion.ResumeLayout(false);
			this._groupCompilerVersion.PerformLayout();
			((System.ComponentModel.ISupportInitialize)this._errNotAvailable).EndInit();
			this._groupSettings.ResumeLayout(false);
			this._groupSettings.PerformLayout();
			this._groupMessages.ResumeLayout(false);
			this._groupMessages.PerformLayout();
			base.ResumeLayout(false);
		}
	}
}
