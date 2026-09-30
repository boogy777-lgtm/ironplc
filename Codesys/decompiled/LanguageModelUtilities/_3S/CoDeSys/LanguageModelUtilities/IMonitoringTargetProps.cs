using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Online;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IMonitoringTargetProps
	{
		TargetByteOrder TargetByteOrder { get; }

		int PointerSizeBytes { get; }

		bool ByteSupport { get; }

		Version RuntimeIdentification { get; }

		bool NewVFTable { get; }
	}
}
