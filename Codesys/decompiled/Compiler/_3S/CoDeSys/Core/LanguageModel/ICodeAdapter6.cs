using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodeAdapter6 : ICodeAdapter5, ICodeAdapter4, ICodeAdapter3, ICodeAdapter2, ICodeAdapter
	{
		object GetExpressionProperty(ExpressionProperties ep, IExpression expr);

		bool GenerateExceptionInfo(ISignature signToCall);
	}
}
