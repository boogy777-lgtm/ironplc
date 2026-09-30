using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using _3S.CoDeSys.Controls.Controls;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.OnlineHelp;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	[AssociatedOnlineHelpTopic("codesys.chm::/_cds_library_development.htm")]
	public class LibraryDevelopmentProperties : UserControl
	{
		private IContainer components;

		private TextBox _textBoxDefines;

		private Button _buttonScan;

		private GroupBox _groupBoxCompilerDefines;

		private GroupBox _groupBoxCheckAllPoolObjects;

		private Label _labelPointerSize;

		private IconComboBox _cmbPointerSize;

		private ToolTip _toolTip;

		private Label _iconLabel;

		public LibraryDevelopmentProperties()
		{
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Expected O, but got Unknown
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Expected O, but got Unknown
			InitializeComponent();
			_textBoxDefines.Text = APEnvironment.LMServiceProvider.ConfigurationService.LibraryDevelopmentOptions.CompilerDefinesToUse;
			if (!(APEnvironment.LMServiceProvider.ConfigurationService.LibraryDevelopmentOptions is ILMLibraryDevelopmentOptions2 iLMLibraryDevelopmentOptions))
			{
				_groupBoxCheckAllPoolObjects.Visible = false;
				return;
			}
			IconComboBoxItem[] array = (IconComboBoxItem[])(object)new IconComboBoxItem[3]
			{
				new IconComboBoxItem(Strings.PointerSize32Bit, (Icon)null, (object)ECheckAllPoolObjectsTargetPointerSize.PointerSize4),
				new IconComboBoxItem(Strings.PointerSize64Bit, (Icon)null, (object)ECheckAllPoolObjectsTargetPointerSize.PointerSize8),
				new IconComboBoxItem(Strings.PointerSize32And64Bit, (Icon)null, (object)ECheckAllPoolObjectsTargetPointerSize.PointerSize4And8)
			};
			ECheckAllPoolObjectsTargetPointerSize checkAllPoolObjectsTargetPointerSize = iLMLibraryDevelopmentOptions.CheckAllPoolObjectsTargetPointerSize;
			IconComboBoxItem val = null;
			for (int i = 0; i < array.Length; i++)
			{
				((ComboBox)(object)_cmbPointerSize).Items.Add(array[i]);
				if (checkAllPoolObjectsTargetPointerSize == (ECheckAllPoolObjectsTargetPointerSize)array[i].get_Tag())
				{
					val = array[i];
				}
			}
			if (val == null)
			{
				val = array[0];
			}
			((ComboBox)(object)_cmbPointerSize).SelectedItem = val;
			bool flag = APEnvironment.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 15, 0);
			Version vinternal = new Version("3.5.15.0");
			_iconLabel.Visible = !flag;
			_toolTip.IsBalloon = true;
			string caption = string.Format(Strings.CheckAllPoolObjectPointerSize_Hint, APEnvironment.CompilerVersionMgr.MapFromInternalToOEMTextSave(vinternal));
			_toolTip.SetToolTip(_iconLabel, caption);
		}

		public bool Validate(ref string stMessage, ref Control failedControl)
		{
			APEnvironment.LMServiceProvider.ConfigurationService.LibraryDevelopmentOptions.CompilerDefinesToUse = _textBoxDefines.Text;
			object selectedItem = ((ComboBox)(object)_cmbPointerSize).SelectedItem;
			IconComboBoxItem val = (IconComboBoxItem)((selectedItem is IconComboBoxItem) ? selectedItem : null);
			if (val != null && APEnvironment.LMServiceProvider.ConfigurationService.LibraryDevelopmentOptions is ILMLibraryDevelopmentOptions2 iLMLibraryDevelopmentOptions)
			{
				iLMLibraryDevelopmentOptions.CheckAllPoolObjectsTargetPointerSize = (ECheckAllPoolObjectsTargetPointerSize)val.get_Tag();
			}
			return true;
		}

		private void OnButtonScanClick(object sender, EventArgs e)
		{
			_textBoxDefines.Text = string.Empty;
			IPreCompileContext[] array = APEnvironment.LanguageModelMgr.AllPreCompileContexts(bWithDevices: true, bWithLibraries: false);
			DefinePragmaCollector definePragmaCollector = new DefinePragmaCollector();
			HashSet<string> hashSet = new HashSet<string>();
			IPreCompileContext[] array2 = array;
			foreach (IPreCompileContext preCompileContext in array2)
			{
				ISignature[] allSignatures = preCompileContext.AllSignatures;
				foreach (ISignature signature in allSignatures)
				{
					ICompiledPOU compiledPOU = preCompileContext.GetCompiledPOU(signature.ObjectGuid);
					if (compiledPOU != null)
					{
						IStatement parseTree = compiledPOU.ParseTree;
						if (parseTree != null)
						{
							definePragmaCollector.GetDefines(parseTree, hashSet);
						}
					}
				}
			}
			string text = string.Empty;
			bool flag = true;
			foreach (string item in hashSet)
			{
				if (flag)
				{
					text += item;
					flag = false;
				}
				else
				{
					text = text + ", " + item;
				}
			}
			_textBoxDefines.Text = text;
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
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(_3S.CoDeSys.LanguageModelManagerConfigurators.LibraryDevelopmentProperties));
			this._textBoxDefines = new System.Windows.Forms.TextBox();
			this._buttonScan = new System.Windows.Forms.Button();
			this._groupBoxCompilerDefines = new System.Windows.Forms.GroupBox();
			this._groupBoxCheckAllPoolObjects = new System.Windows.Forms.GroupBox();
			this._cmbPointerSize = new IconComboBox();
			this._labelPointerSize = new System.Windows.Forms.Label();
			this._toolTip = new System.Windows.Forms.ToolTip(this.components);
			this._iconLabel = new System.Windows.Forms.Label();
			this._groupBoxCompilerDefines.SuspendLayout();
			this._groupBoxCheckAllPoolObjects.SuspendLayout();
			base.SuspendLayout();
			this._textBoxDefines.AcceptsReturn = true;
			resources.ApplyResources(this._textBoxDefines, "_textBoxDefines");
			this._textBoxDefines.Name = "_textBoxDefines";
			resources.ApplyResources(this._buttonScan, "_buttonScan");
			this._buttonScan.Name = "_buttonScan";
			this._buttonScan.UseVisualStyleBackColor = true;
			this._buttonScan.Click += new System.EventHandler(OnButtonScanClick);
			this._groupBoxCompilerDefines.Controls.Add(this._buttonScan);
			this._groupBoxCompilerDefines.Controls.Add(this._textBoxDefines);
			resources.ApplyResources(this._groupBoxCompilerDefines, "_groupBoxCompilerDefines");
			this._groupBoxCompilerDefines.Name = "_groupBoxCompilerDefines";
			this._groupBoxCompilerDefines.TabStop = false;
			this._groupBoxCheckAllPoolObjects.Controls.Add(this._iconLabel);
			this._groupBoxCheckAllPoolObjects.Controls.Add((System.Windows.Forms.Control)(object)this._cmbPointerSize);
			this._groupBoxCheckAllPoolObjects.Controls.Add(this._labelPointerSize);
			resources.ApplyResources(this._groupBoxCheckAllPoolObjects, "_groupBoxCheckAllPoolObjects");
			this._groupBoxCheckAllPoolObjects.Name = "_groupBoxCheckAllPoolObjects";
			this._groupBoxCheckAllPoolObjects.TabStop = false;
			((System.Windows.Forms.ComboBox)(object)this._cmbPointerSize).DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			((System.Windows.Forms.ComboBox)(object)this._cmbPointerSize).DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			((System.Windows.Forms.ListControl)(object)this._cmbPointerSize).FormattingEnabled = true;
			resources.ApplyResources(this._cmbPointerSize, "_cmbPointerSize");
			((System.Windows.Forms.Control)(object)this._cmbPointerSize).Name = "_cmbPointerSize";
			resources.ApplyResources(this._labelPointerSize, "_labelPointerSize");
			this._labelPointerSize.Name = "_labelPointerSize";
			resources.ApplyResources(this._iconLabel, "_iconLabel");
			this._iconLabel.Name = "_iconLabel";
			this._toolTip.SetToolTip(this._iconLabel, resources.GetString("_iconLabel.ToolTip"));
			resources.ApplyResources(this, "$this");
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(this._groupBoxCheckAllPoolObjects);
			base.Controls.Add(this._groupBoxCompilerDefines);
			base.Name = "LibraryDevelopmentProperties";
			this._groupBoxCompilerDefines.ResumeLayout(false);
			this._groupBoxCompilerDefines.PerformLayout();
			this._groupBoxCheckAllPoolObjects.ResumeLayout(false);
			this._groupBoxCheckAllPoolObjects.PerformLayout();
			base.ResumeLayout(false);
		}
	}
}
