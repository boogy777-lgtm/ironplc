using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVarRef
	{
		Guid ApplicationGuid { get; }

		IExpression WatchExpression { get; }

		ISourcePosition Position { get; }

		IAddressInfo AddressInfo { get; }

		object ConstantValue { get; }

		bool GetFlag(VarRefFlag vrflag);

		new bool Equals(object obj);

		new int GetHashCode();
	}
}
