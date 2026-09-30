using System;
using System.Diagnostics;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	[DebuggerDisplay("{Version}")]
	public class Compilerversion
	{
		private readonly Version _version;

		private readonly string _stVersionText;

		public Version Version => _version;

		public string VersionText => _stVersionText;

		internal Compilerversion(Version version, string stVersionText)
		{
			_version = version;
			_stVersionText = stVersionText;
		}
	}
}
