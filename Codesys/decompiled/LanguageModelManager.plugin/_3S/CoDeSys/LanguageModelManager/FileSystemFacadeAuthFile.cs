using System;
using System.IO;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000018 RID: 24
	public class FileSystemFacadeAuthFile : IFileSystemFacade
	{
		// Token: 0x0600001E RID: 30 RVA: 0x00002343 File Offset: 0x00001343
		public bool Exists(string path)
		{
			return AuthFile.Exists(path);
		}

		// Token: 0x0600001F RID: 31 RVA: 0x0000234B File Offset: 0x0000134B
		public void Delete(string path)
		{
			AuthFile.Delete(path);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002353 File Offset: 0x00001353
		public void DeleteIfExists(string path)
		{
			if (AuthFile.Exists(path))
			{
				AuthFile.Delete(path);
			}
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002364 File Offset: 0x00001364
		public void ClearPrimaryProjectDirectory()
		{
			try
			{
				string path = APEnvironment.Engine.Projects.PrimaryProject.Path;
				string directoryName = Path.GetDirectoryName(path);
				if (Directory.Exists(directoryName))
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
					string searchPattern = fileNameWithoutExtension + ".*.compileinfo";
					string[] files = Directory.GetFiles(directoryName, searchPattern);
					for (int i = 0; i < files.Length; i++)
					{
						AuthFile.Delete(files[i]);
					}
					string searchPattern2 = fileNameWithoutExtension + ".*.bootinfo";
					string searchPattern3 = fileNameWithoutExtension + ".*.bootinfo_guids";
					files = Directory.GetFiles(directoryName, searchPattern2);
					for (int i = 0; i < files.Length; i++)
					{
						AuthFile.Delete(files[i]);
					}
					files = Directory.GetFiles(directoryName, searchPattern3);
					for (int i = 0; i < files.Length; i++)
					{
						AuthFile.Delete(files[i]);
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002450 File Offset: 0x00001450
		public void Copy(string sourceFileName, string destFileName, bool overwrite)
		{
			AuthFile.Copy(sourceFileName, destFileName, overwrite);
		}

		// Token: 0x06000023 RID: 35 RVA: 0x0000245A File Offset: 0x0000145A
		public Stream OpenStream(string path, FileMode mode, FileAccess access)
		{
			return new AuthFileStream(path, mode, access);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00002464 File Offset: 0x00001464
		public Stream OpenStream(string path, FileMode mode)
		{
			return new AuthFileStream(path, mode);
		}

		// Token: 0x06000025 RID: 37 RVA: 0x0000246D File Offset: 0x0000146D
		public void AppendAllText(string path, string contents)
		{
			AuthFile.AppendAllText(path, contents);
		}
	}
}
