using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ITypeReference : IExpression2, IExpression, IExprement
	{
		[Obsolete("QualifiedNameExpression is no longer used, function will return null. Use InstanceExpression instead")]
		IQualifiedNameExpression Instance { get; }
	}
}
