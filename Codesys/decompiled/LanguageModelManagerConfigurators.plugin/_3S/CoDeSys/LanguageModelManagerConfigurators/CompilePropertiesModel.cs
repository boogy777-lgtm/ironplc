using System.Collections.Generic;
using System.Linq;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	public sealed class CompilePropertiesModel
	{
		public bool debugMode { get; set; }

		public bool CompilerVersionFixed { get; set; }

		public Compilerversion DesiredVersion { get; set; }

		public Compilerversion NewestVersion { get; set; }

		public IOrderedEnumerable<Compilerversion> SelectableCompilerVersions { get; set; }

		public bool DesiredVersionIsAvailable { get; set; }

		public IList<string> ProjectDefines { get; set; }

		public bool ProjectDefinesEnabled { get; set; }

		public VersionDependendOption AllowUnicodeInIdentifiers { get; set; }

		public VersionDependendOption ReplaceConstants { get; set; }

		public VersionDependendOption EnableLoggingInBreakpoints { get; set; }

		public VersionDependendOption Utf8EncodedStrings { get; set; }

		public VersionDependendOption ReportCompiledPousDuringIncrementalCompile { get; set; }

		public int? MaxNumberOfWarnings { get; set; }

		internal bool IsLibraryWithPinnedStorageVersion { get; set; }
	}
}
