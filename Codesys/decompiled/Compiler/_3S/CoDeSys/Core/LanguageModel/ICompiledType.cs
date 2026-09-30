using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledType : IType, IArchivable
	{
		bool IsInteger { get; }

		ICompiledType BaseType { get; }

		ICompiledType DeRefType { get; }

		[Obsolete("Use IsCompatible(ICompiledType type, IScope scope) instead.")]
		bool IsCompatible(ICompiledType type);

		bool IsEqual(ICompiledType type);

		int Size(IScope scope);
	}
}
