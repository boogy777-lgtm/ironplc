using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITreeFactory5 : ITreeFactory4, ITreeFactory3, ITreeFactory2
	{
		_IProjectDefinedExpression CreateProjectDefinedExpression(_IDefineReference defineReference);
	}
}
