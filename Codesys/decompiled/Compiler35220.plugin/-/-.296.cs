using System;
using System.Linq;
using \u0011;
using \u0018;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Scopes;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0007
{
	// Token: 0x02000318 RID: 792
	internal static class \u0012
	{
		// Token: 0x06002F94 RID: 12180 RVA: 0x000B32F4 File Offset: 0x000B14F4
		internal static void \u0001(_ISignature \u0002, _ICompileContext \u0003)
		{
			if (\u0002.AllLazy.Any<IVariable>())
			{
				global::\u0007.\u0012.\u0002(\u0002, \u0003);
				global::\u0007.\u0012.\u0001(\u0002);
			}
		}

		// Token: 0x06002F95 RID: 12181 RVA: 0x000B3310 File Offset: 0x000B1510
		private static void \u0001(_ISignature \u0002)
		{
			LList<IVariable> llist = \u0002.\u0001();
			bool flag = false;
			foreach (IVariable variable in llist)
			{
				if (APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(\u0002, variable, GUIHidingFlags.AllCommon))
				{
					flag = true;
				}
				else
				{
					\u0002.\u0001(variable.SourcePosition, Severity.Error, MessageId.Err_NoResolutionForLazyVariable, new object[]
					{
						variable.OrgName
					});
				}
			}
			if (flag)
			{
				\u0002.AddMessage(null, Severity.Error, MessageId.Err_NoResolutionForSomeLazyVariables, Array.Empty<object>());
			}
		}

		// Token: 0x06002F96 RID: 12182 RVA: 0x000B33B0 File Offset: 0x000B15B0
		private static void \u0002(_ISignature \u0002, _ICompileContext \u0003)
		{
			ISignature[] subSignatures = \u0002.SubSignatures;
			int i = 0;
			while (i < subSignatures.Length + 1)
			{
				_ICompiledPOU icompiledPOU;
				if (i == subSignatures.Length)
				{
					icompiledPOU = (\u0003.GetCompiledPOU(\u0002.ObjectGuid) as _ICompiledPOU);
					goto IL_58;
				}
				if (subSignatures[i].GetFlag(SignatureFlag.Action) || subSignatures[i].HasAttribute(CompileAttributes.ATTRIBUTE_USESINSTANCELAZIES))
				{
					icompiledPOU = (\u0003.GetCompiledPOUById(subSignatures[i].Id) as _ICompiledPOU);
					goto IL_58;
				}
				IL_65:
				i++;
				continue;
				IL_58:
				if (icompiledPOU == null || !global::\u0007.\u0012.\u0001(\u0002, icompiledPOU, \u0003))
				{
					goto IL_65;
				}
				break;
			}
		}

		// Token: 0x06002F97 RID: 12183 RVA: 0x000B3430 File Offset: 0x000B1630
		private static bool \u0001(_ISignature \u0002, ICompiledPOU \u0003, _ICompileContext \u0004)
		{
			LList<IVariable> llist = \u0002.\u0001();
			if (llist.Count == 0)
			{
				return true;
			}
			bool u = \u0004.HasByteSupport();
			int num = int.MaxValue;
			while (llist.Count != 0 && num > llist.Count)
			{
				num = llist.Count;
				\u0002.SetFlag((SignatureFlag)((ulong)int.MinValue), true);
				_IPrecompileScope2 scope;
				if (!string.IsNullOrEmpty(\u0002.LibraryPath))
				{
					_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(\u0002.LibraryPath);
					scope = new CheckerScope(\u0004.ApplicationGuid, \u0002, libraryContext, null);
				}
				else
				{
					scope = new CheckerScope(\u0004.ApplicationGuid, \u0002, APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(\u0004.ApplicationGuid), APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
				}
				UnknownIdentVisitor unknownIdentVisitor = new UnknownIdentVisitor(scope, true, \u0004, llist);
				((_ICompiledPOU)\u0003).Accept(unknownIdentVisitor);
				\u0018.\u0001[] u2 = unknownIdentVisitor.DeclarationInfo;
				global::\u0007.\u0012.\u0001(\u0002, \u0004, llist, u, unknownIdentVisitor, u2);
				llist = \u0002.\u0001();
			}
			return !\u0002.AllLazy.Any<IVariable>();
		}

		// Token: 0x06002F98 RID: 12184 RVA: 0x000B3534 File Offset: 0x000B1734
		private static void \u0001(_ISignature \u0002, _ICompileContext \u0003, LList<IVariable> \u0004, bool \u0005, UnknownIdentVisitor \u0006, \u0018.\u0001[] \u0007)
		{
			foreach (_IVariable ivariable in \u0004.OfType<_IVariable>())
			{
				foreach (\u0018.\u0001 u in \u0007)
				{
					if (string.Compare(u.Name, ivariable.VersionedName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						IType type = u.DerivedType;
						type = global::\u0007.\u0012.\u0001(\u0003, type);
						if (global::\u0007.\u0012.\u0001(\u0002, \u0005, \u0006, ivariable, type))
						{
							break;
						}
					}
				}
			}
		}

		// Token: 0x06002F99 RID: 12185 RVA: 0x000B35C8 File Offset: 0x000B17C8
		private static bool \u0001(_ISignature \u0002, bool \u0003, UnknownIdentVisitor \u0004, _IVariable \u0005, IType \u0006)
		{
			if (\u0004.\u0001(\u0006))
			{
				\u0005._Type = ((_IType)\u0006).Duplicate;
				if (\u0005._Type.Class == TypeClass.Bit)
				{
					\u0005._Type = TypeTable.Bool;
				}
				if (!\u0003)
				{
					\u0005._Type = \u0002.ReplaceTypes(\u0005._Type, SpecialFeatures.NoByteSupport);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06002F9A RID: 12186 RVA: 0x000B3624 File Offset: 0x000B1824
		private static IType \u0001(_ICompileContext \u0002, IType \u0003)
		{
			if (\u0003 != null && \u0003.Class == TypeClass.AnyInt)
			{
				_IRangeAwareAnyIntType irangeAwareAnyIntType = \u0003 as _IRangeAwareAnyIntType;
				if (irangeAwareAnyIntType != null)
				{
					\u0003 = irangeAwareAnyIntType.ResolveIntegerType(\u0002);
				}
				else
				{
					if (\u0002 != null)
					{
						ICodegenerator4 codegenerator = \u0002.Codegenerator as ICodegenerator4;
						if (codegenerator != null && codegenerator.RegisterSize == 2)
						{
							return TypeTable.Int;
						}
					}
					\u0003 = TypeTable.DInt;
				}
			}
			return \u0003;
		}
	}
}
