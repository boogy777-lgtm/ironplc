using System;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface ITimeOfDayToken : IWhiteToken, INode
	{
		DateTime Date { get; }

		bool Overflow { get; }
	}
}
