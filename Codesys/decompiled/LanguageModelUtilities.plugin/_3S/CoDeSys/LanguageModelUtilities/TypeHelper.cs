using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{887A57A3-E51C-4D27-8DE8-AEF50580BBC4}")]
	internal class TypeHelper : ITypeHelper
	{
		public IIECExprInfo GetIECExprInfo(string stExpression, Guid gdApp)
		{
			return TOU.GetIECExprInfo(stExpression, gdApp);
		}

		public bool IsPointerDeref(string stExp, out string stPointerExp)
		{
			return TOU.IsPointerDeref(stExp, out stPointerExp);
		}

		public bool ContainsPointerDeref(string stExp)
		{
			return TOU.ContainsPointerDeref(stExp);
		}
	}
}
