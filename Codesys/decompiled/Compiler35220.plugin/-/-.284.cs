using System;
using System.Collections.Generic;
using System.Linq;
using \u0001;
using \u0004;
using \u0007;
using \u0012;
using \u0014;
using \u0018;
using \u0019;
using \u001D;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0006
{
	// Token: 0x020002F7 RID: 759
	internal static class \u000F
	{
		// Token: 0x06002E9A RID: 11930 RVA: 0x000AE2FC File Offset: 0x000AC4FC
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			IList<_ISignature> allSignatureList = \u0002.AllSignatureList;
			for (int i = 0; i < allSignatureList.Count; i++)
			{
				global::\u0006.\u000F.\u0001(allSignatureList[i], \u0002, \u0003);
			}
		}

		// Token: 0x06002E9B RID: 11931 RVA: 0x000AE330 File Offset: 0x000AC530
		internal static void \u0001(_ISignature \u0002, _ICompileContext \u0003, _ICompileContext \u0004)
		{
			if (\u0002.POUType != Operator.FunctionBlock && !\u0002.GetFlag(SignatureFlag.Structure))
			{
				return;
			}
			_ISignature isignature = null;
			foreach (object obj in \u0002._SubSignatures)
			{
				_ISignature isignature2 = (_ISignature)obj;
				if (isignature2.POUType == Operator.Method && isignature2.Name == IdentifierConstants.InitMethodName && isignature2.AllInputs.Length >= 3 && isignature2.AllInputs[0].Type.Class == TypeClass.Bool && isignature2.AllInputs[0].Name == "BINITRETAINS" && isignature2.AllInputs[1].Type.Class == TypeClass.Bool && isignature2.AllInputs[1].Name == "BINCOPYCODE")
				{
					isignature = isignature2;
				}
			}
			if (isignature == null)
			{
				return;
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0003, \u0002.Id);
			_IStatement istatement = \u0018.\u0001(\u0002, \u0003, \u0004, isignature, scope);
			_ICompiledPOU icompiledPOU = \u0003._GetCompiledPOUById(isignature.Id);
			if (icompiledPOU != null && icompiledPOU.GetFlag(CompiledPOUFlags.ContainsNoParseTree))
			{
				return;
			}
			if (icompiledPOU == null)
			{
				icompiledPOU = \u0019.\u0003.\u0001(IdentifierConstants.InitMethodName);
				icompiledPOU.SetParseTree(\u0019.\u0003.\u0001());
				\u0003.AddCompiledPOU(icompiledPOU, isignature, true, \u0004);
				icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
				icompiledPOU.MessageGuid = \u0002.ObjectGuid;
				icompiledPOU.LibraryPath = \u0002.LibraryPath;
				global::\u0012.\u0002 u = new global::\u0012.\u0002(false);
				istatement.Accept(u.Traverser);
				icompiledPOU.Checksum = u.Checksum;
			}
			TypifierAndCrossReferenceCollector ivisit = new TypifierAndCrossReferenceCollector(scope, \u0003, icompiledPOU);
			istatement.Accept(ivisit);
		}

		// Token: 0x06002E9C RID: 11932 RVA: 0x000AE4E8 File Offset: 0x000AC6E8
		internal static void \u0002(_ISignature \u0002, _ICompileContext \u0003, _ICompileContext \u0004)
		{
			if (\u0002.POUType == Operator.Method && string.Equals(\u0002.Name, IdentifierConstants.InitMethodName, StringComparison.OrdinalIgnoreCase))
			{
				_ISignature isignature = \u0003[\u0002.ParentSignatureId];
				Debug.\u0001(isignature != null);
				IScope5 scope = global::\u0007.\u0005.\u0001(\u0003, isignature.Id);
				_IStatement istatement = \u0018.\u0001(isignature, \u0003, \u0004, \u0002, scope);
				_ICompiledPOU icompiledPOU = \u0003._GetCompiledPOUById(\u0002.Id);
				Debug.\u0001(icompiledPOU != null);
				if (icompiledPOU.GetFlag(CompiledPOUFlags.ContainsNoParseTree))
				{
					return;
				}
				_ISequenceStatement isequenceStatement = icompiledPOU.GetParseTree() as _ISequenceStatement;
				if (isequenceStatement == null || !isequenceStatement.Statements.Any<IStatement>())
				{
					icompiledPOU.SetParseTree(istatement);
				}
				else
				{
					_ISequenceStatement isequenceStatement2 = \u0019.\u0003.\u0001();
					isequenceStatement2.Add(istatement);
					isequenceStatement2.Add(isequenceStatement);
					icompiledPOU.SetParseTree(isequenceStatement2);
				}
				ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(scope, \u0003, null, false, true, icompiledPOU);
				istatement.Accept(ivisit);
				\u0018.\u000E.\u0001(istatement, \u0003);
				TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(scope, \u0003);
				istatement.Accept(ivisit2);
			}
		}

		// Token: 0x06002E9D RID: 11933 RVA: 0x000AE5E0 File Offset: 0x000AC7E0
		internal static void \u0002(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			foreach (_ICompiledPOU icompiledPOU in \u0002.GetAllCompiledPOUsEx().OfType<_ICompiledPOU>())
			{
				_ISignature isignature = \u0002[icompiledPOU.SignatureId];
				_ISignature u = null;
				if (\u0003 != null)
				{
					u = \u0003[isignature.Id];
				}
				global::\u0006.\u000F.\u0001(\u0002, icompiledPOU, isignature, u);
			}
		}

		// Token: 0x06002E9E RID: 11934 RVA: 0x000AE654 File Offset: 0x000AC854
		private static void \u0001(_ICompileContext \u0002, _ICompiledPOU \u0003, _ISignature \u0004, _ISignature \u0005)
		{
			if (\u0004.GetFlag(SignatureFlag.NoInit) || \u0004.HasErrors)
			{
				return;
			}
			if (\u0004.POUType != Operator.Function && \u0004.POUType != Operator.Method && \u0004.POUType != Operator.Program)
			{
				return;
			}
			IScope5 u = global::\u0007.\u0005.\u0001(\u0002, \u0004.Id);
			\u0018.\u0001(\u0002, \u0004, u, ConstantInit.All, false, false, false, true, \u0004, \u0005, "bInitRetains", "bInCopyCode");
			if (\u0004.GetFlagInternal(SignatureFlagInternal.ContainsAccessToCurrentTask))
			{
				Helper.\u0001(\u0002, \u0004, \u0005);
			}
		}

		// Token: 0x06002E9F RID: 11935 RVA: 0x000AE6D4 File Offset: 0x000AC8D4
		internal static void \u0003(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			IScope u = \u0002.CreateGlobalIScope();
			foreach (_ISignature isignature in \u0002.AllSignatureList)
			{
				if (isignature.GetFlagInternal(SignatureFlagInternal.StructureContainsFBInstances) && !\u0002.MinimalSystem)
				{
					_ISignature u2 = null;
					if (\u0003 != null)
					{
						u2 = \u0003[isignature.Id];
					}
					global::\u0001.\u0015.\u0001(isignature, u2, \u0002, \u0003);
				}
				global::\u0006.\u000F.\u0001(isignature, u, \u0002, \u0003);
			}
		}

		// Token: 0x06002EA0 RID: 11936 RVA: 0x000AE75C File Offset: 0x000AC95C
		internal static void \u0001(_ISignature \u0002, IScope \u0003, _ICompileContext \u0004, _ICompileContext \u0005)
		{
			if (\u0002.POUType == Operator.FunctionBlock || \u0002.GetFlag(SignatureFlag.Structure))
			{
				int u = 0;
				_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001();
				IList<_IVariable> allVariables = \u0002.AllVariables;
				for (int i = allVariables.Count - 1; i >= 0; i--)
				{
					IVariable u2 = allVariables[i];
					_IVariableExpression u3 = \u0019.\u0003.\u0001("bInCopyCode");
					\u001D.\u000F.\u0001(u2, \u0003, isequenceStatement, u3, ref u);
				}
				_ISignature isignature = \u0003[\u0002.BaseSignatureId] as _ISignature;
				bool flag = isignature != null && isignature.GetSubSignature("FB_Exit") != null;
				ISignature signature = \u0002.GetSubSignature(IdentifierConstants.ExitMethodName);
				_ISignature isignature2 = null;
				_ISignature isignature3 = null;
				if (\u0005 != null)
				{
					isignature3 = \u0005[\u0002.Id];
				}
				if (isequenceStatement._StatementList.Any<_IStatement>() || flag)
				{
					IScope5 scope = global::\u0007.\u0005.\u0001(\u0004, \u0002.Id);
					if (signature == null)
					{
						_ISignature isignature4 = global::\u0006.\u000F.\u0001();
						if (isignature3 != null)
						{
							isignature2 = (isignature3.GetSubSignature(isignature4.Name) as _ISignature);
						}
						_ISignature isignature5 = isignature4.CreateCompiledSignature(isignature2, \u0004.HasByteSupport());
						isignature5.ParentObjectGuid = \u0002.ObjectGuid;
						isignature5.ParentSignatureId = \u0002.Id;
						\u0004.AddSignature(isignature5, isignature2, \u0005, true);
						\u0002.AddSubSignature(isignature5);
						scope.LocalSignature = \u0002;
						scope.MethodSignature = isignature5;
						global::\u0014.\u0013.\u0002(isignature5, scope, \u0004);
						Locator.\u0001(isignature5, null, \u0004, null);
						signature = isignature5;
					}
					else if (isignature3 != null)
					{
						isignature2 = (isignature3.GetSubSignature(signature.Name) as _ISignature);
					}
					Helper.\u0001(u, signature as _ISignature, isignature2);
					_ICompiledPOU icompiledPOU = \u0004._GetCompiledPOUById(signature.Id);
					if (icompiledPOU == null)
					{
						icompiledPOU = \u0019.\u0003.\u0001(IdentifierConstants.ExitMethodName);
						icompiledPOU.SetParseTree(\u0019.\u0003.\u0001());
						\u0004.AddCompiledPOU(icompiledPOU, signature as _ISignature, true, \u0005);
					}
					TypifierAndCrossReferenceCollector ivisit = new TypifierAndCrossReferenceCollector(scope, \u0004, icompiledPOU);
					isequenceStatement.Accept(ivisit);
				}
			}
		}

		// Token: 0x06002EA1 RID: 11937 RVA: 0x000AE940 File Offset: 0x000ACB40
		internal static void \u0003(_ISignature \u0002, _ICompileContext \u0003, _ICompileContext \u0004)
		{
			if (\u0002.POUType == Operator.Method && string.Equals(\u0002.Name, IdentifierConstants.ExitMethodName, StringComparison.OrdinalIgnoreCase))
			{
				IScope scope = \u0003.CreateGlobalIScope();
				_ISignature isignature = scope[\u0002.ParentSignatureId] as _ISignature;
				Debug.\u0001(\u0002 != null);
				_ICompiledPOU icompiledPOU = \u0003._GetCompiledPOUById(\u0002.Id);
				Debug.\u0001(icompiledPOU != null);
				if (icompiledPOU.GetFlag(CompiledPOUFlags.ContainsNoParseTree))
				{
					return;
				}
				_IStatement istatement = null;
				int num = 0;
				_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001();
				IList<_IVariable> allVariables = isignature.AllVariables;
				for (int i = allVariables.Count - 1; i >= 0; i--)
				{
					IVariable u = allVariables[i];
					_IVariableExpression u2 = \u0019.\u0003.\u0001("bInCopyCode");
					\u001D.\u000F.\u0001(u, scope, isequenceStatement, u2, ref num);
				}
				_ISignature isignature2 = scope[isignature.BaseSignatureId] as _ISignature;
				if (isignature2 != null && isignature2.GetSubSignature("FB_Exit") != null)
				{
					_ILanguageModelBuilder7 ilanguageModelBuilder = \u0019.\u0003.Builder;
					IStatement state = ilanguageModelBuilder.CreatePragmaStatement2(null, "returnlabelposition");
					isequenceStatement.AddStatement(state);
					_IBaseExpression exp = ilanguageModelBuilder.CreateBaseExpression();
					_IDeRefAccessExpression expLeft = ilanguageModelBuilder.CreateDeRefAccessExpression(exp);
					_IVariableExpression right = ilanguageModelBuilder.CreateVariableExpression(IdentifierConstants.ExitMethodName);
					_ICompoAccessExpression icompoAccessExpression = ilanguageModelBuilder.CreateCompoAccessExpression(expLeft);
					icompoAccessExpression._Right = right;
					List<IAssignmentExpression> list = new List<IAssignmentExpression>();
					List<IAssignmentExpression> outputassignments = new List<IAssignmentExpression>();
					_IVariableExpression expLValue = ilanguageModelBuilder.CreateVariableExpression("bInCopyCode");
					_IVariableExpression rvalue = ilanguageModelBuilder.CreateVariableExpression("bInCopyCode");
					_IAssignmentExpression iassignmentExpression = ilanguageModelBuilder.CreateAssignmentExpression(expLValue);
					iassignmentExpression._RValue = rvalue;
					list.Add(iassignmentExpression);
					_IExpressionStatement iexpressionStatement = ilanguageModelBuilder.CreateCallStatement(null, icompoAccessExpression, null, null, list, outputassignments) as _IExpressionStatement;
					iexpressionStatement.SetFlag(StatementFlag.Implicit, true);
					isequenceStatement.AddStatement(iexpressionStatement);
				}
				if (isequenceStatement._StatementList.Count > 0)
				{
					_ILanguageModelBuilder ilanguageModelBuilder2 = \u0019.\u0003.Builder;
					isequenceStatement.InsertStatement(0, ilanguageModelBuilder2.CreatePragmaStatement2(null, "nobp"));
					isequenceStatement.InsertStatement(0, ilanguageModelBuilder2.CreatePragmaStatement2(null, "implicit on"));
					isequenceStatement.AddStatement(ilanguageModelBuilder2.CreatePragmaStatement2(null, "bp"));
					isequenceStatement.AddStatement(ilanguageModelBuilder2.CreatePragmaStatement2(null, "implicit off"));
					istatement = isequenceStatement;
				}
				if (istatement != null)
				{
					istatement.SetFlag(StatementFlag.Implicit, true);
					_ISequenceStatement isequenceStatement2 = icompiledPOU.GetParseTree() as _ISequenceStatement;
					if (isequenceStatement2 == null)
					{
						icompiledPOU.SetParseTree(istatement);
					}
					else
					{
						_ISequenceStatement isequenceStatement3 = \u0019.\u0003.\u0001();
						isequenceStatement3.Add(isequenceStatement2);
						isequenceStatement3.Add(istatement);
						icompiledPOU.SetParseTree(isequenceStatement3);
					}
					IScope5 scope2 = global::\u0007.\u0005.\u0001(\u0003, \u0002.Id);
					ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(scope2, \u0003, null, true, true, icompiledPOU);
					istatement.Accept(ivisit);
					TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(scope2, \u0003, true);
					istatement.Accept(ivisit2);
					icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
				}
			}
		}

		// Token: 0x06002EA2 RID: 11938 RVA: 0x000AEBD4 File Offset: 0x000ACDD4
		private static _ISignature \u0001()
		{
			return ParserHelper.\u0001(IdentifierConstants.GetFBExitInterface(), true);
		}
	}
}
