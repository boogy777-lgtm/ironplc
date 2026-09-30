using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICompileUtilities5 : ICompileUtilities4, ICompileUtilities3, ICompileUtilities2, ICompileUtilities
	{
		bool GetAddressFromExpression(Guid guidApplication, string stExpression, out int iArea, out int iOffset);
	}
}
