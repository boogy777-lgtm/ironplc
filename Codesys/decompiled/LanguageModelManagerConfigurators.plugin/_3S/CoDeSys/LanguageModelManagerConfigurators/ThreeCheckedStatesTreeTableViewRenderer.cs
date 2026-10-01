using System;
using System.Drawing;
using _3S.CoDeSys.Controls.Controls;
using _3S.CoDeSys.UtilitiesContrib;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	internal class ThreeCheckedStatesTreeTableViewRenderer : SmallImageBasedTreeTableViewRenderer
	{
		private static readonly Image s_checkedImage;

		private static readonly Image s_checkedRedImage;

		private static readonly Image s_checkedYellowImage;

		private static readonly Image s_checkedGrayedImage;

		private static readonly Image s_checkedLightGrayedImage;

		private static readonly Image s_uncheckedImage;

		private static ITreeTableViewRenderer s_checkBox;

		internal static ITreeTableViewRenderer CheckBox => s_checkBox;

		static ThreeCheckedStatesTreeTableViewRenderer()
		{
			s_checkedImage = LoadBitmapFromResource("Checked");
			s_checkedRedImage = LoadBitmapFromResource("CheckedRed");
			s_checkedYellowImage = LoadBitmapFromResource("CheckedYellow");
			s_checkedGrayedImage = LoadBitmapFromResource("CheckedGrayed");
			s_checkedLightGrayedImage = LoadBitmapFromResource("CheckedLightGrayed");
			s_uncheckedImage = LoadBitmapFromResource("Unchecked");
			s_checkBox = (ITreeTableViewRenderer)(object)new ThreeCheckedStatesTreeTableViewRenderer();
		}

		private static Image LoadBitmapFromResource(string stResourceName)
		{
			return ResourceHelper.LoadBitmap(typeof(ThreeCheckedStatesTreeTableViewRenderer), "_3S.CoDeSys.LanguageModelManagerConfigurators.Resources." + stResourceName + ".bmp", Point.Empty);
		}

		public override int GetPreferredWidth(TreeTableViewNode node, int nColumnIndex, Graphics g)
		{
			return s_checkedImage.Width;
		}

		protected override Image GetImageByValue(object value)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected I4, but got Unknown
			SeveralCheckedStates val = (SeveralCheckedStates)value;
			switch ((int)val)
			{
			case 0:
				return s_uncheckedImage;
			case 3:
				return s_checkedGrayedImage;
			case 4:
				return s_checkedLightGrayedImage;
			case 5:
				return s_checkedRedImage;
			case 2:
			case 6:
				return s_checkedYellowImage;
			default:
				throw new InvalidOperationException("Unsupported Checked State");
			}
		}

		protected override bool IsValidValue(object value)
		{
			return value is SeveralCheckedStates;
		}
	}
}
