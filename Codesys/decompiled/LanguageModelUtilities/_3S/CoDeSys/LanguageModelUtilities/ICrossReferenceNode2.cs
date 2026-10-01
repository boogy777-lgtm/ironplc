using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICrossReferenceNode2 : ICrossReferenceNode, IMessage
	{
		string ShadowingQualifier { get; }

		bool Shadowed { get; }

		string ShadowedOldText { get; }

		string ShadowedNewText { get; }

		Guid PositionGuid { get; }

		Guid MessageGuid { get; }

		Guid ApplicationGuid { get; }

		IList<ICrossReferenceNode> AdditionalNodes { get; }
	}
}
