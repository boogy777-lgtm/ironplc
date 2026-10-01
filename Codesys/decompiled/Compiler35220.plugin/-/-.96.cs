using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u001B
{
	// Token: 0x02000128 RID: 296
	internal sealed class \u0002 : IExprVisitor6, IExprVisitor5, IExprVisitor4, IExprVisitor3, IExprVisitor2, IExprVisitor
	{
		// Token: 0x17000507 RID: 1287
		// (get) Token: 0x060014EF RID: 5359 RVA: 0x0003D83C File Offset: 0x0003BA3C
		// (set) Token: 0x060014F0 RID: 5360 RVA: 0x0003D844 File Offset: 0x0003BA44
		public bool HasErrorExpression { get; private set; }

		// Token: 0x17000508 RID: 1288
		// (get) Token: 0x060014F1 RID: 5361 RVA: 0x0003D850 File Offset: 0x0003BA50
		// (set) Token: 0x060014F2 RID: 5362 RVA: 0x0003D858 File Offset: 0x0003BA58
		public bool HasImplicitVariable { get; private set; }

		// Token: 0x060014F3 RID: 5363 RVA: 0x0003D864 File Offset: 0x0003BA64
		public void \u0001(IVariableExpression \u0002)
		{
			if (\u0002.Name.StartsWith("__"))
			{
				this.HasImplicitVariable = true;
			}
		}

		// Token: 0x060014F4 RID: 5364 RVA: 0x0003D880 File Offset: 0x0003BA80
		public void \u0001(IErrorExpression \u0002)
		{
			this.HasErrorExpression = true;
		}

		// Token: 0x060014F5 RID: 5365 RVA: 0x0003D88C File Offset: 0x0003BA8C
		public void \u0001(IThisExpression \u0002)
		{
		}

		// Token: 0x060014F6 RID: 5366 RVA: 0x0003D890 File Offset: 0x0003BA90
		public void \u0001(IBaseExpression \u0002)
		{
		}

		// Token: 0x060014F7 RID: 5367 RVA: 0x0003D894 File Offset: 0x0003BA94
		public void \u0001(ILiteralExpression \u0002)
		{
		}

		// Token: 0x060014F8 RID: 5368 RVA: 0x0003D898 File Offset: 0x0003BA98
		public void \u0001(IAddressExpression \u0002)
		{
		}

		// Token: 0x060014F9 RID: 5369 RVA: 0x0003D89C File Offset: 0x0003BA9C
		public void \u0001(INamespaceAccessExpression \u0002)
		{
		}

		// Token: 0x060014FA RID: 5370 RVA: 0x0003D8A0 File Offset: 0x0003BAA0
		public void \u0001(IEmptyStatement \u0002)
		{
		}

		// Token: 0x060014FB RID: 5371 RVA: 0x0003D8A4 File Offset: 0x0003BAA4
		public void \u0001(ICommentStatement \u0002)
		{
		}

		// Token: 0x060014FC RID: 5372 RVA: 0x0003D8A8 File Offset: 0x0003BAA8
		public void \u0001(IPragmaStatement \u0002)
		{
		}

		// Token: 0x060014FD RID: 5373 RVA: 0x0003D8AC File Offset: 0x0003BAAC
		public void \u0001(IExitStatement \u0002)
		{
		}

		// Token: 0x060014FE RID: 5374 RVA: 0x0003D8B0 File Offset: 0x0003BAB0
		public void \u0001(IContinueStatement \u0002)
		{
		}

		// Token: 0x060014FF RID: 5375 RVA: 0x0003D8B4 File Offset: 0x0003BAB4
		public void \u0001(ILabelStatement \u0002)
		{
		}

		// Token: 0x06001500 RID: 5376 RVA: 0x0003D8B8 File Offset: 0x0003BAB8
		public void \u0001(IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06001501 RID: 5377 RVA: 0x0003D8BC File Offset: 0x0003BABC
		public void \u0001(IPragmaIfStatement \u0002)
		{
		}

		// Token: 0x06001502 RID: 5378 RVA: 0x0003D8C0 File Offset: 0x0003BAC0
		public void \u0001(IDefineStatement \u0002)
		{
		}

		// Token: 0x06001503 RID: 5379 RVA: 0x0003D8C4 File Offset: 0x0003BAC4
		public void \u0001(IErrorStatement \u0002)
		{
		}

		// Token: 0x06001504 RID: 5380 RVA: 0x0003D8C8 File Offset: 0x0003BAC8
		public void \u0001(ICastExpression \u0002)
		{
		}

		// Token: 0x06001505 RID: 5381 RVA: 0x0003D8CC File Offset: 0x0003BACC
		public void \u0001(ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x06001506 RID: 5382 RVA: 0x0003D8D0 File Offset: 0x0003BAD0
		public void \u0001(IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x06001507 RID: 5383 RVA: 0x0003D8D4 File Offset: 0x0003BAD4
		public void \u0001(IStructureInitialization \u0002)
		{
		}

		// Token: 0x06001508 RID: 5384 RVA: 0x0003D8D8 File Offset: 0x0003BAD8
		public void \u0001(IArrayInitialization \u0002)
		{
		}

		// Token: 0x06001509 RID: 5385 RVA: 0x0003D8DC File Offset: 0x0003BADC
		public void \u0001(IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x0600150A RID: 5386 RVA: 0x0003D8E0 File Offset: 0x0003BAE0
		public void \u0001(IDefineReference \u0002)
		{
		}

		// Token: 0x0600150B RID: 5387 RVA: 0x0003D8E4 File Offset: 0x0003BAE4
		public void \u0001(IVariableReference \u0002)
		{
		}

		// Token: 0x0600150C RID: 5388 RVA: 0x0003D8E8 File Offset: 0x0003BAE8
		public void \u0001(ITypeReference \u0002)
		{
		}

		// Token: 0x0600150D RID: 5389 RVA: 0x0003D8EC File Offset: 0x0003BAEC
		public void \u0001(IPouReference \u0002)
		{
		}

		// Token: 0x0600150E RID: 5390 RVA: 0x0003D8F0 File Offset: 0x0003BAF0
		public void \u0001(IDefinedExpression \u0002)
		{
		}

		// Token: 0x0600150F RID: 5391 RVA: 0x0003D8F4 File Offset: 0x0003BAF4
		public void \u0001(IPragmaOperatorExpression \u0002)
		{
		}

		// Token: 0x06001510 RID: 5392 RVA: 0x0003D8F8 File Offset: 0x0003BAF8
		public void \u0001(IHasTypeExpression \u0002)
		{
		}

		// Token: 0x06001511 RID: 5393 RVA: 0x0003D8FC File Offset: 0x0003BAFC
		public void \u0001(IHasAttributeExpression \u0002)
		{
		}

		// Token: 0x06001512 RID: 5394 RVA: 0x0003D900 File Offset: 0x0003BB00
		public void \u0001(IHasValueExpression \u0002)
		{
		}

		// Token: 0x06001513 RID: 5395 RVA: 0x0003D904 File Offset: 0x0003BB04
		public void \u0001(IPragmaAssertion \u0002)
		{
		}

		// Token: 0x06001514 RID: 5396 RVA: 0x0003D908 File Offset: 0x0003BB08
		public void \u0001(ISequenceStatement \u0002)
		{
			IStatement[] statements = \u0002.Statements;
			for (int i = 0; i < statements.Length; i++)
			{
				statements[i].AcceptVisitor(this);
			}
		}

		// Token: 0x06001515 RID: 5397 RVA: 0x0003D934 File Offset: 0x0003BB34
		public void \u0001(IWhileStatement \u0002)
		{
			\u0002.Controlled.AcceptVisitor(this);
			\u0002.Condition.AcceptVisitor(this);
		}

		// Token: 0x06001516 RID: 5398 RVA: 0x0003D950 File Offset: 0x0003BB50
		public void \u0001(IRepeatStatement \u0002)
		{
			\u0002.Controlled.AcceptVisitor(this);
			\u0002.Condition.AcceptVisitor(this);
		}

		// Token: 0x06001517 RID: 5399 RVA: 0x0003D96C File Offset: 0x0003BB6C
		public void \u0001(ICaseLabelStatement \u0002)
		{
			IExpression[] cases = \u0002.cases;
			for (int i = 0; i < cases.Length; i++)
			{
				cases[i].AcceptVisitor(this);
			}
		}

		// Token: 0x06001518 RID: 5400 RVA: 0x0003D998 File Offset: 0x0003BB98
		public void \u0001(ICaseStatement \u0002)
		{
			\u0002.Switch.AcceptVisitor(this);
			foreach (ICase @case in \u0002.Cases)
			{
				@case.Label.AcceptVisitor(this);
				@case.Controlled.AcceptVisitor(this);
			}
			if (\u0002.Else != null)
			{
				\u0002.Else.AcceptVisitor(this);
			}
		}

		// Token: 0x06001519 RID: 5401 RVA: 0x0003D9F4 File Offset: 0x0003BBF4
		public void \u0001(IForStatement \u0002)
		{
			\u0002.CounterStart.AcceptVisitor(this);
			\u0002.UpperBound.AcceptVisitor(this);
			if (\u0002.By != null)
			{
				\u0002.By.AcceptVisitor(this);
			}
			\u0002.Controlled.AcceptVisitor(this);
		}

		// Token: 0x0600151A RID: 5402 RVA: 0x0003DA30 File Offset: 0x0003BC30
		public void \u0001(IIfStatement \u0002)
		{
			\u0002.IfThen.AcceptVisitor(this);
			if (\u0002.IfElse != null)
			{
				\u0002.IfElse.AcceptVisitor(this);
			}
			if (\u0002.ElseIf != null)
			{
				IElseIf[] elseIf = \u0002.ElseIf;
				for (int i = 0; i < elseIf.Length; i++)
				{
					elseIf[i].Controlled.AcceptVisitor(this);
				}
			}
		}

		// Token: 0x0600151B RID: 5403 RVA: 0x0003DA88 File Offset: 0x0003BC88
		public void \u0001(IReturnStatement \u0002)
		{
			if (\u0002.Condition != null)
			{
				\u0002.Condition.AcceptVisitor(this);
			}
		}

		// Token: 0x0600151C RID: 5404 RVA: 0x0003DAA0 File Offset: 0x0003BCA0
		public void \u0001(IJumpStatement \u0002)
		{
			if (\u0002.Condition != null)
			{
				\u0002.Condition.AcceptVisitor(this);
			}
		}

		// Token: 0x0600151D RID: 5405 RVA: 0x0003DAB8 File Offset: 0x0003BCB8
		public void \u0001(IExpressionStatement \u0002)
		{
			\u0002.Expr.AcceptVisitor(this);
		}

		// Token: 0x0600151E RID: 5406 RVA: 0x0003DAC8 File Offset: 0x0003BCC8
		public void \u0001(IAssignmentExpression \u0002)
		{
			\u0002.LValue.AcceptVisitor(this);
			\u0002.RValue.AcceptVisitor(this);
		}

		// Token: 0x0600151F RID: 5407 RVA: 0x0003DAE4 File Offset: 0x0003BCE4
		public void \u0001(ICallExpression \u0002)
		{
			if (\u0002.Condition != null)
			{
				\u0002.Condition.AcceptVisitor(this);
			}
			\u0002.Callee.AcceptVisitor(this);
			IAssignmentExpression[] array = \u0002.InputAssigns;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AcceptVisitor(this);
			}
			array = \u0002.OutputAssigns;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AcceptVisitor(this);
			}
		}

		// Token: 0x06001520 RID: 5408 RVA: 0x0003DB50 File Offset: 0x0003BD50
		public void \u0001(IOperatorExpression \u0002)
		{
			IExpression[] operands = \u0002.Operands;
			for (int i = 0; i < operands.Length; i++)
			{
				operands[i].AcceptVisitor(this);
			}
		}

		// Token: 0x06001521 RID: 5409 RVA: 0x0003DB7C File Offset: 0x0003BD7C
		public void \u0001(IConversionExpression \u0002)
		{
			\u0002.Exp.AcceptVisitor(this);
		}

		// Token: 0x06001522 RID: 5410 RVA: 0x0003DB8C File Offset: 0x0003BD8C
		public void \u0001(IIndexAccessExpression \u0002)
		{
			\u0002.Var.AcceptVisitor(this);
			IExpression[] accesses = \u0002.Accesses;
			for (int i = 0; i < accesses.Length; i++)
			{
				accesses[i].AcceptVisitor(this);
			}
		}

		// Token: 0x06001523 RID: 5411 RVA: 0x0003DBC4 File Offset: 0x0003BDC4
		public void \u0001(ICompoAccessExpression \u0002)
		{
			\u0002.Left.AcceptVisitor(this);
			\u0002.Right.AcceptVisitor(this);
		}

		// Token: 0x06001524 RID: 5412 RVA: 0x0003DBE0 File Offset: 0x0003BDE0
		public void \u0001(IDeRefAccessExpression \u0002)
		{
			\u0002.Base.AcceptVisitor(this);
		}

		// Token: 0x06001525 RID: 5413 RVA: 0x0003DBF0 File Offset: 0x0003BDF0
		public void \u0001(IGlobalScopeExpression \u0002)
		{
			\u0002.Base.AcceptVisitor(this);
		}

		// Token: 0x06001526 RID: 5414 RVA: 0x0003DC00 File Offset: 0x0003BE00
		public void \u0001(ICaseRangeExpression \u0002)
		{
			\u0002.Low.AcceptVisitor(this);
			\u0002.High.AcceptVisitor(this);
		}

		// Token: 0x040003A4 RID: 932
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x040003A5 RID: 933
		[CompilerGenerated]
		private bool \u0002;
	}
}
