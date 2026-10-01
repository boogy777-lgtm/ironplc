using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class StatementTraverser : IExprVisitor2, IExprVisitor
	{
		private IStatementVisitorNoTraversion _visitorCalledForAll;

		internal StatementTraverser(IStatementVisitorNoTraversion visitorCalledForAll)
		{
			_visitorCalledForAll = visitorCalledForAll;
		}

		public virtual void visit(ICompiledPOU cpou)
		{
			cpou.ParseTree.AcceptVisitor(this);
			_visitorCalledForAll.visit(cpou);
		}

		public virtual void visit(IWhileStatement whilst)
		{
			whilst.Controlled.AcceptVisitor(this);
			_visitorCalledForAll.visit(whilst);
		}

		public virtual void visit(IRepeatStatement repeat)
		{
			repeat.Controlled.AcceptVisitor(this);
			_visitorCalledForAll.visit(repeat);
		}

		public virtual void visit(IForStatement forloop)
		{
			forloop.CounterStart.AcceptVisitor(this);
			forloop.Controlled.AcceptVisitor(this);
			_visitorCalledForAll.visit(forloop);
		}

		public virtual void visit(ISequenceStatement seq)
		{
			IStatement[] statements = seq.Statements;
			for (int i = 0; i < statements.Length; i++)
			{
				statements[i].AcceptVisitor(this);
			}
			_visitorCalledForAll.visit(seq);
		}

		public virtual void visit(IIfStatement ifst)
		{
			ifst.IfThen.AcceptVisitor(this);
			IElseIf[] elseIf = ifst.ElseIf;
			if (elseIf != null)
			{
				IElseIf[] array = elseIf;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Controlled.AcceptVisitor(this);
				}
			}
			if (ifst.IfElse != null)
			{
				ifst.IfElse.AcceptVisitor(this);
			}
			_visitorCalledForAll.visit(ifst);
		}

		public virtual void visit(IExpressionStatement expstat)
		{
			_visitorCalledForAll.visit(expstat);
		}

		public virtual void visit(ICaseLabelStatement caselabel)
		{
			_visitorCalledForAll.visit(caselabel);
		}

		public virtual void visit(ICaseStatement casest)
		{
			ICase[] cases = casest.Cases;
			foreach (ICase obj in cases)
			{
				obj.Label.AcceptVisitor(this);
				obj.Controlled.AcceptVisitor(this);
			}
			if (casest.Else != null)
			{
				casest.Else.AcceptVisitor(this);
			}
			_visitorCalledForAll.visit(casest);
		}

		public virtual void visit(IExitStatement exit)
		{
			_visitorCalledForAll.visit(exit);
		}

		public virtual void visit(IContinueStatement cont)
		{
			_visitorCalledForAll.visit(cont);
		}

		public virtual void visit(IEmptyStatement empty)
		{
			_visitorCalledForAll.visit(empty);
		}

		public virtual void visit(IReturnStatement returnst)
		{
			_visitorCalledForAll.visit(returnst);
		}

		public virtual void visit(IJumpStatement gotost)
		{
			_visitorCalledForAll.visit(gotost);
		}

		public virtual void visit(ILabelStatement label)
		{
			_visitorCalledForAll.visit(label);
		}

		public virtual void visit(ICommentStatement comment)
		{
			_visitorCalledForAll.visit(comment);
		}

		public virtual void visit(IPragmaStatement pragma)
		{
			_visitorCalledForAll.visit(pragma);
		}

		public virtual void visit(IPragmaIfStatement pifst)
		{
			pifst.IfThenStatement.AcceptVisitor(this);
			if (pifst.IfElseStatement != null)
			{
				pifst.IfElseStatement.AcceptVisitor(this);
			}
			_visitorCalledForAll.visit(pifst);
		}

		public virtual void visit(IBreakPointStatement bpstate)
		{
			_visitorCalledForAll.visit(bpstate);
		}

		public virtual void visit(IDefineStatement defstate)
		{
			_visitorCalledForAll.visit(defstate);
		}

		public virtual void visit(IPragmaAssertion assertion)
		{
			_visitorCalledForAll.visit(assertion);
		}

		public virtual void visit(IAssignmentExpression assign)
		{
		}

		public virtual void visit(ICallExpression call)
		{
		}

		public virtual void visit(IOperatorExpression op)
		{
		}

		public virtual void visit(ICastExpression cast)
		{
		}

		public virtual void visit(INewExpression newexp)
		{
		}

		public virtual void visit(ITypeExpression typeexp)
		{
		}

		public virtual void visit(IConversionExpression conv)
		{
		}

		public virtual void visit(IIndexAccessExpression indexaccess)
		{
		}

		public virtual void visit(ILiteralExpression literal)
		{
		}

		public virtual void visit(ICompoAccessExpression compo)
		{
		}

		public virtual void visit(IDeRefAccessExpression deref)
		{
		}

		public virtual void visit(IGlobalScopeExpression globexp)
		{
		}

		public virtual void visit(ISystemScopeExpression systemscope)
		{
		}

		public virtual void visit(ICaseRangeExpression caserange)
		{
		}

		public virtual void visit(IThisExpression thisexp)
		{
		}

		public virtual void visit(IBaseExpression baseexp)
		{
		}

		public virtual void visit(IErrorExpression errorexp)
		{
		}

		public virtual void visit(INullExpression errorexp)
		{
		}

		public virtual void visit(IQualifiedNameExpression qne)
		{
		}

		public virtual void visit(IVariableDeclarationStatement vds)
		{
		}

		public virtual void visit(IVariableDeclarationListStatement vdls)
		{
		}

		public virtual void visit(IPOUDeclarationStatement pds)
		{
		}

		public virtual void visit(ITypeDeclarationStatement tds)
		{
		}

		public virtual void visit(IEnumDeclarationStatement eds)
		{
		}

		public virtual void visit(IEnumDeclarationListStatement eds)
		{
		}

		public virtual void visit(IDefineReference defref)
		{
		}

		public virtual void visit(IVariableReference varref)
		{
		}

		public virtual void visit(ITypeReference typeref)
		{
		}

		public virtual void visit(IPouReference pouref)
		{
		}

		public virtual void visit(IDefinedExpression defexp)
		{
		}

		public virtual void visit(IPragmaOperatorExpression popexp)
		{
		}

		public virtual void visit(IHasTypeExpression hastype)
		{
		}

		public virtual void visit(IIsEnumTypeExpression isenumtype)
		{
		}

		public virtual void visit(IHasAttributeExpression hasattribute)
		{
		}

		public virtual void visit(IHasValueExpression hasvalue)
		{
		}

		public virtual void visit(IHasConstantValueExpression hasvalue)
		{
		}

		public virtual void visit(ICompilerVersionExpression compversion)
		{
		}

		public void visit(IAddressExpression address)
		{
		}

		public void visit(IVariableExpression variable)
		{
		}
	}
}
