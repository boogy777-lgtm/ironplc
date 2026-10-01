using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Online;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IToolTipService
	{
		string CreateSymbolInfoToolTip(IPreCompileContext pcc, ISignature signature, IVariable variable);

		string CreateWatchBoxToolTip(IOnlineVarRef onlineVarRef, IVarRef varRef, string stShortenedValueString);

		void SetToolTipCommentSizeCallback(ToolTipCommentSizeCallback callback);
	}
}
