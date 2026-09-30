using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IUserdefTypeAttributedString : IAttributedString
	{
		IUserdefType UserdefType { get; }

		bool Available { get; }
	}
}
