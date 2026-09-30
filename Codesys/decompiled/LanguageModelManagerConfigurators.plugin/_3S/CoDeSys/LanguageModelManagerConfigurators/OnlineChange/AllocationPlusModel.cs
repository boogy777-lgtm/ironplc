using System;
using System.Windows.Forms;
using _3S.CoDeSys.Controls.Controls;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators.OnlineChange
{
	internal class AllocationPlusModel : AbstractTreeTableModel, IDisposable
	{
		public int NodeCount => ((AbstractTreeTableModel)this).get_UnderlyingModel().get_Sentinel().get_ChildCount();

		internal AllocationPlusModel()
		{
			((AbstractTreeTableModel)this).get_UnderlyingModel().AddColumn(Strings.OnlChgColFBName, HorizontalAlignment.Left, LabelTreeTableViewRenderer.get_NormalString(), IconTextBoxTreeTableViewEditor.get_TextBox(), false);
			((AbstractTreeTableModel)this).get_UnderlyingModel().AddColumn(Strings.OnlChgColFBSize, HorizontalAlignment.Right, LabelTreeTableViewRenderer.get_NormalString(), IconTextBoxTreeTableViewEditor.get_TextBox(), false);
			((AbstractTreeTableModel)this).get_UnderlyingModel().AddColumn(Strings.OnlChgColInstCount, HorizontalAlignment.Right, LabelTreeTableViewRenderer.get_NormalString(), IconTextBoxTreeTableViewEditor.get_TextBox(), false);
			((AbstractTreeTableModel)this).get_UnderlyingModel().AddColumn(Strings.OnlChgColMemoryReserve, HorizontalAlignment.Right, LabelTreeTableViewRenderer.get_NormalString(), TextBoxTreeTableViewEditor.get_TextBox(), true);
			((AbstractTreeTableModel)this).get_UnderlyingModel().AddColumn(Strings.OnlChgColAdditionalMemUsed, HorizontalAlignment.Right, LabelTreeTableViewRenderer.get_NormalString(), TextBoxTreeTableViewEditor.get_TextBox(), false);
			((AbstractTreeTableModel)this).get_UnderlyingModel().AddColumn(Strings.OnlChgColRemainingMemReserve, HorizontalAlignment.Right, LabelTreeTableViewRenderer.get_NormalString(), TextBoxTreeTableViewEditor.get_TextBox(), false);
		}

		public void AddNode(ModelNode node)
		{
			((AbstractTreeTableModel)this).get_UnderlyingModel().AddRootNode((ITreeTableNode)(object)node);
		}

		public void Clear()
		{
			((AbstractTreeTableModel)this).get_UnderlyingModel().ClearRootNodes();
		}

		public void Dispose()
		{
		}
	}
}
