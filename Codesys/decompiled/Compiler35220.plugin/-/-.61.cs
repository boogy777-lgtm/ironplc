using System;
using System.IO;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001C
{
	// Token: 0x020000D5 RID: 213
	internal static class \u0004
	{
		// Token: 0x06000EEB RID: 3819 RVA: 0x00029448 File Offset: 0x00027648
		public static _IStatement \u0001(_ICompiledPOU \u0002)
		{
			_IStatement result = null;
			if (\u0002.ObjectGuid == Guid.Empty)
			{
				return null;
			}
			if (string.IsNullOrEmpty(\u0002.LibraryPath))
			{
				return null;
			}
			int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0002.LibraryPath);
			if (projectHandle == -1)
			{
				return null;
			}
			object u = \u0004.\u0001;
			lock (u)
			{
				Stream parseTreeStreamOfCompiledLibraryPOU = APEnvironmentFacade.Instance.GetParseTreeStreamOfCompiledLibraryPOU(projectHandle, \u0002.ObjectGuid);
				if (parseTreeStreamOfCompiledLibraryPOU != null)
				{
					ISharedDataStorage sharedDataStorage = APEnvironmentFacade.Instance.GetSharedDataStorage(projectHandle);
					IArchiveReader archiveReader;
					if (sharedDataStorage != null)
					{
						archiveReader = APEnvironmentFacade.Instance.CreateNewEncryptedBinaryArchiveReader();
					}
					else
					{
						archiveReader = APEnvironmentFacade.Instance.CreateEncryptedBinaryArchiveReader();
					}
					archiveReader.Initialize(parseTreeStreamOfCompiledLibraryPOU);
					result = (_IStatement)((IArchiveReader2)archiveReader).Load(sharedDataStorage);
					parseTreeStreamOfCompiledLibraryPOU.Close();
				}
			}
			return result;
		}

		// Token: 0x040002A0 RID: 672
		private static object \u0001 = new object();
	}
}
