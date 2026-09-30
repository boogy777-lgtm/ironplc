using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExpression : IExprement
	{
		ICompiledType Type { get; }

		int ScratchOffset { get; }

		[Obsolete("this flag is not supported any more!")]
		bool IsStatement { get; }

		bool IsLiteral { get; }

		ILiteralValue Literal(IScope scope);

		IDataLocation DataLocation(IScope scope);
	}
}
