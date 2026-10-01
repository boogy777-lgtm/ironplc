using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMLibraryList
	{
		Guid LibManGuid { get; set; }

		ILMLibraryInfo[] Libraries { get; }

		ILMPlaceholderInfo[] Placeholders { get; }

		Guid ObjectGuid { get; set; }

		void AddLibraryInfo(ILMLibraryInfo libinfo);

		void AddPlaceholderInfo(ILMPlaceholderInfo placeholder);
	}
}
