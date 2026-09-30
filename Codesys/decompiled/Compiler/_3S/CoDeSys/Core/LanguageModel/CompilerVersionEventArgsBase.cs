using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public abstract class CompilerVersionEventArgsBase : EventArgs
	{
		private Version _PreviousCompilerVersion;

		private Version _NewCompilerVersion;

		public Version PreviousCompilerVersion => _PreviousCompilerVersion;

		public Version NewCompilerVersion => _NewCompilerVersion;

		public CompilerVersionEventArgsBase(Version previousVersion, Version newVersion)
		{
			if (newVersion == null)
			{
				throw new ArgumentNullException("newVersion");
			}
			_PreviousCompilerVersion = previousVersion;
			_NewCompilerVersion = newVersion;
		}
	}
}
