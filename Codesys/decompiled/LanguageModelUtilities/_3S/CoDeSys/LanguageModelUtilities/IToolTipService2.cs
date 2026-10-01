using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IToolTipService2 : IToolTipService
	{
		string ConvertReStructuredTextDocuCommentToPlainText(string stDocComment);

		string CreateInfoToolTip(string stInfo);
	}
}
