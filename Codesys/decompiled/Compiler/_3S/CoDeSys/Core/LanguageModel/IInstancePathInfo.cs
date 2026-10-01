using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IInstancePathInfo
	{
		string InstancePath { get; }

		IVariable VarInstance { get; }

		ISignature DeclaringSignature { get; }
	}
}
