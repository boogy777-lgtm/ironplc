using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelUtilities.Legacy
{
	[Obsolete("Use _3S.CoDeSys.Core.LanguageModel.ILMQualifierService.GetQualifiedExpression instead")]
	[SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "This deprecated code cannot be removed because it will be used in case of older compiler versions")]
	internal class LegacyQualifiedExpressionTextVisitor : DummyBaseClassWithNotImplementMethods, IExprVisitor
	{
		private string QualificationNamespace { get; }

		private StringBuilder Builder { get; }

		private IScanner Scanner { get; }

		private LegacyQualifiedExpressionTextVisitor(string stQualificationNamespace)
		{
			QualificationNamespace = stQualificationNamespace;
			Builder = new StringBuilder();
			Scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner("", bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);
		}

		internal static string DumpQualifiedExpressionText(IExpression expressionToQualify, string stQualificationNamespace)
		{
			LegacyQualifiedExpressionTextVisitor legacyQualifiedExpressionTextVisitor = new LegacyQualifiedExpressionTextVisitor(stQualificationNamespace);
			try
			{
				expressionToQualify.AcceptVisitor(legacyQualifiedExpressionTextVisitor);
			}
			catch (NotSupportedException)
			{
				return string.Format(Strings.UnexpectedExpression, expressionToQualify.ToString());
			}
			return legacyQualifiedExpressionTextVisitor.Builder.ToString();
		}

		public void visit(IOperatorExpression op)
		{
			if (IsPrefixOperator(op.Code))
			{
				Builder.Append(Scanner.GetOperatorText(op.Code));
				Builder.Append("(");
				AppendWithSeparator(", ", op.Operands);
				Builder.Append(")");
			}
			else
			{
				string operatorText = Scanner.GetOperatorText(op.Code);
				Builder.Append("(");
				AppendWithSeparator(" " + operatorText + " ", op.Operands);
				Builder.Append(")");
			}
		}

		public void visit(IConversionExpression conv)
		{
			string value = MapTypeClassToName(conv.From);
			string value2 = MapTypeClassToName(conv.To);
			Builder.Append(value);
			Builder.Append("_TO_");
			Builder.Append(value2);
			Builder.Append("(");
			conv.Exp.AcceptVisitor(this);
			Builder.Append(")");
		}

		public void visit(IThisExpression thisexp)
		{
			Builder.Append(thisexp.ToString());
		}

		public void visit(IBaseExpression baseexp)
		{
			Builder.Append(baseexp.ToString());
		}

		public void visit(ILiteralExpression literal)
		{
			Builder.Append(literal.ToString());
		}

		public void visit(IAddressExpression address)
		{
			Builder.Append(address.ToString());
		}

		public void visit(IVariableExpression variable)
		{
			Builder.Append(QualificationNamespace + "." + variable.ToString());
		}

		public void visit(IIndexAccessExpression indexaccess)
		{
			indexaccess.Var.AcceptVisitor(this);
			Builder.Append("[");
			AppendWithSeparator(", ", indexaccess.Accesses);
			Builder.Append("]");
		}

		public void visit(ICompoAccessExpression compo)
		{
			compo.Left.AcceptVisitor(this);
			Builder.Append("." + compo.Right.ToString());
		}

		public void visit(IDeRefAccessExpression deref)
		{
			deref.Base.AcceptVisitor(this);
			Builder.Append("^");
		}

		public void visit(IGlobalScopeExpression globexp)
		{
			globexp.Base.AcceptVisitor(this);
		}

		internal static bool IsPrefixOperator(Operator op)
		{
			switch (op)
			{
			case Operator.__Reloc:
			case Operator.Time:
			case Operator.LTime:
			case Operator.Adr:
			case Operator.BitAdr:
			case Operator.IndexOf:
			case Operator.SizeOf:
			case Operator.Ini:
			case Operator.Abs:
			case Operator.Limit:
			case Operator.Min:
			case Operator.Max:
			case Operator.Trunc:
			case Operator.Mux:
			case Operator.Sel:
			case Operator.Rol:
			case Operator.Ror:
			case Operator.Shl:
			case Operator.Shr:
			case Operator.Exp:
			case Operator.Expt:
			case Operator.Sqrt:
			case Operator.Ln:
			case Operator.Log:
			case Operator.Sin:
			case Operator.Cos:
			case Operator.Tan:
			case Operator.ASin:
			case Operator.ACos:
			case Operator.ATan:
			case Operator.Not:
			case Operator.Move:
			case Operator.TestAndSet:
			case Operator.TruncInt:
			case Operator.__TypeOf:
			case Operator.__CRC:
			case Operator.__MaxOffset:
			case Operator.__Init:
			case Operator.__IsValidRef:
			case Operator.__QueryInterface:
			case Operator.__QueryPointer:
			case Operator.__Delete:
			case Operator.__AdrInst:
			case Operator.__RefAdr:
			case Operator.__BitOffset:
			case Operator.__FCall:
			case Operator.__PropertyInfo:
			case Operator.__MemorySet:
			case Operator.__GetLTick:
			case Operator.__Throw:
			case Operator.__CheckLicense:
			case Operator.__CallInitFunction:
			case Operator.__LateCompiledExpr:
			case Operator.LowerBound:
			case Operator.UpperBound:
			case Operator.__CheckLicenseBit:
			case Operator.__XAdd:
			case Operator.__MemoryBarrier:
			case Operator.__CurrentTask:
			case Operator.__CompareAndSwap:
			case Operator.__vcSetReal:
			case Operator.__vcSetLReal:
			case Operator.__vcLoadReal:
			case Operator.__vcLoadLReal:
			case Operator.__vcStore:
			case Operator.XSizeOf:
			case Operator.__PouName:
			case Operator.__Position:
				return true;
			default:
				return false;
			}
		}

		private void AppendWithSeparator(string seperator, IEnumerable<IExprement> exprementsToVisit)
		{
			bool flag = true;
			foreach (_IExprement item in exprementsToVisit.OfType<_IExprement>())
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					Builder.Append(seperator);
				}
				item.AcceptVisitor(this);
			}
		}

		private static string MapTypeClassToName(TypeClass typeClass)
		{
			switch (typeClass)
			{
			case TypeClass.DateAndTime:
				return "DT";
			case TypeClass.TimeOfDay:
				return "TOD";
			case TypeClass.LTimeOfDay:
				return "LTOD";
			case TypeClass.UXInt:
				return "__UXINT";
			case TypeClass.XWord:
				return "__XWORD";
			case TypeClass.XInt:
				return "__XINT";
			default:
				return typeClass.ToString().ToUpperInvariant();
			}
		}
	}
}
