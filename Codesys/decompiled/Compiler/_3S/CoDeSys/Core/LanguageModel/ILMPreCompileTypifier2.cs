using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileTypifier2 : ILMPreCompileTypifier
	{
		IStatement CreateTypifiedParseTree(Guid guidObject, ConversionOptions options);

		IStatement TypifyStatement(IStatement stmt, Guid guidObject, ConversionOptions options);
	}
}
