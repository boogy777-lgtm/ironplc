using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class StandardTraverser : IExprVisitor2, IExprVisitor, IStandardTraverser
	{
		protected IExprementVisitorNoTraversion _expCalledForAll;

		private LStack<StandardTraverserStackElement> _stack = new LStack<StandardTraverserStackElement>();

		private bool m_bAbort;

		private bool m_bImplicitOn;

		private ISignature4 _localSignature;

		private IPreCompileContext9 _localApplication;

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

		public StandardTraverserStackElement TopOfStack => _stack.Peek();

		public AccessFlag Access => _stack.Peek()._access;

		public IPrecompileScope5 Scope => TopOfStack._scope;

		public StandardTraverser()
		{
		}

		public StandardTraverser(IExprementVisitorNoTraversion expCalledForAll, ISignature4 localSignature, IPreCompileContext9 localApplication)
		{
			_expCalledForAll = expCalledForAll;
			_expCalledForAll.Traverser = this;
			_localSignature = localSignature;
			_localApplication = localApplication;
			Push(AccessFlag.Read, HelperScope._CreateGlobalScope(localSignature, localApplication));
		}

		public void Reset()
		{
			m_bAbort = false;
			m_bImplicitOn = false;
			_stack.Clear();
			Push(AccessFlag.Read, HelperScope._CreateGlobalScope(_localSignature, _localApplication));
		}

		public void Reset(IExprementVisitorNoTraversion expCalledForAll)
		{
			Reset();
			_expCalledForAll = expCalledForAll;
			_expCalledForAll.Traverser = this;
		}

		public void Push(AccessFlag access, HelperScope scope)
		{
			_stack.Push(new StandardTraverserStackElement(access, scope));
		}

		public void Push(AccessFlag access)
		{
			_stack.Push(new StandardTraverserStackElement(access, TopOfStack._scope));
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
			IVariable variable2 = null;
			ISignature signResolved = null;
			HelperScope scope = null;
			IVariable[] variable3 = null;
			ISignature[] signature = null;
			TopOfStack._scope.FindDeclaration(variable.Name, out variable3, out signature, out scope);
			if (variable3 != null && variable3.Length != 0)
			{
				variable2 = variable3[0];
				signResolved = signature[0];
			}
			else if (signature != null && signature.Length != 0)
			{
				signResolved = signature[0];
			}
			if (variable2 != null)
			{
				TopOfStack._typeResolved = variable2.Type;
			}
			_expCalledForAll.visit(variable, TopOfStack._access, TopOfStack._scope);
			TopOfStack._varResolved = variable2;
			TopOfStack._signResolved = signResolved;
			TopOfStack._scopeResolved = scope;
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
			ISignature signResolved = TopOfStack._signResolved;
			IVariable varResolved = TopOfStack._varResolved;
			IType type = TopOfStack._typeResolved;
			Pop();
			ISignature signature = ((varResolved == null || varResolved.Type.Class != TypeClass.Userdef) ? signResolved : TopOfStack._scope.FindSignature(varResolved.Type as IUserdefType));
			if (signature != null && type == null)
			{
				type = ParseType(signature.Name);
			}
			if (type != null && type is IUserdefType && signature == null)
			{
				signature = TopOfStack._scope.FindSignature(type as IUserdefType);
			}
			if (signature == null)
			{
				return;
			}
			if (call.Condition != null)
			{
				Push(AccessFlag.Read);
				call.Condition.AcceptVisitor(this);
				Pop();
			}
			HelperScope scope = TopOfStack._scope.CreateUserdefScope(signature as ISignature4);
			IAssignmentExpression[] inputAssigns = call.InputAssigns;
			foreach (IAssignmentExpression assignmentExpression in inputAssigns)
			{
				if (assignmentExpression != null)
				{
					Push(AccessFlag.Write, scope);
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
					Push(AccessFlag.Read, scope);
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
					TopOfStack._access = AccessFlag.Write | AccessFlag.Address;
				}
				obj.AcceptVisitor(this);
			}
			_expCalledForAll.visit(op);
			TopOfStack.ClearResult();
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
			ICompiledType compiledType = null;
			ISignature signature = null;
			HelperScope helperScope = null;
			ICompiledType compiledType2 = null;
			IVariable variable = null;
			ISignature signature2 = null;
			HelperScope helperScope2 = null;
			Push(TopOfStack._access);
			compo.Left.AcceptVisitor(this);
			compiledType = TopOfStack._typeResolved as ICompiledType;
			_ = TopOfStack._varResolved;
			signature = TopOfStack._signResolved;
			helperScope = TopOfStack._scopeResolved;
			Pop();
			Push(TopOfStack._access);
			HelperScope helperScope3 = TopOfStack._scope;
			if (helperScope != null)
			{
				helperScope3 = helperScope;
			}
			if (compiledType != null && compiledType.DeRefType.Class == TypeClass.Userdef)
			{
				TopOfStack._scope = helperScope3.CreateUserdefScope(helperScope3.FindSignature(compiledType.DeRefType as IUserdefType) as ISignature4);
			}
			else if (signature != null)
			{
				TopOfStack._scope = helperScope3.CreateUserdefScope(signature as ISignature4);
			}
			else if (helperScope != null)
			{
				TopOfStack._scope = helperScope;
			}
			helperScope3 = TopOfStack._scope;
			compo.Right.AcceptVisitor(this);
			variable = TopOfStack._varResolved;
			compiledType2 = TopOfStack._typeResolved as ICompiledType;
			signature2 = TopOfStack._signResolved;
			helperScope2 = TopOfStack._scopeResolved;
			Pop();
			_expCalledForAll.visit(compo, TopOfStack._access, TopOfStack._scope);
			TopOfStack._typeResolved = compiledType2;
			TopOfStack._varResolved = variable;
			TopOfStack._signResolved = signature2;
			if (helperScope2 == null)
			{
				TopOfStack._scopeResolved = helperScope3;
			}
			else
			{
				TopOfStack._scopeResolved = helperScope2;
			}
			if (compiledType != null && compiledType2 != null && compiledType.DeRefType.IsInteger && compiledType2.DeRefType != null)
			{
				TopOfStack._typeResolved = ParseType("BOOL");
			}
		}

		public virtual void visit(IDeRefAccessExpression deref)
		{
			deref.Base.AcceptVisitor(this);
			_expCalledForAll.visit(deref);
		}

		public virtual void visit(IGlobalScopeExpression globexp)
		{
			Push(TopOfStack._access, TopOfStack._scope.CreateGlobalScope());
			globexp.Base.AcceptVisitor(this);
			ICompiledType typeResolved = TopOfStack._typeResolved as ICompiledType;
			ISignature signResolved = TopOfStack._signResolved;
			IVariable varResolved = TopOfStack._varResolved;
			HelperScope scopeResolved = TopOfStack._scopeResolved;
			Pop();
			TopOfStack._typeResolved = typeResolved;
			TopOfStack._signResolved = signResolved;
			TopOfStack._varResolved = varResolved;
			TopOfStack._scopeResolved = scopeResolved;
			_expCalledForAll.visit(globexp);
		}

		public virtual void visit(ISystemScopeExpression systemscope)
		{
			HelperScope scope = TopOfStack._scope.CreateSystemScope();
			Push(TopOfStack._access, scope);
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
