using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x0200002B RID: 43
	[ReleasedClass]
	public abstract class ProjectAttributes
	{
		// Token: 0x04000023 RID: 35
		public static readonly Guid Primary = new Guid("{821327BB-5838-4f18-B46F-29184A3B6B70}");

		// Token: 0x04000024 RID: 36
		public static readonly Guid Library = new Guid("{5AC45F5D-2596-4df2-A1F4-2D018EE65647}");

		// Token: 0x04000025 RID: 37
		public static readonly Guid ProvidesLanguageModel = new Guid("{E972192C-1A60-4186-95F5-9C2151DFEB73}");

		// Token: 0x04000026 RID: 38
		public static readonly Guid ReadOnly = new Guid("{2D3B0722-3A83-448a-8570-9ADBBEEDE1C7}");

		// Token: 0x04000027 RID: 39
		public static readonly Guid CompiledLibrary = new Guid("{267E7819-0090-4a50-A872-A8C5E508901D}");
	}
}
