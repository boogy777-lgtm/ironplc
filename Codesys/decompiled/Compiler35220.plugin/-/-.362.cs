using System;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0001
{
	// Token: 0x020003AF RID: 943
	internal static class \u0013
	{
		// Token: 0x170008D3 RID: 2259
		// (get) Token: 0x06003656 RID: 13910 RVA: 0x000DA990 File Offset: 0x000D8B90
		public static string InitInterface
		{
			get
			{
				return string.Concat(new string[]
				{
					"METHOD ",
					IdentifierConstants.InitMethodName,
					": BOOL",
					Environment.NewLine,
					"VAR_INPUT",
					Environment.NewLine,
					"\tbInitRetains,bInCopyCode: BOOL;",
					Environment.NewLine,
					"END_VAR",
					Environment.NewLine
				});
			}
		}

		// Token: 0x170008D4 RID: 2260
		// (get) Token: 0x06003657 RID: 13911 RVA: 0x000DA9FC File Offset: 0x000D8BFC
		public static _ISignature InitMethod
		{
			get
			{
				if (\u0013.\u0001 == null)
				{
					\u0013.\u0001 = ParserHelper.\u0001(\u0013.InitInterface, false);
				}
				return \u0013.\u0001;
			}
		}

		// Token: 0x170008D5 RID: 2261
		// (get) Token: 0x06003658 RID: 13912 RVA: 0x000DAA1C File Offset: 0x000D8C1C
		public static string VFInitInterface
		{
			get
			{
				return "METHOD __VFINIT: BOOL" + Environment.NewLine;
			}
		}

		// Token: 0x170008D6 RID: 2262
		// (get) Token: 0x06003659 RID: 13913 RVA: 0x000DAA30 File Offset: 0x000D8C30
		public static _ISignature VFInitMethod
		{
			get
			{
				return ParserHelper.\u0001(\u0013.VFInitInterface, true);
			}
		}

		// Token: 0x170008D7 RID: 2263
		// (get) Token: 0x0600365A RID: 13914 RVA: 0x000DAA40 File Offset: 0x000D8C40
		public static string QueryInterfaceMethodInterface
		{
			get
			{
				return "METHOD __GetInterfaceReference : BOOL\r\n\t\t\tVAR_INPUT\r\n\t\t\t\tnInterfaceId : DINT;\r\n\t\t\t\tpRef : POINTER TO POINTER TO DWORD;\r\n\t\t\tEND_VAR";
			}
		}

		// Token: 0x170008D8 RID: 2264
		// (get) Token: 0x0600365B RID: 13915 RVA: 0x000DAA48 File Offset: 0x000D8C48
		public static _ISignature QueryInterfaceMethodSignature
		{
			get
			{
				_ISignature isignature = ParserHelper.\u0001(\u0013.QueryInterfaceMethodInterface, true);
				_IVariable ivariable = \u0019.\u0003.\u0001(null);
				ivariable.Name = "pRef_help";
				ivariable.SetFlag(VarFlag.Local | VarFlag.Implicit, true);
				ivariable.SetType(\u0019.\u0003.\u0001(\u0019.\u0003.\u0001()));
				isignature.AddVariable(ivariable);
				return isignature;
			}
		}

		// Token: 0x170008D9 RID: 2265
		// (get) Token: 0x0600365C RID: 13916 RVA: 0x000DAA98 File Offset: 0x000D8C98
		public static string QueryInterfacePointerMethodInterface
		{
			get
			{
				return "METHOD __GetInterfacePointer : BOOL\r\n\t\t\tVAR_INPUT\r\n\t\t\t\tpRef : POINTER TO POINTER TO DWORD;\r\n\t\t\tEND_VAR";
			}
		}

		// Token: 0x170008DA RID: 2266
		// (get) Token: 0x0600365D RID: 13917 RVA: 0x000DAAA0 File Offset: 0x000D8CA0
		public static _ISignature QueryInterfacePointerMethodSignature
		{
			get
			{
				return ParserHelper.\u0001(\u0013.QueryInterfacePointerMethodInterface, true);
			}
		}

		// Token: 0x170008DB RID: 2267
		// (get) Token: 0x0600365E RID: 13918 RVA: 0x000DAAB0 File Offset: 0x000D8CB0
		public static string PartialInitMethodInterface
		{
			get
			{
				return "METHOD __FB_PARTIALINIT : BOOL\r\n\t\t\tVAR_INPUT\r\n\t\t\tEND_VAR";
			}
		}

		// Token: 0x170008DC RID: 2268
		// (get) Token: 0x0600365F RID: 13919 RVA: 0x000DAAB8 File Offset: 0x000D8CB8
		public static _ISignature PartialInitMethodSignature
		{
			get
			{
				return ParserHelper.\u0001(\u0013.PartialInitMethodInterface, true);
			}
		}

		// Token: 0x04000A9A RID: 2714
		private static _ISignature \u0001;
	}
}
