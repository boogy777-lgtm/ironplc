using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200012F RID: 303
	[ReleasedInterface]
	public interface IStructuredView5 : IStructuredView4, IStructuredView3, IStructuredView2, IStructuredView
	{
		// Token: 0x060004C4 RID: 1220
		bool CanPasteFromStream(ISVNode destNode, ISVObjectStream objectStream);

		// Token: 0x060004C5 RID: 1221
		void PasteFromStream(ISVNode destNode, ISVObjectStream objectStream, StructuredViewPasteNodeEventHandler progressHandler, StructuredViewPasteConflictEventHandler conflictHandler, StructuredViewPasteSkipEventHandler skipHandler);
	}
}
