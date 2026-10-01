using System.Collections.Generic;
using System.Windows.Forms;
using _3S.CoDeSys.Controls.Controls;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	internal class WarningModel : AbstractTreeTableModel
	{
		private TreeTableView _treeTableView;

		internal WarningModel(TreeTableView treeTableView)
		{
			_treeTableView = treeTableView;
			((AbstractTreeTableModel)this).get_UnderlyingModel().AddColumn(Strings.Warnings, HorizontalAlignment.Left, LabelTreeTableViewRenderer.get_NormalString(), TextBoxTreeTableViewEditor.get_TextBox(), false);
			((AbstractTreeTableModel)this).get_UnderlyingModel().AddColumn("", HorizontalAlignment.Left, ThreeCheckedStatesTreeTableViewRenderer.CheckBox, (ITreeTableViewEditor)(object)SeveralCheckStatesEditor.Checkbox, true);
		}

		internal void Refill(IEnumerable<Warning> alWarnings)
		{
			TreeTableView treeTableView = _treeTableView;
			if (treeTableView != null)
			{
				treeTableView.BeginUpdate();
			}
			try
			{
				((AbstractTreeTableModel)this).get_UnderlyingModel().ClearRootNodes();
				WarningNode warningNode = new WarningNode(this, new Warning(0, bActive: true, bAsError: false), null);
				warningNode.Refill(alWarnings);
				((AbstractTreeTableModel)this).get_UnderlyingModel().AddRootNode((ITreeTableNode)(object)warningNode);
				TreeTableView treeTableView2 = _treeTableView;
				TreeTableViewNode val = ((treeTableView2 != null) ? treeTableView2.GetViewNode((ITreeTableNode)(object)warningNode) : null);
				if (val != null)
				{
					val.Expand();
					val.set_Selected(true);
					val.Focus(1);
				}
				UpdateState();
				if (_treeTableView != null)
				{
					_treeTableView.ExpandAll();
					int num = ((Control)(object)_treeTableView).Width - 25;
					_treeTableView.get_Columns()[0].Width = num / 5 * 4;
					_treeTableView.get_Columns()[1].Width = num / 5;
				}
			}
			finally
			{
				TreeTableView treeTableView3 = _treeTableView;
				if (treeTableView3 != null)
				{
					treeTableView3.EndUpdate();
				}
			}
		}

		internal void UpdateState()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Expected O, but got Unknown
			for (int i = 0; i < ((AbstractTreeTableModel)this).get_Sentinel().get_ChildCount(); i++)
			{
				WarningNode warningNode = ((AbstractTreeTableModel)this).get_Sentinel().GetChild(i) as WarningNode;
				UpdateStateRecursive(warningNode);
				((AbstractTreeTableModel)this).RaiseChanged(new TreeTableModelEventArgs((ITreeTableNode)null, ((AbstractTreeTableModel)this).get_Sentinel().GetIndex((ITreeTableNode)(object)warningNode), (ITreeTableNode)(object)warningNode));
			}
		}

		internal IEnumerable<WarningNode> GetRootNodes()
		{
			for (int i = 0; i < <>n__0().get_ChildCount(); i++)
			{
				if (<>n__0().GetChild(i) is WarningNode warningNode)
				{
					yield return warningNode;
				}
			}
		}

		internal SeveralCheckedStates UpdateStateRecursive(WarningNode warning)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Invalid comparison between Unknown and I4
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Invalid comparison between Unknown and I4
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Expected O, but got Unknown
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			if (warning.ChildCount == 0)
			{
				return warning.State;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (WarningNode child in warning.ChildList)
			{
				SeveralCheckedStates val = UpdateStateRecursive(child);
				if ((int)val != 0)
				{
					if ((int)val != 5)
					{
						if ((int)val == 6)
						{
							num2++;
						}
					}
					else
					{
						num++;
					}
				}
				else
				{
					num3++;
				}
			}
			if (num == warning.ChildCount)
			{
				warning.State = (SeveralCheckedStates)5;
			}
			else if (num2 == warning.ChildCount)
			{
				warning.State = (SeveralCheckedStates)6;
			}
			else if (num3 == warning.ChildCount)
			{
				warning.State = (SeveralCheckedStates)0;
			}
			else if (num3 == 0)
			{
				warning.State = (SeveralCheckedStates)4;
			}
			else
			{
				warning.State = (SeveralCheckedStates)3;
			}
			((AbstractTreeTableModel)this).RaiseChanged(new TreeTableModelEventArgs((ITreeTableNode)null, -1, (ITreeTableNode)(object)warning));
			return warning.State;
		}

		internal void Save(HashSet<int> hsDisabledWarningIds, HashSet<int> hsWarningsAsErrorsIds)
		{
			for (int i = 0; i < ((AbstractTreeTableModel)this).get_Sentinel().get_ChildCount(); i++)
			{
				(((AbstractTreeTableModel)this).get_Sentinel().GetChild(i) as WarningNode).Save(hsDisabledWarningIds, hsWarningsAsErrorsIds);
			}
		}
	}
}
