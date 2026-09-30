using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IIECExprInfo
	{
		IType Type { get; }

		IVariable Var { get; }

		bool IsBitAccess { get; }

		bool IsProperty { get; }

		string PropertyMonitor { get; }
	}
}
