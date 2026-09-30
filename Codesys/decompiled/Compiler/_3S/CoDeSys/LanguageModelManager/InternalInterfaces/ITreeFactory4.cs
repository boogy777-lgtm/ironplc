using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITreeFactory4 : ITreeFactory3, ITreeFactory2
	{
		_IPartialAccessExpression CreatePartialAccessExpression(_IExpression left, DirectVariableSize partSize, int partOffset);
	}
}
