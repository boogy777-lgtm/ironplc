using System.Drawing;
using System.Windows.Forms;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Options;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	[TypeGuid("{4C75F723-0995-466F-AA45-A1C4E9E0DB70}")]
	public class LibraryDevelopmentOptionsEditor : IOptionEditor
	{
		public OptionRoot OptionRoot => OptionRoot.Project;

		public string Name => Strings.LibraryDevelopmentOptions_Name;

		public string Description => Strings.LibraryDevelopmentOptions_Description;

		public Icon SmallIcon => APEnvironment.Engine.ResourceManager.GetIcon(GetType(), "_3S.CoDeSys.LanguageModelManagerConfigurators.Resources.LibrarySmall.ico");

		public Icon LargeIcon => SmallIcon;

		public Control CreateControl()
		{
			return new LibraryDevelopmentProperties();
		}

		public bool Save(Control control, ref string stMessage, ref Control failedControl)
		{
			return ((LibraryDevelopmentProperties)control).Validate(ref stMessage, ref failedControl);
		}
	}
}
