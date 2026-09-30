using System.Drawing;
using System.Windows.Forms;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Options;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	[TypeGuid("{4C4DA941-9348-429c-BBE8-F61C588F7337}")]
	public class CompileOptionsEditor : IOptionEditor
	{
		public OptionRoot OptionRoot => OptionRoot.Project;

		public string Name => Strings.CompileOptions_Name;

		public string Description => Strings.CompileOptions_Description;

		public Icon SmallIcon => APEnvironment.Engine.ResourceManager.GetIcon(GetType(), "_3S.CoDeSys.LanguageModelManagerConfigurators.Resources.BuildSmall.ico");

		public Icon LargeIcon => SmallIcon;

		public Control CreateControl()
		{
			return new CompileProperties();
		}

		public bool Save(Control control, ref string stMessage, ref Control failedControl)
		{
			ValidationResult validationResult = ((CompileProperties)control).ValidateAndSave();
			if (!validationResult.IsValid)
			{
				stMessage = validationResult.OptionalErrorMessage;
				failedControl = ((CompileProperties)control).GetControlFromPosition(validationResult.FailedControl);
			}
			return validationResult.IsValid;
		}
	}
}
