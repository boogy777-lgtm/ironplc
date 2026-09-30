using System;
using System.Collections.Generic;
using \u0019;
using \u001E;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0002
{
	// Token: 0x020002D1 RID: 721
	internal sealed class \u0008
	{
		// Token: 0x06002B66 RID: 11110 RVA: 0x00098B4C File Offset: 0x00096D4C
		public \u0008(IScope5 \u009B\u0002, _ICompileContext \u0001\u0002)
		{
			this.\u0001 = \u0019.\u0003.Builder;
			this.\u0001 = \u009B\u0002;
			this.\u0001 = \u0001\u0002;
		}

		// Token: 0x06002B67 RID: 11111 RVA: 0x00098B70 File Offset: 0x00096D70
		public _ISubRoutineStatement \u0001(_ICompiledPOU2 \u0002, _ITryCatchStatement \u0003, ref int \u0004)
		{
			this.\u0001 = \u0002;
			bool flag;
			bool flag2;
			_ISubRoutineStatement result = this.\u0001(\u0003, out flag, out flag2, ref \u0004);
			_ISequenceStatement @catch = \u0003._Catch;
			_ICompiledPOU2 u = this.\u0001;
			int num = \u0004;
			\u0004 = num + 1;
			flag = (\u001E.\u0013.\u0001(@catch, u, num) || flag);
			if (flag)
			{
				this.\u0001();
			}
			_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001();
			this.\u0001(\u0003, flag, isequenceStatement);
			\u0003._ReplacedSequence = isequenceStatement;
			this.\u0001(isequenceStatement);
			return result;
		}

		// Token: 0x06002B68 RID: 11112 RVA: 0x00098BD4 File Offset: 0x00096DD4
		private _IExpression \u0001()
		{
			return \u0019.\u0003.\u0001();
		}

		// Token: 0x06002B69 RID: 11113 RVA: 0x00098BDC File Offset: 0x00096DDC
		private void \u0001(_ITryCatchStatement \u0002, bool \u0003, _ISequenceStatement \u0004)
		{
			_ICallExpression exp = this.\u0001(\u0002);
			_IOperatorExpression ioperatorExpression = \u0019.\u0003.\u0001(Operator.NotEqual);
			ioperatorExpression.AddOperand(\u0019.\u0003.\u0001(0L));
			ioperatorExpression.AddOperand(exp);
			_IIfStatement iifStatement = (_IIfStatement)this.\u0001.CreateIfStatement(null, ioperatorExpression, \u0002._Catch);
			iifStatement.SetFlag(StatementFlag.GenerateBP, true);
			iifStatement.SetFlag(StatementFlag.GenerateFlow, true);
			if (\u0003)
			{
				\u0004.AddStatement(\u0019.\u0003.\u0001(\u0019.\u0003.\u0001("__tryreturn"), \u0019.\u0003.\u0001(false)));
			}
			\u0004.Add(iifStatement);
			if (\u0003)
			{
				_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001(new List<IStatement>
				{
					\u0019.\u0003.\u0001()
				});
				if (\u0002._Finally != null)
				{
					isequenceStatement.InsertStatement(0, \u0002._Finally.Duplicate() as _IStatement);
				}
				_IIfStatement iifStatement2 = (_IIfStatement)this.\u0001.CreateIfStatement(null, \u0019.\u0003.\u0001("__tryreturn"), isequenceStatement);
				iifStatement2.SetFlag(StatementFlag.GenerateBP, true);
				iifStatement2.SetFlag(StatementFlag.GenerateFlow, true);
				\u0004.Add(iifStatement2);
			}
			if (\u0002._Finally != null)
			{
				\u0004.Add(\u0002._Finally);
			}
		}

		// Token: 0x06002B6A RID: 11114 RVA: 0x00098CE8 File Offset: 0x00096EE8
		private _ICallExpression \u0001(_ITryCatchStatement \u0002)
		{
			_ICallExpression icallExpression = \u0019.\u0003.\u0001();
			_ISystemScopeExpression callee = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001("trycatch"), Token.Empty);
			_IVariableExpression expVariable = \u0019.\u0003.\u0001("pFun");
			_IVariableExpression expVariable2 = \u0019.\u0003.\u0001("pParam");
			_IVariableExpression expVariable3 = \u0019.\u0003.\u0001("dwFlags");
			_ILiteralExpression exp = \u0019.\u0003.\u0001(0L);
			_IExpression exp2 = this.\u0001();
			string stName = string.Format(IdentifierConstants.SignTryCatchAddressTemplate, this.\u0001.SignatureId, \u0002.Index);
			_IVariableExpression exp3 = this.\u0001.CreateVariableExpression(null, stName) as _IVariableExpression;
			icallExpression.AddParam(exp3, expVariable);
			icallExpression.AddParam(exp2, expVariable2);
			_IVariableExpression expRight = \u0019.\u0003.\u0001("g_dwExFlags");
			_IVariableExpression expLeft = \u0019.\u0003.\u0001("ExceptionFlags");
			_ICompoAccessExpression expBase = this.\u0001.CreateCompoAccessExpression(null, expLeft, expRight) as _ICompoAccessExpression;
			_ISystemScopeExpression exp4 = this.\u0001.CreateSystemScopeExpression(null, expBase) as _ISystemScopeExpression;
			icallExpression.AddParam(exp4, expVariable3);
			_IVariableExpression expVariable4 = \u0019.\u0003.\u0001("ExceptionCode");
			if (\u0002._Exception != null)
			{
				_IOperatorExpression ioperatorExpression = \u0019.\u0003.\u0001(Operator.Adr);
				ioperatorExpression.AddOperand(\u0002._Exception);
				icallExpression.AddParam(ioperatorExpression, expVariable4);
			}
			else
			{
				icallExpression.AddParam(exp, expVariable4);
			}
			icallExpression._Callee = callee;
			return icallExpression;
		}

		// Token: 0x06002B6B RID: 11115 RVA: 0x00098E28 File Offset: 0x00097028
		private _ISubRoutineStatement \u0001(_ITryCatchStatement \u0002, out bool \u0003, out bool \u0004, ref int \u0005)
		{
			_ISubRoutineStatement isubRoutineStatement = \u0019.\u0003.\u0001();
			_ISequenceStatement @try = \u0002._Try;
			_ICompiledPOU2 u = this.\u0001;
			int num = \u0005;
			\u0005 = num + 1;
			\u0003 = \u001E.\u0013.\u0001(@try, u, num);
			if (\u0003)
			{
				this.\u0001();
			}
			foreach (_IStatement sm in \u0002._Try._StatementList)
			{
				isubRoutineStatement.Add(sm);
			}
			\u0004 = false;
			isubRoutineStatement.SetFlag(StatementFlag.GenerateBP, true);
			isubRoutineStatement.SetFlag(StatementFlag.GenerateFlow, true);
			this.\u0001(isubRoutineStatement);
			\u0002.Subroutine = isubRoutineStatement;
			return isubRoutineStatement;
		}

		// Token: 0x06002B6C RID: 11116 RVA: 0x00098ECC File Offset: 0x000970CC
		private void \u0001()
		{
			_ISignature isignature = (_ISignature)this.\u0001[this.\u0001.SignatureId];
			if (isignature["__tryreturn"] == null)
			{
				this.\u0001.AddLocalStackVariableToCompiledSignature("__tryreturn", VarFlag.None, this.\u0001.CreateBoolType(), null, isignature);
				Locator.\u0001(this.\u0001.DataManager, this.\u0001, null, isignature, null);
			}
		}

		// Token: 0x06002B6D RID: 11117 RVA: 0x00098F3C File Offset: 0x0009713C
		private void \u0001(_IStatement \u0002)
		{
			ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(this.\u0001, this.\u0001, false, this.\u0001)
			{
				NoCrossReferences = true,
				Optimize = true
			};
			TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(this.\u0001, this.\u0001, true)
			{
				ConvertAllTypeMismatches = true
			};
			ErrorVisitor ivisit3 = new ErrorVisitor();
			\u0002.Accept(ivisit);
			\u0002.Accept(ivisit2);
			\u0002.Accept(ivisit3);
		}

		// Token: 0x04000849 RID: 2121
		private _ICompiledPOU2 \u0001;

		// Token: 0x0400084A RID: 2122
		private readonly _ILanguageModelBuilder2 \u0001;

		// Token: 0x0400084B RID: 2123
		private readonly IScope5 \u0001;

		// Token: 0x0400084C RID: 2124
		private readonly _ICompileContext \u0001;

		// Token: 0x0400084D RID: 2125
		public const string \u0001 = "__tryreturn";
	}
}
