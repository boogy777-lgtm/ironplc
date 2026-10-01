using System;
using System.Collections.Concurrent;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000146 RID: 326
	internal static class MinimalPosition
	{
		// Token: 0x06001B44 RID: 6980 RVA: 0x0004D8E2 File Offset: 0x0004C8E2
		public static IMinimalPosition CreateMinimalPosition(long nPosition, short sPositionOffset)
		{
			if (nPosition == 0L && sPositionOffset == 0)
			{
				return MinimalPosition.NullPosition;
			}
			return MinimalPosition.GetOrCreate(nPosition, sPositionOffset);
		}

		// Token: 0x06001B45 RID: 6981 RVA: 0x0004D8F8 File Offset: 0x0004C8F8
		private static IMinimalPosition GetOrCreate(long nPosition, short sPositionOffset)
		{
			int hashCode = MinimalPositionBase.GetHashCode(nPosition, sPositionOffset);
			IMinimalPosition minimalPosition;
			if (!MinimalPosition.positionCache.TryGetValue(hashCode, out minimalPosition))
			{
				MinimalPositionBase minimalPositionBase = new MinimalPositionBase(nPosition, sPositionOffset);
				MinimalPosition.positionCache.TryAdd(hashCode, minimalPositionBase);
				return minimalPositionBase;
			}
			if (((MinimalPositionBase)minimalPosition).Equals(nPosition, sPositionOffset))
			{
				return minimalPosition;
			}
			return new MinimalPositionBase(nPosition, sPositionOffset);
		}

		// Token: 0x06001B46 RID: 6982 RVA: 0x0004D95E File Offset: 0x0004C95E
		public static IMinimalPosition CreateMinimalPosition(ISourcePosition sp)
		{
			if (sp == null)
			{
				return MinimalPosition.CreateMinimalPosition(0L, 0);
			}
			return MinimalPosition.CreateMinimalPosition(sp.Position, sp.PositionOffset);
		}

		// Token: 0x040005BA RID: 1466
		private static readonly ConcurrentDictionary<int, IMinimalPosition> positionCache = new ConcurrentDictionary<int, IMinimalPosition>();

		// Token: 0x040005BB RID: 1467
		private static readonly IMinimalPosition NullPosition = new MinimalPositionBase(0L, 0);
	}
}
