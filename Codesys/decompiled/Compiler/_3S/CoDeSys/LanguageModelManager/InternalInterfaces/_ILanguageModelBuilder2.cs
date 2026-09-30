using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILanguageModelBuilder2 : _ILanguageModelBuilder, ILanguageModelBuilder12, ILanguageModelBuilder11, ILanguageModelBuilder10, ILanguageModelBuilder9, ILanguageModelBuilder8, ILanguageModelBuilder7, ILanguageModelBuilder6, ILanguageModelBuilder5, ILanguageModelBuilder4, ILanguageModelBuilder3, ILanguageModelBuilder2, ILanguageModelBuilder
	{
		ICompactedParseTreeInformation CreateCompactedParseTreeInformation();

		IStaticMemorySegment CreateStaticMemorySegment(Guid guidSubApplication, int offset, int size);

		_ISafeRealType CreateSafeRealType();

		_ISafeLRealType CreateSafeLRealType();

		ILMPlaceholderInfo CreateLibraryPlaceholder(string stName, string stDefaultLibraryId, string stNamespace, bool bPublishSymbols, bool bLinkAllContent, bool bLinkInSimulation, Guid guidResolver, Guid libManGuid);

		_ILDateType CreateLDateType();

		_ILTimeOfDayType CreateLTimeOfDayType();

		_ILDateAndTimeType CreateLDateAndTimeType();
	}
}
