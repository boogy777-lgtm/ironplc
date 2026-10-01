using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IErrorVisitor : IExprementVisitor2, IExprementVisitor
	{
		IList<_ICompilerMessage> MessageList { get; }

		void Suppress(string stWarningId);
	}
}
