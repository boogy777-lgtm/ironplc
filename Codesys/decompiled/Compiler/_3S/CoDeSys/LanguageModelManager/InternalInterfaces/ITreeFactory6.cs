using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITreeFactory6 : ITreeFactory5, ITreeFactory4, ITreeFactory3, ITreeFactory2
	{
		_IImplicitConversionExpression CreateImplicitConversionExpression(TypeClass from, TypeClass to, _IExpression expression);
	}
}
