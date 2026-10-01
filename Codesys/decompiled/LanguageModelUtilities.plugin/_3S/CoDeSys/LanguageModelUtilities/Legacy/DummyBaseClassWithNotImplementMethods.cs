using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.Legacy
{
	internal class DummyBaseClassWithNotImplementMethods : DummyBaseClassWithNotImplementedStatementVisitMethods
	{
		public virtual void visit(IAssignmentExpression assign)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(ICallExpression call)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IEmptyStatement empty)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(ICaseRangeExpression caserange)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(ICaseLabelStatement caselabel)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(ICaseStatement casest)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IBreakPointStatement bpstate)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IDefineReference defref)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IVariableReference varref)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(ITypeReference typeref)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IPouReference pouref)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IDefinedExpression defexp)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IPragmaOperatorExpression popexp)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IPragmaIfStatement pifst)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IDefineStatement defstate)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IHasTypeExpression hastype)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IHasAttributeExpression hasattribute)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IHasValueExpression hasvalue)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IPragmaAssertion assertion)
		{
			throw new NotSupportedException();
		}
	}
}
