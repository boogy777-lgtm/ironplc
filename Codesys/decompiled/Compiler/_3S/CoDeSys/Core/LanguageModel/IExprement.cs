using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExprement
	{
		ISourcePosition Position { get; }

		[Obsolete("no longer supported, function will return null! Use CodeAdapter.GetBreakpoint instead")]
		IBreakpoint Breakpoint { get; }

		[Obsolete("no longer supported, function will return null! Use CodeAdapter.GetCGAttributes and CodeAdapter.SetCGAttributes instead")]
		ICodeGeneratorAttributes CGAttributes { get; set; }

		[Obsolete("no longer supported, function will return null! Use CodeAdapter.SetBreakpoint instead")]
		IBreakpoint SetBreakpoint(int nOffset, byte bySize);

		void AddError(string stError);

		void AddWarning(string stWarning);

		void AcceptVisitor(IExprVisitor visitor);

		new string ToString();
	}
}
