using System;
using \u0004;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0007
{
	// Token: 0x020003A6 RID: 934
	internal static class \u0013
	{
		// Token: 0x06003605 RID: 13829 RVA: 0x000D8374 File Offset: 0x000D6574
		internal static void \u0001(_ICompileContext \u0002, _ICompiledPOU \u0003, _ISignature \u0004, _ISignature \u0005)
		{
			if (\u0004.GetFlag(SignatureFlag.NoInit) || \u0004.HasErrors)
			{
				return;
			}
			if (\u0004.POUType != Operator.Function && \u0004.POUType != Operator.Method && \u0004.POUType != Operator.Program)
			{
				return;
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, \u0004.Id);
			_ISequenceStatement isequenceStatement = \u0018.\u0001(\u0002, \u0004, scope, ConstantInit.All, false, false, false, true, \u0004, \u0005, "bInitRetains", "bInCopyCode");
			if (\u0003.GetFlag(CompiledPOUFlags.ContainsNoParseTree))
			{
				return;
			}
			isequenceStatement = global::\u0007.\u0013.\u0001(\u0002, \u0004, \u0005, isequenceStatement);
			if (isequenceStatement == null)
			{
				return;
			}
			isequenceStatement.SetFlag(StatementFlag.Implicit, true);
			ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(scope, \u0002, false, \u0003);
			TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(scope, \u0002, true);
			isequenceStatement.Accept(ivisit);
			\u0018.\u000E.\u0001(isequenceStatement, \u0002);
			isequenceStatement.Accept(ivisit2);
			ErrorVisitor errorVisitor = new ErrorVisitor();
			isequenceStatement.Accept(errorVisitor);
			foreach (_ICompilerMessage icompilerMessage in errorVisitor.MessageList)
			{
				if (icompilerMessage.Severity == Severity.Error)
				{
					Debug.\u0001(true, "Fehler in " + \u0004.OrgName + ": " + icompilerMessage.Text);
				}
			}
			_ISequenceStatement isequenceStatement2 = \u0003.GetParseTree() as _ISequenceStatement;
			if (isequenceStatement2 == null)
			{
				\u0003.SetParseTree(isequenceStatement);
			}
			else
			{
				_ISequenceStatement isequenceStatement3 = \u0019.\u0003.\u0001();
				isequenceStatement3.Add(isequenceStatement);
				isequenceStatement3.Add(isequenceStatement2);
				\u0003.SetParseTree(isequenceStatement3);
			}
			\u0003.SetFlagInternal(InternalCompiledPOUFlags.ImplicitInitialisationCodeAdded, true);
		}

		// Token: 0x06003606 RID: 13830 RVA: 0x000D84EC File Offset: 0x000D66EC
		private static _ISequenceStatement \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004, _ISequenceStatement \u0005)
		{
			if (\u0002.ApplicationGuid == Guid.Empty)
			{
				return \u0005;
			}
			if (!\u0003.GetFlagInternal(SignatureFlagInternal.ContainsAccessToCurrentTask))
			{
				return \u0005;
			}
			if (\u0005 == null)
			{
				\u0005 = \u0019.\u0003.\u0001();
			}
			Helper.\u0001(\u0002, \u0003, \u0004);
			_IAssignmentExpression iassignmentExpression = \u0019.\u0003.\u0001();
			iassignmentExpression._LValue = \u0019.\u0003.\u0001(IdentifierConstants.CurrentTaskInfoPointer);
			iassignmentExpression._RValue = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001("__CurrentTaskDetection"));
			\u0005._StatementList.Insert(0, \u0019.\u0003.\u0001(iassignmentExpression));
			return \u0005;
		}
	}
}
