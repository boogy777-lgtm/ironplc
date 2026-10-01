using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Factories.Builder;
using CODESYS.WhiteParseTrees.Nodes.Statements;

namespace CODESYS.WhiteParseTrees.Nodes.Factories
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class BuilderFactory : IWhiteParseTreeBuilderFactory3, IWhiteParseTreeBuilderFactory2, IWhiteParseTreeBuilderFactory
	{
		public IProgramNameStep CreateProgramBuilder()
		{
			return new ProgramBuilder();
		}

		public IFunctionNameStep CreateFunctionBuilder()
		{
			return new FunctionBuilder();
		}

		public IMethodNameStep CreateMethodBuilder()
		{
			return new MethodBuilder();
		}

		public IFunctionBlockNameStep CreateFunctionBlockBuilder()
		{
			return new FunctionBlockBuilder();
		}

		public IUnionNameStep CreateUnionBuilder()
		{
			return new UnionBuilder();
		}

		public IPropertyNameStep CreatePropertyBuilder()
		{
			return new PropertyBuilder();
		}

		public IInterfaceNameStep3 CreateInterfaceBuilder()
		{
			return new InterfaceBuilder();
		}

		IInterfaceNameStep2 IWhiteParseTreeBuilderFactory2.CreateInterfaceBuilder()
		{
			return new InterfaceBuilder();
		}

		IInterfaceNameStep IWhiteParseTreeBuilderFactory.CreateInterfaceBuilder()
		{
			return new InterfaceBuilder();
		}

		public IVarDeclNameStep CreateVariableDeclarationBuilder()
		{
			return new VariableDeclarationBuilder();
		}

		public ILValueStep CreateAssignmentBuilder()
		{
			return new AssignmentBuilder();
		}

		public ICalleeStep CreateCallBuilder()
		{
			return new CallBuilder();
		}

		public ISwitchCaseStep CreateCaseBuilder()
		{
			return new CaseBuilder();
		}

		public ICaseExpressionListStep CreateCaseLabelBuilder()
		{
			return new CaseLabelBuilder();
		}

		public IForStartExpressiontStep CreateForBuilder()
		{
			return new ForBuilder();
		}

		public IIfConditionStep CreateIfBuilder()
		{
			return new IfBuilder();
		}

		public IElseIfConditionStep CreateElseIfBuilder()
		{
			return new ElseIfBuilder();
		}

		public IWhiteCase CreateWhiteCase(IWhiteCaseLabelStatement label, IWhiteSequenceStatement controlled)
		{
			return new WhiteCase(label, controlled);
		}
	}
}
