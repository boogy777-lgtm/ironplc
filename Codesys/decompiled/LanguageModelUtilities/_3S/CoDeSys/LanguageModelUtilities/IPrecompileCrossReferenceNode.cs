using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPrecompileCrossReferenceNode : ICrossReferenceNode, IMessage
	{
		Guid MessageGuid { get; }

		Guid SignatureGuid { get; }
	}
}
