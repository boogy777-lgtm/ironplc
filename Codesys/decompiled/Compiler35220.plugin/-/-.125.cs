using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification.MessageSuppression;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0002
{
	// Token: 0x02000174 RID: 372
	internal class \u0006 : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2
	{
		// Token: 0x060018FB RID: 6395 RVA: 0x0004DD78 File Offset: 0x0004BF78
		public \u0006(EmptyVisitor351900 \u0080\u0003)
		{
			this.\u0001 = \u0080\u0003;
		}

		// Token: 0x060018FC RID: 6396 RVA: 0x0004DD94 File Offset: 0x0004BF94
		public void \u0001()
		{
			this.\u0001.Clear();
		}

		// Token: 0x060018FD RID: 6397 RVA: 0x0004DDA4 File Offset: 0x0004BFA4
		private void \u0001(_IExprement \u0002)
		{
			this.\u0001.Push(new \u0006.\u0001(\u0002));
		}

		// Token: 0x060018FE RID: 6398 RVA: 0x0004DDB8 File Offset: 0x0004BFB8
		private void \u0002(_IStatement \u0002)
		{
			this.\u0001.Push(new \u0006.\u0001(\u0002));
		}

		// Token: 0x060018FF RID: 6399 RVA: 0x0004DDCC File Offset: 0x0004BFCC
		private void \u0002()
		{
			this.\u0001.Pop();
		}

		// Token: 0x06001900 RID: 6400 RVA: 0x0004DDDC File Offset: 0x0004BFDC
		public static void \u0001(_IExpression \u0002, IScope5 \u0003, _ICompileContext \u0004, _ICompiledPOU \u0005)
		{
			ExpressionTypifierWithSpecialTasks expressionTypifierWithSpecialTasks = new ExpressionTypifierWithSpecialTasks(\u0003, \u0004, false, \u0005);
			expressionTypifierWithSpecialTasks.IgnoreErrors = false;
			expressionTypifierWithSpecialTasks.NoCrossReferences = true;
			expressionTypifierWithSpecialTasks.Optimize = false;
			TypeCheckerVisitor typeCheckerVisitor = new TypeCheckerVisitor(\u0003, \u0004, true);
			typeCheckerVisitor.ConvertAllTypeMismatches = false;
			typeCheckerVisitor.MessageSuppressionController = NoSuppressions.Instance;
			ErrorVisitor ivisit = new ErrorVisitor();
			\u0002.Accept(expressionTypifierWithSpecialTasks);
			\u0002.Accept(typeCheckerVisitor);
			\u0002.Accept(ivisit);
		}

		// Token: 0x06001901 RID: 6401 RVA: 0x0004DE40 File Offset: 0x0004C040
		public void \u0001(_IExpression \u0002)
		{
			this.TopOfStackExpression = \u0002;
		}

		// Token: 0x06001902 RID: 6402 RVA: 0x0004DE4C File Offset: 0x0004C04C
		public void \u0003(_IStatement \u0002)
		{
			this.TopOfStackStatement = \u0002;
		}

		// Token: 0x06001903 RID: 6403 RVA: 0x0004DE58 File Offset: 0x0004C058
		public void \u0003()
		{
			this.\u0001 = true;
		}

		// Token: 0x17000593 RID: 1427
		// (get) Token: 0x06001904 RID: 6404 RVA: 0x0004DE64 File Offset: 0x0004C064
		// (set) Token: 0x06001905 RID: 6405 RVA: 0x0004DE7C File Offset: 0x0004C07C
		private _IExpression TopOfStackExpression
		{
			get
			{
				return this.\u0001.Peek().ChangedExpression as _IExpression;
			}
			set
			{
				this.\u0001.Peek().ChangedExpression = value;
			}
		}

		// Token: 0x17000594 RID: 1428
		// (get) Token: 0x06001906 RID: 6406 RVA: 0x0004DE90 File Offset: 0x0004C090
		// (set) Token: 0x06001907 RID: 6407 RVA: 0x0004DEA8 File Offset: 0x0004C0A8
		private _IStatement TopOfStackStatement
		{
			get
			{
				return this.\u0001.Peek().ChangedExpression as _IStatement;
			}
			set
			{
				this.\u0001.Peek().ChangedExpression = value;
			}
		}

		// Token: 0x06001908 RID: 6408 RVA: 0x0004DEBC File Offset: 0x0004C0BC
		public _IExpression \u0001(_IExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002.Accept(this.\u0001);
			_IExpression result = this.TopOfStackExpression;
			this.\u0002();
			return result;
		}

		// Token: 0x06001909 RID: 6409 RVA: 0x0004DEE0 File Offset: 0x0004C0E0
		public virtual _IAssignmentExpression \u0001(_IAssignmentExpression \u0002)
		{
			this.\u0001(\u0002);
			\u0002.Accept(this.\u0001);
			_IAssignmentExpression result = this.TopOfStackExpression as _IAssignmentExpression;
			this.\u0002();
			return result;
		}

		// Token: 0x0600190A RID: 6410 RVA: 0x0004DF08 File Offset: 0x0004C108
		public _IStatement \u0001(_IStatement \u0002)
		{
			this.\u0002(\u0002);
			\u0002.Accept(this.\u0001);
			_IStatement result = this.TopOfStackStatement;
			this.\u0002();
			return result;
		}

		// Token: 0x0600190B RID: 6411 RVA: 0x0004DF2C File Offset: 0x0004C12C
		public void \u0002(_IExprement \u0002)
		{
			if (!this.\u0001)
			{
				\u0002.Accept(this);
			}
			this.\u0001 = false;
		}

		// Token: 0x0600190C RID: 6412 RVA: 0x0004DF44 File Offset: 0x0004C144
		public virtual void \u0001(_ICompiledPOU \u0002)
		{
		}

		// Token: 0x0600190D RID: 6413 RVA: 0x0004DF48 File Offset: 0x0004C148
		public virtual void \u0001(_IAddressExpression \u0002)
		{
		}

		// Token: 0x0600190E RID: 6414 RVA: 0x0004DF4C File Offset: 0x0004C14C
		public virtual void \u0001(_IVariableExpression \u0002)
		{
		}

		// Token: 0x0600190F RID: 6415 RVA: 0x0004DF50 File Offset: 0x0004C150
		public virtual void \u0001(_IWhileStatement \u0002)
		{
			\u0002._Condition = this.\u0001(\u0002._Condition);
			this.\u0002(\u0002._Condition);
			\u0002._Controlled = this.\u0001(\u0002._Controlled);
			this.\u0002(\u0002._Controlled);
		}

		// Token: 0x06001910 RID: 6416 RVA: 0x0004DF90 File Offset: 0x0004C190
		public virtual void \u0001(_IRepeatStatement \u0002)
		{
			\u0002._Condition = this.\u0001(\u0002._Condition);
			this.\u0002(\u0002._Condition);
			\u0002._Controlled = this.\u0001(\u0002._Controlled);
			this.\u0002(\u0002._Controlled);
		}

		// Token: 0x06001911 RID: 6417 RVA: 0x0004DFD0 File Offset: 0x0004C1D0
		public virtual void \u0001(_IForStatement \u0002)
		{
			\u0002._CounterStart = this.\u0001(\u0002._CounterStart);
			this.\u0002(\u0002._CounterStart);
			\u0002._UpperBound = this.\u0001(\u0002._UpperBound);
			this.\u0002(\u0002._UpperBound);
			if (\u0002.By != null)
			{
				\u0002._By = this.\u0001(\u0002._By);
				this.\u0002(\u0002._By);
			}
			if (\u0002._Condition != null)
			{
				\u0002._Condition = this.\u0001(\u0002._Condition);
				this.\u0002(\u0002._Condition);
			}
			if (\u0002._Counter != null)
			{
				\u0002._Counter = this.\u0001(\u0002._Counter);
				this.\u0002(\u0002._Counter);
			}
			\u0002._Controlled = this.\u0001(\u0002._Controlled);
			this.\u0002(\u0002._Controlled);
		}

		// Token: 0x06001912 RID: 6418 RVA: 0x0004E0AC File Offset: 0x0004C2AC
		public virtual void \u0001(_ISequenceStatement \u0002)
		{
			IList<_IStatement> statementList = \u0002._StatementList;
			for (int i = 0; i < statementList.Count; i++)
			{
				statementList[i] = this.\u0001(statementList[i]);
				this.\u0002(statementList[i]);
			}
		}

		// Token: 0x06001913 RID: 6419 RVA: 0x0004E0F4 File Offset: 0x0004C2F4
		public virtual void \u0001(_IIfStatement \u0002)
		{
			\u0002._Condition = this.\u0001(\u0002._Condition);
			this.\u0002(\u0002._Condition);
			\u0002._IfThen = this.\u0001(\u0002._IfThen);
			this.\u0002(\u0002._IfThen);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				ielseIf._Condition = this.\u0001(ielseIf._Condition);
				this.\u0002(ielseIf._Condition);
				ielseIf._Controlled = this.\u0001(ielseIf._Controlled);
				this.\u0002(ielseIf._Controlled);
			}
			if (\u0002._IfElse != null)
			{
				\u0002._IfElse = this.\u0001(\u0002._IfElse);
				this.\u0002(\u0002._IfElse);
			}
		}

		// Token: 0x06001914 RID: 6420 RVA: 0x0004E1D8 File Offset: 0x0004C3D8
		public virtual void \u0001(_IExpressionStatement \u0002)
		{
			\u0002._Expr = this.\u0001(\u0002._Expr);
			this.\u0002(\u0002._Expr);
		}

		// Token: 0x06001915 RID: 6421 RVA: 0x0004E1F8 File Offset: 0x0004C3F8
		public virtual void \u0001(_IAssignmentExpression \u0002)
		{
			\u0002._LValue = this.\u0001(\u0002._LValue);
			this.\u0002(\u0002._LValue);
			\u0002._RValue = this.\u0001(\u0002._RValue);
			this.\u0002(\u0002._RValue);
		}

		// Token: 0x06001916 RID: 6422 RVA: 0x0004E238 File Offset: 0x0004C438
		public virtual void \u0001(_ICallExpression \u0002)
		{
			\u0002._Callee = this.\u0001(\u0002._Callee);
			this.\u0002(\u0002._Callee);
			if (\u0002._Condition != null)
			{
				\u0002._Condition = this.\u0001(\u0002._Condition);
				this.\u0002(\u0002._Condition);
			}
			for (int i = 0; i < \u0002.ParamExpressions.Count; i++)
			{
				\u0002[i] = this.\u0001(\u0002[i]);
				this.\u0002(\u0002[i]);
			}
		}

		// Token: 0x06001917 RID: 6423 RVA: 0x0004E2C0 File Offset: 0x0004C4C0
		public virtual void \u0001(_IOperatorExpression \u0002)
		{
			for (int i = 0; i < \u0002._OperandsList.Count; i++)
			{
				\u0002[i] = this.\u0001(\u0002[i]);
				this.\u0002(\u0002[i]);
			}
		}

		// Token: 0x06001918 RID: 6424 RVA: 0x0004E304 File Offset: 0x0004C504
		public virtual void \u0001(_ICastExpression \u0002)
		{
			\u0002.BaseExpression = this.\u0001(\u0002.BaseExpression);
			\u0002.BaseExpression.Accept(this);
		}

		// Token: 0x06001919 RID: 6425 RVA: 0x0004E324 File Offset: 0x0004C524
		public virtual void \u0001(_INewExpression \u0002)
		{
			\u0002._Count = this.\u0001(\u0002._Count);
			\u0002._Count.Accept(this);
			if (\u0002._FBInitParams != null)
			{
				for (int i = 0; i < \u0002._FBInitParams.Count; i++)
				{
					\u0002._FBInitParams[i] = this.\u0001(\u0002._FBInitParams[i] as _IAssignmentExpression);
					(\u0002._FBInitParams[i] as _IAssignmentExpression).Accept(this);
				}
			}
		}

		// Token: 0x0600191A RID: 6426 RVA: 0x0004E3A8 File Offset: 0x0004C5A8
		public virtual void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x0600191B RID: 6427 RVA: 0x0004E3AC File Offset: 0x0004C5AC
		public virtual void \u0001(_IConversionExpression \u0002)
		{
			\u0002._Exp = this.\u0001(\u0002._Exp);
			\u0002._Exp.Accept(this);
		}

		// Token: 0x0600191C RID: 6428 RVA: 0x0004E3CC File Offset: 0x0004C5CC
		public virtual void \u0001(_IIndexAccessExpression \u0002)
		{
			\u0002._Var = this.\u0001(\u0002._Var);
			\u0002._Var.Accept(this);
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				\u0002[i] = this.\u0001(\u0002[i]);
				\u0002[i].Accept(this);
			}
		}

		// Token: 0x0600191D RID: 6429 RVA: 0x0004E42C File Offset: 0x0004C62C
		public virtual void \u0001(_ILiteralExpression \u0002)
		{
		}

		// Token: 0x0600191E RID: 6430 RVA: 0x0004E430 File Offset: 0x0004C630
		public virtual void \u0001(_ICompoAccessExpression \u0002)
		{
			\u0002._Right = this.\u0001(\u0002._Right);
			\u0002._Right.Accept(this);
			\u0002._Left = this.\u0001(\u0002._Left);
			\u0002._Left.Accept(this);
		}

		// Token: 0x0600191F RID: 6431 RVA: 0x0004E470 File Offset: 0x0004C670
		public virtual void \u0001(_IDeRefAccessExpression \u0002)
		{
			\u0002._Base = this.\u0001(\u0002._Base);
			\u0002._Base.Accept(this);
		}

		// Token: 0x06001920 RID: 6432 RVA: 0x0004E490 File Offset: 0x0004C690
		public virtual void \u0001(_ICopyScopeExpression \u0002)
		{
			\u0002._Base = this.\u0001(\u0002._Base);
			\u0002._Base.Accept(this);
		}

		// Token: 0x06001921 RID: 6433 RVA: 0x0004E4B0 File Offset: 0x0004C6B0
		public virtual void \u0001(_IGlobalScopeExpression \u0002)
		{
			\u0002._Base = this.\u0001(\u0002._Base);
			\u0002._Base.Accept(this);
		}

		// Token: 0x06001922 RID: 6434 RVA: 0x0004E4D0 File Offset: 0x0004C6D0
		public virtual void \u0001(_ISystemScopeExpression \u0002)
		{
			\u0002._Base = this.\u0001(\u0002._Base);
			\u0002._Base.Accept(this);
		}

		// Token: 0x06001923 RID: 6435 RVA: 0x0004E4F0 File Offset: 0x0004C6F0
		public virtual void \u0001(_IPoolScopeExpression \u0002)
		{
			\u0002._Base = this.\u0001(\u0002._Base);
			\u0002._Base.Accept(this);
		}

		// Token: 0x06001924 RID: 6436 RVA: 0x0004E510 File Offset: 0x0004C710
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			\u0002._Access = this.\u0001(\u0002._Access);
			_IExpression access = \u0002._Access;
			if (access == null)
			{
				return;
			}
			access.Accept(this);
		}

		// Token: 0x06001925 RID: 6437 RVA: 0x0004E538 File Offset: 0x0004C738
		public virtual void \u0001(_ICurrentTaskExpression \u0002)
		{
			\u0002._Base = this.\u0001(\u0002._Base);
			\u0002._Base.Accept(this);
		}

		// Token: 0x06001926 RID: 6438 RVA: 0x0004E558 File Offset: 0x0004C758
		public virtual void \u0001(_ICaseRangeExpression \u0002)
		{
			\u0002._Low = this.\u0001(\u0002._Low);
			\u0002._Low.Accept(this);
			\u0002._High = this.\u0001(\u0002._High);
			\u0002._High.Accept(this);
		}

		// Token: 0x06001927 RID: 6439 RVA: 0x0004E598 File Offset: 0x0004C798
		public virtual void \u0001(_ICaseLabelStatement \u0002)
		{
			for (int i = 0; i < \u0002._cases.Count; i++)
			{
				\u0002._cases[i] = this.\u0001(\u0002._cases[i]);
				\u0002._cases[i].Accept(this);
			}
		}

		// Token: 0x06001928 RID: 6440 RVA: 0x0004E5EC File Offset: 0x0004C7EC
		public virtual void \u0001(_ICaseStatement \u0002)
		{
			\u0002._Switch = this.\u0001(\u0002._Switch);
			\u0002._Switch.Accept(this);
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
			}
			if (\u0002._Else != null)
			{
				\u0002._Else = this.\u0001(\u0002._Else);
				\u0002._Else.Accept(this);
			}
		}

		// Token: 0x06001929 RID: 6441 RVA: 0x0004E68C File Offset: 0x0004C88C
		public virtual void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x0600192A RID: 6442 RVA: 0x0004E690 File Offset: 0x0004C890
		public virtual void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x0600192B RID: 6443 RVA: 0x0004E694 File Offset: 0x0004C894
		public virtual void \u0001(_IThisExpression \u0002)
		{
		}

		// Token: 0x0600192C RID: 6444 RVA: 0x0004E698 File Offset: 0x0004C898
		public virtual void \u0001(_IBaseExpression \u0002)
		{
		}

		// Token: 0x0600192D RID: 6445 RVA: 0x0004E69C File Offset: 0x0004C89C
		public virtual void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x0600192E RID: 6446 RVA: 0x0004E6A0 File Offset: 0x0004C8A0
		public virtual void \u0001(_IReturnStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition = this.\u0001(\u0002._Condition);
				\u0002._Condition.Accept(this);
			}
		}

		// Token: 0x0600192F RID: 6447 RVA: 0x0004E6C8 File Offset: 0x0004C8C8
		public virtual void \u0001(_IJumpStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition = this.\u0001(\u0002._Condition);
				\u0002._Condition.Accept(this);
			}
		}

		// Token: 0x06001930 RID: 6448 RVA: 0x0004E6F0 File Offset: 0x0004C8F0
		public virtual void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x06001931 RID: 6449 RVA: 0x0004E6F4 File Offset: 0x0004C8F4
		public virtual void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x06001932 RID: 6450 RVA: 0x0004E6F8 File Offset: 0x0004C8F8
		public virtual void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x06001933 RID: 6451 RVA: 0x0004E6FC File Offset: 0x0004C8FC
		public virtual void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x06001934 RID: 6452 RVA: 0x0004E700 File Offset: 0x0004C900
		public virtual void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x06001935 RID: 6453 RVA: 0x0004E704 File Offset: 0x0004C904
		public virtual void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x06001936 RID: 6454 RVA: 0x0004E708 File Offset: 0x0004C908
		public virtual void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x06001937 RID: 6455 RVA: 0x0004E70C File Offset: 0x0004C90C
		public virtual void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x06001938 RID: 6456 RVA: 0x0004E710 File Offset: 0x0004C910
		public virtual void \u0001(_IVariableDeclarationStatement \u0002)
		{
			for (int i = 0; i < \u0002.NameList.Count; i++)
			{
				\u0002.NameList[i] = this.\u0001(\u0002.NameList[i]);
				\u0002.NameList[i].Accept(this);
			}
			if (\u0002.Initial != null)
			{
				\u0002.Initial = this.\u0001(\u0002.Initial);
				\u0002.Initial.Accept(this);
			}
			if (\u0002.InputAssigns != null)
			{
				int num = 0;
				foreach (_IAssignmentExpression iassignmentExpression in \u0002.InputAssigns)
				{
					iassignmentExpression.Accept(this);
					\u0002.SetFormalParam(iassignmentExpression._LValue, num);
					\u0002.SetActualParam(iassignmentExpression._RValue, num);
					num++;
				}
			}
		}

		// Token: 0x06001939 RID: 6457 RVA: 0x0004E7F4 File Offset: 0x0004C9F4
		public virtual void \u0001(_IVariableDeclarationListStatement \u0002)
		{
			\u0002.VariableDeclaration = this.\u0001(\u0002.VariableDeclaration);
			\u0002.VariableDeclaration.Accept(this);
		}

		// Token: 0x0600193A RID: 6458 RVA: 0x0004E814 File Offset: 0x0004CA14
		public virtual void \u0001(_IPOUDeclarationStatement \u0002)
		{
			\u0002.Declarations = this.\u0001(\u0002.Declarations);
			\u0002.Declarations.Accept(this);
			if (\u0002.Implements != null)
			{
				for (int i = 0; i < \u0002.Implements.Count; i++)
				{
					\u0002.Implements[i] = this.\u0001(\u0002.Implements[i]);
					\u0002.Implements[i].Accept(this);
				}
			}
			if (\u0002.Extends != null)
			{
				for (int j = 0; j < \u0002.Extends.Count; j++)
				{
					\u0002.Extends[j] = this.\u0001(\u0002.Extends[j]);
					\u0002.Extends[j].Accept(this);
				}
			}
		}

		// Token: 0x0600193B RID: 6459 RVA: 0x0004E8DC File Offset: 0x0004CADC
		public virtual void \u0001(_ITypeDeclarationStatement \u0002)
		{
			\u0002.Declarations = this.\u0001(\u0002.Declarations);
			\u0002.Declarations.Accept(this);
			if (\u0002.Initial != null)
			{
				\u0002.Initial = this.\u0001(\u0002.Initial);
				\u0002.Initial.Accept(this);
			}
			if (\u0002.Extends != null)
			{
				\u0002.Extends = this.\u0001(\u0002.Extends);
				\u0002.Extends.Accept(this);
			}
		}

		// Token: 0x0600193C RID: 6460 RVA: 0x0004E954 File Offset: 0x0004CB54
		public virtual void \u0001(_IEnumDeclarationStatement \u0002)
		{
			if (\u0002._Value != null)
			{
				\u0002._Value = this.\u0001(\u0002._Value);
				\u0002._Value.Accept(this);
			}
		}

		// Token: 0x0600193D RID: 6461 RVA: 0x0004E97C File Offset: 0x0004CB7C
		public virtual void \u0001(_IEnumDeclarationListStatement \u0002)
		{
			foreach (_IEnumDeclarationStatement ienumDeclarationStatement in \u0002.Enums)
			{
				ienumDeclarationStatement.Accept(this);
			}
		}

		// Token: 0x0600193E RID: 6462 RVA: 0x0004E9C8 File Offset: 0x0004CBC8
		public virtual void \u0001(_IMultipleIndexInitialization \u0002)
		{
			\u0002._Value = this.\u0001(\u0002._Value);
			\u0002._Value.Accept(this);
			\u0002._Number = this.\u0001(\u0002._Number);
			\u0002._Number.Accept(this);
		}

		// Token: 0x0600193F RID: 6463 RVA: 0x0004EA08 File Offset: 0x0004CC08
		public virtual void \u0001(_IArrayInitialization \u0002)
		{
			for (int i = 0; i < \u0002._InitValues.Count; i++)
			{
				\u0002._InitValues[i] = this.\u0001(\u0002._InitValues[i]);
				\u0002._InitValues[i].Accept(this);
			}
		}

		// Token: 0x06001940 RID: 6464 RVA: 0x0004EA5C File Offset: 0x0004CC5C
		public virtual void \u0001(_IStructureInitialization \u0002)
		{
			for (int i = 0; i < \u0002._CompoInits.Count; i++)
			{
				\u0002._CompoInits[i] = this.\u0001(\u0002._CompoInits[i]);
				this.\u0002(\u0002._CompoInits[i]);
			}
		}

		// Token: 0x06001941 RID: 6465 RVA: 0x0004EAB0 File Offset: 0x0004CCB0
		public virtual void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x06001942 RID: 6466 RVA: 0x0004EAB4 File Offset: 0x0004CCB4
		public virtual void \u0001(_IVariableReference \u0002)
		{
			if (\u0002.InstancePath != null)
			{
				\u0002.InstancePath = this.\u0001(\u0002.InstancePath);
				\u0002.InstancePath.Accept(this);
			}
		}

		// Token: 0x06001943 RID: 6467 RVA: 0x0004EADC File Offset: 0x0004CCDC
		public virtual void \u0001(_ITypeReference \u0002)
		{
			if (\u0002.InstancePath != null)
			{
				\u0002.InstancePath = this.\u0001(\u0002.InstancePath);
				\u0002.InstancePath.Accept(this);
			}
		}

		// Token: 0x06001944 RID: 6468 RVA: 0x0004EB04 File Offset: 0x0004CD04
		public virtual void \u0001(_IPouReference \u0002)
		{
			if (\u0002.InstancePath != null)
			{
				\u0002.InstancePath = this.\u0001(\u0002.InstancePath);
				\u0002.InstancePath.Accept(this);
			}
		}

		// Token: 0x06001945 RID: 6469 RVA: 0x0004EB2C File Offset: 0x0004CD2C
		public virtual void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x06001946 RID: 6470 RVA: 0x0004EB30 File Offset: 0x0004CD30
		public virtual void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x06001947 RID: 6471 RVA: 0x0004EB34 File Offset: 0x0004CD34
		public virtual void \u0001(_IDefinedExpression \u0002)
		{
			\u0002.ItemReference = this.\u0001(\u0002.ItemReference);
			\u0002.ItemReference.Accept(this);
		}

		// Token: 0x06001948 RID: 6472 RVA: 0x0004EB54 File Offset: 0x0004CD54
		public virtual void \u0001(_IPragmaOperatorExpression \u0002)
		{
			for (int i = 0; i < \u0002.Operands.Count; i++)
			{
				\u0002.Operands[i] = this.\u0001(\u0002.Operands[i]);
				\u0002.Operands[i].Accept(this);
			}
			this.\u0001.visit(\u0002);
		}

		// Token: 0x06001949 RID: 6473 RVA: 0x0004EBB4 File Offset: 0x0004CDB4
		public virtual void \u0001(_IPragmaIfStatement \u0002)
		{
			\u0002.Condition = this.\u0001(\u0002.Condition);
			\u0002.Condition.Accept(this);
			\u0002.IfThen = this.\u0001(\u0002.IfThen);
			\u0002.IfThen.Accept(this);
			for (int i = 0; i < \u0002.ElseIf.Count; i++)
			{
				\u0002.ElseIf[i].Condition = this.\u0001(\u0002.ElseIf[i].Condition);
				\u0002.ElseIf[i].Condition.Accept(this);
				\u0002.ElseIf[i].Controlled = this.\u0001(\u0002.ElseIf[i].Controlled);
				\u0002.ElseIf[i].Controlled.Accept(this);
			}
			if (\u0002.IfElse != null)
			{
				\u0002.IfElse = this.\u0001(\u0002.IfElse);
				\u0002.IfElse.Accept(this);
			}
		}

		// Token: 0x0600194A RID: 6474 RVA: 0x0004ECC0 File Offset: 0x0004CEC0
		public virtual void \u0001(_IBreakPointStatement \u0002)
		{
			this.\u0001.visit(\u0002);
		}

		// Token: 0x0600194B RID: 6475 RVA: 0x0004ECD0 File Offset: 0x0004CED0
		public virtual void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x0600194C RID: 6476 RVA: 0x0004ECD4 File Offset: 0x0004CED4
		public virtual void \u0001(_IXRefExpression \u0002)
		{
			\u0002.XRef = this.\u0001(\u0002.XRef);
			\u0002.XRef.Accept(this);
			\u0002.XRefFrom = this.\u0001(\u0002.XRefFrom);
			\u0002.XRefFrom.Accept(this);
		}

		// Token: 0x0600194D RID: 6477 RVA: 0x0004ED14 File Offset: 0x0004CF14
		public virtual void \u0001(_IHasTypeExpression \u0002)
		{
			\u0002.Variable = this.\u0001(\u0002.Variable);
			\u0002.Variable.Accept(this);
		}

		// Token: 0x0600194E RID: 6478 RVA: 0x0004ED34 File Offset: 0x0004CF34
		public virtual void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x0600194F RID: 6479 RVA: 0x0004ED38 File Offset: 0x0004CF38
		public virtual void \u0001(_IHasAttributeExpression \u0002)
		{
			\u0002.ItemReference = this.\u0001(\u0002.ItemReference);
			\u0002.ItemReference.Accept(this);
		}

		// Token: 0x06001950 RID: 6480 RVA: 0x0004ED58 File Offset: 0x0004CF58
		public virtual void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x06001951 RID: 6481 RVA: 0x0004ED5C File Offset: 0x0004CF5C
		public virtual void \u0001(_IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x06001952 RID: 6482 RVA: 0x0004ED60 File Offset: 0x0004CF60
		public virtual void \u0001(_IHasConstantTypeExpression \u0002)
		{
		}

		// Token: 0x06001953 RID: 6483 RVA: 0x0004ED64 File Offset: 0x0004CF64
		public virtual void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x06001954 RID: 6484 RVA: 0x0004ED68 File Offset: 0x0004CF68
		public virtual void \u0001(_IPragmaAssertion \u0002)
		{
			\u0002.Condition.Accept(this);
		}

		// Token: 0x06001955 RID: 6485 RVA: 0x0004ED78 File Offset: 0x0004CF78
		public virtual void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x06001956 RID: 6486 RVA: 0x0004ED7C File Offset: 0x0004CF7C
		public void \u0001(_ITryCatchStatement \u0002)
		{
			\u0002.DefaultTraverse(this);
		}

		// Token: 0x06001957 RID: 6487 RVA: 0x0004ED88 File Offset: 0x0004CF88
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			\u0002._Left = this.\u0001(\u0002._Left);
			\u0002._Left.Accept(this);
		}

		// Token: 0x06001958 RID: 6488 RVA: 0x0004EDA8 File Offset: 0x0004CFA8
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
		}

		// Token: 0x0400046D RID: 1133
		protected EmptyVisitor351900 \u0001;

		// Token: 0x0400046E RID: 1134
		private readonly LStack<\u0006.\u0001> \u0001 = new LStack<\u0006.\u0001>();

		// Token: 0x0400046F RID: 1135
		private bool \u0001;

		// Token: 0x02000175 RID: 373
		private sealed class \u0001
		{
			// Token: 0x06001959 RID: 6489 RVA: 0x0004EDAC File Offset: 0x0004CFAC
			public \u0001(_IExprement \u0007\u0005)
			{
				this.ChangedExpression = \u0007\u0005;
			}

			// Token: 0x17000595 RID: 1429
			// (get) Token: 0x0600195A RID: 6490 RVA: 0x0004EDBC File Offset: 0x0004CFBC
			// (set) Token: 0x0600195B RID: 6491 RVA: 0x0004EDC4 File Offset: 0x0004CFC4
			public _IExprement ChangedExpression { get; set; }

			// Token: 0x04000470 RID: 1136
			[CompilerGenerated]
			private _IExprement \u0001;
		}
	}
}
