using System;
using System.IO;
using System.Runtime.CompilerServices;
using \u000F;
using _3S.CoDeSys.Compiler35220.Serialization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;

namespace \u0012
{
	// Token: 0x02000084 RID: 132
	internal sealed class \u0005 : \u0082.\u0001
	{
		// Token: 0x17000421 RID: 1057
		// (get) Token: 0x06000B82 RID: 2946 RVA: 0x000192F0 File Offset: 0x000174F0
		internal new static global::\u0012.\u0005 Instance { get; } = new global::\u0012.\u0005();

		// Token: 0x06000B83 RID: 2947 RVA: 0x000192F8 File Offset: 0x000174F8
		private \u0005()
		{
		}

		// Token: 0x06000B84 RID: 2948 RVA: 0x00019300 File Offset: 0x00017500
		public override void \u0001(_IUserdefType \u0002)
		{
			base.\u0001(global::\u000F.\u0003.\u000E);
			global::\u000F.\u0004.\u0001(base.Writer, (_IExpression)\u0002.NameExpression);
		}

		// Token: 0x06000B85 RID: 2949 RVA: 0x00019320 File Offset: 0x00017520
		public override void \u0001(IGenericUserdefType \u0002)
		{
			base.\u0001(global::\u000F.\u0003.\u000F);
			global::\u000F.\u0004.\u0001(base.Writer, (_IExpression)\u0002.NameExpression);
			CommonSerializer.SerializeList<_IExpression>(base.Writer, \u0002.GenericConstantsInitializations, new Action<BinaryWriter, _IExpression>(global::\u000F.\u0004.\u0001));
		}

		// Token: 0x06000B86 RID: 2950 RVA: 0x00019360 File Offset: 0x00017560
		public override void \u0001(_IAliasType \u0002)
		{
			\u0002.BeforeSerialize();
			base.\u0001(global::\u000F.\u0003.\u0010);
			global::\u000F.\u0004.\u0001(base.Writer, (_IExpression)\u0002.NameExpression);
			base.\u0001(\u0002.EffectiveType as _IType);
			base.Writer.Write(\u0002.IsCompiled);
			global::\u000F.\u0004.\u0001(base.Writer, \u0002._DefaultValue);
		}

		// Token: 0x06000B87 RID: 2951 RVA: 0x000193C4 File Offset: 0x000175C4
		public override void \u0001(_IEnumType \u0002)
		{
			\u0002.BeforeSerialize();
			base.\u0001(global::\u000F.\u0003.\u0012);
			base.Writer.Write(\u0002.Name);
			base.\u0001(\u0002._Base);
			global::\u000F.\u0004.\u0001(base.Writer, \u0002._DefaultValue);
		}

		// Token: 0x04000156 RID: 342
		[CompilerGenerated]
		private new static readonly global::\u0012.\u0005 \u0001;
	}
}
