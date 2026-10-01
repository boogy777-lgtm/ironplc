using System.IO;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	internal static class SideCarEntryHelper
	{
		public static bool ExistsFromPath(int projectHandle, string path)
		{
			return APEnvironment.ProjectSideCarService.ExistsEntry(projectHandle, Path.GetFileName(path));
		}

		public static void ReadFromPath(int projectHandle, string path, Stream stream)
		{
			using (Stream stream2 = APEnvironment.ProjectSideCarService.OpenEntryForRead(projectHandle, Path.GetFileName(path)))
			{
				stream2.CopyTo(stream);
			}
		}
	}
}
