using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using \u001B;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0016
{
	// Token: 0x02000129 RID: 297
	internal sealed class \u0005 : IExprVisitor6, IExprVisitor5, IExprVisitor4, IExprVisitor3, IExprVisitor2, IExprVisitor
	{
		// Token: 0x06001528 RID: 5416 RVA: 0x0003DC24 File Offset: 0x0003BE24
		internal \u0005(ISequenceStatement2 \u0080\u0008, string \u0081\u0008, string \u0082\u0008)
		{
			this.\u0001 = \u0081\u0008;
			this.\u0002 = \u0082\u0008;
			this.\u0001 = \u0080\u0008;
			this.\u0001();
		}

		// Token: 0x17000509 RID: 1289
		// (get) Token: 0x06001529 RID: 5417 RVA: 0x0003DC74 File Offset: 0x0003BE74
		// (set) Token: 0x0600152A RID: 5418 RVA: 0x0003DC7C File Offset: 0x0003BE7C
		internal IExpression ExplicitTransitionAssignment { get; private set; }

		// Token: 0x1700050A RID: 1290
		// (get) Token: 0x0600152B RID: 5419 RVA: 0x0003DC88 File Offset: 0x0003BE88
		// (set) Token: 0x0600152C RID: 5420 RVA: 0x0003DC90 File Offset: 0x0003BE90
		private IExpression ExplicitAndUnconditionalTransitionAssignment { get; set; }

		// Token: 0x1700050B RID: 1291
		// (get) Token: 0x0600152D RID: 5421 RVA: 0x0003DC9C File Offset: 0x0003BE9C
		// (set) Token: 0x0600152E RID: 5422 RVA: 0x0003DCA4 File Offset: 0x0003BEA4
		private bool HasErrorStatement { get; set; }

		// Token: 0x1700050C RID: 1292
		// (get) Token: 0x0600152F RID: 5423 RVA: 0x0003DCB0 File Offset: 0x0003BEB0
		internal IExpression SingleExpression
		{
			get
			{
				if (this.\u0001.Count == 1 && (this.\u0001.Count == 0 || (this.\u0001.Count == 1 && this.\u0001[0] is IExpressionStatement)))
				{
					return this.\u0001[0];
				}
				return null;
			}
		}

		// Token: 0x1700050D RID: 1293
		// (get) Token: 0x06001530 RID: 5424 RVA: 0x0003DD08 File Offset: 0x0003BF08
		private IExpression ExpressionForAnalyzation
		{
			get
			{
				if (this.\u0001.Count == 1)
				{
					IExpressionStatement expressionStatement = this.\u0001[0] as IExpressionStatement;
					if (expressionStatement != null)
					{
						IAssignmentExpression assignmentExpression = expressionStatement.Expr as IAssignmentExpression;
						if (assignmentExpression == null)
						{
							return expressionStatement.Expr;
						}
						if (assignmentExpression.LValue.ToString().Equals(this.\u0002, StringComparison.OrdinalIgnoreCase))
						{
							return assignmentExpression.RValue;
						}
						goto IL_77;
					}
				}
				if (this.SingleExpression != null)
				{
					return this.SingleExpression;
				}
				if (this.ExplicitAndUnconditionalTransitionAssignment != null)
				{
					return this.ExplicitAndUnconditionalTransitionAssignment;
				}
				IL_77:
				return null;
			}
		}

		// Token: 0x1700050E RID: 1294
		// (get) Token: 0x06001531 RID: 5425 RVA: 0x0003DD90 File Offset: 0x0003BF90
		internal string StringForAnalyzation
		{
			get
			{
				IExpression expression = this.ExpressionForAnalyzation;
				if (expression == null)
				{
					return null;
				}
				if (expression is IErrorExpression)
				{
					return null;
				}
				\u0002 u = new \u0002();
				expression.AcceptVisitor(u);
				if (u.HasImplicitVariable || u.HasErrorExpression)
				{
					return null;
				}
				IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(expression.ToString(), false, false, false, false);
				IExpression expression2 = (APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner) as IParser2).ParseExpression();
				if (expression2 == null)
				{
					return null;
				}
				return expression2.ToString();
			}
		}

		// Token: 0x1700050F RID: 1295
		// (get) Token: 0x06001532 RID: 5426 RVA: 0x0003DE14 File Offset: 0x0003C014
		internal IStatement[] TopLevelStatements
		{
			get
			{
				return this.\u0001.ToArray();
			}
		}

		// Token: 0x17000510 RID: 1296
		// (get) Token: 0x06001533 RID: 5427 RVA: 0x0003DE24 File Offset: 0x0003C024
		// (set) Token: 0x06001534 RID: 5428 RVA: 0x0003DE2C File Offset: 0x0003C02C
		internal ISequenceStatement OwningSequenceStatement { get; private set; }

		// Token: 0x17000511 RID: 1297
		// (get) Token: 0x06001535 RID: 5429 RVA: 0x0003DE38 File Offset: 0x0003C038
		// (set) Token: 0x06001536 RID: 5430 RVA: 0x0003DE40 File Offset: 0x0003C040
		internal int StatementPosition { get; private set; }

		// Token: 0x06001537 RID: 5431 RVA: 0x0003DE4C File Offset: 0x0003C04C
		private void \u0001(IStatement \u0002)
		{
			if (this.TopOfStack.TopLevelStatement)
			{
				this.\u0001.Add(\u0002);
			}
		}

		// Token: 0x06001538 RID: 5432 RVA: 0x0003DE68 File Offset: 0x0003C068
		private void \u0003(IExpression \u0002)
		{
			if (this.TopOfStack.TopLevelExpression)
			{
				this.\u0001.Add(\u0002);
				this.OwningSequenceStatement = this.TopOfStack.OwningSequenceStatement;
				this.StatementPosition = this.TopOfStack.StatementPosition;
			}
		}

		// Token: 0x06001539 RID: 5433 RVA: 0x0003DEA8 File Offset: 0x0003C0A8
		public void \u0001(IThisExpression \u0002)
		{
			this.\u0003(\u0002);
		}

		// Token: 0x0600153A RID: 5434 RVA: 0x0003DEB4 File Offset: 0x0003C0B4
		public void \u0001(IBaseExpression \u0002)
		{
			this.\u0003(\u0002);
		}

		// Token: 0x0600153B RID: 5435 RVA: 0x0003DEC0 File Offset: 0x0003C0C0
		public void \u0001(ILiteralExpression \u0002)
		{
			this.\u0003(\u0002);
		}

		// Token: 0x0600153C RID: 5436 RVA: 0x0003DECC File Offset: 0x0003C0CC
		public void \u0001(IAddressExpression \u0002)
		{
			this.\u0003(\u0002);
		}

		// Token: 0x0600153D RID: 5437 RVA: 0x0003DED8 File Offset: 0x0003C0D8
		public void \u0001(IVariableExpression \u0002)
		{
			this.\u0003(\u0002);
		}

		// Token: 0x0600153E RID: 5438 RVA: 0x0003DEE4 File Offset: 0x0003C0E4
		public void \u0001(INamespaceAccessExpression \u0002)
		{
			this.\u0003(\u0002);
		}

		// Token: 0x0600153F RID: 5439 RVA: 0x0003DEF0 File Offset: 0x0003C0F0
		public void \u0001(IEmptyStatement \u0002)
		{
		}

		// Token: 0x06001540 RID: 5440 RVA: 0x0003DEF4 File Offset: 0x0003C0F4
		public void \u0001(ICommentStatement \u0002)
		{
		}

		// Token: 0x06001541 RID: 5441 RVA: 0x0003DEF8 File Offset: 0x0003C0F8
		public void \u0001(IPragmaStatement \u0002)
		{
		}

		// Token: 0x06001542 RID: 5442 RVA: 0x0003DEFC File Offset: 0x0003C0FC
		public void \u0001(IExitStatement \u0002)
		{
		}

		// Token: 0x06001543 RID: 5443 RVA: 0x0003DF00 File Offset: 0x0003C100
		public void \u0001(IContinueStatement \u0002)
		{
		}

		// Token: 0x06001544 RID: 5444 RVA: 0x0003DF04 File Offset: 0x0003C104
		public void \u0001(ILabelStatement \u0002)
		{
		}

		// Token: 0x06001545 RID: 5445 RVA: 0x0003DF08 File Offset: 0x0003C108
		public void \u0001(IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06001546 RID: 5446 RVA: 0x0003DF0C File Offset: 0x0003C10C
		public void \u0001(IPragmaIfStatement \u0002)
		{
		}

		// Token: 0x06001547 RID: 5447 RVA: 0x0003DF10 File Offset: 0x0003C110
		public void \u0001(IDefineStatement \u0002)
		{
		}

		// Token: 0x06001548 RID: 5448 RVA: 0x0003DF14 File Offset: 0x0003C114
		public void \u0001(IErrorExpression \u0002)
		{
		}

		// Token: 0x06001549 RID: 5449 RVA: 0x0003DF18 File Offset: 0x0003C118
		public void \u0001(ICastExpression \u0002)
		{
		}

		// Token: 0x0600154A RID: 5450 RVA: 0x0003DF1C File Offset: 0x0003C11C
		public void \u0001(ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x0600154B RID: 5451 RVA: 0x0003DF20 File Offset: 0x0003C120
		public void \u0001(IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x0600154C RID: 5452 RVA: 0x0003DF24 File Offset: 0x0003C124
		public void \u0001(IStructureInitialization \u0002)
		{
		}

		// Token: 0x0600154D RID: 5453 RVA: 0x0003DF28 File Offset: 0x0003C128
		public void \u0001(IArrayInitialization \u0002)
		{
		}

		// Token: 0x0600154E RID: 5454 RVA: 0x0003DF2C File Offset: 0x0003C12C
		public void \u0001(IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x0600154F RID: 5455 RVA: 0x0003DF30 File Offset: 0x0003C130
		public void \u0001(IDefineReference \u0002)
		{
		}

		// Token: 0x06001550 RID: 5456 RVA: 0x0003DF34 File Offset: 0x0003C134
		public void \u0001(IVariableReference \u0002)
		{
		}

		// Token: 0x06001551 RID: 5457 RVA: 0x0003DF38 File Offset: 0x0003C138
		public void \u0001(ITypeReference \u0002)
		{
		}

		// Token: 0x06001552 RID: 5458 RVA: 0x0003DF3C File Offset: 0x0003C13C
		public void \u0001(IPouReference \u0002)
		{
		}

		// Token: 0x06001553 RID: 5459 RVA: 0x0003DF40 File Offset: 0x0003C140
		public void \u0001(IDefinedExpression \u0002)
		{
		}

		// Token: 0x06001554 RID: 5460 RVA: 0x0003DF44 File Offset: 0x0003C144
		public void \u0001(IPragmaOperatorExpression \u0002)
		{
		}

		// Token: 0x06001555 RID: 5461 RVA: 0x0003DF48 File Offset: 0x0003C148
		public void \u0001(IHasTypeExpression \u0002)
		{
		}

		// Token: 0x06001556 RID: 5462 RVA: 0x0003DF4C File Offset: 0x0003C14C
		public void \u0001(IHasAttributeExpression \u0002)
		{
		}

		// Token: 0x06001557 RID: 5463 RVA: 0x0003DF50 File Offset: 0x0003C150
		public void \u0001(IHasValueExpression \u0002)
		{
		}

		// Token: 0x06001558 RID: 5464 RVA: 0x0003DF54 File Offset: 0x0003C154
		public void \u0001(IPragmaAssertion \u0002)
		{
		}

		// Token: 0x06001559 RID: 5465 RVA: 0x0003DF58 File Offset: 0x0003C158
		public void \u0002(ISequenceStatement \u0002)
		{
			this.TopOfStack.OwningSequenceStatement = \u0002;
			int num = 0;
			foreach (IStatement exprement in \u0002.Statements)
			{
				this.TopOfStack.StatementPosition = num;
				num++;
				this.\u0002(this.TopOfStack.TopLevelStatement);
				exprement.AcceptVisitor(this);
				this.\u0005();
			}
		}

		// Token: 0x0600155A RID: 5466 RVA: 0x0003DFB8 File Offset: 0x0003C1B8
		public void \u0001(IWhileStatement \u0002)
		{
			this.\u0001(\u0002);
			this.\u0003();
			\u0002.Controlled.AcceptVisitor(this);
			this.\u0005();
			this.\u0004();
			\u0002.Condition.AcceptVisitor(this);
			this.\u0005();
		}

		// Token: 0x0600155B RID: 5467 RVA: 0x0003DFF4 File Offset: 0x0003C1F4
		public void \u0001(IRepeatStatement \u0002)
		{
			this.\u0001(\u0002);
			this.\u0003();
			\u0002.Controlled.AcceptVisitor(this);
			this.\u0005();
			this.\u0004();
			\u0002.Condition.AcceptVisitor(this);
			this.\u0005();
		}

		// Token: 0x0600155C RID: 5468 RVA: 0x0003E030 File Offset: 0x0003C230
		public void \u0001(ICaseLabelStatement \u0002)
		{
			this.\u0001(\u0002);
			foreach (IExpression exprement in \u0002.cases)
			{
				this.\u0004();
				exprement.AcceptVisitor(this);
				this.\u0005();
			}
		}

		// Token: 0x0600155D RID: 5469 RVA: 0x0003E070 File Offset: 0x0003C270
		public void \u0001(ICaseStatement \u0002)
		{
			this.\u0001(\u0002);
			this.\u0004();
			\u0002.Switch.AcceptVisitor(this);
			this.\u0005();
			foreach (ICase @case in \u0002.Cases)
			{
				this.\u0003();
				@case.Label.AcceptVisitor(this);
				@case.Controlled.AcceptVisitor(this);
				this.\u0005();
			}
			if (\u0002.Else != null)
			{
				this.\u0002();
				\u0002.Else.AcceptVisitor(this);
				this.\u0005();
			}
		}

		// Token: 0x0600155E RID: 5470 RVA: 0x0003E0F8 File Offset: 0x0003C2F8
		public void \u0001(IForStatement \u0002)
		{
			this.\u0001(\u0002);
			this.\u0004();
			\u0002.CounterStart.AcceptVisitor(this);
			\u0002.UpperBound.AcceptVisitor(this);
			this.\u0005();
			if (\u0002.By != null)
			{
				this.\u0004();
				\u0002.By.AcceptVisitor(this);
				this.\u0005();
			}
			this.\u0003();
			\u0002.Controlled.AcceptVisitor(this);
			this.\u0005();
		}

		// Token: 0x0600155F RID: 5471 RVA: 0x0003E168 File Offset: 0x0003C368
		public void \u0001(IIfStatement \u0002)
		{
			this.\u0001(\u0002);
			this.\u0003();
			\u0002.IfThen.AcceptVisitor(this);
			this.\u0005();
			if (\u0002.IfElse != null)
			{
				this.\u0003();
				\u0002.IfElse.AcceptVisitor(this);
				this.\u0005();
			}
			if (\u0002.ElseIf != null)
			{
				foreach (IElseIf elseIf2 in \u0002.ElseIf)
				{
					this.\u0003();
					elseIf2.Controlled.AcceptVisitor(this);
					this.\u0005();
				}
			}
		}

		// Token: 0x06001560 RID: 5472 RVA: 0x0003E1EC File Offset: 0x0003C3EC
		public void \u0001(IReturnStatement \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.Condition != null)
			{
				this.\u0004();
				\u0002.Condition.AcceptVisitor(this);
				this.\u0005();
			}
		}

		// Token: 0x06001561 RID: 5473 RVA: 0x0003E218 File Offset: 0x0003C418
		public void \u0001(IJumpStatement \u0002)
		{
			this.\u0001(\u0002);
			if (\u0002.Condition != null)
			{
				this.\u0004();
				\u0002.Condition.AcceptVisitor(this);
				this.\u0005();
			}
		}

		// Token: 0x06001562 RID: 5474 RVA: 0x0003E244 File Offset: 0x0003C444
		public void \u0001(IExpressionStatement \u0002)
		{
			this.\u0001(\u0002);
			this.\u0003(this.TopOfStack.TopLevelStatement);
			\u0002.Expr.AcceptVisitor(this);
			this.\u0005();
		}

		// Token: 0x06001563 RID: 5475 RVA: 0x0003E270 File Offset: 0x0003C470
		public void \u0001(IErrorStatement \u0002)
		{
			if (this.SingleExpression != null || this.HasErrorStatement)
			{
				return;
			}
			if (this.\u0001 == null && this.\u0001 != null)
			{
				this.\u0001 = this.\u0001.ToString();
				this.\u0001 = null;
			}
			IToken token = null;
			TokenType tokenType = TokenType.None;
			string stText = string.Empty;
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(this.\u0001, false, false, false, false);
			scanner.AllowMultipleUnderlines = true;
			IToken token2;
			while (scanner.GetNext(out token2) != TokenType.End)
			{
				if (token2.Position == \u0002.Position.Position)
				{
					stText = this.\u0001.Substring(token2.SourceOffset, token2.Length);
					tokenType = scanner.GetNext(out token);
					break;
				}
			}
			if ((tokenType == TokenType.Operator && token != null && scanner.GetOperator(token) == Operator.Semicolon) || tokenType == TokenType.End)
			{
				IScanner scanner2 = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(stText, false, false, false, false);
				IExpression expression = (APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner2) as IParser2).ParseExpression();
				if (expression != null)
				{
					this.\u0001.Add(expression);
					return;
				}
			}
			this.HasErrorStatement = true;
		}

		// Token: 0x06001564 RID: 5476 RVA: 0x0003E38C File Offset: 0x0003C58C
		public void \u0001(IAssignmentExpression \u0002)
		{
			this.\u0003(\u0002);
			if (\u0002.LValue.ToString().Equals(this.\u0002, StringComparison.OrdinalIgnoreCase))
			{
				this.ExplicitTransitionAssignment = \u0002.RValue;
				if (!this.TopOfStack.InConditionalStatement)
				{
					this.ExplicitAndUnconditionalTransitionAssignment = \u0002.RValue;
				}
			}
			this.\u0004();
			\u0002.LValue.AcceptVisitor(this);
			\u0002.RValue.AcceptVisitor(this);
			this.\u0005();
		}

		// Token: 0x06001565 RID: 5477 RVA: 0x0003E404 File Offset: 0x0003C604
		public void \u0001(ICallExpression \u0002)
		{
			this.\u0003(\u0002);
			if (\u0002.Condition != null)
			{
				this.\u0004();
				\u0002.Condition.AcceptVisitor(this);
				this.\u0005();
			}
			this.\u0004();
			\u0002.Callee.AcceptVisitor(this);
			this.\u0005();
			foreach (IAssignmentExpression exprement in \u0002.InputAssigns)
			{
				this.\u0004();
				exprement.AcceptVisitor(this);
				this.\u0005();
			}
			foreach (IAssignmentExpression exprement2 in \u0002.OutputAssigns)
			{
				this.\u0004();
				exprement2.AcceptVisitor(this);
				this.\u0005();
			}
		}

		// Token: 0x06001566 RID: 5478 RVA: 0x0003E4A4 File Offset: 0x0003C6A4
		public void \u0001(IOperatorExpression \u0002)
		{
			this.\u0003(\u0002);
			foreach (IExpression exprement in \u0002.Operands)
			{
				this.\u0004();
				exprement.AcceptVisitor(this);
				this.\u0005();
			}
		}

		// Token: 0x06001567 RID: 5479 RVA: 0x0003E4E4 File Offset: 0x0003C6E4
		public void \u0001(IConversionExpression \u0002)
		{
			this.\u0003(\u0002);
			this.\u0004();
			\u0002.Exp.AcceptVisitor(this);
			this.\u0005();
		}

		// Token: 0x06001568 RID: 5480 RVA: 0x0003E508 File Offset: 0x0003C708
		public void \u0001(IIndexAccessExpression \u0002)
		{
			this.\u0003(\u0002);
			this.\u0004();
			\u0002.Var.AcceptVisitor(this);
			this.\u0005();
			foreach (IExpression exprement in \u0002.Accesses)
			{
				this.\u0004();
				exprement.AcceptVisitor(this);
				this.\u0005();
			}
		}

		// Token: 0x06001569 RID: 5481 RVA: 0x0003E560 File Offset: 0x0003C760
		public void \u0001(ICompoAccessExpression \u0002)
		{
			this.\u0003(\u0002);
			this.\u0004();
			\u0002.Left.AcceptVisitor(this);
			\u0002.Right.AcceptVisitor(this);
			this.\u0005();
		}

		// Token: 0x0600156A RID: 5482 RVA: 0x0003E590 File Offset: 0x0003C790
		public void \u0001(IDeRefAccessExpression \u0002)
		{
			this.\u0003(\u0002);
			this.\u0004();
			\u0002.Base.AcceptVisitor(this);
			this.\u0005();
		}

		// Token: 0x0600156B RID: 5483 RVA: 0x0003E5B4 File Offset: 0x0003C7B4
		public void \u0001(IGlobalScopeExpression \u0002)
		{
			this.\u0003(\u0002);
			this.\u0004();
			\u0002.Base.AcceptVisitor(this);
			this.\u0005();
		}

		// Token: 0x0600156C RID: 5484 RVA: 0x0003E5D8 File Offset: 0x0003C7D8
		public void \u0001(ICaseRangeExpression \u0002)
		{
			this.\u0003(\u0002);
			this.\u0004();
			\u0002.Low.AcceptVisitor(this);
			\u0002.High.AcceptVisitor(this);
			this.\u0005();
		}

		// Token: 0x0600156D RID: 5485 RVA: 0x0003E608 File Offset: 0x0003C808
		private void \u0001()
		{
			this.\u0001.Push(new \u0005.\u0001
			{
				TopLevelStatement = this.TopOfStack.TopLevelStatement,
				TopLevelExpression = this.TopOfStack.TopLevelExpression,
				InConditionalStatement = this.TopOfStack.InConditionalStatement,
				OwningSequenceStatement = this.TopOfStack.OwningSequenceStatement,
				StatementPosition = this.TopOfStack.StatementPosition
			});
		}

		// Token: 0x0600156E RID: 5486 RVA: 0x0003E67C File Offset: 0x0003C87C
		private void \u0001(bool \u0002, bool \u0003)
		{
			this.\u0001();
			this.TopOfStack.TopLevelStatement = \u0002;
			this.TopOfStack.TopLevelExpression = \u0003;
		}

		// Token: 0x0600156F RID: 5487 RVA: 0x0003E69C File Offset: 0x0003C89C
		private void \u0002()
		{
			this.\u0002(false);
		}

		// Token: 0x06001570 RID: 5488 RVA: 0x0003E6A8 File Offset: 0x0003C8A8
		private void \u0002(bool \u0002)
		{
			this.\u0001(\u0002, \u0002);
		}

		// Token: 0x06001571 RID: 5489 RVA: 0x0003E6C0 File Offset: 0x0003C8C0
		private void \u0003()
		{
			this.\u0002(false);
			this.TopOfStack.InConditionalStatement = true;
		}

		// Token: 0x06001572 RID: 5490 RVA: 0x0003E6D8 File Offset: 0x0003C8D8
		private void \u0004()
		{
			this.\u0003(false);
		}

		// Token: 0x06001573 RID: 5491 RVA: 0x0003E6E4 File Offset: 0x0003C8E4
		private void \u0003(bool \u0002)
		{
			this.\u0001();
			this.TopOfStack.TopLevelExpression = \u0002;
		}

		// Token: 0x06001574 RID: 5492 RVA: 0x0003E6F8 File Offset: 0x0003C8F8
		private void \u0005()
		{
			this.\u0001.Pop();
		}

		// Token: 0x17000512 RID: 1298
		// (get) Token: 0x06001575 RID: 5493 RVA: 0x0003E708 File Offset: 0x0003C908
		private \u0005.\u0001 TopOfStack
		{
			get
			{
				if (0 < this.\u0001.Count)
				{
					return this.\u0001.Peek();
				}
				return new \u0005.\u0001();
			}
		}

		// Token: 0x040003A6 RID: 934
		private string \u0001;

		// Token: 0x040003A7 RID: 935
		private readonly string \u0002;

		// Token: 0x040003A8 RID: 936
		private readonly List<IExpression> \u0001 = new List<IExpression>();

		// Token: 0x040003A9 RID: 937
		private readonly List<IStatement> \u0001 = new List<IStatement>();

		// Token: 0x040003AA RID: 938
		private ISequenceStatement2 \u0001;

		// Token: 0x040003AB RID: 939
		[CompilerGenerated]
		private IExpression \u0001;

		// Token: 0x040003AC RID: 940
		[CompilerGenerated]
		private IExpression \u0002;

		// Token: 0x040003AD RID: 941
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x040003AE RID: 942
		[CompilerGenerated]
		private ISequenceStatement \u0001;

		// Token: 0x040003AF RID: 943
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x040003B0 RID: 944
		private readonly Stack<\u0005.\u0001> \u0001 = new Stack<\u0005.\u0001>();

		// Token: 0x0200012A RID: 298
		[DebuggerDisplay("TopLevelStatement: {TopLevelStatement}, TopLevelExpression: {TopLevelExpression}InConditionalStatement: {InConditionalStatement}")]
		private sealed class \u0001
		{
			// Token: 0x17000513 RID: 1299
			// (get) Token: 0x06001576 RID: 5494 RVA: 0x0003E72C File Offset: 0x0003C92C
			// (set) Token: 0x06001577 RID: 5495 RVA: 0x0003E734 File Offset: 0x0003C934
			internal bool TopLevelStatement { get; set; }

			// Token: 0x17000514 RID: 1300
			// (get) Token: 0x06001578 RID: 5496 RVA: 0x0003E740 File Offset: 0x0003C940
			// (set) Token: 0x06001579 RID: 5497 RVA: 0x0003E748 File Offset: 0x0003C948
			internal bool TopLevelExpression { get; set; }

			// Token: 0x17000515 RID: 1301
			// (get) Token: 0x0600157A RID: 5498 RVA: 0x0003E754 File Offset: 0x0003C954
			// (set) Token: 0x0600157B RID: 5499 RVA: 0x0003E75C File Offset: 0x0003C95C
			internal bool InConditionalStatement { get; set; }

			// Token: 0x17000516 RID: 1302
			// (get) Token: 0x0600157C RID: 5500 RVA: 0x0003E768 File Offset: 0x0003C968
			// (set) Token: 0x0600157D RID: 5501 RVA: 0x0003E770 File Offset: 0x0003C970
			internal ISequenceStatement OwningSequenceStatement { get; set; }

			// Token: 0x17000517 RID: 1303
			// (get) Token: 0x0600157E RID: 5502 RVA: 0x0003E77C File Offset: 0x0003C97C
			// (set) Token: 0x0600157F RID: 5503 RVA: 0x0003E784 File Offset: 0x0003C984
			internal int StatementPosition { get; set; }

			// Token: 0x06001580 RID: 5504 RVA: 0x0003E790 File Offset: 0x0003C990
			internal \u0001()
			{
				this.TopLevelStatement = true;
				this.TopLevelExpression = true;
				this.InConditionalStatement = false;
			}

			// Token: 0x040003B1 RID: 945
			[CompilerGenerated]
			private bool \u0001;

			// Token: 0x040003B2 RID: 946
			[CompilerGenerated]
			private bool \u0002;

			// Token: 0x040003B3 RID: 947
			[CompilerGenerated]
			private bool \u0003;

			// Token: 0x040003B4 RID: 948
			[CompilerGenerated]
			private ISequenceStatement \u0001;

			// Token: 0x040003B5 RID: 949
			[CompilerGenerated]
			private int \u0001;
		}
	}
}
