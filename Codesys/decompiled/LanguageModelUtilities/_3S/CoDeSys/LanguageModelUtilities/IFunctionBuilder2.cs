using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IFunctionBuilder2 : IFunctionBuilder, IPouBuilder, IHasVarDeclaration, IHasAttributes
	{
		int Slot { get; set; }

		Guid TaskGuid { get; set; }
	}
}
