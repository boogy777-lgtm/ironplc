using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILibInfo
	{
		string Path { get; }

		int Id { get; }

		string Namespace { get; }

		bool LibReference { get; }

		bool PoolReference { get; }
	}
}
