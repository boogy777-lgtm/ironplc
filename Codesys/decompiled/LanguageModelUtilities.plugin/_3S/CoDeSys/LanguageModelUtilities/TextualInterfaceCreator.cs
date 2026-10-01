using System;
using System.Linq;
using System.Text;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelUtilities.Legacy;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class TextualInterfaceCreator
	{
		private struct ScopeAndFlags : IEquatable<ScopeAndFlags>
		{
			private readonly Operator _scope;

			private readonly bool _bConstant;

			internal ScopeAndFlags(Operator eScope)
				: this(eScope, bConstant: false)
			{
			}

			internal ScopeAndFlags(Operator eScope, bool bConstant)
			{
				_scope = eScope;
				_bConstant = bConstant;
			}

			internal void AppendBeginVar(StringBuilder strb, IScanner scanner)
			{
				strb.Append(scanner.GetOperatorText(_scope));
				if (_bConstant)
				{
					strb.Append(" ");
					strb.Append(scanner.GetOperatorText(Operator.Constant));
				}
				strb.AppendLine();
			}

			internal void AppendEndVar(StringBuilder strb, IScanner scanner)
			{
				if (_scope != 0)
				{
					strb.Append(scanner.GetOperatorText(Operator.EndVar) + "\r\n");
				}
			}

			public bool Equals(ScopeAndFlags other)
			{
				if (other._scope == _scope)
				{
					return _bConstant == other._bConstant;
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is ScopeAndFlags other)
				{
					return Equals(other);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (856363837 * -842765757 + _scope.GetHashCode()) * -842765757 + _bConstant.GetHashCode();
			}
		}

		private ISignature2 MaybeParentSignature { get; }

		private ISignature2 SignatureMethod { get; }

		private IPreCompileContext3 PrecompileContextSource { get; }

		private IPreCompileContext3 PrecompileContextDest { get; }

		private string LibraryNamespace { get; }

		private StringBuilder Builder { get; }

		private IAdditionalAttributeProvider AdditionalAttributeProvider { get; }

		private TextualInterfaceCreator(ISignature2 signFB, ISignature2 signMethod, IPreCompileContext3 precomSource, IPreCompileContext3 precomDest, string stLibraryNamespace, IAdditionalAttributeProvider additionalAttributeProvider)
		{
			MaybeParentSignature = signFB;
			SignatureMethod = signMethod;
			PrecompileContextSource = precomSource;
			PrecompileContextDest = precomDest;
			LibraryNamespace = stLibraryNamespace;
			AdditionalAttributeProvider = additionalAttributeProvider;
			Builder = new StringBuilder();
		}

		public static string CreateTextualInterface(ISignature2 maybeParentSignature, ISignature2 signMethod, IPreCompileContext3 precomSource, IPreCompileContext3 precomDest, IAdditionalAttributeProvider additionalAttributeProvider)
		{
			string namespaceOfLibrary = precomDest.GetNamespaceOfLibrary(precomSource, null);
			return new TextualInterfaceCreator(maybeParentSignature, signMethod, precomSource, precomDest, namespaceOfLibrary, additionalAttributeProvider).Dump();
		}

		public static string CreateTextualInterface(ISignature2 signMethod, IPreCompileContext3 precomSource, IPreCompileContext3 precomDest)
		{
			return CreateTextualInterface(null, signMethod, precomSource, precomDest, null);
		}

		private string Dump()
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner("", bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);
			WriteAttributes();
			if (MaybeParentSignature != null && AdditionalAttributeProvider != null)
			{
				string text = AdditionalAttributeProvider.ProvideAdditionalAttribute(MaybeParentSignature, SignatureMethod, PrecompileContextSource);
				if (text != null)
				{
					Builder.Append(text);
				}
			}
			WriteCommentary();
			WriteHeader(scanner);
			WriteVariables(scanner);
			return Builder.ToString();
		}

		private void WriteVariables(IScanner scanner)
		{
			IVariable[] all = SignatureMethod.All;
			ScopeAndFlags other = new ScopeAndFlags(Operator.None);
			foreach (IVariable2 item in all.OfType<IVariable2>())
			{
				if ((!(item.Name == SignatureMethod.Name) || (SignatureMethod.POUType != Operator.Function && SignatureMethod.POUType != Operator.Method)) && !item.GetFlag(VarFlag.Implicit) && !item.Name.Contains("__") && (!item.GetFlag(VarFlag.Local) || (SignatureMethod.POUType != Operator.Function && SignatureMethod.POUType != Operator.Method)))
				{
					ScopeAndFlags scopeAndFlags = DetermineNewScopeAndFlags(item);
					if (!scopeAndFlags.Equals(other))
					{
						other.AppendEndVar(Builder, scanner);
						scopeAndFlags.AppendBeginVar(Builder, scanner);
					}
					WriteVariableDeclaration(item);
					other = scopeAndFlags;
				}
			}
			other.AppendEndVar(Builder, scanner);
		}

		private void WriteVariableDeclaration(IVariable2 var)
		{
			if (var.Comment != null && var.Comment != string.Empty)
			{
				Builder.AppendLine("\t(*" + var.Comment + "*)");
			}
			string[] attributes = var.Attributes;
			foreach (string text in attributes)
			{
				if (!(CompileAttributes.ATTRIBUTE_DOCUCOMMENT == text) && !(CompileAttributes.ATTRIBUTE_COMMENT == text))
				{
					string attributeValue = var.GetAttributeValue(text);
					string value = (string.IsNullOrEmpty(attributeValue) ? ("\t{attribute '" + text + "'}") : ("\t{attribute '" + text + "' := '" + attributeValue + "'}"));
					Builder.AppendLine(value);
				}
			}
			string qualifiedTypeText = LegacySwitch.GetQualifiedTypeText(var.Type, PrecompileContextSource, PrecompileContextDest);
			Builder.Append("\t" + var.OrgName + "\t: " + qualifiedTypeText);
			if (var.Initial != null)
			{
				string text2 = LegacySwitch.DumpQualifiedExpressionText(var.Initial, PrecompileContextSource, PrecompileContextDest, LibraryNamespace);
				Builder.Append(" := " + text2);
			}
			Builder.AppendLine(";");
		}

		private ScopeAndFlags DetermineNewScopeAndFlags(IVariable2 var2)
		{
			Operator eScope = Operator.None;
			if (var2.GetFlag(VarFlag.Input))
			{
				eScope = Operator.VarInput;
			}
			else if (var2.GetFlag(VarFlag.Output))
			{
				eScope = Operator.VarOutput;
			}
			else if (var2.GetFlag(VarFlag.Inout))
			{
				eScope = Operator.VarInOut;
			}
			return new ScopeAndFlags(eScope, var2.GetFlag(VarFlag.Constant));
		}

		private string TryGetAccessName()
		{
			if (SignatureMethod.GetFlag(SignatureFlag.Private))
			{
				return "PRIRVATE";
			}
			if (SignatureMethod.GetFlag(SignatureFlag.Protected))
			{
				return "PROTECTED";
			}
			if (SignatureMethod.GetFlag(SignatureFlag.Internal))
			{
				return "INTERNAL";
			}
			return null;
		}

		private void WriteHeader(IScanner scanner)
		{
			Builder.Append(scanner.GetOperatorText(SignatureMethod.POUType));
			string text = TryGetAccessName();
			if (text != null)
			{
				Builder.Append(" ");
				Builder.Append(text);
			}
			Builder.Append(" ");
			Builder.Append(SignatureMethod.OrgName);
			if (SignatureMethod.POUType == Operator.Method || SignatureMethod.POUType == Operator.Function)
			{
				IVariable variable = SignatureMethod[SignatureMethod.Name];
				if (variable != null)
				{
					Builder.Append(" : " + LegacySwitch.GetQualifiedTypeText(variable.Type, PrecompileContextSource, PrecompileContextDest));
					if (variable.Comment != null && variable.Comment != string.Empty)
					{
						Builder.Append(" (*" + variable.Comment + "*)");
					}
				}
			}
			Builder.AppendLine();
		}

		private void WriteCommentary()
		{
			string attributeValue = SignatureMethod.GetAttributeValue(CompileAttributes.ATTRIBUTE_DOCUCOMMENT);
			if (string.IsNullOrEmpty(attributeValue))
			{
				attributeValue = SignatureMethod.GetAttributeValue(CompileAttributes.ATTRIBUTE_COMMENT);
			}
			if (!string.IsNullOrEmpty(attributeValue))
			{
				Builder.Append("(*" + attributeValue + "*)");
				Builder.AppendLine();
			}
		}

		private void WriteAttributes()
		{
			string[] attributes = SignatureMethod.Attributes;
			foreach (string text in attributes)
			{
				if ((!APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 8, 0) || !text.Equals(CompileAttributes.ATTRIBUTE_OBJECT_NAME, StringComparison.OrdinalIgnoreCase)) && !text.Equals(CompileAttributes.ATTRIBUTE_DOCUCOMMENT, StringComparison.OrdinalIgnoreCase) && !text.Equals(CompileAttributes.ATTRIBUTE_COMMENT, StringComparison.OrdinalIgnoreCase))
				{
					string attributeValue = SignatureMethod.GetAttributeValue(text);
					Builder.Append("{");
					if (string.IsNullOrEmpty(attributeValue))
					{
						Builder.AppendFormat("attribute '{0}'", text);
					}
					else
					{
						Builder.AppendFormat("attribute '{0}' := '{1}'", text, attributeValue);
					}
					Builder.AppendLine("}");
				}
			}
		}
	}
}
