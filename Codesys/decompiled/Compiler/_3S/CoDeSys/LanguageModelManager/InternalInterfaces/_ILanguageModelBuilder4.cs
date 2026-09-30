using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILanguageModelBuilder4 : _ILanguageModelBuilder3, _ILanguageModelBuilder2, _ILanguageModelBuilder, ILanguageModelBuilder12, ILanguageModelBuilder11, ILanguageModelBuilder10, ILanguageModelBuilder9, ILanguageModelBuilder8, ILanguageModelBuilder7, ILanguageModelBuilder6, ILanguageModelBuilder5, ILanguageModelBuilder4, ILanguageModelBuilder3, ILanguageModelBuilder2, ILanguageModelBuilder
	{
		IGenericUserdefType CreateGenericUserdefType(_IExpression expname);

		_IHasConstantTypeExpression CreateHasConstantTypeExpression();

		_IHasConstantTypeExpression CreateHasConstantTypeExpression(IToken token);

		_IHasConstantTypeExpression CreateHasConstantTypeExpression(IToken token, _IExpression constant, bool bConstantTypeReplaced);

		_ILiteralExpression CreateLiteralExpression(string stVal, TypeClass tc, IToken token, StringEncoding stringEncoding);
	}
}
