using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace CODESYS.Parser35220.Resources
{
	// Token: 0x0200000B RID: 11
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Strings
	{
		// Token: 0x0600004C RID: 76 RVA: 0x00002076 File Offset: 0x00000276
		internal Strings()
		{
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600004D RID: 77 RVA: 0x00002F21 File Offset: 0x00001121
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Strings.resourceMan == null)
				{
					Strings.resourceMan = new ResourceManager("CODESYS.Parser35220.Resources.Strings", typeof(Strings).Assembly);
				}
				return Strings.resourceMan;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600004E RID: 78 RVA: 0x00002F4D File Offset: 0x0000114D
		// (set) Token: 0x0600004F RID: 79 RVA: 0x00002F54 File Offset: 0x00001154
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Strings.resourceCulture;
			}
			set
			{
				Strings.resourceCulture = value;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000050 RID: 80 RVA: 0x00002F5C File Offset: 0x0000115C
		internal static string CompilerFeature_GenericConstantVariable
		{
			get
			{
				return Strings.ResourceManager.GetString("CompilerFeature_GenericConstantVariable", Strings.resourceCulture);
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000051 RID: 81 RVA: 0x00002F72 File Offset: 0x00001172
		internal static string CompilerFeature_PartialVariableAccess
		{
			get
			{
				return Strings.ResourceManager.GetString("CompilerFeature_PartialVariableAccess", Strings.resourceCulture);
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000052 RID: 82 RVA: 0x00002F88 File Offset: 0x00001188
		internal static string CompilerFeature_ProjectDefines
		{
			get
			{
				return Strings.ResourceManager.GetString("CompilerFeature_ProjectDefines", Strings.resourceCulture);
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000053 RID: 83 RVA: 0x00002F9E File Offset: 0x0000119E
		internal static string CompilerFeature_UChar_Literals
		{
			get
			{
				return Strings.ResourceManager.GetString("CompilerFeature_UChar_Literals", Strings.resourceCulture);
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000054 RID: 84 RVA: 0x00002FB4 File Offset: 0x000011B4
		internal static string CompilerFeature_UTF8_Strings
		{
			get
			{
				return Strings.ResourceManager.GetString("CompilerFeature_UTF8_Strings", Strings.resourceCulture);
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000055 RID: 85 RVA: 0x00002FCA File Offset: 0x000011CA
		internal static string Constant
		{
			get
			{
				return Strings.ResourceManager.GetString("Constant", Strings.resourceCulture);
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000056 RID: 86 RVA: 0x00002FE0 File Offset: 0x000011E0
		internal static string Literal
		{
			get
			{
				return Strings.ResourceManager.GetString("Literal", Strings.resourceCulture);
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000057 RID: 87 RVA: 0x00002FF6 File Offset: 0x000011F6
		internal static string OR
		{
			get
			{
				return Strings.ResourceManager.GetString("OR", Strings.resourceCulture);
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000058 RID: 88 RVA: 0x0000300C File Offset: 0x0000120C
		internal static string Scanner_Initialize_char_array_must_end_with_null_value
		{
			get
			{
				return Strings.ResourceManager.GetString("Scanner_Initialize_char_array_must_end_with_null_value", Strings.resourceCulture);
			}
		}

		// Token: 0x04000006 RID: 6
		private static ResourceManager resourceMan;

		// Token: 0x04000007 RID: 7
		private static CultureInfo resourceCulture;
	}
}
