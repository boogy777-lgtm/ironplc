using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal sealed class VariableVisitor : EmptyVisitor
	{
		private readonly IVariableVisitor _varvis;

		public VariableVisitor(IVariableVisitor varvis)
		{
			_varvis = varvis;
		}

		public override void visit(IVariableExpression variable, AccessFlag access, IPrecompileScope5 scope)
		{
			_varvis.visit(variable, access, scope);
		}
	}
}
