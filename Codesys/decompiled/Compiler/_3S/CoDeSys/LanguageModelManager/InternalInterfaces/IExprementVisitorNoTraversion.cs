using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IExprementVisitorNoTraversion
	{
		IStandardTraverser Traverser { get; set; }

		bool bResolveCompoAccessExpression { get; }

		bool DoCallExpression { get; }

		bool DoAssignExpression { get; }

		void visit(_ICompiledPOU cpou);

		void visit(_IWhileStatement whilst);

		void visit(_IRepeatStatement repeat);

		void visit(_IForStatement forloop);

		void visit(_IExitStatement exit);

		void visit(_IContinueStatement cont);

		void visit(_ISequenceStatement seq);

		void visit(_IAssignmentExpression assign);

		void visit(_IIfStatement ifst);

		void visit(_IReturnStatement returnst);

		void visit(_IJumpStatement gotost);

		void visit(_ILabelStatement label);

		void visit(_ICommentStatement comment);

		void visit(_IPragmaStatement pragma);

		void visit(_IExpressionStatement expstat);

		void visit(_ICallExpression call);

		void visit(_IOperatorExpression op);

		void visit(_IConversionExpression conv);

		void visit(_ICastExpression cast);

		void visit(_IThisExpression thisexp);

		void visit(_IBaseExpression baseexp);

		void visit(_ILiteralExpression literal);

		void visit(_IAddressExpression address);

		void visit(_IVariableExpression variable, AccessFlag access);

		void visit(_IIndexAccessExpression indexaccess);

		void visit(_ICompoAccessExpression compo, AccessFlag access);

		void visit(_IDeRefAccessExpression deref);

		void visit(_ICopyScopeExpression copyexp);

		void visit(_IGlobalScopeExpression globexp);

		void visit(_ISystemScopeExpression systemscope);

		void visit(_IEmptyStatement empty);

		void visit(_ICaseRangeExpression caserange);

		void visit(_ICaseLabelStatement caselabel);

		void visit(_ICaseStatement casest);

		void visit(_IErrorExpression errorexp);

		void visit(_IErrorStatement errorst);

		void visit(_INullExpression errorexp);

		void visit(_INullStatement errorst);

		void visit(_IQualifiedNameExpression qne);

		void visit(_IVariableDeclarationStatement vds);

		void visit(_IVariableDeclarationListStatement vdls);

		void visit(_IPOUDeclarationStatement pds);

		void visit(_ITypeDeclarationStatement tds);

		void visit(_IEnumDeclarationStatement eds);

		void visit(_IEnumDeclarationListStatement eds);

		void visit(_IMultipleIndexInitialization errorst);

		void visit(_IArrayInitialization errorexp);

		void visit(_IStructureInitialization errorst);

		void visit(_IDefineReference defref);

		void visit(_IVariableReference varref);

		void visit(_ITypeReference typeref);

		void visit(_IPouReference pouref);

		void visit(_ITaskReference taskref);

		void visit(_IResourceReference resref);

		void visit(_IDefinedExpression defexp);

		void visit(_IPragmaOperatorExpression popexp);

		void visit(_IPragmaIfStatement pifst);

		void visit(_IBreakPointStatement bpstate);

		void visit(_IDefineStatement defstate);

		void visit(_IXRefExpression xref);

		void visit(_IHasTypeExpression hastype);

		void visit(_IIsEnumTypeExpression isenumtype);

		void visit(_IHasAttributeExpression hasattribute);

		void visit(_IHasValueExpression hasvalue);

		void visit(_IHasConstantValueExpression hasvalue);

		void visit(_IPragmaAssertion assertion);

		void visit(_ICompilerVersionExpression compversion);

		void visit(_INewExpression newexp);

		void visit(_ITypeExpression newexp);

		void visit(_ITryCatchStatement trycatch);
	}
}
