using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public interface _ICrossReferenceNode : ICrossReferenceNode3, ICrossReferenceNode2, ICrossReferenceNode, IMessage
	{
		new CrossReferenceMatchType MatchType { get; set; }

		void ShadowedBy(IIdentifierInfo2 shadowConfilictIdentInfo, string stShadowedNewText);
	}
}
