using System;
using System.Collections.Generic;
using \u0007;
using \u000E;
using \u000F;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0008
{
	// Token: 0x02000253 RID: 595
	internal class \u0010 : global::\u0019.\u0008, IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2
	{
		// Token: 0x060026E5 RID: 9957 RVA: 0x000869F8 File Offset: 0x00084BF8
		public \u0010(\u0081.\u0010 \u0096\u0007, ISpecificExpressionReplacer \u0018\u0006, global::\u000E.\u0011 \u0083\u0005)
		{
			this.\u0001 = new ExpressionReplacer(\u0018\u0006);
			this.\u0001 = \u0083\u0005;
			this.\u0001 = \u0096\u0007;
		}

		// Token: 0x060026E6 RID: 9958 RVA: 0x00086A1C File Offset: 0x00084C1C
		protected virtual _IExpression \u0001(_IExpression \u0002, bool \u0003 = true)
		{
			_IExpression iexpression = this.\u0001.ReplaceExpression(\u0002, \u0003);
			iexpression.Accept(this);
			return iexpression;
		}

		// Token: 0x060026E7 RID: 9959 RVA: 0x00086A34 File Offset: 0x00084C34
		public void \u0001(_IWhileStatement \u0002)
		{
			\u0002._Condition = this.\u0001(\u0002._Condition, true);
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x060026E8 RID: 9960 RVA: 0x00086A58 File Offset: 0x00084C58
		public void \u0001(_IRepeatStatement \u0002)
		{
			\u0002._Condition = this.\u0001(\u0002._Condition, true);
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x060026E9 RID: 9961 RVA: 0x00086A7C File Offset: 0x00084C7C
		public void \u0001(_IForStatement \u0002)
		{
			\u0002._CounterStart = this.\u0001(\u0002._CounterStart, true);
			\u0002._UpperBound = this.\u0001(\u0002._UpperBound, true);
			if (\u0002.By != null)
			{
				\u0002._By = this.\u0001(\u0002._By, true);
			}
			if (\u0002._Condition != null)
			{
				\u0002._Condition = this.\u0001(\u0002._Condition, true);
			}
			if (\u0002._Counter != null)
			{
				\u0002._Counter = this.\u0001(\u0002._Counter, true);
			}
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x060026EA RID: 9962 RVA: 0x00086B0C File Offset: 0x00084D0C
		public virtual void \u0001(_ISequenceStatement \u0002)
		{
			IList<_IStatement> statementList = \u0002._StatementList;
			for (int i = 0; i < statementList.Count; i++)
			{
				statementList[i].Accept(this);
			}
		}

		// Token: 0x060026EB RID: 9963 RVA: 0x00086B40 File Offset: 0x00084D40
		public void \u0001(_IIfStatement \u0002)
		{
			\u0002._Condition = this.\u0001(\u0002._Condition, true);
			\u0002._IfThen.Accept(this);
			if (\u0002._IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
		}

		// Token: 0x060026EC RID: 9964 RVA: 0x00086B78 File Offset: 0x00084D78
		public virtual void \u0001(_IExpressionStatement \u0002)
		{
			\u0002._Expr = this.\u0001(\u0002._Expr, true);
		}

		// Token: 0x060026ED RID: 9965 RVA: 0x00086B90 File Offset: 0x00084D90
		public virtual void \u0001(_IAssignmentExpression \u0002)
		{
			\u0002._LValue = this.\u0001(\u0002._LValue, false);
			\u0002._RValue = this.\u0001(\u0002._RValue, true);
		}

		// Token: 0x060026EE RID: 9966 RVA: 0x00086BB8 File Offset: 0x00084DB8
		public virtual void \u0001(_ICallExpression \u0002)
		{
			\u0002._Callee = this.\u0001(\u0002._Callee, true);
			if (\u0002._Condition != null)
			{
				\u0002._Condition = this.\u0001(\u0002._Condition, true);
			}
			CallParameterEnumerable callParameterEnumerable = \u0002.\u0001();
			int num = 0;
			foreach (AssignmentInfo assignmentInfo in callParameterEnumerable)
			{
				_IExpression expInput = this.\u0001(assignmentInfo.RValue, true);
				\u0002.SetActualParam(expInput, num);
				num++;
			}
			CallOutputParameterEnumerable callOutputParameterEnumerable = \u0002.\u0001();
			num = 0;
			foreach (AssignmentInfo assignmentInfo2 in callOutputParameterEnumerable)
			{
				_IExpression exp = this.\u0001(assignmentInfo2.LValue, true);
				\u0002.SetActualOutput(exp, num);
				num++;
			}
		}

		// Token: 0x060026EF RID: 9967 RVA: 0x00086C78 File Offset: 0x00084E78
		public virtual void \u0001(_IOperatorExpression \u0002)
		{
			for (int i = 0; i < \u0002._OperandsList.Count; i++)
			{
				\u0002[i] = this.\u0001(\u0002[i], true);
			}
		}

		// Token: 0x060026F0 RID: 9968 RVA: 0x00086CB0 File Offset: 0x00084EB0
		public void \u0001(_ICastExpression \u0002)
		{
			\u0002.BaseExpression = this.\u0001(\u0002.BaseExpression, true);
		}

		// Token: 0x060026F1 RID: 9969 RVA: 0x00086CC8 File Offset: 0x00084EC8
		public void \u0001(_IPragmaStatement \u0002)
		{
			_IImplicitCodeSectionPragma iimplicitCodeSectionPragma = \u0002 as _IImplicitCodeSectionPragma;
			if (iimplicitCodeSectionPragma != null)
			{
				this.\u0001.\u0001 = iimplicitCodeSectionPragma.ImplicitOn;
				return;
			}
			_ILocalSignatureIdPragma ilocalSignatureIdPragma = \u0002 as _ILocalSignatureIdPragma;
			if (ilocalSignatureIdPragma != null)
			{
				this.\u0001._Scope.LocalSignature = this.\u0001._Scope[ilocalSignatureIdPragma.LocalSignatureId];
				((global::\u0007.\u0005)this.\u0001._Scope).\u0003();
				return;
			}
			if (\u0002.Text.Contains("implicit"))
			{
				this.\u0001.\u0001 = !\u0002.Text.Contains("implicit off");
			}
		}

		// Token: 0x060026F2 RID: 9970 RVA: 0x00086D70 File Offset: 0x00084F70
		public void \u0001(_INewExpression \u0002)
		{
			\u0002._Count = this.\u0001(\u0002._Count, true);
			if (\u0002._FBInitParams != null)
			{
				for (int i = 0; i < \u0002._FBInitParams.Count; i++)
				{
					\u0002._FBInitParams[i] = (this.\u0001(\u0002._FBInitParams[i] as _IExpression, true) as _IAssignmentExpression);
				}
			}
		}

		// Token: 0x060026F3 RID: 9971 RVA: 0x00086DD8 File Offset: 0x00084FD8
		public void \u0001(_IConversionExpression \u0002)
		{
			\u0002._Exp = this.\u0001(\u0002._Exp, true);
		}

		// Token: 0x060026F4 RID: 9972 RVA: 0x00086DF0 File Offset: 0x00084FF0
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			\u0002._Var = this.\u0001(\u0002._Var, true);
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				\u0002[i] = this.\u0001(\u0002[i], true);
			}
		}

		// Token: 0x060026F5 RID: 9973 RVA: 0x00086E38 File Offset: 0x00085038
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			\u0002._Left = this.\u0001(\u0002._Left, true);
		}

		// Token: 0x060026F6 RID: 9974 RVA: 0x00086E50 File Offset: 0x00085050
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			\u0002._Base = this.\u0001(\u0002._Base, true);
		}

		// Token: 0x060026F7 RID: 9975 RVA: 0x00086E68 File Offset: 0x00085068
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			\u0002._Base = this.\u0001(\u0002._Base, true);
		}

		// Token: 0x060026F8 RID: 9976 RVA: 0x00086E80 File Offset: 0x00085080
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			\u0002._Base = this.\u0001(\u0002._Base, true);
		}

		// Token: 0x060026F9 RID: 9977 RVA: 0x00086E98 File Offset: 0x00085098
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			\u0002._Base = this.\u0001(\u0002._Base, true);
		}

		// Token: 0x060026FA RID: 9978 RVA: 0x00086EB0 File Offset: 0x000850B0
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			\u0002._Base = this.\u0001(\u0002._Base, true);
		}

		// Token: 0x060026FB RID: 9979 RVA: 0x00086EC8 File Offset: 0x000850C8
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			\u0002._Access = this.\u0001(\u0002._Access, true);
		}

		// Token: 0x060026FC RID: 9980 RVA: 0x00086EE0 File Offset: 0x000850E0
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
			\u0002._Base = this.\u0001(\u0002._Base, true);
		}

		// Token: 0x060026FD RID: 9981 RVA: 0x00086EF8 File Offset: 0x000850F8
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			\u0002._Low = this.\u0001(\u0002._Low, true);
			\u0002._High = this.\u0001(\u0002._High, true);
		}

		// Token: 0x060026FE RID: 9982 RVA: 0x00086F20 File Offset: 0x00085120
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			for (int i = 0; i < \u0002._cases.Count; i++)
			{
				\u0002._cases[i] = this.\u0001(\u0002._cases[i], true);
			}
		}

		// Token: 0x060026FF RID: 9983 RVA: 0x00086F64 File Offset: 0x00085164
		public void \u0001(_ICaseStatement \u0002)
		{
			\u0002._Switch = this.\u0001(\u0002._Switch, true);
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
			}
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
			}
		}

		// Token: 0x06002700 RID: 9984 RVA: 0x00086FE8 File Offset: 0x000851E8
		public void \u0001(_IReturnStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition = this.\u0001(\u0002._Condition, true);
			}
		}

		// Token: 0x06002701 RID: 9985 RVA: 0x00087008 File Offset: 0x00085208
		public void \u0001(_IJumpStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition = this.\u0001(\u0002._Condition, true);
			}
		}

		// Token: 0x06002702 RID: 9986 RVA: 0x00087028 File Offset: 0x00085228
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			\u0002._Value = this.\u0001(\u0002._Value, true);
			\u0002._Number = this.\u0001(\u0002._Number, true);
		}

		// Token: 0x06002703 RID: 9987 RVA: 0x00087050 File Offset: 0x00085250
		public void \u0001(_IArrayInitialization \u0002)
		{
			for (int i = 0; i < \u0002._InitValues.Count; i++)
			{
				\u0002._InitValues[i] = this.\u0001(\u0002._InitValues[i], true);
			}
		}

		// Token: 0x06002704 RID: 9988 RVA: 0x00087094 File Offset: 0x00085294
		public void \u0001(_IStructureInitialization \u0002)
		{
			for (int i = 0; i < \u0002._CompoInits.Count; i++)
			{
				\u0002._CompoInits[i] = (this.\u0001(\u0002._CompoInits[i], true) as _IAssignmentExpression);
			}
		}

		// Token: 0x06002705 RID: 9989 RVA: 0x000870DC File Offset: 0x000852DC
		public void \u0001(_ITryCatchStatement \u0002)
		{
			if (\u0002._ReplacedSequence != null)
			{
				\u0002._ReplacedSequence.Accept(this);
				return;
			}
			if (\u0002._Exception != null)
			{
				\u0002._Exception = this.\u0001(\u0002._Exception, true);
			}
			if (\u0002._Try != null)
			{
				\u0002._Try.Accept(this);
			}
			if (\u0002._Catch != null)
			{
				\u0002._Catch.Accept(this);
			}
			if (\u0002._Finally != null)
			{
				\u0002._Finally.Accept(this);
			}
		}

		// Token: 0x06002706 RID: 9990 RVA: 0x00087158 File Offset: 0x00085358
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			\u0002._Left = this.\u0001(\u0002._Left, true);
		}

		// Token: 0x04000716 RID: 1814
		protected readonly IExpressionReplacer \u0001;

		// Token: 0x04000717 RID: 1815
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x04000718 RID: 1816
		private readonly \u0081.\u0010 \u0001;
	}
}
