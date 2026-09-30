using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITreeFactory3 : ITreeFactory2
	{
		_IHasConstantTypeExpression CreateHasConstantTypeExpression(_IExpression Constant, bool bConstantTypeReplaced);

		_ILiteralExpression CreateStringLiteralExpression(string stValue, TypeClass constantType, StringEncoding stringEncoding);
	}
}
