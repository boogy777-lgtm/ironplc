using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteCase
	{
		IWhiteCaseLabelStatement Label { get; set; }

		IWhiteSequenceStatement Controlled { get; set; }
	}
}
