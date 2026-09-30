using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodegenerator4 : ICodegenerator3, ICodegenerator2, ICodegenerator
	{
		void GenerateCallInstanceAccess(ICallInstanceExpression callinstexp);

		void GenerateCall2(ICallExpression call, ISignature signToCall, KindOfCall calltype, IExpression expCallTarget, IAssignmentExpression expInstanceAssignment);
	}
}
