using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICrossReference
	{
		[Obsolete("for memory optimization, this function is no longer used use ICompileContext4.GetReferencePositionsOfPOU instead")]
		ICodePosition[] Positions { get; }

		int CodeId { get; }
	}
}
