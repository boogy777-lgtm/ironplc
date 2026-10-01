using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVariable5 : IVariable4, IVariable3, IVariable2, IVariable
	{
		Guid MessageGuid { get; }
	}
}
