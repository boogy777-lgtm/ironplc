using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IByteCmdCreator
	{
		IList<byte> ByteCode { get; }
	}
}
