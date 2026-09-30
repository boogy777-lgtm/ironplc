using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager25 : ILanguageModelManager24, ILanguageModelManager23, ILanguageModelManager22, ILanguageModelManager21
	{
		event EventHandler<LibraryContextDeletedEventArgs> LibraryContextDeleted;

		int CalculatePointerSize(Guid guidApplication);

		IVarRef[] GetAllVarReferences(Guid objectguid, Guid guidExplicitApplicationGuid, string stInstance, long[] alPositionsOfInterest);

		void UpdateDownloadContextWithoutWriteContext(Guid guidApplication);
	}
}
