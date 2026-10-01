using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using _3S.CoDeSys.Controls.Collections;
using _3S.CoDeSys.Controls.Controls;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.OnlineHelp;
using _3S.CoDeSys.Core.Views;
using _3S.CoDeSys.LanguageModelManagerConfigurators.OnlineChange;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	[AssociatedOnlineHelpTopic("codesys.chm::/_cds_cmd_memory_reserve_online_change.htm")]
	public class OnlineChangeAllocationPlusView : UserControl, IView, ICloseViewOnClose
	{
		private delegate void AddSignatureNodeToCategory(IDictionary<int, ListViewCategoryItem> dic, ModelNode node, int cat);

		private const int CAT_ALL = -2;

		private const int CAT_POOL = -1;

		private const int CAT_NO_RESERVE = 0;

		private readonly MemoryReserveViewContext _viewContext = new MemoryReserveViewContext();

		private int _totalNumberOfFBs;

		private IList<ApplicationOption> _applications;

		private IContainer components;

		private TreeTableView _treeTableView;

		private ComboBox _cbApplication;

		private Button _btnScan;

		private Label label1;

		private Label _lblFbNo;

		private ListBox _lstCategories;

		private Label label3;

		private Label _lblAdditionalMemSize;

		private GroupBox groupBox1;

		private GroupBox groupBox2;

		private Label label5;

		private Button _btn_MultiApply;

		private TextBox _txtNewMemReserve;

		private GroupBox groupBox3;

		private SplitContainer splitContainer1;

		private TableLayoutPanel tableLayoutPanel1;

		private Label _lblBuildAppInfo;

		private GroupBox groupBox4;

		private Label label2;

		private Button _btnEnableEditing;

		public string Caption => Strings.OnlineChangeAllocPlusViewName;

		public Control Control => this;

		public DockingPosition DefaultDockingPosition => DockingPosition.MDIChild;

		public string Description => Strings.OnlineChangeAllocPlusViewDescr;

		public Icon LargeIcon => SmallIcon;

		public Control[] Panes => new Control[1] { this };

		public DockingPosition PossibleDockingPositions => DockingPosition.All;

		public Icon SmallIcon => null;

		private Guid ActiveApplication { get; set; }

		public int LastSortedColumn { get; private set; }

		public bool LastSortedAscending { get; private set; }

		public OnlineChangeAllocationPlusView()
		{
			InitializeComponent();
			APEnvironment.Engine.Projects.PrimaryProjectSwitched += Projects_PrimaryProjectSwitched;
			APEnvironment.CompilerVersionMgr.CompilerVersionChanged += CompilerVersionMgrOnCompilerVersionChanged;
			ResetColumnSortState();
			Reload();
		}

		private void CompilerVersionMgrOnCompilerVersionChanged(object sender, CompilerVersionChangedEventArgs args)
		{
			Clear();
			Reload();
		}

		private void Projects_PrimaryProjectSwitched(IProject oldProject, IProject newProject)
		{
			Clear();
			Reload();
		}

		public void Reload()
		{
			ListApplicationsInComboBox();
			DisableEditing();
		}

		private void ListApplicationsInComboBox()
		{
			_cbApplication.BeginUpdate();
			_cbApplication.SelectedIndexChanged -= _cbApplication_SelectedIndexChanged;
			_cbApplication.DataSource = null;
			if (APEnvironment.Engine.Projects.PrimaryProject != null)
			{
				if (ActiveApplication == Guid.Empty)
				{
					ActiveApplication = APEnvironment.Engine.Projects.PrimaryProject.ActiveApplication;
				}
				_applications = Utilities.CollectApplications();
				_cbApplication.DataSource = _applications;
				_cbApplication.DisplayMember = "Name";
				_cbApplication.ValueMember = "Value";
				_cbApplication.SelectedValue = ActiveApplication;
			}
			_cbApplication.SelectedIndexChanged += _cbApplication_SelectedIndexChanged;
			_cbApplication.EndUpdate();
		}

		private void SetStateOfButtonForEnablingEditing()
		{
			DisableEditing();
			if (APEnvironment.Engine.Projects.PrimaryProject != null)
			{
				if (APEnvironment.LanguageModelMgr.GetReferenceContextIfAvailable(ActiveApplication) != null)
				{
					_btnEnableEditing.Enabled = Utilities.MemoryReserveSupported;
					_btnEnableEditing.Text = Strings.BtnEnable;
				}
				else
				{
					EnableEditing();
				}
			}
		}

		private void _cbApplication_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_cbApplication.SelectedValue != null && ActiveApplication != (Guid)_cbApplication.SelectedValue)
			{
				Clear();
				ActiveApplication = (Guid)_cbApplication.SelectedValue;
			}
		}

		private void Clear()
		{
			_treeTableView.BeginUpdate();
			_treeTableView.set_Model((ITreeTableModel)null);
			_treeTableView.EndUpdate();
			_lstCategories.BeginUpdate();
			_lstCategories.Items.Clear();
			_lstCategories.EndUpdate();
			_viewContext.ActiveModel = null;
			_viewContext.AllFBsModel = null;
			UpdateStatusInfo();
		}

		public bool CanExecuteStandardCommand(Guid commandGuid)
		{
			return false;
		}

		public void ExecuteStandardCommand(Guid commandGuid)
		{
		}

		private void _btnScan_Click(object sender, EventArgs e)
		{
			if (APEnvironment.Engine.Projects.PrimaryProject == null)
			{
				return;
			}
			ResetColumnSortState();
			IDictionary<int, ListViewCategoryItem> dictionary = CreateListOfAllFunctionBlocks();
			_lstCategories.BeginUpdate();
			_lstCategories.Items.Clear();
			List<int> list = dictionary.Keys.ToList();
			list.Sort();
			foreach (int item in list)
			{
				ListViewCategoryItem listViewCategoryItem = dictionary[item];
				string name;
				switch (item)
				{
				case -2:
					name = Strings.LstCatAllFbs;
					break;
				case 0:
					name = Strings.LstCatNoReserve;
					break;
				case -1:
					name = Strings.LstCatPool;
					break;
				default:
					name = string.Format(Strings.LstCatSize, item);
					break;
				}
				listViewCategoryItem.Name = name;
				_lstCategories.Items.Add(listViewCategoryItem);
			}
			_viewContext.AllFBsModel = dictionary[-2].Model;
			_lstCategories.SelectedIndex = 0;
			_lstCategories.EndUpdate();
			SortByColumn(0);
			UpdateStatusInfo();
			SetStateOfButtonForEnablingEditing();
			ListApplicationsInComboBox();
		}

		private void UpdateStatusInfo()
		{
			if (_viewContext.AllFBsModel != null)
			{
				_lblFbNo.Text = _totalNumberOfFBs.ToString();
				int num = Utilities.CalculateTotalAdditionalSizeForMemoryReserve(_viewContext.AllFBsModel);
				_lblAdditionalMemSize.Text = string.Format(Strings.SizeForMemoryReserve, num);
				_viewContext.UpToDate = APEnvironment.LMServiceProvider.CompileService.IsUpToDate(ActiveApplication);
				if (APEnvironment.LMServiceProvider.CompileService.GetCompiledApplicationSet(ActiveApplication) == null)
				{
					_lblBuildAppInfo.Text = Strings.InfoBuildAppForDetails;
				}
				else if (!_viewContext.UpToDate)
				{
					_lblBuildAppInfo.Text = Strings.InfoAppNotUpToDate;
				}
				else
				{
					_lblBuildAppInfo.Text = string.Empty;
				}
			}
			else
			{
				_lblFbNo.Text = string.Empty;
				_lblAdditionalMemSize.Text = string.Empty;
				_lblBuildAppInfo.Text = string.Empty;
			}
			if (!Utilities.MemoryReserveSupported)
			{
				_lblBuildAppInfo.Text = Strings.InfoCVTooOld;
			}
		}

		private IDictionary<int, ListViewCategoryItem> CreateListOfAllFunctionBlocks()
		{
			IDictionary<int, ListViewCategoryItem> dic = (IDictionary<int, ListViewCategoryItem>)new LDictionary<int, ListViewCategoryItem>();
			dic[-2] = new ListViewCategoryItem();
			dic[-1] = new ListViewCategoryItem();
			_totalNumberOfFBs = 0;
			int handle = APEnvironment.Engine.Projects.PrimaryProject.Handle;
			APEnvironment.ObjectMgr.FinishLoadProject(handle);
			if (!(APEnvironment.LanguageModelMgr.GetPrecompileContext(ActiveApplication) is IPreCompileContext14 precom))
			{
				return dic;
			}
			AddSignatureNodeToCategory addNodeToCat = delegate(IDictionary<int, ListViewCategoryItem> items, ModelNode node, int cat)
			{
				dic[-2].Model.AddNode(node);
				dic[cat].Model.AddNode(node);
			};
			AddAllSignaturesOfPrecom(ActiveApplication, precom, dic, addNodeToCat);
			AddSignatureNodeToCategory addNodeToCat2 = delegate(IDictionary<int, ListViewCategoryItem> items, ModelNode node, int cat)
			{
				dic[-2].Model.AddNode(node);
				dic[-1].Model.AddNode(node);
				dic[cat].Model.AddNode(node);
				node.PouName = node.PouName + " (" + Strings.LstCatPool + ")";
			};
			IPreCompileContext14 precom2 = APEnvironment.LanguageModelMgr.GetPrecompileContext(Guid.Empty) as IPreCompileContext14;
			AddAllSignaturesOfPrecom(ActiveApplication, precom2, dic, addNodeToCat2);
			return dic;
		}

		private void AddAllSignaturesOfPrecom(Guid appGuid, IPreCompileContext14 precom, IDictionary<int, ListViewCategoryItem> dic, AddSignatureNodeToCategory addNodeToCat)
		{
			int handle = APEnvironment.Engine.Projects.PrimaryProject.Handle;
			ISignature[] allSignatures = precom.AllSignatures;
			foreach (ISignature signature in allSignatures)
			{
				if (signature.POUType == Operator.FunctionBlock && !APEnvironment.LanguageModelMgr.IsHiddenSignature(signature, GUIHidingFlags.AllCommon) && APEnvironment.ObjectMgr.ExistsObject(handle, signature.ObjectGuid))
				{
					int memoryReserveForPOU = Utilities.GetMemoryReserveForPOU(handle, signature.ObjectGuid);
					if (!dic.ContainsKey(memoryReserveForPOU))
					{
						dic[memoryReserveForPOU] = new ListViewCategoryItem();
					}
					int compiledSizeOfSignature = Utilities.GetCompiledSizeOfSignature(appGuid, precom, signature);
					int remainingMemReserveSizeOfSignature = Utilities.GetRemainingMemReserveSizeOfSignature(appGuid, precom, signature);
					int instanceCount = Utilities.GetInstanceCount(appGuid, precom, signature);
					ModelNode node = new ModelNode(handle, signature.ObjectGuid, signature.OrgName, compiledSizeOfSignature, memoryReserveForPOU, instanceCount, remainingMemReserveSizeOfSignature, _viewContext);
					addNodeToCat(dic, node, memoryReserveForPOU);
					_totalNumberOfFBs++;
				}
			}
		}

		private void _lstCategories_SelectedIndexChanged(object sender, EventArgs e)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Expected O, but got Unknown
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Expected O, but got Unknown
			try
			{
				_treeTableView.BeginUpdate();
				if (_treeTableView.get_Model() != null)
				{
					_treeTableView.get_Model().remove_Changed(new TreeTableModelEventHandler(ModelOnChanged));
				}
				_treeTableView.set_Model((ITreeTableModel)(object)((ListViewCategoryItem)_lstCategories.SelectedItem).Model);
				_viewContext.ActiveModel = _treeTableView.get_Model();
				_treeTableView.get_Model().add_Changed(new TreeTableModelEventHandler(ModelOnChanged));
			}
			finally
			{
				_treeTableView.EndUpdate();
			}
		}

		private void ModelOnChanged(object sender, TreeTableModelEventArgs treeTableModelEventArgs)
		{
			UpdateStatusInfo();
		}

		private void _btn_MultiApply_Click(object sender, EventArgs e)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Expected O, but got Unknown
			if (!int.TryParse(_txtNewMemReserve.Text, out var result))
			{
				return;
			}
			foreach (TreeTableViewNode item in (TreeTableViewNodeCollection)_treeTableView.get_SelectedNodes())
			{
				TreeTableViewNode val = item;
				_treeTableView.GetModelNode(val).SetValue(3, (object)result);
			}
		}

		private void _txtNewMemReserve_TextChanged(object sender, EventArgs e)
		{
			int result;
			bool enabled = int.TryParse(_txtNewMemReserve.Text, out result);
			_btn_MultiApply.Enabled = enabled;
		}

		private void _treeTableView_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			SortByColumn(e.Column);
		}

		public void SortByColumn(int iColumnIndex)
		{
			if (iColumnIndex < 0 || _treeTableView.get_Model() == null || _treeTableView.get_Nodes().get_Count() < 1)
			{
				return;
			}
			if (LastSortedColumn == iColumnIndex)
			{
				LastSortedAscending = !LastSortedAscending;
			}
			else
			{
				LastSortedAscending = false;
				if (LastSortedColumn >= 0)
				{
					_treeTableView.SetColumnHeaderSortOrderIcon(LastSortedColumn, SortOrder.None);
				}
			}
			_treeTableView.SetColumnHeaderSortOrderIcon(iColumnIndex, LastSortedAscending ? SortOrder.Ascending : SortOrder.Descending);
			try
			{
				_treeTableView.BeginUpdate();
				if (iColumnIndex == 0)
				{
					_treeTableView.get_Model().Sort(_treeTableView.get_Model().get_Sentinel(), true, (IComparer)new StringColumnComparer(iColumnIndex, LastSortedAscending));
				}
				else
				{
					_treeTableView.get_Model().Sort(_treeTableView.get_Model().get_Sentinel(), true, (IComparer)new SizeColumnComparer(iColumnIndex, LastSortedAscending));
				}
			}
			finally
			{
				_treeTableView.EndUpdate();
			}
			LastSortedColumn = iColumnIndex;
		}

		private void ResetColumnSortState()
		{
			LastSortedColumn = -1;
			LastSortedAscending = false;
		}

		private void _btnEnableEditing_Click(object sender, EventArgs e)
		{
			EnableEditing();
		}

		private void EnableEditing()
		{
			if (Utilities.MemoryReserveSupported)
			{
				_btnEnableEditing.Enabled = false;
				_btnEnableEditing.Text = Strings.BtnEnabled;
				_btn_MultiApply.Enabled = true;
				_txtNewMemReserve.Enabled = true;
				_viewContext.ChangingMemoryReserveSizeEnabled = true;
			}
		}

		private void DisableEditing()
		{
			_btnEnableEditing.Enabled = false;
			_btnEnableEditing.Text = Strings.BtnEnable;
			_btn_MultiApply.Enabled = false;
			_txtNewMemReserve.Enabled = false;
			_viewContext.ChangingMemoryReserveSizeEnabled = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
				APEnvironment.Engine.Projects.PrimaryProjectSwitched -= Projects_PrimaryProjectSwitched;
				APEnvironment.CompilerVersionMgr.CompilerVersionChanged -= CompilerVersionMgrOnCompilerVersionChanged;
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Expected O, but got Unknown
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager componentResourceManager = new System.ComponentModel.ComponentResourceManager(typeof(_3S.CoDeSys.LanguageModelManagerConfigurators.OnlineChangeAllocationPlusView));
			this._treeTableView = new TreeTableView();
			this._cbApplication = new System.Windows.Forms.ComboBox();
			this._btnScan = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this._lblFbNo = new System.Windows.Forms.Label();
			this._lstCategories = new System.Windows.Forms.ListBox();
			this.label3 = new System.Windows.Forms.Label();
			this._lblAdditionalMemSize = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this._btn_MultiApply = new System.Windows.Forms.Button();
			this._txtNewMemReserve = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this._lblBuildAppInfo = new System.Windows.Forms.Label();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this._btnEnableEditing = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.groupBox4.SuspendLayout();
			base.SuspendLayout();
			this._treeTableView.set_AllowColumnReorder(false);
			this._treeTableView.set_AutoRestoreSelection(false);
			((System.Windows.Forms.Control)(object)this._treeTableView).BackColor = System.Drawing.SystemColors.Window;
			this._treeTableView.set_BorderStyle(System.Windows.Forms.BorderStyle.Fixed3D);
			componentResourceManager.ApplyResources(this._treeTableView, "_treeTableView");
			this._treeTableView.set_DoNotShrinkColumnsAutomatically(false);
			this._treeTableView.set_ForceFocusOnClick(false);
			this._treeTableView.set_GridLines(false);
			this._treeTableView.set_HeaderStyle(System.Windows.Forms.ColumnHeaderStyle.Clickable);
			this._treeTableView.set_ImmediateEdit(true);
			this._treeTableView.set_Indent(20);
			this._treeTableView.set_KeepColumnWidthsAdjusted(true);
			this._treeTableView.set_Model((ITreeTableModel)null);
			this._treeTableView.set_MultiSelect(true);
			((System.Windows.Forms.Control)(object)this._treeTableView).Name = "_treeTableView";
			this._treeTableView.set_NoSearchStrings(false);
			this._treeTableView.set_OnlyWhenFocused(true);
			this._treeTableView.set_OpenEditOnDblClk(true);
			this._treeTableView.set_ReadOnly(false);
			this._treeTableView.set_Scrollable(true);
			this._treeTableView.set_ShowLines(true);
			this._treeTableView.set_ShowPlusMinus(false);
			this._treeTableView.set_ShowRootLines(true);
			this._treeTableView.set_ToggleOnDblClk(false);
			this._treeTableView.add_ColumnClick(new System.Windows.Forms.ColumnClickEventHandler(_treeTableView_ColumnClick));
			this._cbApplication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._cbApplication.FormattingEnabled = true;
			componentResourceManager.ApplyResources(this._cbApplication, "_cbApplication");
			this._cbApplication.Name = "_cbApplication";
			componentResourceManager.ApplyResources(this._btnScan, "_btnScan");
			this._btnScan.Name = "_btnScan";
			this._btnScan.UseVisualStyleBackColor = true;
			this._btnScan.Click += new System.EventHandler(_btnScan_Click);
			componentResourceManager.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			componentResourceManager.ApplyResources(this._lblFbNo, "_lblFbNo");
			this._lblFbNo.Name = "_lblFbNo";
			componentResourceManager.ApplyResources(this._lstCategories, "_lstCategories");
			this._lstCategories.FormattingEnabled = true;
			this._lstCategories.Name = "_lstCategories";
			this._lstCategories.SelectedIndexChanged += new System.EventHandler(_lstCategories_SelectedIndexChanged);
			componentResourceManager.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			componentResourceManager.ApplyResources(this._lblAdditionalMemSize, "_lblAdditionalMemSize");
			this._lblAdditionalMemSize.Name = "_lblAdditionalMemSize";
			componentResourceManager.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Controls.Add(this.tableLayoutPanel1);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			componentResourceManager.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
			this.tableLayoutPanel1.Controls.Add(this._lblAdditionalMemSize, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this._lblFbNo, 1, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			componentResourceManager.ApplyResources(this.groupBox2, "groupBox2");
			this.groupBox2.Controls.Add(this._btn_MultiApply);
			this.groupBox2.Controls.Add(this._txtNewMemReserve);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.TabStop = false;
			componentResourceManager.ApplyResources(this._btn_MultiApply, "_btn_MultiApply");
			this._btn_MultiApply.Name = "_btn_MultiApply";
			this._btn_MultiApply.UseVisualStyleBackColor = true;
			this._btn_MultiApply.Click += new System.EventHandler(_btn_MultiApply_Click);
			componentResourceManager.ApplyResources(this._txtNewMemReserve, "_txtNewMemReserve");
			this._txtNewMemReserve.Name = "_txtNewMemReserve";
			this._txtNewMemReserve.TextChanged += new System.EventHandler(_txtNewMemReserve_TextChanged);
			componentResourceManager.ApplyResources(this.label5, "label5");
			this.label5.Name = "label5";
			componentResourceManager.ApplyResources(this.groupBox3, "groupBox3");
			this.groupBox3.Controls.Add(this.splitContainer1);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.TabStop = false;
			componentResourceManager.ApplyResources(this.splitContainer1, "splitContainer1");
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Panel1.Controls.Add(this._lstCategories);
			this.splitContainer1.Panel2.Controls.Add((System.Windows.Forms.Control)(object)this._treeTableView);
			componentResourceManager.ApplyResources(this._lblBuildAppInfo, "_lblBuildAppInfo");
			this._lblBuildAppInfo.Name = "_lblBuildAppInfo";
			componentResourceManager.ApplyResources(this.groupBox4, "groupBox4");
			this.groupBox4.Controls.Add(this.label2);
			this.groupBox4.Controls.Add(this._btnEnableEditing);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.TabStop = false;
			componentResourceManager.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			componentResourceManager.ApplyResources(this._btnEnableEditing, "_btnEnableEditing");
			this._btnEnableEditing.Name = "_btnEnableEditing";
			this._btnEnableEditing.UseVisualStyleBackColor = true;
			this._btnEnableEditing.Click += new System.EventHandler(_btnEnableEditing_Click);
			componentResourceManager.ApplyResources(this, "$this");
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(this.groupBox4);
			base.Controls.Add(this._lblBuildAppInfo);
			base.Controls.Add(this.groupBox3);
			base.Controls.Add(this.groupBox2);
			base.Controls.Add(this.groupBox1);
			base.Controls.Add(this._btnScan);
			base.Controls.Add(this._cbApplication);
			base.Name = "OnlineChangeAllocationPlusView";
			this.groupBox1.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
