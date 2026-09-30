using System;
using System.Collections.Generic;
using System.Linq;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace CODESYS.Parser35220
{
	// Token: 0x02000008 RID: 8
	internal static class LanguageModelOfRawST
	{
		// Token: 0x06000031 RID: 49 RVA: 0x0000289C File Offset: 0x00000A9C
		internal static void CreateLanguageModelOfRawST(ParserContext context, IPOUSyntax[] pouSyntax, Guid objectGuid, Guid guidParent, ILanguageModel dstLanguageModel)
		{
			Guid namespaceGuid = (guidParent != Guid.Empty) ? guidParent : dstLanguageModel.ApplicationGuid;
			foreach (IPOUSyntax pou in pouSyntax)
			{
				ValueTuple<string, string> pouname = LanguageModelOfRawST.GetPOUName(pou);
				string item = pouname.Item1;
				Guid objectGuid2 = LanguageModelOfRawST.GetObjectGuid(pouname.Item2 ?? item, namespaceGuid);
				LanguageModelOfRawST.CreateLanguageModelOfPOUSyntax(context, dstLanguageModel, pou, item, objectGuid, guidParent, objectGuid2);
			}
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00002905 File Offset: 0x00000B05
		private static Guid GetObjectGuid(string stName, Guid namespaceGuid)
		{
			return GuidHelper.CreateNameBasedGuid(namespaceGuid, stName, 5);
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00002910 File Offset: 0x00000B10
		private static void CreateLanguageModelOfPOUSyntax(ParserContext context, ILanguageModel lm, IPOUSyntax pou, string stName, Guid sourceObjectGuid, Guid parentObjectGuid, Guid pouGuid)
		{
			_IPOUDeclarationStatement ipoudeclarationStatement = pou.Declaration.Statements.OfType<_IPOUDeclarationStatement>().FirstOrDefault<_IPOUDeclarationStatement>();
			_ITypeDeclarationStatement itypeDeclarationStatement = pou.Declaration.Statements.OfType<_ITypeDeclarationStatement>().FirstOrDefault<_ITypeDeclarationStatement>();
			if (ipoudeclarationStatement != null && 287 == ipoudeclarationStatement.Class)
			{
				ILMGlobVarlist ilmglobVarlist = context.LMItemFactory.CreateGlobVarlist(stName, pouGuid);
				ilmglobVarlist.Interface = LanguageModelOfRawST.GetInterfaceForGlobalVarList(context, pou);
				ilmglobVarlist.ObjectGuid = sourceObjectGuid;
				lm.AddGlobalVariableList(ilmglobVarlist);
			}
			else if (itypeDeclarationStatement != null)
			{
				ILMDataType ilmdataType = context.LMItemFactory.CreateDataType(stName, pouGuid);
				ilmdataType.Interface = pou.Declaration;
				ilmdataType.ObjectGuid = sourceObjectGuid;
				lm.AddDataType(ilmdataType);
			}
			else
			{
				ILMPOU ilmpou = context.LMItemFactory.CreatePou(stName, pouGuid);
				ilmpou.Interface = pou.Declaration;
				ilmpou.Body = pou.Implementation;
				ilmpou.ParentObjectGuid = parentObjectGuid;
				ilmpou.ObjectGuid = sourceObjectGuid;
				ilmpou.MessageGuid = sourceObjectGuid;
				lm.AddPou(ilmpou);
			}
			foreach (IPOUSyntax pou2 in pou.SubPOUs)
			{
				ValueTuple<string, string> pouname = LanguageModelOfRawST.GetPOUName(pou2);
				string item = pouname.Item1;
				Guid objectGuid = LanguageModelOfRawST.GetObjectGuid(pouname.Item2 ?? item, pouGuid);
				LanguageModelOfRawST.CreateLanguageModelOfPOUSyntax(context, lm, pou2, item, sourceObjectGuid, pouGuid, objectGuid);
			}
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00002A78 File Offset: 0x00000C78
		private static ValueTuple<string, string> GetTypeName(IPOUSyntax pou)
		{
			_ITypeDeclarationStatement itypeDeclarationStatement = pou.Declaration.Statements.OfType<_ITypeDeclarationStatement>().FirstOrDefault<_ITypeDeclarationStatement>();
			if (itypeDeclarationStatement == null)
			{
				return new ValueTuple<string, string>(string.Empty, null);
			}
			return new ValueTuple<string, string>(itypeDeclarationStatement.Name, null);
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00002AB8 File Offset: 0x00000CB8
		private static ValueTuple<string, string> GetPOUName(IPOUSyntax pou)
		{
			IPOUDeclarationStatement ipoudeclarationStatement = pou.Declaration._StatementList.OfType<IPOUDeclarationStatement>().FirstOrDefault<IPOUDeclarationStatement>();
			if (ipoudeclarationStatement == null)
			{
				return LanguageModelOfRawST.GetTypeName(pou);
			}
			if (ipoudeclarationStatement.Class == 285 || 290 == ipoudeclarationStatement.Class)
			{
				return new ValueTuple<string, string>(IdentifierConstants.CreateGetterName(ipoudeclarationStatement.Name), null);
			}
			if (ipoudeclarationStatement.Class == 284)
			{
				return new ValueTuple<string, string>(IdentifierConstants.CreateSetterName(ipoudeclarationStatement.Name), null);
			}
			string text = ipoudeclarationStatement.IsOverloaded();
			if (text != null)
			{
				return new ValueTuple<string, string>(ipoudeclarationStatement.Name, text);
			}
			return new ValueTuple<string, string>(ipoudeclarationStatement.Name, null);
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00002B54 File Offset: 0x00000D54
		private static _ISequenceStatement GetInterfaceForGlobalVarList(ParserContext context, IPOUSyntax pou)
		{
			_ISequenceStatement isequenceStatement = context.LMItemFactory.CreateSequenceStatement();
			foreach (_IStatement istatement in pou.Declaration.Statements.OfType<_IStatement>())
			{
				_IPOUDeclarationStatement ipoudeclarationStatement = istatement as _IPOUDeclarationStatement;
				if (ipoudeclarationStatement != null)
				{
					LanguageModelOfRawST.ApplyAccessModifier(ipoudeclarationStatement, ipoudeclarationStatement.Declarations as _ISequenceStatement);
					LanguageModelOfRawST.ApplyMessages(ipoudeclarationStatement, isequenceStatement);
					LanguageModelOfRawST.ForceQualifiedOnlyForNamespace(context, isequenceStatement);
					isequenceStatement.Add(ipoudeclarationStatement.Declarations);
				}
				else
				{
					isequenceStatement.Add(istatement);
				}
			}
			return isequenceStatement;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00002BF0 File Offset: 0x00000DF0
		private static void ForceQualifiedOnlyForNamespace(ParserContext context, _ISequenceStatement seq)
		{
			if (seq.Statements.OfType<_IPragmaStatement>().Any((_IPragmaStatement pragmaStmt) => "attribute 'allow_unqualified_access'" == pragmaStmt.Text))
			{
				return;
			}
			_IStatement istatement = (_IStatement)context.LMItemFactory.CreatePragmaStatement2(null, "attribute '" + CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY + "'");
			seq.Add(istatement);
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00002C5C File Offset: 0x00000E5C
		private static void ApplyMessages(_IPOUDeclarationStatement pouDeclStmt, _ISequenceStatement seq)
		{
			if (pouDeclStmt.MessagesList == null)
			{
				return;
			}
			foreach (_ICompilerMessage icompilerMessage in pouDeclStmt.MessagesList)
			{
				seq.AddMessage(icompilerMessage);
			}
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00002CB4 File Offset: 0x00000EB4
		private static void ApplyAccessModifier(_IPOUDeclarationStatement pouDeclStmt, _ISequenceStatement seqDeclarations)
		{
			if (seqDeclarations == null)
			{
				return;
			}
			SignatureFlag access = pouDeclStmt.Access;
			if ((4398046511104L & access) == null)
			{
				return;
			}
			foreach (_IVariableDeclarationListStatement ivariableDeclarationListStatement in seqDeclarations.Statements.OfType<_IVariableDeclarationListStatement>())
			{
				ivariableDeclarationListStatement.Flags |= 34359738368L;
			}
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00002D2C File Offset: 0x00000F2C
		private static string IsOverloaded(this IPOUDeclarationStatement decl)
		{
			_IMethodDeclarationStatement imethodDeclarationStatement = decl as _IMethodDeclarationStatement;
			if (imethodDeclarationStatement == null || !imethodDeclarationStatement.Overload)
			{
				return null;
			}
			IEnumerable<string> values = (from d in decl.DeclarationLists
			where LanguageModelOfRawST.IsInputFlag(d.Flags)
			select d).SelectMany((IVariableDeclarationListStatement x) => x.Declarations).Select(delegate(IVariableDeclarationStatement x)
			{
				string str = "@";
				string text = x.DeclaredType.ToString();
				return str + ((text != null) ? text.ToUpperInvariant() : null) + "@";
			});
			return "`" + decl.Name + string.Concat(values) + "`";
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00002DDE File Offset: 0x00000FDE
		private static bool IsInputFlag(VarFlag flag)
		{
			return (flag & 10L) > 0L;
		}
	}
}
