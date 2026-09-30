using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IUserdefType2 : IUserdefType, IType, IArchivable
	{
		IExpression NameExpression { get; }
	}
}
