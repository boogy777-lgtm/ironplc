using System;
using \u0007;
using \u0014;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0005
{
	// Token: 0x020003C6 RID: 966
	internal static class \u0008
	{
		// Token: 0x060036D7 RID: 14039 RVA: 0x000DF61C File Offset: 0x000DD81C
		internal static void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ICompileContext \u0004)
		{
			if (\u0003.POUType != Operator.Interface)
			{
				return;
			}
			if (\u0003.POUType == Operator.Interface)
			{
				_ISignature isignature = ParserHelper.\u0001(global::\u0005.\u0008.\u0001(IdentifierConstants.InterfaceUnion(\u0003.OrgName), \u0003.Name), true);
				isignature.LibraryPath = \u0003.LibraryPath;
				isignature.SetFlag(SignatureFlag.InterfaceLibraryObject, \u0003.GetFlag(SignatureFlag.InterfaceLibraryObject));
				isignature.SetFlag(SignatureFlag.PoolSignature, \u0003.GetFlag(SignatureFlag.PoolSignature));
				isignature.SetFlagInternal(SignatureFlagInternal.VersionFreeLibrary, \u0003.GetFlagInternal(SignatureFlagInternal.VersionFreeLibrary));
				_ISignature isignature2 = null;
				if (\u0004 != null)
				{
					isignature2 = \u0004[isignature.GetSearchName(\u0002)];
				}
				isignature = isignature.CreateCompiledSignature(isignature2, \u0002.HasByteSupport());
				isignature.ObjectGuid = \u0003.ObjectGuid;
				isignature.SetFlag(SignatureFlag.Generated | SignatureFlag.ImplicitInterfaceUnion, true);
				isignature.SetFlag(SignatureFlag.InterfaceLibraryObject, \u0003.GetFlag(SignatureFlag.InterfaceLibraryObject));
				isignature.SetFlagInternal(SignatureFlagInternal.VersionFreeLibrary, \u0003.GetFlagInternal(SignatureFlagInternal.VersionFreeLibrary));
				\u0002.AddSignature(isignature, isignature2, \u0004, true);
				IScope5 u = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
				global::\u0014.\u0013.\u0002(isignature, u, \u0002);
				Locator.\u0001(isignature, isignature2, \u0002, \u0004);
			}
		}

		// Token: 0x060036D8 RID: 14040 RVA: 0x000DF74C File Offset: 0x000DD94C
		internal static _ISignature \u0001(_ICompileContext \u0002, _ISignature \u0003)
		{
			if (\u0003.POUType != Operator.Interface)
			{
				return null;
			}
			_ISignature isignature = ParserHelper.\u0001(global::\u0005.\u0008.\u0001(IdentifierConstants.InterfaceUnion(\u0003.OrgName), \u0003.Name), true);
			isignature.LibraryPath = \u0003.LibraryPath;
			isignature.SetFlag(SignatureFlag.InterfaceLibraryObject, \u0003.GetFlag(SignatureFlag.InterfaceLibraryObject));
			isignature.SetFlag(SignatureFlag.PoolSignature, \u0003.GetFlag(SignatureFlag.PoolSignature));
			isignature.SetFlagInternal(SignatureFlagInternal.VersionFreeLibrary, \u0003.GetFlagInternal(SignatureFlagInternal.VersionFreeLibrary));
			return \u0002[isignature.GetSearchName(\u0002)];
		}

		// Token: 0x060036D9 RID: 14041 RVA: 0x000DF7EC File Offset: 0x000DD9EC
		private static string \u0001(string \u0002, string \u0003)
		{
			int num = 100000;
			return string.Format("\r\n\t\t\t\t\tTYPE {0} :\r\n\t\t\t\t\tUNION\r\n\t\t\t\t\t\t__Interface: POINTER TO {1};\r\n\t\t\t\t\t\t__vfTablePointer: POINTER TO POINTER TO ARRAY[0..{2}] OF POINTER TO POINTER TO DWORD;\r\n\t\t\t\t\tEND_UNION\r\n\t\t\t\t\tEND_TYPE\r\n\t\t\t\t", \u0002, \u0003, num);
		}
	}
}
