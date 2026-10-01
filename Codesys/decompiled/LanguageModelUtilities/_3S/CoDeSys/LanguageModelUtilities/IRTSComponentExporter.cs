using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IRTSComponentExporter
	{
		string ProjectDirectory { get; set; }

		string ProjectName { get; set; }

		string Namespace { get; set; }

		bool GenerateM4 { get; set; }

		bool GenerateC { get; set; }

		bool GenerateTypeHeader { get; set; }

		bool UseCAAGuidelines { get; set; }

		[Obsolete("Setting is ignored. TopLevelPOUs will be exported if passed to the Run method.")]
		bool ExportTopLevelPOUs { get; set; }

		uint LibVersion { get; set; }

		IMessageCategory MessageCategory { get; set; }

		void SetAdditionalIncludes(IList<string> list);

		void Run(ICompileContext comcon, ISignature[] signatures);
	}
}
