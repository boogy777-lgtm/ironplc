using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IReferencedInstanceWatchVarDescription
	{
		string FullyQualifiedWatchExpression { get; }

		string PointerTempVar { get; }

		string InstanceType { get; }

		ulong AddressInstance { get; }
	}
}
