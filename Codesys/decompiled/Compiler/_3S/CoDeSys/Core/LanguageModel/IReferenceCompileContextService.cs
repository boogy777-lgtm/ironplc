using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IReferenceCompileContextService
	{
		string GetReferenceContextPath(Guid gdApplication);

		void SetReferenceContextPath(Guid gdApplication, string stPath);

		void SetReferenceContext(Guid gdApplication, Stream stream);

		void SetReferenceContext(Guid gdApplication, ICompileContext comcon);

		void MarkCompileContextAsExternalOnly(Guid gdApplication, bool bExternalOnly);
	}
}
