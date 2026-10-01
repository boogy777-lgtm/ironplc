using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IUserdefType : IType, IArchivable
	{
		[Obsolete("QualifiedNameExpression is no longer used, function will return null. Use NameExpression instead")]
		IQualifiedNameExpression Name { get; }

		int SignatureId { get; }
	}
}
