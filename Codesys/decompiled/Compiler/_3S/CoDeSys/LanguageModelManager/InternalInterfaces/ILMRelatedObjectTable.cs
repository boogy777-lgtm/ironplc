using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ILMRelatedObjectTable
	{
		IEnumerable<Guid> Keys { get; }

		void Add(Guid key, IEnumerable<Guid> values);

		IEnumerable<Guid> GetValues(Guid key);
	}
}
