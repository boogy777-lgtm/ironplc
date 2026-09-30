using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledPOU6 : ICompiledPOU5, ICompiledPOU4, ICompiledPOU3, ICompiledPOU
	{
		Guid MessageGuid { get; }
	}
}
