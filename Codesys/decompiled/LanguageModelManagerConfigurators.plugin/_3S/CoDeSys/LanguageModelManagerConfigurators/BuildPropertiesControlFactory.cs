using System;
using System.Drawing;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Views;
using _3S.CoDeSys.TaskConfig;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	[TypeGuid("{DEA2A43F-00FB-4c1c-AA64-C0FF8278357D}")]
	public class BuildPropertiesControlFactory : IPropertiesEditorViewFactory, IEditorViewFactory
	{
		public Icon LargeIcon => SmallIcon;

		public string Description => Strings.PropertiesControlFactory_Description;

		public string Name => Strings.PropertiesControlFactory_Name;

		public Icon SmallIcon => APEnvironment.Engine.ResourceManager.GetIcon(GetType(), "_3S.CoDeSys.LanguageModelManager.Resources.BuildSmall.ico");

		public IPropertiesEditorView Create(IMetaObjectStub mos)
		{
			bool flag = false;
			IPropertiesEditorView result = null;
			if (typeof(ILanguageModelBuildPropertiesControl).IsAssignableFrom(mos.ObjectType) || typeof(ILanguageModelProviderBuildPropertiesControl).IsAssignableFrom(mos.ObjectType))
			{
				IMetaObject objectToRead = APEnvironment.ObjectMgr.GetObjectToRead(mos.ProjectHandle, mos.ObjectGuid);
				ILanguageModelBuildPropertiesControl languageModelBuildPropertiesControl = objectToRead.Object as ILanguageModelBuildPropertiesControl;
				ILanguageModelProviderBuildPropertiesControl languageModelProviderBuildPropertiesControl = objectToRead.Object as ILanguageModelProviderBuildPropertiesControl;
				if (languageModelBuildPropertiesControl != null)
				{
					flag = languageModelBuildPropertiesControl.ShowPropertiesDialog;
				}
				else if (languageModelProviderBuildPropertiesControl != null)
				{
					flag = languageModelProviderBuildPropertiesControl.ShowPropertiesDialog;
				}
			}
			else if (typeof(ILanguageModelProvider).IsAssignableFrom(mos.ObjectType))
			{
				flag = true;
			}
			if (flag)
			{
				result = new BuildPropertiesControl(mos);
			}
			return result;
		}

		public bool AcceptsObjectType(Type objectType, Type[] embeddedObjectTypes)
		{
			if (objectType == null)
			{
				return false;
			}
			if (typeof(ILanguageModelProvider).IsAssignableFrom(objectType))
			{
				return true;
			}
			if (typeof(ITaskObject).IsAssignableFrom(objectType))
			{
				return true;
			}
			if (typeof(IFolderObject).IsAssignableFrom(objectType))
			{
				return true;
			}
			return false;
		}
	}
}
