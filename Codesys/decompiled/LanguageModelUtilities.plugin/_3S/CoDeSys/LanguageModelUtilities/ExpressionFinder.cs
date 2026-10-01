using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{B6C02CF5-B1A8-4F25-AA33-5EFB7246F7FA}")]
	public sealed class ExpressionFinder : IExprVisitor, IExpressionFinder
	{
		private IExpression _expr;

		private ISequenceStatement _seqStmt;

		private List<IExpression> _expressions = new List<IExpression>();

		private IExpressionFinderExceptionHandler _exceptionHandler;

		private void Initialize(string stExpr, bool bExpression)
		{
			if (stExpr == null)
			{
				throw new ArgumentNullException("stExpr");
			}
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(stExpr, bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);
			if (!(APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner) is IParser2 parser))
			{
				throw new NotSupportedException("IParser2 not implemented");
			}
			if (bExpression)
			{
				_expr = parser.ParseExpression();
			}
			else
			{
				_seqStmt = parser.ParseStatement();
			}
		}

		private void Initialize(IExpression expr)
		{
			if (expr == null)
			{
				throw new ArgumentNullException("expr");
			}
			_expr = expr;
		}

		private void Initialize(ISequenceStatement seqStmt)
		{
			if (seqStmt == null)
			{
				throw new ArgumentNullException("seqStmt");
			}
			_seqStmt = seqStmt;
		}

		private IEnumerable<IExpression> FindVariableAccesses()
		{
			if (_expr == null && _seqStmt == null)
			{
				return new IExpression[0];
			}
			try
			{
				if (_expr != null)
				{
					_expr.AcceptVisitor(this);
				}
				else
				{
					_seqStmt.AcceptVisitor(this);
				}
				return _expressions;
			}
			catch (Exception ex)
			{
				if (_exceptionHandler != null)
				{
					_exceptionHandler.HandleException(ex);
				}
				return new IExpression[0];
			}
		}

		public void Initialize(IExpressionFinderExceptionHandler exceptionHandler)
		{
			_exceptionHandler = exceptionHandler;
		}

		public IEnumerable<IExpression> FindVariableAccesses(string stExpr, bool bExpression)
		{
			Initialize(stExpr, bExpression);
			return FindVariableAccesses();
		}

		public IEnumerable<IExpression> FindVariableAccesses(IExpression expr)
		{
			Initialize(expr);
			return FindVariableAccesses();
		}

		public IEnumerable<IExpression> FindVariableAccesses(ISequenceStatement seqStmt)
		{
			Initialize(seqStmt);
			return FindVariableAccesses();
		}

		public void visit(IWhileStatement whilst)
		{
			if (whilst.Condition != null && !AddExpressionIf(whilst.Condition))
			{
				whilst.Condition.AcceptVisitor(this);
			}
			if (whilst.Controlled != null)
			{
				whilst.Controlled.AcceptVisitor(this);
			}
		}

		public void visit(IRepeatStatement repeat)
		{
			if (repeat.Condition != null && !AddExpressionIf(repeat.Condition))
			{
				repeat.Condition.AcceptVisitor(this);
			}
			if (repeat.Controlled != null)
			{
				repeat.Controlled.AcceptVisitor(this);
			}
		}

		public void visit(IForStatement forloop)
		{
			if (forloop.Counter != null && !AddExpressionIf(forloop.Counter))
			{
				forloop.Counter.AcceptVisitor(this);
			}
			if (forloop.CounterStart != null && !AddExpressionIf(forloop.CounterStart))
			{
				forloop.CounterStart.AcceptVisitor(this);
			}
			if (forloop.UpperBound != null && !AddExpressionIf(forloop.UpperBound))
			{
				forloop.UpperBound.AcceptVisitor(this);
			}
			if (forloop.By != null && !AddExpressionIf(forloop.By))
			{
				forloop.By.AcceptVisitor(this);
			}
			if (forloop.Condition != null && !AddExpressionIf(forloop.Condition))
			{
				forloop.Condition.AcceptVisitor(this);
			}
			if (forloop.Controlled != null)
			{
				forloop.Controlled.AcceptVisitor(this);
			}
		}

		public void visit(IExitStatement exit)
		{
		}

		public void visit(IContinueStatement cont)
		{
		}

		public void visit(ISequenceStatement seq)
		{
			IStatement[] statements = seq.Statements;
			for (int i = 0; i < statements.Length; i++)
			{
				statements[i].AcceptVisitor(this);
			}
		}

		public void visit(IIfStatement ifst)
		{
			if (ifst.Condition != null && !AddExpressionIf(ifst.Condition))
			{
				ifst.Condition.AcceptVisitor(this);
			}
			if (ifst.IfThen != null)
			{
				ifst.IfThen.AcceptVisitor(this);
			}
			if (ifst.ElseIf != null)
			{
				IElseIf[] elseIf = ifst.ElseIf;
				foreach (IElseIf elseIf2 in elseIf)
				{
					if (elseIf2.Condition != null && !AddExpressionIf(elseIf2.Condition))
					{
						elseIf2.Condition.AcceptVisitor(this);
					}
					if (elseIf2.Controlled != null)
					{
						elseIf2.Controlled.AcceptVisitor(this);
					}
				}
			}
			if (ifst.IfElse != null)
			{
				ifst.IfElse.AcceptVisitor(this);
			}
		}

		public void visit(IReturnStatement returnst)
		{
		}

		public void visit(IJumpStatement gotost)
		{
		}

		public void visit(ILabelStatement label)
		{
		}

		public void visit(ICommentStatement comment)
		{
		}

		public void visit(IPragmaStatement pragma)
		{
		}

		public void visit(ICaseRangeExpression caserange)
		{
		}

		public void visit(ICaseLabelStatement caselabel)
		{
		}

		public void visit(ICaseStatement casest)
		{
			if (casest.Switch != null && !AddExpressionIf(casest.Switch))
			{
				casest.Switch.AcceptVisitor(this);
			}
			if (casest.Cases != null)
			{
				ICase[] cases = casest.Cases;
				foreach (ICase @case in cases)
				{
					if (@case.Controlled != null)
					{
						@case.Controlled.AcceptVisitor(this);
					}
				}
			}
			if (casest.Else != null)
			{
				casest.Else.AcceptVisitor(this);
			}
		}

		public void visit(IBreakPointStatement bpstate)
		{
		}

		public void visit(IDefineReference defref)
		{
		}

		public void visit(IVariableReference varref)
		{
		}

		public void visit(ITypeReference typeref)
		{
		}

		public void visit(IPouReference pouref)
		{
		}

		public void visit(IDefinedExpression defexp)
		{
		}

		public void visit(IPragmaOperatorExpression popexp)
		{
		}

		public void visit(IPragmaIfStatement pifst)
		{
		}

		public void visit(IDefineStatement defstate)
		{
		}

		public void visit(IHasTypeExpression hastype)
		{
		}

		public void visit(IHasAttributeExpression hasattribute)
		{
		}

		public void visit(IHasValueExpression hasvalue)
		{
		}

		public void visit(IPragmaAssertion assertion)
		{
		}

		public void visit(IExpressionStatement expstat)
		{
			if (expstat.Expr != null && !AddExpressionIf(expstat.Expr))
			{
				expstat.Expr.AcceptVisitor(this);
			}
		}

		public void visit(IThisExpression thisexp)
		{
		}

		public void visit(IBaseExpression baseexp)
		{
		}

		public void visit(IAssignmentExpression assign)
		{
			if (assign.LValue != null && !AddExpressionIf(assign.LValue))
			{
				assign.LValue.AcceptVisitor(this);
			}
			if (assign.RValue != null && !AddExpressionIf(assign.RValue))
			{
				assign.RValue.AcceptVisitor(this);
			}
		}

		public void visit(ICallExpression call)
		{
			IAssignmentExpression[] inputAssigns = call.InputAssigns;
			foreach (IAssignmentExpression assignmentExpression in inputAssigns)
			{
				if (assignmentExpression.RValue != null && !AddExpressionIf(assignmentExpression.RValue))
				{
					assignmentExpression.RValue.AcceptVisitor(this);
				}
			}
		}

		public void visit(IOperatorExpression op)
		{
			IExpression[] operands = op.Operands;
			foreach (IExpression expression in operands)
			{
				if (expression != null && !AddExpressionIf(expression))
				{
					expression.AcceptVisitor(this);
				}
			}
		}

		public void visit(IConversionExpression conv)
		{
			if (conv.Exp != null && !AddExpressionIf(conv.Exp))
			{
				conv.Exp.AcceptVisitor(this);
			}
		}

		public void visit(ILiteralExpression literal)
		{
		}

		public void visit(IAddressExpression address)
		{
		}

		public void visit(IVariableExpression variable)
		{
			AddExpression(variable);
		}

		public void visit(IIndexAccessExpression indexaccess)
		{
			AddExpression(indexaccess);
		}

		public void visit(ICompoAccessExpression compo)
		{
			if (compo.Right is ILiteralExpression)
			{
				AddExpressionIf(compo.Left);
			}
			else
			{
				AddExpression(compo);
			}
		}

		public void visit(IDeRefAccessExpression deref)
		{
		}

		public void visit(IGlobalScopeExpression globexp)
		{
			AddExpression(globexp);
		}

		public void visit(IEmptyStatement empty)
		{
		}

		private bool AddExpressionIf(IExpression expr)
		{
			if (expr is ICompoAccessExpression || expr is IIndexAccessExpression || expr is IGlobalScopeExpression || expr is IVariableExpression)
			{
				AddExpression(expr);
				return true;
			}
			return false;
		}

		private void AddExpression(IExpression expr)
		{
			_expressions.Add(expr);
		}
	}
}
