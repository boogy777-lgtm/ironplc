using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IApplicationContent2 : IApplicationContent
	{
		IEnumerable<IPOUMethodInfoStruct> POUMethods { get; }
	}
}
