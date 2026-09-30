using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPreCompileUtilities8 : IPreCompileUtilities7, IPreCompileUtilities6, IPreCompileUtilities5, IPreCompileUtilities4, IPreCompileUtilities3, IPreCompileUtilities2, IPreCompileUtilities
	{
		string AddDeviceApplicationPrefix(string stWatchExpression, Guid applicationGuid);
	}
}
