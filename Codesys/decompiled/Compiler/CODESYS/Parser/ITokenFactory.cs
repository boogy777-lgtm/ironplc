using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface ITokenFactory
	{
		_IToken CreateEmptyToken();

		_IToken CreateToken(long position, short positionOffset, int length);
	}
}
