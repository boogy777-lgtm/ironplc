using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledCode2 : ICompiledCode, ICloneable
	{
		Stream GetCode();
	}
}
