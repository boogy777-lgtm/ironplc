using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IAddressWatchVarDescription : IReferencedInstanceWatchVarDescription
	{
		string DereferencedPointer { get; }

		void Initialize(Guid gdApplication, string stDevice, string stApplication, ulong ulAddressInstance, ICompiledType type, string stInstancePath, string stPointerExpression);
	}
}
