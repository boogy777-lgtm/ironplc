using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompilerServiceExprementWriter
	{
		string WriteExprement(_IExprement exprement, WriteExprementFlags flags);
	}
}
