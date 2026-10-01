using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Online;

namespace _3S.CoDeSys.LanguageModelUtilities.MonitoringByteCode
{
	internal static class MonH
	{
		public struct TargetProps
		{
			public TargetByteOrder targetByteOrder;

			public int nPointerSizeBytes;

			public bool bByteSupport;

			public Version vRuntimeIdentification;

			public bool bNewVFTable;
		}

		public static ByteOrder TargetByteOrderToByteOrder(TargetByteOrder boTarget)
		{
			if ((uint)boTarget <= 1u || boTarget != TargetByteOrder.Motorola)
			{
				return ByteOrder.Intel;
			}
			return ByteOrder.Motorola;
		}

		public static int GetByteSize(IAddressInfo ai, bool bByteSupport)
		{
			if (ai.Size == 0)
			{
				if (!bByteSupport)
				{
					return 2;
				}
				return 1;
			}
			return ai.Size;
		}
	}
}
