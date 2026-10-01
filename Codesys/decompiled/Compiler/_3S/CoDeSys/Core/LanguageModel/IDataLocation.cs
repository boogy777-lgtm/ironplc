using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDataLocation
	{
		bool IsRelativ { get; }

		bool IsBitLocation { get; }

		ushort Area { get; }

		int Offset { get; }

		byte BitNr { get; }

		[Obsolete("Never use this")]
		bool HasRetainLocation { get; }

		[Obsolete("Never use this")]
		ushort AreaRetain { get; }

		[Obsolete("Never use this")]
		int OffsetRetain { get; }

		bool IsEqual(IDataLocation locIn);
	}
}
