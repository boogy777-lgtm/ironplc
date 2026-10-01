using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ITypeHelper
	{
		IIECExprInfo GetIECExprInfo(string stExpression, Guid gdApp);

		bool IsPointerDeref(string stExp, out string stPointerExp);

		bool ContainsPointerDeref(string stExp);
	}
}
