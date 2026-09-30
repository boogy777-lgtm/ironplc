using System;
using System.Runtime.CompilerServices;
using \u0001;
using \u000E;
using \u0011;
using \u0016;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0017
{
	// Token: 0x020000CB RID: 203
	internal sealed class \u0003 : IStringEncodingService, ISingleByteStringEncodingService, ILMStringEncodingService
	{
		// Token: 0x06000EB6 RID: 3766 RVA: 0x00028808 File Offset: 0x00026A08
		private \u0003()
		{
			this.DefaultService = new global::\u0001.\u0002();
			this.UTF8Service = new global::\u000E.\u0005();
			this.WStringService = new global::\u0011.\u0003();
			this.MotorolaWStringService = new global::\u0016.\u0003();
		}

		// Token: 0x17000446 RID: 1094
		// (get) Token: 0x06000EB7 RID: 3767 RVA: 0x0002883C File Offset: 0x00026A3C
		internal static global::\u0017.\u0003 Singleton { get; } = new global::\u0017.\u0003();

		// Token: 0x17000447 RID: 1095
		// (get) Token: 0x06000EB8 RID: 3768 RVA: 0x00028844 File Offset: 0x00026A44
		private global::\u0001.\u0002 DefaultService { get; }

		// Token: 0x17000448 RID: 1096
		// (get) Token: 0x06000EB9 RID: 3769 RVA: 0x0002884C File Offset: 0x00026A4C
		private global::\u000E.\u0005 UTF8Service { get; }

		// Token: 0x17000449 RID: 1097
		// (get) Token: 0x06000EBA RID: 3770 RVA: 0x00028854 File Offset: 0x00026A54
		private global::\u0011.\u0003 WStringService { get; }

		// Token: 0x1700044A RID: 1098
		// (get) Token: 0x06000EBB RID: 3771 RVA: 0x0002885C File Offset: 0x00026A5C
		private global::\u0016.\u0003 MotorolaWStringService { get; }

		// Token: 0x06000EBC RID: 3772 RVA: 0x00028864 File Offset: 0x00026A64
		private global::\u000E.\u0004 \u0001(TypeClass \u0002, ByteOrder \u0003, StringEncoding \u0004)
		{
			if (\u0002 != TypeClass.String)
			{
				if (\u0002 != TypeClass.WString)
				{
					Debug.\u0001(false);
					return this.DefaultService;
				}
				if (\u0003 == ByteOrder.Motorola)
				{
					return this.MotorolaWStringService;
				}
				return this.WStringService;
			}
			else
			{
				if (APEnvironmentFacade.Instance.CompileOptions.UTF8Encoding || \u0004 == StringEncoding.UTF8)
				{
					return this.UTF8Service;
				}
				return this.DefaultService;
			}
		}

		// Token: 0x06000EBD RID: 3773 RVA: 0x000288C0 File Offset: 0x00026AC0
		public byte[] \u0001(ByteOrder \u0002, TypeClass \u0003, int \u0004, string \u0005)
		{
			return this.\u0001(\u0002, \u0003, \u0004, \u0005, StringEncoding.Default);
		}

		// Token: 0x06000EBE RID: 3774 RVA: 0x000288D0 File Offset: 0x00026AD0
		public byte[] \u0001(ByteOrder \u0002, TypeClass \u0003, int \u0004, string \u0005, StringEncoding \u0006)
		{
			byte[] result = null;
			try
			{
				char[] u = \u0005.ToCharArray();
				result = this.\u0001(\u0003, \u0002, \u0006).\u0001(\u0004, u);
			}
			catch (Exception arg)
			{
				Debug.\u0001(false, string.Format("Error in string constant {0}{1} {2}", arg, Environment.NewLine, \u0005));
			}
			return result;
		}

		// Token: 0x06000EBF RID: 3775 RVA: 0x00028928 File Offset: 0x00026B28
		public long \u0001(string \u0002, ByteOrder \u0003, TypeClass \u0004)
		{
			return this.\u0001(\u0002, \u0003, \u0004, StringEncoding.Default);
		}

		// Token: 0x06000EC0 RID: 3776 RVA: 0x00028934 File Offset: 0x00026B34
		public long \u0001(string \u0002, ByteOrder \u0003, TypeClass \u0004, StringEncoding \u0005)
		{
			return this.\u0001(\u0004, \u0003, \u0005).\u0001(\u0002);
		}

		// Token: 0x06000EC1 RID: 3777 RVA: 0x00028948 File Offset: 0x00026B48
		public string \u0001(byte[] \u0002, ByteOrder \u0003, TypeClass \u0004)
		{
			return this.\u0001(\u0002, \u0003, \u0004, StringEncoding.Default);
		}

		// Token: 0x06000EC2 RID: 3778 RVA: 0x00028954 File Offset: 0x00026B54
		public string \u0001(byte[] \u0002, ByteOrder \u0003, TypeClass \u0004, StringEncoding \u0005)
		{
			return this.\u0001(\u0004, \u0003, \u0005).\u0001(\u0002);
		}

		// Token: 0x06000EC3 RID: 3779 RVA: 0x00028968 File Offset: 0x00026B68
		public string \u0001(byte[] \u0002, StringEncoding \u0003)
		{
			return this.\u0001(\u0002, ByteOrder.Intel, TypeClass.String, \u0003);
		}

		// Token: 0x06000EC4 RID: 3780 RVA: 0x00028978 File Offset: 0x00026B78
		public byte[] \u0001(string \u0002, StringEncoding \u0003)
		{
			return this.\u0001(ByteOrder.Intel, TypeClass.String, 1, \u0002, \u0003);
		}

		// Token: 0x06000EC5 RID: 3781 RVA: 0x00028988 File Offset: 0x00026B88
		public static string \u0001(string \u0002)
		{
			return \u0002.Replace("$", "$$").Replace("'", "$'").Replace("\"", "$\"").Replace("\n", "$N").Replace("\f", "$P").Replace("\r", "$R").Replace("\t", "$T");
		}

		// Token: 0x0400029A RID: 666
		[CompilerGenerated]
		private static readonly global::\u0017.\u0003 \u0001;

		// Token: 0x0400029B RID: 667
		[CompilerGenerated]
		private readonly global::\u0001.\u0002 \u0001;

		// Token: 0x0400029C RID: 668
		[CompilerGenerated]
		private readonly global::\u000E.\u0005 \u0001;

		// Token: 0x0400029D RID: 669
		[CompilerGenerated]
		private readonly global::\u0011.\u0003 \u0001;

		// Token: 0x0400029E RID: 670
		[CompilerGenerated]
		private readonly global::\u0016.\u0003 \u0001;
	}
}
