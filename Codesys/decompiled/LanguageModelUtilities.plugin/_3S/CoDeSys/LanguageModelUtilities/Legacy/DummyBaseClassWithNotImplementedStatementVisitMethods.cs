using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.Legacy
{
	internal class DummyBaseClassWithNotImplementedStatementVisitMethods
	{
		public virtual void visit(IWhileStatement whilst)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IRepeatStatement repeat)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IForStatement forloop)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IExitStatement exit)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IContinueStatement cont)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(ISequenceStatement seq)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IIfStatement ifst)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IReturnStatement returnst)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IJumpStatement gotost)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(ILabelStatement label)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(ICommentStatement comment)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IPragmaStatement pragma)
		{
			throw new NotSupportedException();
		}

		public virtual void visit(IExpressionStatement expstat)
		{
			throw new NotSupportedException();
		}
	}
}
