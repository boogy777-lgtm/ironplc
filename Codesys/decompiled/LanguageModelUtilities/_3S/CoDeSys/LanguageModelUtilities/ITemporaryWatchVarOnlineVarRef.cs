using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Online;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ITemporaryWatchVarOnlineVarRef : IOnlineVarRef
	{
		void Initialize(Guid gdApplication, IReferencedInstanceWatchVarDescription referencedInstanceWatchVarDescription);

		void WriteAddress(ulong ulAddress);
	}
}
