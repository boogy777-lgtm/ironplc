using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICrossReferenceNode3 : ICrossReferenceNode2, ICrossReferenceNode, IMessage
	{
		IExprement ExpressionAtSourcePosition { get; }
	}
}
