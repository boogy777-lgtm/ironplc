using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledPOU2 : ICompiledPOU
	{
		[Obsolete("Do not use this function: it will always return null")]
		string GetCode();
	}
}
