using System;
using \u0003;
using \u000E;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0016
{
	// Token: 0x02000352 RID: 850
	internal static class \u0014
	{
		// Token: 0x0600332C RID: 13100 RVA: 0x000C6170 File Offset: 0x000C4370
		internal static bool \u0001(_ICompileContext \u0002, _IPreCompileContext \u0003, IMessageCategory \u0004)
		{
			bool flag = global::\u0016.\u0014.\u0002(\u0002, \u0003, \u0004);
			bool flag2 = global::\u0016.\u0014.\u0002(\u0002, \u0004);
			return flag || flag2;
		}

		// Token: 0x0600332D RID: 13101 RVA: 0x000C6190 File Offset: 0x000C4390
		internal static bool \u0001(_ICompileContext \u0002, IMessageCategory \u0003)
		{
			bool result = true;
			foreach (object obj in \u0002._LibraryTable.Get32BitOnly())
			{
				_IPreCompileContext ipreCompileContext = (_IPreCompileContext)obj;
				global::\u0016.\u0014.\u0001(ipreCompileContext.LibraryPath, ipreCompileContext.LibraryId, \u0003, MessageId.Err_LibSupports32BitOnly, global::\u0003.\u0006.\u0001(MessageId.Err_LibSupports32BitOnly, new object[]
				{
					ipreCompileContext.LibraryPath
				}));
				result = false;
			}
			return result;
		}

		// Token: 0x0600332E RID: 13102 RVA: 0x000C621C File Offset: 0x000C441C
		private static bool \u0002(_ICompileContext \u0002, _IPreCompileContext \u0003, IMessageCategory \u0004)
		{
			if (Scanner.UnicodeIdentifierOption)
			{
				return true;
			}
			bool result = true;
			foreach (_IPreCompileContext ipreCompileContext in \u0002._LibraryTable.GetVisibleLibraries(\u0003))
			{
				bool flag = false;
				if (ipreCompileContext.PrecompiledLibrary)
				{
					flag = ipreCompileContext.SavedWithUnicodeIdentifiers;
				}
				else
				{
					IProject2 project = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectByLibraryId(ipreCompileContext.LibraryPath) as IProject2;
					if (project != null)
					{
						IOptionKey optionKey = project.GetProjectOptionsRootKey().OpenSubKey(global::\u0016.\u0014.\u0002);
						if (optionKey != null && optionKey.HasValue(global::\u0016.\u0014.\u0001, typeof(bool)))
						{
							flag = (bool)optionKey[global::\u0016.\u0014.\u0001];
						}
					}
				}
				if (flag)
				{
					Guid projectInfoObjectGuid = APEnvironmentFacade.Instance.ProjectInfoObjectGuid;
					string u = \u0018.\u0001(MessageId.Err_LibraryWithUnicodeIdentifiers, new object[]
					{
						ipreCompileContext.LibraryPath
					});
					_ICompilerMessage message = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.PrimaryProjectHandle, projectInfoObjectGuid, 0L, 0, 0), u, Severity.Error, MessageId.Err_LibraryWithUnicodeIdentifiers);
					APEnvironmentFacade.Instance.AddMessage(\u0004, message);
					result = false;
				}
			}
			return result;
		}

		// Token: 0x0600332F RID: 13103 RVA: 0x000C6350 File Offset: 0x000C4550
		private static bool \u0002(_ICompileContext \u0002, IMessageCategory \u0003)
		{
			bool result = true;
			Version v = APEnvironmentFacade.Instance.CompilerVersionToUseInternal();
			foreach (string stLibraryId in \u0002._LibraryTable.AllReferencedLibraries())
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(stLibraryId);
				if (libraryContext != null && libraryContext.PrecompiledLibrary)
				{
					Version version = libraryContext.CompilerVersionSavedWith();
					if (version > v)
					{
						string u = global::\u0003.\u0006.\u0001(MessageId.Err_UnknownCompilerVersionInCompiledLib, new object[]
						{
							libraryContext.LibraryPath,
							APEnvironmentFacade.Instance.CompilerVersionSettings.MapFromInternalToOEMTextSave(version)
						});
						global::\u0016.\u0014.\u0001(libraryContext.LibraryPath, libraryContext.LibraryId, \u0003, MessageId.Err_UnknownCompilerVersionInCompiledLib, u);
						result = false;
					}
				}
			}
			return result;
		}

		// Token: 0x06003330 RID: 13104 RVA: 0x000C6430 File Offset: 0x000C4630
		private static void \u0001(string \u0002, string \u0003, IMessageCategory \u0004, MessageId \u0005, string \u0006)
		{
			int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0002);
			Guid u = Guid.Empty;
			Guid[] libraryReferences = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryReferences(\u0003);
			if (libraryReferences.Length != 0)
			{
				u = libraryReferences[0];
			}
			_ICompilerMessage message = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(projectHandle, u, 0L, 0, 0), \u0006, Severity.Error, \u0005);
			APEnvironmentFacade.Instance.AddMessage(\u0004, message);
		}

		// Token: 0x040009B2 RID: 2482
		internal static readonly string \u0001 = "UnicodeIdentifiers";

		// Token: 0x040009B3 RID: 2483
		internal static readonly string \u0002 = "{E709B08B-B6E4-4966-8EED-D793A13114C6}";
	}
}
