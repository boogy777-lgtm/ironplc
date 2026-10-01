using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDownloadInfo
	{
		IArea[] Areas { get; }

		ICodePiece[] CodePieces { get; }

		IExternalReference[] ExternalReferences { get; }

		IDataLocation GlobalInitPointerLocation { get; }

		IDataLocation GlobalExitPointerLocation { get; }

		IDataLocation DownloadPOUPointerLocation { get; }

		IDataLocation CodeInitLocation { get; }

		IDataLocation RelocCodeLocation { get; }

		Guid CodeId { get; }

		Guid DataId { get; }
	}
}
