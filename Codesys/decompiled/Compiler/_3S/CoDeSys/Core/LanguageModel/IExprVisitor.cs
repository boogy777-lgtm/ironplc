using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExprVisitor
	{
		void visit(IWhileStatement whilst);

		void visit(IRepeatStatement repeat);

		void visit(IForStatement forloop);

		void visit(IExitStatement exit);

		void visit(IContinueStatement cont);

		void visit(ISequenceStatement seq);

		void visit(IAssignmentExpression assign);

		void visit(IIfStatement ifst);

		void visit(IReturnStatement returnst);

		void visit(IJumpStatement gotost);

		void visit(ILabelStatement label);

		void visit(ICommentStatement comment);

		void visit(IPragmaStatement pragma);

		void visit(IExpressionStatement expstat);

		void visit(ICallExpression call);

		void visit(IOperatorExpression op);

		void visit(IConversionExpression conv);

		void visit(IThisExpression thisexp);

		void visit(IBaseExpression baseexp);

		void visit(ILiteralExpression literal);

		void visit(IAddressExpression address);

		void visit(IVariableExpression variable);

		void visit(IIndexAccessExpression indexaccess);

		void visit(ICompoAccessExpression compo);

		void visit(IDeRefAccessExpression deref);

		void visit(IGlobalScopeExpression globexp);

		void visit(IEmptyStatement empty);

		void visit(ICaseRangeExpression caserange);

		void visit(ICaseLabelStatement caselabel);

		void visit(ICaseStatement casest);

		void visit(IBreakPointStatement bpstate);

		void visit(IDefineReference defref);

		void visit(IVariableReference varref);

		void visit(ITypeReference typeref);

		void visit(IPouReference pouref);

		void visit(IDefinedExpression defexp);

		void visit(IPragmaOperatorExpression popexp);

		void visit(IPragmaIfStatement pifst);

		void visit(IDefineStatement defstate);

		void visit(IHasTypeExpression hastype);

		void visit(IHasAttributeExpression hasattribute);

		void visit(IHasValueExpression hasvalue);

		void visit(IPragmaAssertion assertion);
	}
}
