using System;
using System.Drawing;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.ProjectInfoObject;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	public class APEnvironmentFacade : IAPEnvironmentFacade
	{
		private static readonly Guid GUID_PROJECTINFOOBJECT = new Guid("{11C0FC3A-9BCF-4dd8-AC38-EFB93363E521}");

		public ILMCompileOptions CompileOptions => APEnvironment.LMServiceProvider.ConfigurationService.CompileOptions;

		public Version[] AvailableCompilerVersionsOEMFilteredNotReplaced => APEnvironment.CompilerVersionMgr.AvailableCompilerVersionsOEMFilteredNotReplaced;

		public void ClearLanguageModel()
		{
			APEnvironment.CompilerVersionMgr.ClearLanguageModel();
		}

		public Version CompilerVersionToUse()
		{
			return APEnvironment.CompilerVersionMgr.CompilerVersionToUse();
		}

		public Version GetCustomizationVersion()
		{
			return APEnvironment.CompilerVersionMgr.GetCustomizationVersion();
		}

		public string MapFromInternalToOEMText(Version version)
		{
			return APEnvironment.CompilerVersionMgr.MapFromInternalToOEMText(version);
		}

		public string MapFromInternalToOEMTextSave(Version version)
		{
			return APEnvironment.CompilerVersionMgr.MapFromInternalToOEMTextSave(version);
		}

		public Version MapFromOEMTextToInternal(string version)
		{
			return APEnvironment.CompilerVersionMgr.MapFromOEMTextToInternal(version);
		}

		public Version MapFromOEMTextToInternalSave(string version)
		{
			return APEnvironment.CompilerVersionMgr.MapFromOEMTextToInternalSave(version);
		}

		public void SetCompilerVersionExact(Version version)
		{
			APEnvironment.CompilerVersionMgr.SetCompilerVersionExact(version);
		}

		public Icon GetIcon(Type type, string location)
		{
			return APEnvironment.Engine.ResourceManager.GetIcon(type, location);
		}

		public bool IsLibraryWithPinnedStorageVersion()
		{
			if (APEnvironment.Engine.Projects.PrimaryProject == null)
			{
				return false;
			}
			if (APEnvironment.Engine.Projects.PrimaryProject.Path == null)
			{
				return false;
			}
			if (!APEnvironment.Engine.Projects.PrimaryProject.Path.ToLowerInvariant().EndsWith(".library"))
			{
				return false;
			}
			IProjectInfoObject projectInfoObject = GetProjectInfoObject(APEnvironment.Engine.Projects.PrimaryProject);
			if (projectInfoObject == null)
			{
				return false;
			}
			object value = projectInfoObject.GetValue("IsStorageFormatPinned");
			if (value == null)
			{
				return false;
			}
			return (bool)value;
		}

		private static IProjectInfoObject GetProjectInfoObject(IProject project)
		{
			if (!APEnvironment.ObjectMgr.ExistsObject(project.Handle, GUID_PROJECTINFOOBJECT))
			{
				return null;
			}
			IMetaObject objectToRead = APEnvironment.ObjectMgr.GetObjectToRead(project.Handle, GUID_PROJECTINFOOBJECT);
			if (objectToRead == null)
			{
				return null;
			}
			if (objectToRead.Object is IProjectInfoObject result)
			{
				return result;
			}
			return null;
		}
	}
}
