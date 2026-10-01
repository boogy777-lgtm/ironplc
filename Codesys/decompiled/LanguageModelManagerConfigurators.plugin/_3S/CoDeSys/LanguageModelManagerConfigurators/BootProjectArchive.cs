using System;
using System.Collections.Generic;
using System.IO;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.ProjectArchive;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	[TypeGuid("{BC06F52F-4558-4bab-8387-553A311C9CE9}")]
	internal class BootProjectArchive : IProjectArchiveCategory
	{
		public string Name => Strings.ProjectArchiveBootProjectCategory_Name;

		public string Description => Strings.ProjectArchiveBootProjectCategory_Description;

		public bool SelectByDefault => false;

		public IEnumerable<string> GetArchiveItemIds(int nProjectHandle)
		{
			if (APEnvironment.Engine.Projects.PrimaryProject == null || APEnvironment.Engine.Projects.PrimaryProject.Handle != nProjectHandle)
			{
				return new string[0];
			}
			string directoryName = Path.GetDirectoryName(APEnvironment.Engine.Projects.PrimaryProject.Path);
			IEnumerable<ILMPreCompileSet> enumerable = APEnvironment.LMServiceProvider.PreCompileService.AllPreCompileSets(bWithDevices: true, bWithLibraries: false);
			LList<string> val = new LList<string>();
			foreach (ILMPreCompileSet item in enumerable)
			{
				if (item.ApplicationGuid != Guid.Empty)
				{
					string applicationNameByGuid = APEnvironment.LMServiceProvider.LanguageModelProviderService.GetApplicationNameByGuid(item.ApplicationGuid, EQueryApplicationNameFlags.Unqualified);
					string pathByArchiveItem = GetPathByArchiveItem(directoryName, applicationNameByGuid);
					if (SideCarEntryHelper.ExistsFromPath(nProjectHandle, pathByArchiveItem))
					{
						val.Add(applicationNameByGuid);
					}
				}
			}
			return (IEnumerable<string>)val;
		}

		public string GetArchiveItemDisplayName(string stArchiveItemId)
		{
			return stArchiveItemId;
		}

		public string GetPathByArchiveItem(string stProjectFilePath, string stArchiveItemId)
		{
			string path = stArchiveItemId + ".app";
			return Path.Combine(stProjectFilePath, path);
		}

		public void SaveArchiveItem(int nProjectHandle, string stArchiveItemId, Stream stream)
		{
			string directoryName = Path.GetDirectoryName(APEnvironment.Engine.Projects.PrimaryProject.Path);
			string pathByArchiveItem = GetPathByArchiveItem(directoryName, stArchiveItemId);
			SideCarEntryHelper.ReadFromPath(nProjectHandle, pathByArchiveItem, stream);
		}

		public bool ExtractArchiveItem(string stProjectFilePath, string stArchiveItemId, Stream stream, IExtractProjectArchiveNotifyHandler notifyHandler)
		{
			return CompileInfoProjectArchive.FileWriteHelper(GetPathByArchiveItem(Path.GetDirectoryName(stProjectFilePath), stArchiveItemId), stream, notifyHandler);
		}

		public void FinishArchiveItems(string stProjectFilePath, string[] stArchiveItemIds)
		{
		}
	}
}
