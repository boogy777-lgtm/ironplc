using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodegenerator
	{
		bool MotorolaByteOrder { get; }

		int StackDisplacement { get; }

		int ParameterDisplacement { get; }

		string[] FunctionsToLinkAlways { get; }

		void Setup(ITargetSettings targetSettings);

		void Initialize(ICodeAdapter codapt, IScope globalScope);

		void StartGeneration();

		void EndGeneration();

		void Generate(ICompiledPOU cpou, bool bKeepCompileInformation);

		void GenerateLiteral(ILiteralExpression literal);

		void GenerateVarConstant(IVariableExpression varexp, ILiteralValue literal);

		void GenerateVarAbsolut(IVariableExpression varexp, int iArea, int nAddress, IIndexInfo indexinfo, ICompiledType ctype, int nTypeSize, IAccessMode am);

		void GenerateDirectAddress(IAddressExpression addr, int iArea, int nAddress, ICompiledType ctype, int nTypeSize, IAccessMode am);

		void GenerateVarRelative(IVariableExpression varexp, int nOffset, IIndexInfo indexinfo, ICompiledType ctype, int nTypeSize, IAccessMode am);

		void GenerateBitAccess(IExpression varexp, ICompiledType ctype, IAccessMode am, int nBitNr);

		void GenerateCall(ICallExpression call, ISignature signToCall, KindOfCall calltype, IExpression expCallTarget);

		bool Generate(IEmptyStatement empty);

		void Generate(ISequenceStatement seq);

		bool Generate(IWhileStatement whilst);

		bool Generate(IRepeatStatement repeat);

		bool Generate(IForStatement forloop);

		bool Generate(IExitStatement exit);

		bool Generate(IContinueStatement cont);

		bool Generate(IAssignmentExpression assign);

		bool Generate(IIfStatement ifst);

		bool Generate(IJumpStatement gotost);

		bool Generate(ILabelStatement label);

		bool Generate(ICommentStatement comment);

		bool Generate(IPragmaStatement pragma);

		bool Generate(IExpressionStatement expstat);

		bool Generate(IOperatorExpression op, int[] anNestingDepths, ICompiledType type);

		bool Generate(IConversionExpression conv);

		bool Generate(IThisExpression thisexp, IExpression expInstead);

		bool GenerateInstanceAccess(IDeRefAccessExpression deref, int nOffset, IIndexInfo indexinfo, ICompiledType ctype, IAccessMode am);

		bool Generate(IDeRefAccessExpression deref, int nOffset, IIndexInfo indexinfo, ICompiledType ctype, IAccessMode am);

		bool Generate(ICaseRangeExpression caserange);

		bool Generate(ICaseLabelStatement caselabel);

		bool Generate(ICaseStatement casest);

		bool Generate(IBreakPointStatement bpstate);

		bool NeedsExternalFunctionCall(IOperatorExpression op, ICompiledType type, ref string stFunctionName, ref TypeClass tcWithType);

		bool NeedsExternalFunctionCall(IConversionExpression conv, ref string stFunctionName, ref TypeClass tcWithType);
	}
}
