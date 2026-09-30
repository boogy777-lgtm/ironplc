using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IStatementVisitorNoTraversion
	{
		void visit(_ICompiledPOU cpou);

		void visit(_IWhileStatement whilst);

		void visit(_IRepeatStatement repeat);

		void visit(_IForStatement forloop);

		void visit(_IExitStatement exit);

		void visit(_IContinueStatement cont);

		void visit(_ISequenceStatement seq);

		void visit(_IIfStatement ifst);

		void visit(_IReturnStatement returnst);

		void visit(_IJumpStatement gotost);

		void visit(_ILabelStatement label);

		void visit(_ICommentStatement comment);

		void visit(_IPragmaStatement pragma);

		void visit(_IExpressionStatement expstat);

		void visit(_IEmptyStatement empty);

		void visit(_ICaseLabelStatement caselabel);

		void visit(_ICaseStatement casest);

		void visit(_IErrorStatement errorst);

		void visit(_INullStatement errorst);

		void visit(_IPragmaIfStatement pifst);

		void visit(_IBreakPointStatement bpstate);

		void visit(_IDefineStatement defstate);

		void visit(_IPragmaAssertion assertion);

		void visit(_ITryCatchStatement trycatch);
	}
}
