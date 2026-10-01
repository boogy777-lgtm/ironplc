using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IParser4 : IParser3, IParser2, IParser
	{
		ICompiledType ParseTypeDeclaration(out IMessage message);
	}
}
