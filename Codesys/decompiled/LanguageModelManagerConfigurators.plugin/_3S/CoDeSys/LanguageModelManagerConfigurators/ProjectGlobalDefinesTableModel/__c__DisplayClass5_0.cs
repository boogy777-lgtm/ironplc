using System.Windows.Forms;
using _3S.CoDeSys.Controls.Controls;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	public sealed class ProjectGlobalDefinesTableModel : AbstractTreeTableModel
	{
		internal delegate bool GenericNodeAction(ProjectGlobalDefinesTreeTableNode node);

		internal ProjectGlobalDefinesTableModel()
		{
			((AbstractTreeTableModel)this).get_UnderlyingModel().AddColumn(Strings.ProjectGLobalDefinesDialogNameCol, HorizontalAlignment.Left, IconLabelTreeTableViewRenderer.get_NormalString(), IconTextBoxTreeTableViewEditor.get_TextBox(), true);
		}

		internal void AddNode(ITreeTableNode node)
		{
			((AbstractTreeTableModel)this).get_UnderlyingModel().AddRootNode(node);
		}

		internal void RemoveNode(int index)
		{
			ITreeTableNode child = ((AbstractTreeTableModel)this).get_Sentinel().GetChild(index);
			((AbstractTreeTableModel)this).get_UnderlyingModel().RemoveRootNode(child);
		}

		internal void GenericChildNodeTraversal(GenericNodeAction childNodeAction)
		{
			int childCount = ((AbstractTreeTableModel)this).get_Sentinel().get_ChildCount();
			for (int i = 0; i < childCount && (!(((AbstractTreeTableModel)this).get_Sentinel().GetChild(i) is ProjectGlobalDefinesTreeTableNode node) || childNodeAction(node)); i++)
			{
			}
		}

		internal bool IsDefineUnique(string stDefineToCheck, out int iOccurrenceCount)
		{
			int iCount = 0;
			bool bUnique = true;
			GenericChildNodeTraversal(delegate(ProjectGlobalDefinesTreeTableNode node)
			{
				if (string.Equals(node.DefineName, stDefineToCheck))
				{
					iCount++;
				}
				bUnique = 2 > iCount;
				return bUnique;
			});
			iOccurrenceCount = iCount;
			return bUnique;
		}

		internal void SetDefineUnique(string stDefineToCheck, bool bUnique)
		{
			GenericChildNodeTraversal(delegate(ProjectGlobalDefinesTreeTableNode node)
			{
				if (node.DefineName == stDefineToCheck)
				{
					node.NameIsUnique = bUnique;
				}
				return true;
			});
		}

		internal void UpdateAllNodes()
		{
			GenericChildNodeTraversal(delegate(ProjectGlobalDefinesTreeTableNode node)
			{
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				((AbstractTreeTableModel)this).RaiseChanged(new TreeTableModelEventArgs(((AbstractTreeTableModel)this).get_Sentinel(), ((AbstractTreeTableModel)this).get_Sentinel().GetIndex((ITreeTableNode)(object)node), (ITreeTableNode)(object)node));
				return true;
			});
		}
	}
}
