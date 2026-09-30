using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{62F77C38-425E-4D18-AD5B-A408263D7503}")]
	public class LMMAttributeProvider : IAttributeProvider
	{
		private readonly DocumentedAttributes DOCUMENTED_ATTRIBUTES = new DocumentedAttributes
		{
			{ "m4export", "Force m4 export independant of the \"External Link in runtime\" option" },
			{ "m4export_hide", "Hide function, functionblock, interface, structure or enum from m4-Export" },
			{ "m4export_nosignature", "Don't export a signature for this function or method" },
			{ "m4export_32bit_nosignature", "Don't export a signature for 32-Bit targets" },
			{ "m4export_64bit_nosignature", "Don't export a signature for 64-Bit targets" },
			{ "m4export_enum-as-C", "Export an IEC enum as C enum. Attention: This worked only for a typed enum in IEC from type DINT!" },
			{ "m4export_enum-no-prefix", "Don't export the name of the enum as a prefix at every enum constant" },
			{ "m4export_stringlen-as-value", "Export a string len in IEC with its value:\ne.g.MY_STRING_LEN : INT := 100;\nmystring : STRING(MY_STRING_LEN);\n==> char mystring[101];" },
			{ "m4export_stringlen-as-is", "\tExport a string len in IEC with its value:\ne.g.MY_STRING_LEN : INT := 100;\nmystring : STRING(MY_STRING_LEN);\n==> char mystring[MY_STRING_LEN];" },
			{ "m4export_arraylen-as-value", "Export an array in IEC with its value:\ne.g.MY_ARRAY_LEN : INT := 100;\nmystring : ARRAY [0..MY_ARRAY_LEN];\n==> char mystring[101];" },
			{ "m4export_arraylen-as-is", "Export an array in IEC with its value:\ne.g.MY_ARRAY_LEN : INT := 100;\nmystring : ARRAY [0..MY_ARRAY_LEN];\n==> char mystring[MY_ARRAY_LEN];" }
		};

		private LList<IAttribute> _attributes = new LList<IAttribute>();

		public IEnumerable<IAttribute> ProvidedAttributes => (IEnumerable<IAttribute>)_attributes;

		public LMMAttributeProvider()
		{
			foreach (Tuple<string, string> dOCUMENTED_ATTRIBUTE in DOCUMENTED_ATTRIBUTES)
			{
				_attributes.Add((IAttribute)new DocumentedAttribute(dOCUMENTED_ATTRIBUTE.Item1, dOCUMENTED_ATTRIBUTE.Item2));
			}
		}
	}
}
