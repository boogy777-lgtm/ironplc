using System;
using System.IO;
using System.Linq;
using CODESYS.ProjectFormat.SideCar;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000AB RID: 171
	internal static class SideCarEntryHelper
	{
		// Token: 0x1700026F RID: 623
		// (get) Token: 0x06000A43 RID: 2627 RVA: 0x0001744C File Offset: 0x0001644C
		private static IProjectSideCarService Service
		{
			get
			{
				return APEnvironmentFacade.Instance.ProjectSideCarService;
			}
		}

		// Token: 0x06000A44 RID: 2628 RVA: 0x00017458 File Offset: 0x00016458
		public static bool ExistsFromPath(string path)
		{
			return APEnvironmentFacade.Instance.ExistsPrimaryProject && SideCarEntryHelper.Service.ExistsEntry(APEnvironmentFacade.Instance.PrimaryProjectHandle, Path.GetFileName(path));
		}

		// Token: 0x06000A45 RID: 2629 RVA: 0x00017482 File Offset: 0x00016482
		public static Stream OpenReadFromPath(string path)
		{
			return SideCarEntryHelper.Service.OpenEntryForRead(APEnvironmentFacade.Instance.PrimaryProjectHandle, Path.GetFileName(path));
		}

		// Token: 0x06000A46 RID: 2630 RVA: 0x0001749E File Offset: 0x0001649E
		public static ISideCarEntry GetWrapperFromPath(string path)
		{
			return SideCarEntryHelper.Service.GetEntry(APEnvironmentFacade.Instance.PrimaryProjectHandle, Path.GetFileName(path));
		}

		// Token: 0x06000A47 RID: 2631 RVA: 0x000174BC File Offset: 0x000164BC
		public static void CopyFromPath(string sourcePath, string destPath, bool overwrite)
		{
			int primaryProjectHandle = APEnvironmentFacade.Instance.PrimaryProjectHandle;
			string fileName = Path.GetFileName(destPath);
			if (!overwrite && SideCarEntryHelper.Service.ExistsEntry(primaryProjectHandle, fileName))
			{
				throw new UnauthorizedAccessException();
			}
			string fileName2 = Path.GetFileName(sourcePath);
			SideCarEntryHelper.Service.CopyEntry(primaryProjectHandle, fileName2, fileName, overwrite);
		}

		// Token: 0x06000A48 RID: 2632 RVA: 0x00017507 File Offset: 0x00016507
		public static void DeleteFromPath(string path)
		{
			if (APEnvironmentFacade.Instance.ExistsPrimaryProject)
			{
				SideCarEntryHelper.Service.DeleteEntry(APEnvironmentFacade.Instance.PrimaryProjectHandle, Path.GetFileName(path));
			}
		}

		// Token: 0x06000A49 RID: 2633 RVA: 0x00017530 File Offset: 0x00016530
		public static void ClearAll()
		{
			try
			{
				string primaryProjectPath = APEnvironmentFacade.Instance.PrimaryProjectPath;
				int primaryProjectHandle = APEnvironmentFacade.Instance.PrimaryProjectHandle;
				string[] allEntries = SideCarEntryHelper.Service.GetEntries(primaryProjectHandle).ToArray<string>();
				string startsWith = Path.GetFileNameWithoutExtension(primaryProjectPath) + ".";
				string endsWith = ".compileinfo";
				SideCarEntryHelper.RemoveAllMatchingEntries(primaryProjectHandle, allEntries, startsWith, endsWith);
				string endsWith2 = ".bootinfo";
				SideCarEntryHelper.RemoveAllMatchingEntries(primaryProjectHandle, allEntries, startsWith, endsWith2);
				string endsWith3 = ".bootinfo_guids";
				SideCarEntryHelper.RemoveAllMatchingEntries(primaryProjectHandle, allEntries, startsWith, endsWith3);
			}
			catch
			{
			}
		}

		// Token: 0x06000A4A RID: 2634 RVA: 0x000175BC File Offset: 0x000165BC
		private static void RemoveAllMatchingEntries(int projectHandle, string[] allEntries, string startsWith, string endsWith)
		{
			foreach (string text in allEntries)
			{
				if (text.StartsWith(startsWith, StringComparison.InvariantCultureIgnoreCase) && text.EndsWith(endsWith, StringComparison.InvariantCultureIgnoreCase))
				{
					SideCarEntryHelper.Service.DeleteEntry(projectHandle, text);
				}
			}
		}
	}
}
