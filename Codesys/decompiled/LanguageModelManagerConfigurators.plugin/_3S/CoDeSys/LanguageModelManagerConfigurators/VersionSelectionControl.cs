using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.Utilities;
using _3S.CoDeSys.VersionCompatibilityManager;
using _3S.CoDeSys.WorkspaceObject;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	public class VersionSelectionControl : UserControl, IVersionSelectionControl2, IVersionSelectionControl
	{
		private Version[] _availableVersions;

		private Version _currentVersion;

		private Version _newestAvailableVersion = new Version(0, 0, 0, 0);

		private bool _bNeedToInformOfNewestVersion = true;

		private readonly APEnvironmentFacade _apEnvironmentFacade = new APEnvironmentFacade();

		private static readonly Guid GUID_WORKSPACEOBJECT = new Guid("{6470A90F-B7CB-43ac-9AE5-94B2338B4573}");

		private IContainer components;

		private ComboBox _cbVersions;

		private Label label1;

		private Label label2;

		private Label _lbCurrentVersion;

		private Label label3;

		private Label _lbRecommendedVersion;

		private GroupBox groupBox1;

		private Label _noteLabel;

		public Guid Provider => VersionSelectionControlProvider.GUID;

		public VersionSelectionControl()
		{
			InitializeComponent();
		}

		public bool NewVersionAvailable(bool bAutomaticUpdate)
		{
			InitVersions();
			if (_apEnvironmentFacade.IsLibraryWithPinnedStorageVersion())
			{
				if (IsPatchVersion(_currentVersion))
				{
					return Array.Exists(_availableVersions, (Version v) => _currentVersion < v && !IsPatchVersion(v));
				}
				return false;
			}
			return _currentVersion != _newestAvailableVersion;
		}

		public void Initialize()
		{
			_ = string.Empty;
			string empty = string.Empty;
			List<string> stProfileStringList = new List<string>();
			if (_availableVersions == null || _currentVersion == null || _newestAvailableVersion.Major == 0)
			{
				InitVersions();
			}
			if (_newestAvailableVersion != null)
			{
				string text = APEnvironment.CompilerVersionMgr.MapFromInternalToOEMText(_newestAvailableVersion);
				_lbRecommendedVersion.Text = text;
			}
			else
			{
				_lbRecommendedVersion.Text = "???";
			}
			if (HandleLegacyUsecaseNewestCompilerversion(stProfileStringList))
			{
				return;
			}
			bool bIsLibraryWithPinnedStorageVersion = _apEnvironmentFacade.IsLibraryWithPinnedStorageVersion();
			string text2 = APEnvironment.CompilerVersionMgr.MapFromInternalToOEMTextSave(_currentVersion);
			_lbCurrentVersion.Text = text2;
			_cbVersions.SelectedIndex = _cbVersions.Items.Add(Strings.VersionSelection_DoNotUpdate);
			if (Array.Exists(_availableVersions, (Version v) => v == _currentVersion))
			{
				if (_currentVersion == _newestAvailableVersion)
				{
					empty = Strings.VersionSelection_Ok;
				}
				else
				{
					empty = string.Format(Strings.VersionSelection_Outdated, _lbRecommendedVersion.Text);
					AddPossibleUpdateVersions(_lbRecommendedVersion.Text, bIsLibraryWithPinnedStorageVersion);
				}
			}
			else
			{
				_cbVersions.Items.Add(string.Format(Strings.VersionSelection_UpdateToVersion, _lbRecommendedVersion.Text));
				_cbVersions.SelectedItem = Strings.VersionSelection_DoNotUpdate;
				empty = string.Format(Strings.VersionSelection_Missing, _lbRecommendedVersion.Text);
			}
			_noteLabel.Text = empty;
		}

		public void Save()
		{
			if (SaveForLibraryWithPinnedStorageVersion())
			{
				return;
			}
			Profile profile;
			string stProfileName;
			if ((string)_cbVersions.SelectedItem != Strings.VersionSelection_DoNotUpdate)
			{
				if (null != _newestAvailableVersion)
				{
					SetCompilerVersion(_newestAvailableVersion);
				}
			}
			else if (_currentVersion.ToString() == "255.255.255.255" && APEnvironment.Engine.Projects.PrimaryProject.GetProfile(out profile, out stProfileName) && profile != null)
			{
				Guid guid = PlugInGuidAttribute.FromAssembly(Assembly.GetExecutingAssembly()).Guid;
				if (profile.GetVersionConstraint(guid) is ExactVersionConstraint exactVersionConstraint)
				{
					SetCompilerVersion(exactVersionConstraint.Version);
				}
			}
		}

		private bool SaveForLibraryWithPinnedStorageVersion()
		{
			if (!_apEnvironmentFacade.IsLibraryWithPinnedStorageVersion())
			{
				return false;
			}
			string text = (string)_cbVersions.SelectedItem;
			if (Strings.VersionSelection_DoNotUpdate == text)
			{
				return true;
			}
			string text2 = string.Format(Strings.VersionSelection_UpdateToVersion, string.Empty);
			if (!text.StartsWith(text2))
			{
				return true;
			}
			if (!Version.TryParse(text.Substring(text2.Length), out var result))
			{
				return true;
			}
			SetCompilerVersion(result);
			return true;
		}

		private bool HandleLegacyUsecaseNewestCompilerversion(List<string> stProfileStringList)
		{
			if (_currentVersion != null && _currentVersion.ToString() == "255.255.255.255")
			{
				_lbCurrentVersion.Text = Strings.VersionSelection_Newest;
				string item = string.Format(Strings.VersionSelection_UpdateToVersion, _lbRecommendedVersion.Text);
				string text = string.Format(Strings.VersionSelection_InfoNewest, Strings.VersionSelection_Newest, _lbRecommendedVersion.Text);
				stProfileStringList.Add(item);
				_cbVersions.Items.Add(Strings.VersionSelection_DoNotUpdate);
				_cbVersions.SelectedIndex = _cbVersions.Items.Add(item);
				_noteLabel.Text = text;
				return true;
			}
			return false;
		}

		private static bool IsPatchVersion(Version v)
		{
			return v.Revision != 0;
		}

		private void AddPossibleUpdateVersions(string stRecommendedVersion, bool bIsLibraryWithPinnedStorageVersion)
		{
			if (bIsLibraryWithPinnedStorageVersion)
			{
				if (!IsPatchVersion(_currentVersion))
				{
					return;
				}
				{
					foreach (Version item in _availableVersions.Where((Version v) => _currentVersion < v && !IsPatchVersion(v)))
					{
						_cbVersions.Items.Add(string.Format(Strings.VersionSelection_UpdateToVersion, item));
					}
					return;
				}
			}
			_cbVersions.Items.Add(string.Format(Strings.VersionSelection_UpdateToVersion, stRecommendedVersion));
		}

		private void SetCompilerVersion(Version version)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Expected O, but got Unknown
			APEnvironment.CompilerVersionMgr.SetCompilerVersionExact(version);
			int handle = APEnvironment.Engine.Projects.PrimaryProject.Handle;
			IMetaObject objectToModify = APEnvironment.ObjectMgr.GetObjectToModify(handle, GUID_WORKSPACEOBJECT);
			ChunkedMemoryStream val = new ChunkedMemoryStream();
			APEnvironment.OptionStorage.Save(OptionRoot.Project, (Stream)(object)val);
			((Stream)(object)val).Close();
			(objectToModify.Object as IWorkspaceObject).OptionData = val.ToArray();
			APEnvironment.ObjectMgr.SetObject(objectToModify, bCommit: true, this);
		}

		public void Cancel()
		{
		}

		public Dictionary<string, string> SetAllItemsToNewestVersion()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (_bNeedToInformOfNewestVersion && _lbCurrentVersion.Text == Strings.VersionSelection_Newest)
			{
				_bNeedToInformOfNewestVersion = false;
				dictionary.Add(_lbCurrentVersion.Text, _lbRecommendedVersion.Text);
				return dictionary;
			}
			int index = ((!_apEnvironmentFacade.IsLibraryWithPinnedStorageVersion()) ? 1 : (_cbVersions.Items.Count - 1));
			if (_cbVersions.Items.Count > 1 && _cbVersions.SelectedItem != _cbVersions.Items[index])
			{
				_cbVersions.SelectedItem = _cbVersions.Items[index];
				dictionary.Add(_lbCurrentVersion.Text, _lbRecommendedVersion.Text);
				return dictionary;
			}
			return null;
		}

		private void InitVersions()
		{
			_availableVersions = APEnvironment.CompilerVersionMgr.AvailableCompilerVersionsOEMFilteredNotReplaced.OrderBy((Version v) => v).ToArray();
			if (APEnvironment.CompilerVersionMgr.UseNewestVersion)
			{
				_currentVersion = new Version(255, 255, 255, 255);
			}
			else
			{
				_currentVersion = APEnvironment.CompilerVersionMgr.CompilerVersionToUse();
			}
			Version[] availableVersions = _availableVersions;
			foreach (Version version in availableVersions)
			{
				if (version > _newestAvailableVersion)
				{
					_newestAvailableVersion = version;
				}
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
			System.ComponentModel.ComponentResourceManager componentResourceManager = new System.ComponentModel.ComponentResourceManager(typeof(_3S.CoDeSys.LanguageModelManagerConfigurators.VersionSelectionControl));
			this._cbVersions = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this._lbCurrentVersion = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this._lbRecommendedVersion = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this._noteLabel = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			base.SuspendLayout();
			this._cbVersions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._cbVersions.FormattingEnabled = true;
			componentResourceManager.ApplyResources(this._cbVersions, "_cbVersions");
			this._cbVersions.Name = "_cbVersions";
			componentResourceManager.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			componentResourceManager.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			componentResourceManager.ApplyResources(this._lbCurrentVersion, "_lbCurrentVersion");
			this._lbCurrentVersion.Name = "_lbCurrentVersion";
			componentResourceManager.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			componentResourceManager.ApplyResources(this._lbRecommendedVersion, "_lbRecommendedVersion");
			this._lbRecommendedVersion.Name = "_lbRecommendedVersion";
			componentResourceManager.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this._cbVersions);
			this.groupBox1.Controls.Add(this._lbRecommendedVersion);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this._lbCurrentVersion);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			componentResourceManager.ApplyResources(this._noteLabel, "_noteLabel");
			this._noteLabel.BackColor = System.Drawing.SystemColors.Info;
			this._noteLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this._noteLabel.ForeColor = System.Drawing.Color.Blue;
			this._noteLabel.Name = "_noteLabel";
			componentResourceManager.ApplyResources(this, "$this");
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(this._noteLabel);
			base.Controls.Add(this.groupBox1);
			this.MinimumSize = new System.Drawing.Size(580, 486);
			base.Name = "VersionSelectionControl";
			this.groupBox1.ResumeLayout(false);
			base.ResumeLayout(false);
		}
	}
}
