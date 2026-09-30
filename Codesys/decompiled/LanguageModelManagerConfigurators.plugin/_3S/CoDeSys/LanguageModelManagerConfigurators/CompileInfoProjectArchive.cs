using System;
using System.Collections.Generic;
using System.IO;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.ProjectArchive;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	[TypeGuid("{B0B53F83-AF78-49aa-8133-0063F476BD7C}")]
	internal class CompileInfoProjectArchive : IProjectArchiveCategory
	{
		public string Name => Strings.ProjectArchiveCompileCategory_Name;

		public string Description => Strings.ProjectArchiveCompileCategory_Description;

		public bool SelectByDefault => true;

		public IEnumerable<string> GetArchiveItemIds(int nProjectHandle)
		{
			if (APEnvironment.Engine.Projects.PrimaryProject == null || APEnvironment.Engine.Projects.PrimaryProject.Handle != nProjectHandle)
			{
				return new string[0];
			}
			IEnumerable<ILMPreCompileSet> enumerable = APEnvironment.LMServiceProvider.PreCompileService.AllPreCompileSets(bWithDevices: true, bWithLibraries: false);
			LList<string> val = new LList<string>();
			foreach (ILMPreCompileSet item in enumerable)
			{
				if (!(item.ApplicationGuid != Guid.Empty))
				{
					continue;
				}
				string downloadInfoFileName = APEnvironment.LMServiceProvider.CommandService.GetDownloadInfoFileName(item.ApplicationGuid, EQueryDownloadInfoFileNameFlags.Default);
				string applicationNameByGuid = APEnvironment.LMServiceProvider.LanguageModelProviderService.GetApplicationNameByGuid(item.ApplicationGuid);
				if (SideCarEntryHelper.ExistsFromPath(nProjectHandle, downloadInfoFileName))
				{
					string text = item.ApplicationGuid.ToString();
					if (!string.IsNullOrEmpty(text))
					{
						string text2 = text + "   " + applicationNameByGuid + "   " + Path.GetFileName(downloadInfoFileName);
						val.Add(text2);
					}
				}
			}
			return (IEnumerable<string>)val;
		}

		private static string GetFileNameFromItemId(string stArchiveItemId)
		{
			string[] array = stArchiveItemId.Split(new string[1] { "   " }, StringSplitOptions.None);
			if (array.Length != 3)
			{
				return string.Empty;
			}
			return array[2];
		}

		private static string GetApplicationNameFromItemId(string stArchiveItemId)
		{
			string[] array = stArchiveItemId.Split(new string[1] { "   " }, StringSplitOptions.None);
			if (array.Length != 3)
			{
				return string.Empty;
			}
			return array[1];
		}

		private static Guid GetApplicationGuidFromItemId(string stArchiveItemId)
		{
			string[] array = stArchiveItemId.Split(new string[1] { "   " }, StringSplitOptions.None);
			if (array.Length != 3)
			{
				return Guid.Empty;
			}
			return new Guid(array[0]);
		}

		public string GetArchiveItemDisplayName(string stArchiveItemId)
		{
			return GetApplicationNameFromItemId(stArchiveItemId);
		}

		public void SaveArchiveItem(int nProjectHandle, string stArchiveItemId, Stream stream)
		{
			string path = APEnvironment.Engine.Projects.GetProjectByHandle(nProjectHandle).Path;
			string path2 = Path.Combine(path2: GetFileNameFromItemId(stArchiveItemId), path1: Path.GetDirectoryName(path));
			WaitForDownloadInfoToBeWritten(stArchiveItemId);
			SideCarEntryHelper.ReadFromPath(nProjectHandle, path2, stream);
		}

		private static void WaitForDownloadInfoToBeWritten(string stArchiveItemId)
		{
			Guid applicationGuidFromItemId = GetApplicationGuidFromItemId(stArchiveItemId);
			if (APEnvironment.LMServiceProvider.CommandService is ILMCommandService3 iLMCommandService && iLMCommandService.IsAsyncUpdateDownloadInfoInProgress(applicationGuidFromItemId))
			{
				iLMCommandService.WaitForAsyncUpdateDownloadInfoCompleted(applicationGuidFromItemId);
			}
		}

		public bool ExtractArchiveItem(string stProjectFilePath, string stArchiveItemId, Stream stream, IExtractProjectArchiveNotifyHandler notifyHandler)
		{
			string fileNameFromItemId = GetFileNameFromItemId(stArchiveItemId);
			return FileWriteHelper(Path.Combine(Path.GetDirectoryName(stProjectFilePath), fileNameFromItemId), stream, notifyHandler);
		}

		public void FinishArchiveItems(string stProjectFilePath, string[] stArchiveItemIds)
		{
		}

		public static bool FileWriteHelper(string stFile, Stream stream, IExtractProjectArchiveNotifyHandler notifyHandler)
		{
			AuthFileStream val = null;
			try
			{
				PromptOverwriteResult promptOverwriteResult = PromptOverwriteResult.Yes;
				if (AuthFile.Exists(stFile) && notifyHandler != null)
				{
					promptOverwriteResult = notifyHandler.PromptOverwrite(stFile, null);
				}
				switch (promptOverwriteResult)
				{
				case PromptOverwriteResult.Yes:
				{
					val = AuthFile.Create(stFile);
					byte[] array = new byte[65536];
					int count;
					while ((count = stream.Read(array, 0, array.Length)) > 0)
					{
						((Stream)(object)val).Write(array, 0, count);
					}
					((Stream)(object)val).Close();
					val = null;
					break;
				}
				case PromptOverwriteResult.No:
					return true;
				case PromptOverwriteResult.Cancel:
					return false;
				}
			}
			finally
			{
				((Stream)(object)val)?.Close();
			}
			return true;
		}
	}
}
