using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.WhiteParseTrees.Resources
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	public class WhiteParserMessageStrings
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					resourceMan = new ResourceManager("CODESYS.WhiteParseTrees.WhiteParseTrees.Resources.WhiteParserMessageStrings", typeof(WhiteParserMessageStrings).Assembly);
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static CultureInfo Culture
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

		public static string ExpectedOneOf => ResourceManager.GetString("ExpectedOneOf", resourceCulture);

		public static string FailedToParseAssignmentExpression => ResourceManager.GetString("FailedToParseAssignmentExpression", resourceCulture);

		public static string FailedToParseDeclaration => ResourceManager.GetString("FailedToParseDeclaration", resourceCulture);

		public static string FailedToParseStatement => ResourceManager.GetString("FailedToParseStatement", resourceCulture);

		public static string InternalError => ResourceManager.GetString("InternalError", resourceCulture);

		public static string UnexpectedEndOfInput => ResourceManager.GetString("UnexpectedEndOfInput", resourceCulture);

		public static string UnexpectedOperator => ResourceManager.GetString("UnexpectedOperator", resourceCulture);

		public static string UnexpectedOperatorExpected => ResourceManager.GetString("UnexpectedOperatorExpected", resourceCulture);

		public static string UnexpectedToken => ResourceManager.GetString("UnexpectedToken", resourceCulture);

		public static string UnexpectedTokenExpected => ResourceManager.GetString("UnexpectedTokenExpected", resourceCulture);

		internal WhiteParserMessageStrings()
		{
		}
	}
}
