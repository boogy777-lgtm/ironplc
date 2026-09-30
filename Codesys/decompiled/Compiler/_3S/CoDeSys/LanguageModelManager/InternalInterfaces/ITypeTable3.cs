using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITypeTable3 : ITypeTable2, ITypeTable
	{
		int GetDirectVariableSizeInBits(DirectVariableSize size);

		bool IsPartialAccessSupportedType(TypeClass typeClass, out int sizeInBits, ICommonScope scope);
	}
}
