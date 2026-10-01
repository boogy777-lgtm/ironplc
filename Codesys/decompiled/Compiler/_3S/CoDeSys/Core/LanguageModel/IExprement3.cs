using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExprement3 : IExprement2, IExprement
	{
		object VisitorAttribute { get; set; }

		ISourcePosition CreatePosition(int nProjectHandle, Guid objectGuid);
	}
}
