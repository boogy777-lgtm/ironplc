using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class VariableTraverser : IExprVisitor2, IExprVisitor, IStandardTraverser
	{
		protected IExprementVisitorNoTraversion _expCalledForAll;

		private LStack<AccessFlag> _stack = new LStack<AccessFlag>();

		private bool m_bAbort;

		private bool m_bImplicitOn;

		public bool Abort
		{
			get
			{
				return m_bAbort;
			}
			set
			{
				m_bAbort = value;
			}
		}

		public bool ImplicitOn => m_bImplicitOn;

		public AccessFlag TopOfStack => _stack.Peek();

		public AccessFlag Access => _stack.Peek();

		public IPrecompileScope5 Scope => null;

		public VariableTraverser(IExprementVisitorNoTraversion expCalledForAll)
		{
			_expCalledForAll = expCalledForAll;
			_expCalledForAll.Traverser = this;
			Push(AccessFlag.Read);
		}

		public void Reset()
		{
			m_bAbort = false;
			m_bImplicitOn = false;
			_stack.Clear();
			Push(AccessFlag.Read);
		}

		public void Reset(IExprementVisitorNoTraversion expCalledForAll)
		{
			Reset();
			_expCalledForAll = expCalledForAll;
			_expCalledForAll.Traverser = this;
		}

		public void Push(AccessFlag access)
		{
			_stack.Push(access);
		}

		public void Pop()
		{
			_stack.Pop();
		}

		public virtual void visit(IAddressExpression address)
		{
			_expCalledForAll.visit(address);
		}

		public virtual void visit(IVariableExpression variable)
		{
			_expCalledForAll.visit(variable, TopOfStack, null);
		}

		public virtual void visit(ICompiledPOU cpou)
		{
			cpou.ParseTree.AcceptVisitor(this);
			_expCalledForAll.visit(cpou);
		}

		public virtual void visit(IWhileStatement whilst)
		{
			Push(AccessFlag.Read);
			whilst.Condition.AcceptVisitor(this);
			Pop();
			whilst.Controlled.AcceptVisitor(this);
			_expCalledForAll.visit(whilst);
		}

		public virtual void visit(IRepeatStatement repeat)
		{
			Push(AccessFlag.Read);
			repeat.Condition.AcceptVisitor(this);
			Pop();
			repeat.Controlled.AcceptVisitor(this);
			_expCalledForAll.visit(repeat);
		}

		public virtual void visit(IForStatement forloop)
		{
			forloop.CounterStart.AcceptVisitor(this);
			Push(AccessFlag.Read);
			forloop.UpperBound.AcceptVisitor(this);
			Pop();
			if (forloop.By != null)
			{
				Push(AccessFlag.Read);
				forloop.By.AcceptVisitor(this);
				Pop();
			}
			forloop.Controlled.AcceptVisitor(this);
			_expCalledForAll.visit(forloop);
		}

		public virtual void visit(ISequenceStatement seq)
		{
			if (Abort)
			{
				return;
			}
			Push(AccessFlag.Unknown);
			IStatement[] statements = seq.Statements;
			for (int i = 0; i < statements.Length; i++)
			{
				statements[i].AcceptVisitor(this);
				if (Abort)
				{
					break;
				}
			}
			Pop();
			_expCalledForAll.visit(seq);
		}

		public virtual void visit(IIfStatement ifst)
		{
			Push(AccessFlag.Read);
			ifst.Condition.AcceptVisitor(this);
			Pop();
			ifst.IfThen.AcceptVisitor(this);
			IElseIf[] elseIf = ifst.ElseIf;
			if (elseIf != null)
			{
				IElseIf[] array = elseIf;
				foreach (IElseIf obj in array)
				{
					Push(AccessFlag.Read);
					obj.Condition.AcceptVisitor(this);
					Pop();
					obj.Controlled.AcceptVisitor(this);
				}
			}
			if (ifst.IfElse != null)
			{
				ifst.IfElse.AcceptVisitor(this);
			}
			_expCalledForAll.visit(ifst);
		}

		public virtual void visit(IExpressionStatement expstat)
		{
			Push(AccessFlag.Read);
			expstat.Expr.AcceptVisitor(this);
			Pop();
			_expCalledForAll.visit(expstat);
		}

		public virtual void visit(IAssignmentExpression assign)
		{
			Push(AccessFlag.Write);
			assign.LValue.AcceptVisitor(this);
			Pop();
			if (assign.KindOf == Operator.RefAssign || (assign.LValue.Type != null && assign.LValue.Type.Class == TypeClass.Reference))
			{
				Push(AccessFlag.Write);
			}
			else
			{
				Push(AccessFlag.Read);
			}
			assign.RValue.AcceptVisitor(this);
			Pop();
			_expCalledForAll.visit(assign);
		}

		public ICompiledType ParseType(string stType)
		{
			try
			{
				if (!(APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder() is ILanguageModelBuilder3 languageModelBuilder))
				{
					return null;
				}
				return languageModelBuilder.ParseType(stType);
			}
			catch
			{
				return null;
			}
		}

		public virtual void visit(ICallExpression call)
		{
			Push(AccessFlag.Call);
			call.Callee.AcceptVisitor(this);
			Pop();
			if (call.Condition != null)
			{
				Push(AccessFlag.Read);
				call.Condition.AcceptVisitor(this);
				Pop();
			}
			IAssignmentExpression[] inputAssigns = call.InputAssigns;
			foreach (IAssignmentExpression assignmentExpression in inputAssigns)
			{
				if (assignmentExpression != null)
				{
					Push(AccessFlag.Write);
					assignmentExpression.LValue.AcceptVisitor(this);
					Pop();
					Push(AccessFlag.Read);
					assignmentExpression.AcceptVisitor(this);
					Pop();
				}
			}
			inputAssigns = call.OutputAssigns;
			foreach (IAssignmentExpression assignmentExpression2 in inputAssigns)
			{
				if (assignmentExpression2 != null)
				{
					Push(AccessFlag.Write);
					assignmentExpression2.LValue.AcceptVisitor(this);
					Pop();
					Push(AccessFlag.Read);
					assignmentExpression2.RValue.AcceptVisitor(this);
					Pop();
				}
			}
			_expCalledForAll.visit(call);
		}

		public virtual void visit(IOperatorExpression op)
		{
			IExpression[] operands = op.Operands;
			foreach (IExpression obj in operands)
			{
				if (op.Code == Operator.Adr || op.Code == Operator.__RefAdr)
				{
					Pop();
					Push(AccessFlag.Write | AccessFlag.Address);
				}
				obj.AcceptVisitor(this);
			}
			_expCalledForAll.visit(op);
		}

		public virtual void visit(ICastExpression cast)
		{
			cast.Base.AcceptVisitor(this);
			_expCalledForAll.visit(cast);
		}

		public virtual void visit(INewExpression newexp)
		{
			newexp.Count.AcceptVisitor(this);
			if (newexp is INewExpression2 && (newexp as INewExpression2).FBInitParams != null)
			{
				foreach (IAssignmentExpression fBInitParam in (newexp as INewExpression2).FBInitParams)
				{
					fBInitParam.AcceptVisitor(this);
				}
			}
			_expCalledForAll.visit(newexp);
		}

		public virtual void visit(ITypeExpression typeexp)
		{
			_expCalledForAll.visit(typeexp);
		}

		public virtual void visit(IConversionExpression conv)
		{
			conv.Exp.AcceptVisitor(this);
			_expCalledForAll.visit(conv);
		}

		public virtual void visit(IIndexAccessExpression indexaccess)
		{
			Push(AccessFlag.Read);
			IExpression[] accesses = indexaccess.Accesses;
			for (int i = 0; i < accesses.Length; i++)
			{
				accesses[i].AcceptVisitor(this);
			}
			Pop();
			indexaccess.Var.AcceptVisitor(this);
			_expCalledForAll.visit(indexaccess);
		}

		public virtual void visit(ILiteralExpression literal)
		{
			_expCalledForAll.visit(literal);
		}

		public virtual void visit(ICompoAccessExpression compo)
		{
			Push(TopOfStack);
			compo.Left.AcceptVisitor(this);
			Pop();
			Push(TopOfStack);
			compo.Right.AcceptVisitor(this);
			Pop();
		}

		public virtual void visit(IDeRefAccessExpression deref)
		{
			deref.Base.AcceptVisitor(this);
			_expCalledForAll.visit(deref);
		}

		public virtual void visit(IGlobalScopeExpression globexp)
		{
			Push(TopOfStack);
			globexp.Base.AcceptVisitor(this);
			Pop();
			_expCalledForAll.visit(globexp);
		}

		public virtual void visit(ISystemScopeExpression systemscope)
		{
			Push(TopOfStack);
			systemscope.Base.AcceptVisitor(this);
			Pop();
			_expCalledForAll.visit(systemscope);
		}

		public virtual void visit(ICaseRangeExpression caserange)
		{
			caserange.Low.AcceptVisitor(this);
			caserange.High.AcceptVisitor(this);
			_expCalledForAll.visit(caserange);
		}

		public virtual void visit(ICaseLabelStatement caselabel)
		{
			IExpression[] cases = caselabel.cases;
			for (int i = 0; i < cases.Length; i++)
			{
				cases[i].AcceptVisitor(this);
			}
			_expCalledForAll.visit(caselabel);
		}

		public virtual void visit(ICaseStatement casest)
		{
			Push(AccessFlag.Read);
			casest.Switch.AcceptVisitor(this);
			Pop();
			ICase[] cases = casest.Cases;
			foreach (ICase obj in cases)
			{
				Push(AccessFlag.Read);
				obj.Label.AcceptVisitor(this);
				Pop();
				obj.Controlled.AcceptVisitor(this);
			}
			if (casest.Else != null)
			{
				casest.Else.AcceptVisitor(this);
			}
			_expCalledForAll.visit(casest);
		}

		public virtual void visit(IExitStatement exit)
		{
			_expCalledForAll.visit(exit);
		}

		public virtual void visit(IContinueStatement cont)
		{
			_expCalledForAll.visit(cont);
		}

		public virtual void visit(IThisExpression thisexp)
		{
			_expCalledForAll.visit(thisexp);
		}

		public virtual void visit(IBaseExpression baseexp)
		{
			_expCalledForAll.visit(baseexp);
		}

		public virtual void visit(IEmptyStatement empty)
		{
			_expCalledForAll.visit(empty);
		}

		public virtual void visit(IReturnStatement returnst)
		{
			if (returnst.Condition != null)
			{
				Push(AccessFlag.Read);
				returnst.Condition.AcceptVisitor(this);
				Pop();
			}
			_expCalledForAll.visit(returnst);
		}

		public virtual void visit(IJumpStatement gotost)
		{
			if (gotost.Condition != null)
			{
				Push(AccessFlag.Read);
				gotost.Condition.AcceptVisitor(this);
				Pop();
			}
			_expCalledForAll.visit(gotost);
		}

		public virtual void visit(ILabelStatement label)
		{
			_expCalledForAll.visit(label);
		}

		public virtual void visit(ICommentStatement comment)
		{
			_expCalledForAll.visit(comment);
		}

		public virtual void visit(IPragmaStatement pragma)
		{
			if (pragma.Text == "implicit on")
			{
				m_bImplicitOn = true;
			}
			if (pragma.Text == "implicit off")
			{
				m_bImplicitOn = false;
			}
			_expCalledForAll.visit(pragma);
		}

		public virtual void visit(IErrorExpression errorexp)
		{
			_expCalledForAll.visit(errorexp);
		}

		public virtual void visit(INullExpression errorexp)
		{
			_expCalledForAll.visit(errorexp);
		}

		public virtual void visit(IQualifiedNameExpression qne)
		{
			_expCalledForAll.visit(qne);
		}

		public virtual void visit(IDefineReference defref)
		{
			_expCalledForAll.visit(defref);
		}

		public virtual void visit(IVariableReference varref)
		{
			if (varref.Instance != null)
			{
				varref.Instance.AcceptVisitor(this);
			}
			_expCalledForAll.visit(varref);
		}

		public virtual void visit(ITypeReference typeref)
		{
			if ((typeref as ITypeReference2).InstanceExpression != null)
			{
				(typeref as ITypeReference2).InstanceExpression.AcceptVisitor(this);
			}
			_expCalledForAll.visit(typeref);
		}

		public virtual void visit(IPouReference pouref)
		{
			if ((pouref as ITypeReference2).InstanceExpression != null)
			{
				(pouref as ITypeReference2).InstanceExpression.AcceptVisitor(this);
			}
			_expCalledForAll.visit(pouref);
		}

		public virtual void visit(IDefinedExpression defexp)
		{
			Push(AccessFlag.Read);
			defexp.ReferencedItem.AcceptVisitor(this);
			Pop();
			_expCalledForAll.visit(defexp);
		}

		public virtual void visit(IPragmaOperatorExpression popexp)
		{
			IExpression[] allOperands = popexp.AllOperands;
			for (int i = 0; i < allOperands.Length; i++)
			{
				allOperands[i].AcceptVisitor(this);
			}
			_expCalledForAll.visit(popexp);
		}

		public virtual void visit(IPragmaIfStatement pifst)
		{
			pifst.ConditionExpression.AcceptVisitor(this);
			pifst.IfThenStatement.AcceptVisitor(this);
			if (pifst.IfElseStatement != null)
			{
				pifst.IfElseStatement.AcceptVisitor(this);
			}
			_expCalledForAll.visit(pifst);
		}

		public virtual void visit(IBreakPointStatement bpstate)
		{
			_expCalledForAll.visit(bpstate);
		}

		public virtual void visit(IDefineStatement defstate)
		{
			_expCalledForAll.visit(defstate);
		}

		public virtual void visit(IHasTypeExpression hastype)
		{
			hastype.Instance.AcceptVisitor(this);
			_expCalledForAll.visit(hastype);
		}

		public virtual void visit(IIsEnumTypeExpression isenumtype)
		{
			_expCalledForAll.visit(isenumtype);
		}

		public virtual void visit(IHasAttributeExpression hasattribute)
		{
			hasattribute.ReferencedItem.AcceptVisitor(this);
			_expCalledForAll.visit(hasattribute);
		}

		public virtual void visit(IHasValueExpression hasvalue)
		{
			_expCalledForAll.visit(hasvalue);
		}

		public virtual void visit(IHasConstantValueExpression hasvalue)
		{
			_expCalledForAll.visit(hasvalue);
		}

		public virtual void visit(IPragmaAssertion assertion)
		{
			assertion.ConditionExpression.AcceptVisitor(this);
			_expCalledForAll.visit(assertion);
		}

		public virtual void visit(ICompilerVersionExpression compversion)
		{
			_expCalledForAll.visit(compversion);
		}
	}
}
