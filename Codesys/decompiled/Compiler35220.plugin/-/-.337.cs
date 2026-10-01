using System;
using System.Collections.Generic;
using \u0007;
using \u0011;
using \u0014;
using \u0017;
using \u0018;
using \u001A;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Compile.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;
using \u0084;

namespace \u0010
{
	// Token: 0x02000380 RID: 896
	internal sealed class \u000E : \u001E
	{
		// Token: 0x06003488 RID: 13448 RVA: 0x000CEF90 File Offset: 0x000CD190
		public bool \u0001(global::\u0018.\u0010 \u0002)
		{
			LDictionary<global::\u0017.\u0004, global::\u0017.\u0004> ldictionary = new LDictionary<global::\u0017.\u0004, global::\u0017.\u0004>();
			if (!global::\u0010.\u000E.\u0001(ldictionary, \u0002.ComconNew))
			{
				return false;
			}
			for (int i = 0; i < \u0002.ComconNew.CompiledPOUList.Count; i++)
			{
				_ICompiledPOU u = \u0002.ComconNew.CompiledPOUList[i];
				if (!global::\u0010.\u000E.\u0001(\u0002, ldictionary, u))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06003489 RID: 13449 RVA: 0x000CEFF0 File Offset: 0x000CD1F0
		private static bool \u0001(global::\u0018.\u0010 \u0002, LDictionary<global::\u0017.\u0004, global::\u0017.\u0004> \u0003, _ICompiledPOU \u0004)
		{
			if (\u0004.GetFlag(CompiledPOUFlags.Typified))
			{
				return true;
			}
			_ISignature isignature = \u0002.ComconNew[\u0004.SignatureId];
			if (isignature == null || isignature.GetFlag(SignatureFlag.External))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(isignature.LibraryPath))
			{
				return false;
			}
			if (!isignature.GetFlag(SignatureFlag.Located) && isignature.POUType != Operator.Function)
			{
				return false;
			}
			if (!global::\u0010.\u000E.\u0001(\u0002, \u0004, isignature))
			{
				return false;
			}
			global::\u0007.\u0013.\u0001(\u0002.ComconNew, \u0004, isignature, null);
			_ISignature isignature2 = \u0002.ComconOld[isignature.Id];
			if (isignature2 == null || isignature.ChecksumNoInit != isignature2.ChecksumNoInit)
			{
				Locator.\u0001(\u0002.ComconNew.DataManager, \u0002.ComconNew, \u0002.ComconOld, isignature, isignature2);
			}
			if (global::\u0010.\u000E.\u0001(\u0004, \u0002))
			{
				return false;
			}
			\u0081.\u0008 u;
			if (!global::\u0010.\u000E.\u0001(\u0002, \u0003, \u0004, isignature, out u))
			{
				return false;
			}
			\u001A.\u0005.\u0001(\u0004.GetParseTree(), isignature.Id, u);
			ConstantFolder.ReplaceFoldedConstants(\u0004.GetParseTree(), \u0002.ComconNew, u, \u0004);
			global::\u0018.\u000E.\u0001(\u0004.GetParseTree(), \u0002.ComconNew);
			TypeCheckerVisitor visitor = new TypeCheckerVisitor(u, \u0002.ComconNew, false, \u0004, false);
			\u0004.Accept(visitor);
			if (\u0002.\u0001(\u0004, new ErrorVisitor()))
			{
				return false;
			}
			\u0004.SetMessages(Array.Empty<_ICompilerMessage>());
			\u0004.SetFlag(CompiledPOUFlags.Typified, true);
			\u0004.SetFlag(CompiledPOUFlags.Compiled, true);
			\u0004.SetFlag(CompiledPOUFlags.ToCompile, true);
			\u0004.SetFlag(CompiledPOUFlags.ContainsNoCode, false);
			\u0004.SetFlagInternal(InternalCompiledPOUFlags.TypeCheckDone, true);
			\u0002.compiledpous.Add(\u0004);
			return true;
		}

		// Token: 0x0600348A RID: 13450 RVA: 0x000CF174 File Offset: 0x000CD374
		private static bool \u0001(global::\u0018.\u0010 \u0002, _ICompiledPOU \u0003, _ISignature \u0004)
		{
			\u0003.DuplicateParseTreeForCompilation();
			if (\u0004.GetFlag(SignatureFlag.Located))
			{
				return true;
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002.ComconNew, \u0004.Id);
			scope.LocalSignature = \u0004;
			if (!\u0004.GetFlagInternal(SignatureFlagInternal.VariablesTypified))
			{
				return false;
			}
			\u0002.CompileInformation.InterfaceCompiler.\u0005(\u0004, scope);
			Locator.\u0001(\u0004, null, \u0002.ComconNew, null);
			Locator.\u0001(\u0002.ComconNew, \u0004, null);
			return !\u0002.\u0001(\u0004);
		}

		// Token: 0x0600348B RID: 13451 RVA: 0x000CF1F8 File Offset: 0x000CD3F8
		private static bool \u0001(global::\u0018.\u0010 \u0002, LDictionary<global::\u0017.\u0004, global::\u0017.\u0004> \u0003, _ICompiledPOU \u0004, _ISignature \u0005, out \u0081.\u0008 \u0006)
		{
			\u0006 = \u0081.\u0008.\u0001(\u0002.ComconNew, \u0005, \u0002.PrecompPool, \u0002.ComconOld);
			ExpressionTypifierWithSpecialTasks expressionTypifierWithSpecialTasks = new ExpressionTypifierWithSpecialTasks(\u0006, \u0002.ComconNew, null, true, true, \u0004);
			expressionTypifierWithSpecialTasks.CheckForFastOnlineChange = true;
			expressionTypifierWithSpecialTasks.TypeChecker = new global::\u0014.\u0012(\u0006, \u0002.ComconNew, \u0004);
			expressionTypifierWithSpecialTasks.VarsToCheck = \u0003;
			\u0004.Accept(expressionTypifierWithSpecialTasks);
			return expressionTypifierWithSpecialTasks.FastOnlineChangeOK;
		}

		// Token: 0x0600348C RID: 13452 RVA: 0x000CF264 File Offset: 0x000CD464
		private static bool \u0001(_ICompiledPOU \u0002, global::\u0018.\u0010 \u0003)
		{
			return \u0003.\u0001(\u0002, new ErrorVisitor
			{
				VisitErrorStatements = false,
				VisitErrorStatementsInConditionalPragmas = false
			});
		}

		// Token: 0x0600348D RID: 13453 RVA: 0x000CF280 File Offset: 0x000CD480
		private static bool \u0001(LDictionary<global::\u0017.\u0004, global::\u0017.\u0004> \u0002, _ICompileContext \u0003)
		{
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0003);
			IList<ISignature> list = scope["IoConfig_Globals_Mapping"];
			if (list != null && list.Count > 1)
			{
				return false;
			}
			if (list == null || 1 != list.Count)
			{
				return true;
			}
			string[] attributes = list[0].Attributes;
			IScanner u = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner();
			foreach (string u2 in attributes)
			{
				global::\u0010.\u000E.\u0001(\u0002, \u0003, scope, u, u2);
			}
			return true;
		}

		// Token: 0x0600348E RID: 13454 RVA: 0x000CF300 File Offset: 0x000CD500
		private static void \u0001(LDictionary<global::\u0017.\u0004, global::\u0017.\u0004> \u0002, _ICompileContext \u0003, IScope5 \u0004, IScanner \u0005, string \u0006)
		{
			if (!\u0006.StartsWith("IoMap", StringComparison.Ordinal))
			{
				return;
			}
			int num = \u0006.IndexOf("@", StringComparison.OrdinalIgnoreCase);
			if (num < 0 || num + 1 >= \u0006.Length)
			{
				return;
			}
			string stInput = \u0006.Substring(num + 1);
			\u0005.Initialize(stInput);
			IToken token;
			IToken token2;
			if (\u0005.GetNext(out token) != TokenType.Identifier || \u0005.GetNext(out token2) != TokenType.Operator || \u0005.GetOperator(token2) != Operator.Period)
			{
				return;
			}
			_IExpression iexpression = ((IParser)new global::\u0011.\u0006(\u0005)).ParseOperand() as _IExpression;
			ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(\u0004, \u0003, null, false, false, null);
			iexpression.Accept(ivisit);
			IVariable variable = iexpression.GetVariable(\u0004);
			ISignature signature = \u0004[iexpression.SignatureId];
			if (variable != null && signature != null)
			{
				global::\u0017.\u0004 u = new global::\u0017.\u0004(variable.Id, signature.Id);
				if (!\u0002.ContainsKey(u))
				{
					\u0002.Add(u, null);
				}
			}
		}
	}
}
