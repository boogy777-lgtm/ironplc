using System;
using System.IO;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000019 RID: 25
	public interface IFileSystemFacade
	{
		// Token: 0x06000027 RID: 39
		bool Exists(string path);

		// Token: 0x06000028 RID: 40
		void Delete(string path);

		// Token: 0x06000029 RID: 41
		void DeleteIfExists(string path);

		// Token: 0x0600002A RID: 42
		void Copy(string sourceFileName, string destFileName, bool overwrite);

		// Token: 0x0600002B RID: 43
		Stream OpenStream(string path, FileMode mode, FileAccess access);

		// Token: 0x0600002C RID: 44
		Stream OpenStream(string path, FileMode mode);

		// Token: 0x0600002D RID: 45
		void AppendAllText(string path, string contents);

		// Token: 0x0600002E RID: 46
		void ClearPrimaryProjectDirectory();
	}
}
