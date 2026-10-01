using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledPOU7 : ICompiledPOU6, ICompiledPOU5, ICompiledPOU4, ICompiledPOU3, ICompiledPOU
	{
		IMessage4[] GetMessages();
	}
}
