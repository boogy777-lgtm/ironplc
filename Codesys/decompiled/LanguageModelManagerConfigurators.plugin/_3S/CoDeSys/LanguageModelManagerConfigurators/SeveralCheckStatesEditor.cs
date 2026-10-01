using System.Drawing;
using System.Windows.Forms;
using _3S.CoDeSys.Controls.Controls;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	internal class SeveralCheckStatesEditor : ITreeTableViewEditor
	{
		public static readonly SeveralCheckStatesEditor Checkbox = new SeveralCheckStatesEditor();

		public object AcceptEdit(TreeTableViewNode node, Control control)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Invalid comparison between Unknown and I4
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Invalid comparison between Unknown and I4
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Invalid comparison between Unknown and I4
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Invalid comparison between Unknown and I4
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Invalid comparison between I4 and Unknown
			if (!(control?.Tag is SeveralCheckedStates val))
			{
				return null;
			}
			if ((int)val == 0)
			{
				return (object)(SeveralCheckedStates)6;
			}
			if ((int)val == 6)
			{
				return (object)(SeveralCheckedStates)5;
			}
			if ((int)val == 5)
			{
				return (object)(SeveralCheckedStates)0;
			}
			if ((int)val == 3 || (int)val == 4)
			{
				return (object)(SeveralCheckedStates)0;
			}
			if (2 == (int)val)
			{
				return (object)(SeveralCheckedStates)5;
			}
			return null;
		}

		public Control BeginEdit(TreeTableViewNode node, int nColumnIndex, char cImmediate, ref bool bEditComplete)
		{
			TextBox result = new TextBox
			{
				Bounds = Rectangle.Empty,
				Tag = node.get_CellValues()[nColumnIndex]
			};
			bEditComplete = true;
			return result;
		}

		public bool OneClickEdit(TreeTableViewNode node, int nColumnIndex)
		{
			return true;
		}
	}
}
