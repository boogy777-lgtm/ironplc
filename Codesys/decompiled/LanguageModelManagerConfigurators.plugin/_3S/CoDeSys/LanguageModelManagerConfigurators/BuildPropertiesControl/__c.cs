using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using _3S.CoDeSys.ApplicationObject;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Views;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	public class BuildPropertiesControl : UserControl, IPropertiesEditorView, IEditor, IObjectPropertiesControl
	{
		internal static readonly Guid BuildPropertyGuid = new Guid("{24568A24-C491-472c-A21F-EE5D33859FAB}");

		private IEditor _editor;

		private bool _bDuringReload;

		private string _stCompilerDefinesOrig = string.Empty;

		private string[] _originalUndefines = new string[0];

		private CheckBox _excludeCheckBox;

		private CheckBox _externalCheckBox;

		private Label _externalLabel;

		private TextBox _tbDefines;

		private Label _labelDefines;

		private CheckBox _enableSystemCallCheckBox;

		private CheckBox _linkAlways;

		private Panel _panelDeviceDefines;

		private Label _labelDeviceDefines;

		private ListBox _lbUndefines;

		private Button _btnUndefine;

		private ListBox _lbDefinedByDevice;

		private Button _btnRemoveUndefine;

		private IContainer components;

		private Label label2;

		private Label label1;

		private ToolTip toolTip1;

		private IMetaObjectStub _mos;

		public Control Control => this;

		public Guid ObjectGuid => _editor.ObjectGuid;

		public int ProjectHandle => _editor.ProjectHandle;

		private IEnumerable<string> UndefinedItems
		{
			get
			{
				if (_lbUndefines.DataSource != null)
				{
					return (string[])_lbUndefines.DataSource;
				}
				return new string[0];
			}
		}

		private IEnumerable<string> SelectedDefines => _lbDefinedByDevice.SelectedItems.Cast<string>();

		private IEnumerable<string> SelectedItemsThatCanBeUndefined => SelectedDefines.Except(UndefinedItems);

		private IEnumerable<string> SelectedUndefines => _lbUndefines.SelectedItems.Cast<string>();

		public BuildPropertiesControl(IMetaObjectStub mos)
		{
			_mos = mos;
			InitializeComponent();
		}

		private IBuildProperty5 GetBuildProperty(IMetaObject mo)
		{
			IBuildProperty5 buildProperty = mo.GetProperty(BuildPropertyGuid) as IBuildProperty5;
			if (buildProperty == null)
			{
				buildProperty = APEnvironment.CreateBuildProperty();
				mo.AddProperty(buildProperty);
			}
			return buildProperty;
		}

		private void UpdateDeviceDefineButtonStates()
		{
			_btnRemoveUndefine.Enabled = SelectedUndefines.Any();
			_btnUndefine.Enabled = SelectedItemsThatCanBeUndefined.Any();
		}

		private string[] GetDeviceDefines()
		{
			string[] result = new string[0];
			string compilerDefinesOfDeviceDescription = APEnvironment.LMServiceProvider.CompileService.GetCompilerDefinesOfDeviceDescription(_mos.ObjectGuid);
			if (string.IsNullOrWhiteSpace(compilerDefinesOfDeviceDescription))
			{
				return result;
			}
			return (from p in compilerDefinesOfDeviceDescription.Split(',')
				where !string.IsNullOrWhiteSpace(p)
				select p.Trim()).ToArray();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(_3S.CoDeSys.LanguageModelManagerConfigurators.BuildPropertiesControl));
			this._excludeCheckBox = new System.Windows.Forms.CheckBox();
			this._externalCheckBox = new System.Windows.Forms.CheckBox();
			this._externalLabel = new System.Windows.Forms.Label();
			this._tbDefines = new System.Windows.Forms.TextBox();
			this._labelDefines = new System.Windows.Forms.Label();
			this._enableSystemCallCheckBox = new System.Windows.Forms.CheckBox();
			this._linkAlways = new System.Windows.Forms.CheckBox();
			this._panelDeviceDefines = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this._btnRemoveUndefine = new System.Windows.Forms.Button();
			this._lbUndefines = new System.Windows.Forms.ListBox();
			this._btnUndefine = new System.Windows.Forms.Button();
			this._lbDefinedByDevice = new System.Windows.Forms.ListBox();
			this._labelDeviceDefines = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this._panelDeviceDefines.SuspendLayout();
			base.SuspendLayout();
			resources.ApplyResources(this._excludeCheckBox, "_excludeCheckBox");
			this._excludeCheckBox.Name = "_excludeCheckBox";
			this._excludeCheckBox.CheckedChanged += new System.EventHandler(OnExcludeCheckBoxCheckedChanged);
			resources.ApplyResources(this._externalCheckBox, "_externalCheckBox");
			this._externalCheckBox.Name = "_externalCheckBox";
			this._externalCheckBox.CheckedChanged += new System.EventHandler(OnExternalCheckBoxCheckedChanged);
			resources.ApplyResources(this._externalLabel, "_externalLabel");
			this._externalLabel.Name = "_externalLabel";
			resources.ApplyResources(this._tbDefines, "_tbDefines");
			this._tbDefines.Name = "_tbDefines";
			this._tbDefines.TextChanged += new System.EventHandler(_tbDefines_TextChanged);
			resources.ApplyResources(this._labelDefines, "_labelDefines");
			this._labelDefines.Name = "_labelDefines";
			resources.ApplyResources(this._enableSystemCallCheckBox, "_enableSystemCallCheckBox");
			this._enableSystemCallCheckBox.Name = "_enableSystemCallCheckBox";
			this._enableSystemCallCheckBox.CheckedChanged += new System.EventHandler(OnEnableSystemCallCheckBoxCheckedChanged);
			resources.ApplyResources(this._linkAlways, "_linkAlways");
			this._linkAlways.Name = "_linkAlways";
			this._linkAlways.UseVisualStyleBackColor = true;
			this._linkAlways.CheckedChanged += new System.EventHandler(OnLinkAlwaysChanged);
			resources.ApplyResources(this._panelDeviceDefines, "_panelDeviceDefines");
			this._panelDeviceDefines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this._panelDeviceDefines.Controls.Add(this.label2);
			this._panelDeviceDefines.Controls.Add(this.label1);
			this._panelDeviceDefines.Controls.Add(this._btnRemoveUndefine);
			this._panelDeviceDefines.Controls.Add(this._lbUndefines);
			this._panelDeviceDefines.Controls.Add(this._btnUndefine);
			this._panelDeviceDefines.Controls.Add(this._lbDefinedByDevice);
			this._panelDeviceDefines.Name = "_panelDeviceDefines";
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			resources.ApplyResources(this._btnRemoveUndefine, "_btnRemoveUndefine");
			this._btnRemoveUndefine.Name = "_btnRemoveUndefine";
			this.toolTip1.SetToolTip(this._btnRemoveUndefine, resources.GetString("_btnRemoveUndefine.ToolTip"));
			this._btnRemoveUndefine.UseVisualStyleBackColor = true;
			this._btnRemoveUndefine.Click += new System.EventHandler(_btnRemoveUndefine_Click);
			resources.ApplyResources(this._lbUndefines, "_lbUndefines");
			this._lbUndefines.FormattingEnabled = true;
			this._lbUndefines.Name = "_lbUndefines";
			this._lbUndefines.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this._lbUndefines.SelectedIndexChanged += new System.EventHandler(_lbUndefines_SelectedIndexChanged);
			resources.ApplyResources(this._btnUndefine, "_btnUndefine");
			this._btnUndefine.Name = "_btnUndefine";
			this.toolTip1.SetToolTip(this._btnUndefine, resources.GetString("_btnUndefine.ToolTip"));
			this._btnUndefine.UseVisualStyleBackColor = true;
			this._btnUndefine.Click += new System.EventHandler(_btnUndefine_Click);
			resources.ApplyResources(this._lbDefinedByDevice, "_lbDefinedByDevice");
			this._lbDefinedByDevice.FormattingEnabled = true;
			this._lbDefinedByDevice.Name = "_lbDefinedByDevice";
			this._lbDefinedByDevice.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this._lbDefinedByDevice.SelectedIndexChanged += new System.EventHandler(_lbDefinedByDevice_SelectedIndexChanged);
			resources.ApplyResources(this._labelDeviceDefines, "_labelDeviceDefines");
			this._labelDeviceDefines.Name = "_labelDeviceDefines";
			base.Controls.Add(this._labelDeviceDefines);
			base.Controls.Add(this._panelDeviceDefines);
			base.Controls.Add(this._linkAlways);
			base.Controls.Add(this._enableSystemCallCheckBox);
			base.Controls.Add(this._labelDefines);
			base.Controls.Add(this._tbDefines);
			base.Controls.Add(this._externalLabel);
			base.Controls.Add(this._externalCheckBox);
			base.Controls.Add(this._excludeCheckBox);
			resources.ApplyResources(this, "$this");
			base.Name = "BuildPropertiesControl";
			this._panelDeviceDefines.ResumeLayout(false);
			this._panelDeviceDefines.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		public void Initialize(IEditor editor)
		{
			if (editor == null)
			{
				throw new ArgumentNullException("editor");
			}
			_editor = editor;
		}

		public IMetaObject GetObjectToRead()
		{
			return _editor.GetObjectToRead();
		}

		private void GetExcludeFromBuildInfo(out bool check, out bool enable)
		{
			check = APEnvironment.ProjectLanguageModel.IsExcludedFromBuild(_editor.ProjectHandle, _editor.ObjectGuid, out var inherited);
			enable = !inherited;
		}

		public void Reload()
		{
			try
			{
				_bDuringReload = true;
				GetExcludeFromBuildInfo(out var check, out var enable);
				_excludeCheckBox.Checked = check;
				_excludeCheckBox.Enabled = enable;
				IMetaObject objectToRead = GetObjectToRead();
				IBuildProperty5 buildProperty = objectToRead.GetProperty(BuildPropertyGuid) as IBuildProperty5;
				if (buildProperty != null)
				{
					_externalCheckBox.Checked = buildProperty.External;
					_tbDefines.Text = buildProperty.CompilerDefines;
					_stCompilerDefinesOrig = buildProperty.CompilerDefines.Trim();
					_originalUndefines = buildProperty.Undefines?.ToArray() ?? new string[0];
					_enableSystemCallCheckBox.Checked = buildProperty.EnableSystemCall;
					_linkAlways.Checked = buildProperty.LinkAlways;
				}
				else
				{
					_externalCheckBox.Checked = false;
					_tbDefines.Text = string.Empty;
					_enableSystemCallCheckBox.Checked = false;
					_linkAlways.Checked = false;
				}
				_excludeCheckBox.Enabled = false;
				_externalCheckBox.Enabled = false;
				_externalLabel.Enabled = false;
				_linkAlways.Enabled = false;
				_tbDefines.Enabled = false;
				_labelDefines.Enabled = false;
				_enableSystemCallCheckBox.Enabled = false;
				_panelDeviceDefines.Visible = false;
				_labelDeviceDefines.Visible = false;
				bool flag = IsUsedOnline(objectToRead);
				ILanguageModelBuildPropertiesControl languageModelBuildPropertiesControl = objectToRead.Object as ILanguageModelBuildPropertiesControl;
				ILanguageModelProviderBuildPropertiesControl languageModelProviderBuildPropertiesControl = objectToRead.Object as ILanguageModelProviderBuildPropertiesControl;
				if (flag)
				{
					return;
				}
				bool enabled = false;
				bool enabled2 = false;
				bool enabled3 = false;
				bool enabled4 = false;
				bool enabled5 = false;
				bool flag2 = languageModelProviderBuildPropertiesControl is ILanguageModelProviderBuildPropertiesControl2 languageModelProviderBuildPropertiesControl2 && languageModelProviderBuildPropertiesControl2.UndefinesEnabled && APEnvironment.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 11, 20);
				if (languageModelBuildPropertiesControl != null)
				{
					enabled = languageModelBuildPropertiesControl.ExternalEnabled;
					enabled2 = languageModelBuildPropertiesControl.ExcludeFromBuildEnabled && enable;
					enabled3 = languageModelBuildPropertiesControl.LinkAlwaysEnabled;
					enabled4 = languageModelBuildPropertiesControl.CompilerDefinesEnabled;
					enabled5 = languageModelBuildPropertiesControl.EnableSystemCallEnabled;
				}
				else if (languageModelProviderBuildPropertiesControl != null)
				{
					enabled = languageModelProviderBuildPropertiesControl.ExternalEnabled;
					enabled2 = languageModelProviderBuildPropertiesControl.ExcludeFromBuildEnabled && enable;
					enabled3 = languageModelProviderBuildPropertiesControl.LinkAlwaysEnabled;
					enabled4 = languageModelProviderBuildPropertiesControl.CompilerDefinesEnabled;
					enabled5 = languageModelProviderBuildPropertiesControl.EnableSystemCallEnabled;
				}
				else if (objectToRead.Object is ILanguageModelProvider)
				{
					enabled2 = true;
				}
				_excludeCheckBox.Enabled = true;
				_linkAlways.Enabled = enabled3;
				_excludeCheckBox.Enabled = enabled2;
				_externalCheckBox.Enabled = enabled;
				_externalLabel.Enabled = enabled;
				_tbDefines.Enabled = enabled4;
				_labelDefines.Enabled = enabled4;
				_enableSystemCallCheckBox.Enabled = enabled5;
				_panelDeviceDefines.Visible = flag2;
				_labelDeviceDefines.Visible = flag2;
				if (flag2)
				{
					_lbDefinedByDevice.DataSource = GetDeviceDefines();
					if (buildProperty?.Undefines != null)
					{
						_lbUndefines.DataSource = buildProperty.Undefines.ToArray();
					}
				}
				UpdateDeviceDefineButtonStates();
			}
			finally
			{
				_bDuringReload = false;
			}
		}

		public void SetObject(int nProjectHandle, Guid objectGuid)
		{
		}

		public void Save(bool bCommit)
		{
		}

		public void ChangesCommitted()
		{
			bool num = _stCompilerDefinesOrig != _tbDefines.Text.Trim();
			bool flag = !BuildPropertyHelper.OrderInsensitiveSequenceEqual(_originalUndefines, UndefinedItems);
			if (!(num || flag))
			{
				return;
			}
			_stCompilerDefinesOrig = _tbDefines.Text.Trim();
			IMetaObject objectToRead = _editor.GetObjectToRead();
			if (objectToRead == null)
			{
				return;
			}
			IProject primaryProject = APEnvironment.Engine.Projects.PrimaryProject;
			if (primaryProject == null || !typeof(IApplicationObject).IsAssignableFrom(objectToRead.Object.GetType()))
			{
				return;
			}
			try
			{
				APEnvironment.Engine.BeginCleanAll();
				APEnvironment.Engine.UpdateLanguageModel(primaryProject.Handle);
			}
			finally
			{
				APEnvironment.Engine.EndCleanAll();
			}
		}

		public IMetaObject GetObjectToModify()
		{
			return _editor.GetObjectToModify();
		}

		private void OnExcludeCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (_bDuringReload)
			{
				return;
			}
			try
			{
				IMetaObject objectToModify = GetObjectToModify();
				if (objectToModify != null)
				{
					GetBuildProperty(objectToModify).ExcludeFromBuild = _excludeCheckBox.Checked;
				}
				else
				{
					Reload();
				}
			}
			catch (Exception ex)
			{
				APEnvironment.Engine.MessageService.Error(ex.Message);
			}
		}

		private void OnExternalCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (_bDuringReload)
			{
				return;
			}
			try
			{
				IMetaObject objectToModify = GetObjectToModify();
				if (objectToModify != null)
				{
					GetBuildProperty(objectToModify).External = _externalCheckBox.Checked;
				}
				else
				{
					Reload();
				}
			}
			catch (Exception ex)
			{
				APEnvironment.Engine.MessageService.Error(ex.Message);
			}
		}

		private void _tbDefines_TextChanged(object sender, EventArgs e)
		{
			if (_bDuringReload)
			{
				return;
			}
			try
			{
				IMetaObject objectToModify = GetObjectToModify();
				if (objectToModify != null)
				{
					GetBuildProperty(objectToModify).CompilerDefines = _tbDefines.Text;
				}
				else
				{
					Reload();
				}
			}
			catch (Exception ex)
			{
				APEnvironment.Engine.MessageService.Error(ex.Message);
			}
		}

		private void OnEnableSystemCallCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (_bDuringReload)
			{
				return;
			}
			try
			{
				IMetaObject objectToModify = GetObjectToModify();
				if (objectToModify != null)
				{
					GetBuildProperty(objectToModify).EnableSystemCall = _enableSystemCallCheckBox.Checked;
				}
				else
				{
					Reload();
				}
			}
			catch (Exception ex)
			{
				APEnvironment.Engine.MessageService.Error(ex.Message);
			}
		}

		private void OnLinkAlwaysChanged(object sender, EventArgs e)
		{
			if (_bDuringReload)
			{
				return;
			}
			try
			{
				IMetaObject objectToModify = GetObjectToModify();
				if (objectToModify != null)
				{
					GetBuildProperty(objectToModify).LinkAlways = _linkAlways.Checked;
				}
				else
				{
					Reload();
				}
			}
			catch (Exception ex)
			{
				APEnvironment.Engine.MessageService.Error(ex.Message);
			}
		}

		private static bool IsUsedOnline(IMetaObject mo)
		{
			IMetaObjectStub metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(mo.ProjectHandle, mo.ObjectGuid);
			return APEnvironment.OnlineUIServicesOrNull?.IsUsedOnline(metaObjectStub) ?? false;
		}

		private void _btnUndefine_Click(object sender, EventArgs e)
		{
			LList<string> val = Enumerable.ToLList<string>(UndefinedItems);
			foreach (string item in SelectedItemsThatCanBeUndefined)
			{
				val.Add(item);
			}
			_lbUndefines.DataSource = ((IEnumerable<string>)val).Distinct().ToArray();
			WriteDeviceDefinesToObject();
			UpdateDeviceDefineButtonStates();
		}

		private void WriteDeviceDefinesToObject()
		{
			if (_bDuringReload)
			{
				return;
			}
			try
			{
				IMetaObject objectToModify = GetObjectToModify();
				if (objectToModify != null)
				{
					if (GetBuildProperty(objectToModify) is IBuildProperty6 buildProperty)
					{
						buildProperty.Undefines = UndefinedItems.ToList();
					}
				}
				else
				{
					Reload();
				}
			}
			catch (Exception ex)
			{
				APEnvironment.Engine.MessageService.Error(ex.Message);
			}
		}

		private void _lbDefinedByDevice_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDeviceDefineButtonStates();
		}

		private void _lbUndefines_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDeviceDefineButtonStates();
		}

		private void _btnRemoveUndefine_Click(object sender, EventArgs e)
		{
			IEnumerable<string> selectedUndefines = SelectedUndefines;
			if (selectedUndefines.Any())
			{
				_lbUndefines.DataSource = UndefinedItems.Except(selectedUndefines).ToArray();
			}
			WriteDeviceDefinesToObject();
			UpdateDeviceDefineButtonStates();
		}
	}
}
