using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IStatementVisitorNoTraversion
	{
		void visit(ICompiledPOU cpou);

		void visit(IWhileStatement whilst);

		void visit(IRepeatStatement repeat);

		void visit(IForStatement forloop);

		void visit(IExitStatement exit);

		void visit(IContinueStatement cont);

		void visit(ISequenceStatement seq);

		void visit(IIfStatement ifst);

		void visit(IReturnStatement returnst);

		void visit(IJumpStatement gotost);

		void visit(ILabelStatement label);

		void visit(ICommentStatement comment);

		void visit(IPragmaStatement pragma);

		void visit(IExpressionStatement expstat);

		void visit(IEmptyStatement empty);

		void visit(ICaseLabelStatement caselabel);

		void visit(ICaseStatement casest);

		void visit(IPragmaIfStatement pifst);

		void visit(IBreakPointStatement bpstate);

		void visit(IDefineStatement defstate);

		void visit(IPragmaAssertion assertion);
	}
}
