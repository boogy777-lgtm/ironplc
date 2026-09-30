using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	internal class DefinePragmaCollector : IExprVisitor3, IExprVisitor2, IExprVisitor
	{
		private HashSet<string> _hsDefines;

		public void GetDefines(IStatement stmt, HashSet<string> hsDefines)
		{
			_hsDefines = hsDefines;
			stmt.AcceptVisitor(this);
		}

		public void visit(IWhileStatement whilst)
		{
			whilst.Condition.AcceptVisitor(this);
			whilst.Controlled.AcceptVisitor(this);
		}

		public void visit(IRepeatStatement repeat)
		{
			repeat.Condition.AcceptVisitor(this);
			repeat.Controlled.AcceptVisitor(this);
		}

		public void visit(IForStatement forloop)
		{
			forloop.CounterStart.AcceptVisitor(this);
			forloop.UpperBound.AcceptVisitor(this);
			forloop.By?.AcceptVisitor(this);
			forloop.Controlled.AcceptVisitor(this);
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

		public void visit(IAssignmentExpression assign)
		{
			assign.LValue.AcceptVisitor(this);
			assign.RValue.AcceptVisitor(this);
		}

		public void visit(IIfStatement ifst)
		{
			ifst.Condition.AcceptVisitor(this);
			ifst.IfThen.AcceptVisitor(this);
			IElseIf[] elseIf = ifst.ElseIf;
			foreach (IElseIf obj in elseIf)
			{
				obj.Condition.AcceptVisitor(this);
				obj.Controlled.AcceptVisitor(this);
			}
			ifst.IfElse?.AcceptVisitor(this);
		}

		public void visit(IReturnStatement returnst)
		{
			returnst.Condition?.AcceptVisitor(this);
		}

		public void visit(IJumpStatement gotost)
		{
			gotost.Condition?.AcceptVisitor(this);
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
			expstat.Expr.AcceptVisitor(this);
		}

		public void visit(ICallExpression call)
		{
			call.Callee.AcceptVisitor(this);
			call.Condition?.AcceptVisitor(this);
			if (call.InputAssigns != null)
			{
				IAssignmentExpression[] inputAssigns = call.InputAssigns;
				for (int i = 0; i < inputAssigns.Length; i++)
				{
					inputAssigns[i]?.AcceptVisitor(this);
				}
			}
			if (call.OutputAssigns != null)
			{
				IAssignmentExpression[] inputAssigns = call.OutputAssigns;
				for (int i = 0; i < inputAssigns.Length; i++)
				{
					inputAssigns[i]?.AcceptVisitor(this);
				}
			}
		}

		public void visit(IOperatorExpression op)
		{
			if (op.Operands != null)
			{
				IExpression[] operands = op.Operands;
				for (int i = 0; i < operands.Length; i++)
				{
					operands[i].AcceptVisitor(this);
				}
			}
		}

		public void visit(IConversionExpression conv)
		{
			conv.Exp.AcceptVisitor(this);
		}

		public void visit(IThisExpression thisexp)
		{
		}

		public void visit(IBaseExpression baseexp)
		{
		}

		public void visit(ILiteralExpression literal)
		{
		}

		public void visit(IAddressExpression address)
		{
		}

		public void visit(IVariableExpression variable)
		{
		}

		public void visit(IIndexAccessExpression indexaccess)
		{
			IExpression[] accesses = indexaccess.Accesses;
			if (accesses != null)
			{
				IExpression[] array = accesses;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].AcceptVisitor(this);
				}
			}
			indexaccess.Var.AcceptVisitor(this);
		}

		public void visit(ICompoAccessExpression compo)
		{
			compo.Left.AcceptVisitor(this);
			compo.Right.AcceptVisitor(this);
		}

		public void visit(IDeRefAccessExpression deref)
		{
			deref.Base.AcceptVisitor(this);
		}

		public void visit(IGlobalScopeExpression globexp)
		{
			globexp.Base.AcceptVisitor(this);
		}

		public void visit(IEmptyStatement empty)
		{
		}

		public void visit(ICaseRangeExpression caserange)
		{
			caserange.Low.AcceptVisitor(this);
			caserange.High.AcceptVisitor(this);
		}

		public void visit(ICaseLabelStatement caselabel)
		{
			IExpression[] cases = caselabel.cases;
			for (int i = 0; i < cases.Length; i++)
			{
				cases[i].AcceptVisitor(this);
			}
		}

		public void visit(ICaseStatement casest)
		{
			casest.Switch.AcceptVisitor(this);
			ICase[] cases = casest.Cases;
			foreach (ICase obj in cases)
			{
				obj.Label.AcceptVisitor(this);
				obj.Controlled.AcceptVisitor(this);
			}
			casest.Else?.AcceptVisitor(this);
		}

		public void visit(IBreakPointStatement bpstate)
		{
		}

		public void visit(IDefineReference defref)
		{
		}

		public void visit(IVariableReference varref)
		{
			varref.Instance?.AcceptVisitor(this);
		}

		public void visit(ITypeReference typeref)
		{
			(typeref as ITypeReference2)?.InstanceExpression.AcceptVisitor(this);
		}

		public void visit(IPouReference pouref)
		{
			(pouref as IPouReference2)?.InstanceExpression.AcceptVisitor(this);
		}

		public void visit(IDefinedExpression defexp)
		{
			IExpression referencedItem = defexp.ReferencedItem;
			if (referencedItem is IDefineReference)
			{
				IDefineReference defineReference = referencedItem as IDefineReference;
				_hsDefines.Add(defineReference.Define);
			}
		}

		public void visit(IPragmaOperatorExpression popexp)
		{
			IExpression[] allOperands = popexp.AllOperands;
			for (int i = 0; i < allOperands.Length; i++)
			{
				allOperands[i].AcceptVisitor(this);
			}
		}

		public void visit(IPragmaIfStatement pifst)
		{
			pifst.ConditionExpression.AcceptVisitor(this);
			pifst.IfThenStatement.AcceptVisitor(this);
			IElseIf[] elseIfs = pifst.ElseIfs;
			foreach (IElseIf obj in elseIfs)
			{
				obj.Condition.AcceptVisitor(this);
				obj.Controlled.AcceptVisitor(this);
			}
			pifst.IfElseStatement?.AcceptVisitor(this);
		}

		public void visit(IDefineStatement defstate)
		{
		}

		public void visit(IHasTypeExpression hastype)
		{
			hastype.Instance.AcceptVisitor(this);
		}

		public void visit(IHasAttributeExpression hasattribute)
		{
			hasattribute.ReferencedItem.AcceptVisitor(this);
		}

		public void visit(IHasValueExpression hasvalue)
		{
		}

		public void visit(IPragmaAssertion assertion)
		{
			assertion.ConditionExpression.AcceptVisitor(this);
		}

		public void visit(IHasConstantValueExpression hasvalue)
		{
			hasvalue.Constant.AcceptVisitor(this);
			hasvalue.ConstantValue.AcceptVisitor(this);
		}

		public void visit(IStructureInitialization structInit)
		{
			IAssignmentExpression[] compoInits = structInit.CompoInits;
			if (compoInits != null)
			{
				IAssignmentExpression[] array = compoInits;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].AcceptVisitor(this);
				}
			}
		}

		public void visit(IArrayInitialization arrayInit)
		{
			IExpression[] initValues = arrayInit.InitValues;
			if (initValues != null)
			{
				IExpression[] array = initValues;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].AcceptVisitor(this);
				}
			}
		}
	}
}
