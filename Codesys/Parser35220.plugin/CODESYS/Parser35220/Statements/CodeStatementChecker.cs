using System;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Statements
{
	// Token: 0x0200002A RID: 42
	internal class CodeStatementChecker : DummyBaseClass, IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x17000084 RID: 132
		// (get) Token: 0x060002BE RID: 702 RVA: 0x0000E2F8 File Offset: 0x0000C4F8
		private ParserContext Context { get; }

		// Token: 0x060002BF RID: 703 RVA: 0x0000E300 File Offset: 0x0000C500
		private CodeStatementChecker(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x0000E31C File Offset: 0x0000C51C
		internal static void CheckForUnexpectedStatementsInImplementation(_IStatement statement, ParserContext context)
		{
			CodeStatementChecker codeStatementChecker = new CodeStatementChecker(context);
			try
			{
				statement.Accept(codeStatementChecker);
			}
			catch (MaximumNestingDepthExceededException)
			{
				context.ErrorHandler.AddErrorST(statement, 584, Array.Empty<object>());
			}
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x0000E364 File Offset: 0x0000C564
		private void CheckNotExpected(_IStatement statement, _ISequenceStatement sequence, int index)
		{
			if (statement is _IPOUDeclarationStatement || statement is _ITypeDeclarationStatement || statement is _IVariableDeclarationListStatement || statement is _IVariableDeclarationStatement || statement is _IEnumDeclarationStatement || statement is IEnumDeclarationListStatement)
			{
				_IStatement istatement = this.Context.LMItemFactory.CreateErrorStatement();
				istatement._Position = statement._Position;
				istatement.LengthIntern = statement.LengthIntern;
				this.Context.ErrorHandler.AddErrorST(istatement, 578, Array.Empty<object>());
				sequence._StatementList[index] = istatement;
			}
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x0000E3F4 File Offset: 0x0000C5F4
		public void visit(_ISequenceStatement seq)
		{
			this._nStackDepth++;
			if (this._nStackDepth > this._iMaxNestingDepth)
			{
				seq._StatementList.Clear();
				_IErrorStatement ierrorStatement = this.Context.LMItemFactory.CreateErrorStatement();
				ierrorStatement.SetPositionIntern(this.Context.LMItemFactory.CreateMinimalPosition(0L, 0));
				this.Context.ErrorHandler.AddErrorST(ierrorStatement, 584, Array.Empty<object>());
				seq._StatementList.Add(ierrorStatement);
				throw new MaximumNestingDepthExceededException();
			}
			for (int i = 0; i < seq._StatementList.Count; i++)
			{
				_IStatement statement = seq._StatementList[i];
				seq._StatementList[i].Accept(this);
				this.CheckNotExpected(statement, seq, i);
			}
			this._nStackDepth--;
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x0000E4CB File Offset: 0x0000C6CB
		public void visit(_ICompiledPOU cpou)
		{
			cpou.GetParseTree().Accept(this);
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x0000E4D9 File Offset: 0x0000C6D9
		public void visit(_IWhileStatement whilst)
		{
			whilst._Controlled.Accept(this);
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x0000E4E7 File Offset: 0x0000C6E7
		public void visit(_IRepeatStatement repeat)
		{
			repeat._Controlled.Accept(this);
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x0000E4F5 File Offset: 0x0000C6F5
		public void visit(_IForStatement forloop)
		{
			forloop._Controlled.Accept(this);
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x0000E504 File Offset: 0x0000C704
		public void visit(_IIfStatement ifst)
		{
			ifst._IfThen.Accept(this);
			_IStatement ifElse = ifst._IfElse;
			if (ifElse != null)
			{
				ifElse.Accept(this);
			}
			foreach (_IElseIf ielseIf in ifst._ElseIf)
			{
				ielseIf._Controlled.Accept(this);
			}
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x0000E574 File Offset: 0x0000C774
		public void visit(_ICaseStatement casest)
		{
			foreach (_ICase icase in casest._Cases)
			{
				icase._Controlled.Accept(this);
			}
			if (casest._Else != null)
			{
				casest._Else.Accept(this);
			}
		}

		// Token: 0x0400008A RID: 138
		private readonly int _iMaxNestingDepth = 5000;

		// Token: 0x0400008B RID: 139
		private int _nStackDepth;
	}
}
