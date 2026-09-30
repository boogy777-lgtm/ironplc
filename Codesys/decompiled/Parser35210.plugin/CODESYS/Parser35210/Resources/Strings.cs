using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace CODESYS.Parser35210.Resources
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Strings
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					resourceMan = new ResourceManager("CODESYS.Parser35210.Resources.Strings", typeof(Strings).Assembly);
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal static string CompilerFeature_GenericConstantVariable => ResourceManager.GetString("CompilerFeature_GenericConstantVariable", resourceCulture);

		internal static string CompilerFeature_PartialVariableAccess => ResourceManager.GetString("CompilerFeature_PartialVariableAccess", resourceCulture);

		internal static string CompilerFeature_ProjectDefines => ResourceManager.GetString("CompilerFeature_ProjectDefines", resourceCulture);

		internal static string CompilerFeature_UChar_Literals => ResourceManager.GetString("CompilerFeature_UChar_Literals", resourceCulture);

		internal static string CompilerFeature_UTF8_Strings => ResourceManager.GetString("CompilerFeature_UTF8_Strings", resourceCulture);

		internal static string Constant => ResourceManager.GetString("Constant", resourceCulture);

		internal static string Literal => ResourceManager.GetString("Literal", resourceCulture);

		internal static string OR => ResourceManager.GetString("OR", resourceCulture);

		internal static string Scanner_Initialize_char_array_must_end_with_null_value => ResourceManager.GetString("Scanner_Initialize_char_array_must_end_with_null_value", resourceCulture);

		internal Strings()
		{
		}
	}
}
