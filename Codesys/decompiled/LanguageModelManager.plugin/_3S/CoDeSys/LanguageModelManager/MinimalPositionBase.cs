using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000145 RID: 325
	internal readonly struct MinimalPositionBase : IMinimalPosition
	{
		// Token: 0x1700074D RID: 1869
		// (get) Token: 0x06001B3D RID: 6973 RVA: 0x0004D852 File Offset: 0x0004C852
		public long EditorPosition { get; }

		// Token: 0x1700074E RID: 1870
		// (get) Token: 0x06001B3E RID: 6974 RVA: 0x0004D85A File Offset: 0x0004C85A
		public short PositionOffset { get; }

		// Token: 0x06001B3F RID: 6975 RVA: 0x0004D862 File Offset: 0x0004C862
		internal MinimalPositionBase(long editorPosition, short positionOffset)
		{
			this.EditorPosition = editorPosition;
			this.PositionOffset = positionOffset;
		}

		// Token: 0x06001B40 RID: 6976 RVA: 0x0004D874 File Offset: 0x0004C874
		public override bool Equals(object obj)
		{
			IMinimalPosition minimalPosition = obj as IMinimalPosition;
			return minimalPosition != null && minimalPosition.PositionOffset == this.PositionOffset && minimalPosition.EditorPosition == this.EditorPosition;
		}

		// Token: 0x06001B41 RID: 6977 RVA: 0x0004D8AB File Offset: 0x0004C8AB
		public bool Equals(long l, short s)
		{
			return l == this.EditorPosition && s == this.PositionOffset;
		}

		// Token: 0x06001B42 RID: 6978 RVA: 0x0004D8C1 File Offset: 0x0004C8C1
		public override int GetHashCode()
		{
			return MinimalPositionBase.GetHashCode(this.EditorPosition, this.PositionOffset);
		}

		// Token: 0x06001B43 RID: 6979 RVA: 0x0004D8D4 File Offset: 0x0004C8D4
		public static int GetHashCode(long l, short s)
		{
			return (int)(l * 7079L ^ (long)s);
		}
	}
}
