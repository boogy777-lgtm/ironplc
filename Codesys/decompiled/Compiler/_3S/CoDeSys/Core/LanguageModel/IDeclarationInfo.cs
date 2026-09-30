using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDeclarationInfo
	{
		string Name { get; }

		IType DerivedType { get; }

		AccessFlag Access { get; }

		Guid ObjectGuid { get; }

		VarFlag VarFlag { get; }

		string LibraryPath { get; }
	}
}
