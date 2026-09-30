using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDataLocationInformation3 : IDataLocationInformation2, IDataLocationInformation
	{
		Guid ApplicationGuid { get; }
	}
}
