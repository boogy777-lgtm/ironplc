using System;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IDateAndTimeToken : IWhiteToken, INode
	{
		DateTime Date { get; }

		bool Overflow { get; }
	}
}
