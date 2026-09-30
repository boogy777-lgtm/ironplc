using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IEnumDeclarationListStatement : IStatement, IExprement
	{
		List<IEnumDeclarationStatement> Enumerations { get; }

		IType BaseType { get; }
	}
}
