using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileService5 : ILMPreCompileService4, ILMPreCompileService3, ILMPreCompileService2, ILMPreCompileService
	{
		ILMQualifierService CreateQualifierService();

		ILiteralValue GetEnumInitValue(IVariable variable, ICommonScope scope);

		ILMTypeService CreateTypeService();

		ILMStringEncodingService CreateStringEncodingService();
	}
}
