using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IExprement3 : _IExprement2, _IExprement, IExprement3, IExprement2, IExprement
	{
		T Accept<T>(IExprementVisitor<T> visitor);
	}
}
