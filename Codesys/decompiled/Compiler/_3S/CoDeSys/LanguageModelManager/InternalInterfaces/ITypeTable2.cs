using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ITypeTable2 : ITypeTable
	{
		_ISafeRealType SafeReal { get; }

		_ISafeLRealType SafeLReal { get; }
	}
}
