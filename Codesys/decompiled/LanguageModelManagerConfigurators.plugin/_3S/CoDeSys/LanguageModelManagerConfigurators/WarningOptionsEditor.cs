using System.Drawing;
using System.Windows.Forms;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Options;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	[TypeGuid("{E0D85205-464B-405D-BE4E-8E4256B7716B}")]
	public class WarningOptionsEditor : IOptionEditor
	{
		public OptionRoot OptionRoot => OptionRoot.Project;

		public string Name => Strings.WarningOptions_Name;

		public string Description => Strings.WarningOptions_Description;

		public Icon SmallIcon => APEnvironment.Engine.ResourceManager.GetIcon(GetType(), "_3S.CoDeSys.LanguageModelManagerConfigurators.Resources.Warning.ico");

		public Icon LargeIcon => SmallIcon;

		public Control CreateControl()
		{
			return new WarningOptions();
		}

		public bool Save(Control control, ref string stMessage, ref Control failedControl)
		{
			return ((WarningOptions)control).Save();
		}
	}
}
