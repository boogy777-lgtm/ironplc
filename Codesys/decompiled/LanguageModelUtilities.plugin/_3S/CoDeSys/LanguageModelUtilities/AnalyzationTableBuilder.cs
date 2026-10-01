using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class AnalyzationTableBuilder : IExprVisitor2, IExprVisitor
	{
		private ILanguageModelBuilder3 _lmb;

		private Stack<LList<IStatement>> _states = new Stack<LList<IStatement>>();

		private IScope _scope;

		private IExpression _expInstance;

		private LStack<Operator> _operatorStack = new LStack<Operator>();

		private LList<IStatement> CurrentStatementList => _states.Peek();

		private Operator LastOperator
		{
			get
			{
				if (_operatorStack.get_Count() == 0)
				{
					return Operator.None;
				}
				return _operatorStack.Peek();
			}
		}

		private AnalyzationTableBuilder(ICompileContext comcon, IExpression expInstance, LList<IStatement> statementlist)
		{
			_lmb = APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder() as ILanguageModelBuilder3;
			_states.Push(statementlist);
			_scope = comcon.CreateGlobalIScope();
			_expInstance = expInstance;
		}

		internal static void CreateStringBuilderStatement(IExpression expToAnalyze, IExpression expInstance, ICompileContext comcon, LList<IStatement> statementlist)
		{
			AnalyzationTableBuilder visitor = new AnalyzationTableBuilder(comcon, expInstance, statementlist);
			expToAnalyze.AcceptVisitor(visitor);
		}

		private void GenerateConditionalStringAppend(bool bNegate, IExpression exp)
		{
			IVariableExpression2 expLeft = _lmb.CreateVariableExpression(null, "name");
			IVariableExpression2 expLeft2 = _lmb.CreateVariableExpression(null, "address");
			IVariableExpression2 expLeft3 = _lmb.CreateVariableExpression(null, "comment");
			IVariableExpression2 expLeft4 = _lmb.CreateVariableExpression(null, "value");
			IVariableExpression2 expLeft5 = _lmb.CreateVariableExpression(null, "failed");
			IExpression expLeft6 = _lmb.DuplicateExprement(_expInstance) as IExpression;
			IVariableExpression2 expRight = _lmb.CreateVariableExpression(null, "AddEntry");
			ICompoAccessExpression expCallee = _lmb.CreateCompoAccessExpression(null, expLeft6, expRight);
			IVariable variable = (exp as IExpression4).GetVariable(_scope);
			IDirectVariable directVariable = null;
			string text = null;
			if (variable != null)
			{
				directVariable = variable.Address;
				text = variable.Comment;
			}
			List<IAssignmentExpression> list = new List<IAssignmentExpression>();
			list.Add(_lmb.CreateAssignmentExpression(null, expLeft, _lmb.CreateLiteralExpression(null, Analyzation.WriteExprement(exp))));
			if (directVariable != null)
			{
				list.Add(_lmb.CreateAssignmentExpression(null, expLeft2, _lmb.CreateLiteralExpression(null, directVariable.ToString())));
			}
			else
			{
				list.Add(_lmb.CreateAssignmentExpression(null, expLeft2, _lmb.CreateLiteralExpression(null, "")));
			}
			if (text != null)
			{
				list.Add(_lmb.CreateAssignmentExpression(null, expLeft3, _lmb.CreateLiteralExpression(null, text)));
			}
			else
			{
				list.Add(_lmb.CreateAssignmentExpression(null, expLeft3, _lmb.CreateLiteralExpression(null, "")));
			}
			list.Add(_lmb.CreateAssignmentExpression(null, expLeft4, _lmb.DuplicateExprement(exp) as IExpression));
			IExpression expression = _lmb.DuplicateExprement(exp) as IExpression;
			if (LastOperator != Operator.Not)
			{
				expression = _lmb.CreateOperatorExpression(null, Operator.Not, expression);
			}
			list.Add(_lmb.CreateAssignmentExpression(null, expLeft5, expression));
			IStatement statement = _lmb.CreateCallStatement(null, expCallee, null, null, list, new List<IAssignmentExpression>());
			CurrentStatementList.Add(statement);
		}

		private IStatement GenerateConditionalStatementList(IExpression exp, IList<IStatement> statements)
		{
			IExpression expression = _lmb.DuplicateExprement(exp) as IExpression;
			if (LastOperator != Operator.Not)
			{
				expression = _lmb.CreateOperatorExpression(null, Operator.Not, expression);
			}
			ISequenceStatement2 seqThen = _lmb.CreateSequenceStatementEx(null, statements);
			return _lmb.CreateIfStatement(null, expression, seqThen);
		}

		public void visit(ICallExpression call)
		{
			GenerateConditionalStringAppend(bNegate: true, call);
		}

		private void HandleExpressionsDirectly(IOperatorExpression op)
		{
			IExpression[] operands = op.Operands;
			foreach (IExpression obj in operands)
			{
				_operatorStack.Push(LastOperator);
				obj.AcceptVisitor(this);
				_operatorStack.Pop();
			}
		}

		private void HandleExpressionsConditionally(IOperatorExpression op)
		{
			LList<IStatement> currentStatementList = CurrentStatementList;
			_states.Push(new LList<IStatement>());
			HandleExpressionsDirectly(op);
			LList<IStatement> currentStatementList2 = CurrentStatementList;
			IStatement statement = GenerateConditionalStatementList(op, (IList<IStatement>)currentStatementList2);
			_states.Pop();
			currentStatementList.Add(statement);
		}

		public void visit(IOperatorExpression op)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 16, 0))
			{
				switch (op.Code)
				{
				case Operator.Not:
				{
					Operator @operator = Operator.Not;
					if (LastOperator == Operator.Not)
					{
						@operator = Operator.None;
					}
					_operatorStack.Push(@operator);
					op.Operands[0].AcceptVisitor(this);
					_operatorStack.Pop();
					break;
				}
				case Operator.Or:
				case Operator.Or_Else:
					if (LastOperator == Operator.Not)
					{
						HandleExpressionsDirectly(op);
					}
					else
					{
						HandleExpressionsConditionally(op);
					}
					break;
				case Operator.And:
				case Operator.And_Then:
					if (LastOperator == Operator.Not)
					{
						HandleExpressionsConditionally(op);
					}
					else
					{
						HandleExpressionsDirectly(op);
					}
					break;
				default:
					GenerateConditionalStringAppend(bNegate: true, op);
					break;
				}
			}
			else
			{
				VisitOperatorExpressionForCompilerBefore35160(op);
			}
		}

		private void VisitOperatorExpressionForCompilerBefore35160(IOperatorExpression op)
		{
			switch (op.Code)
			{
			case Operator.Not:
			{
				Operator @operator = Operator.Not;
				if (LastOperator == Operator.Not)
				{
					@operator = Operator.None;
				}
				_operatorStack.Push(@operator);
				op.Operands[0].AcceptVisitor(this);
				_operatorStack.Pop();
				break;
			}
			case Operator.Xor:
			{
				IExpression[] operands = op.Operands;
				foreach (IExpression obj2 in operands)
				{
					_operatorStack.Push(op.Code);
					obj2.AcceptVisitor(this);
					_operatorStack.Pop();
					_operatorStack.Push(Operator.Not);
					obj2.AcceptVisitor(this);
					_operatorStack.Pop();
				}
				break;
			}
			case Operator.And:
			case Operator.Or:
			{
				IExpression[] operands = op.Operands;
				foreach (IExpression obj in operands)
				{
					_operatorStack.Push(LastOperator);
					obj.AcceptVisitor(this);
					_operatorStack.Pop();
				}
				break;
			}
			default:
				GenerateConditionalStringAppend(bNegate: true, op);
				break;
			}
		}

		public void visit(IConversionExpression conv)
		{
			GenerateConditionalStringAppend(bNegate: true, conv);
		}

		public void visit(IThisExpression thisexp)
		{
		}

		public void visit(IBaseExpression baseexp)
		{
		}

		public void visit(ILiteralExpression literal)
		{
			GenerateConditionalStringAppend(bNegate: true, literal);
		}

		public void visit(IAddressExpression address)
		{
			GenerateConditionalStringAppend(bNegate: true, address);
		}

		public void visit(IVariableExpression variable)
		{
			GenerateConditionalStringAppend(bNegate: true, variable);
		}

		public void visit(IIndexAccessExpression indexaccess)
		{
			GenerateConditionalStringAppend(bNegate: true, indexaccess);
		}

		public void visit(ICompoAccessExpression compo)
		{
			GenerateConditionalStringAppend(bNegate: true, compo);
		}

		public void visit(IDeRefAccessExpression deref)
		{
			GenerateConditionalStringAppend(bNegate: true, deref);
		}

		public void visit(IGlobalScopeExpression globexp)
		{
			GenerateConditionalStringAppend(bNegate: true, globexp);
		}

		public void visit(IEmptyStatement empty)
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

		public void visit(IHasConstantValueExpression hasvalue)
		{
		}

		public void visit(IPragmaAssertion assertion)
		{
		}

		public void visit(IWhileStatement whilst)
		{
		}

		public void visit(IRepeatStatement repeat)
		{
		}

		public void visit(IForStatement forloop)
		{
		}

		public void visit(IExitStatement exit)
		{
		}

		public void visit(IContinueStatement cont)
		{
		}

		public void visit(ISequenceStatement seq)
		{
		}

		public void visit(IAssignmentExpression assign)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 11, 50))
			{
				assign.RValue.AcceptVisitor(this);
			}
		}

		public void visit(IIfStatement ifst)
		{
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

		public void visit(IExpressionStatement expstat)
		{
		}
	}
}
