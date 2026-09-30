using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ITypeInfo
	{
		bool IsEquivalent(TypeClass tc1, TypeClass tc2);

		bool IsBlock(TypeClass tc);

		bool IsSigned(TypeClass tc);

		bool IsLType(TypeClass tc);

		bool IsBoolean(TypeClass tc);

		bool IsNumber(TypeClass tc);

		bool IsInteger(TypeClass tc);

		[Obsolete("Use IsLInteger(TypeClass tc, IScope scope) instead.")]
		bool IsLInteger(TypeClass tc);

		bool IsReal(TypeClass tc);

		bool IsString(TypeClass tc);

		[Obsolete("Use GetSize(TypeClass tc, IScope scope) instead.")]
		int GetSize(TypeClass tc);
	}
}
