using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Serializable]
	[ReleasedClass]
	public class CompiledPOUChangedEventArgs2 : CompiledPOUChangedEventArgs
	{
		private IDictionary<long, long> _mapOldToNewPositions;

		public IDictionary<long, long> MapSourcePositions => _mapOldToNewPositions;

		public CompiledPOUChangedEventArgs2(Guid guidApplication, ICompiledPOU cpouOld, ICompiledPOU cpouNew, bool bSignificant, IDictionary<long, long> mapOldToNewPositions)
			: base(guidApplication, cpouOld, cpouNew, bSignificant)
		{
			_mapOldToNewPositions = mapOldToNewPositions;
		}
	}
}
