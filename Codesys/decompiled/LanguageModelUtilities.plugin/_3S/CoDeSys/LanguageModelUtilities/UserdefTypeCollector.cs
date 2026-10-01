using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "High coupling is unavoidable for the visitor pattern")]
	internal sealed class UserdefTypeCollector : IExprVisitor8, IExprVisitor7, IExprVisitor6, IExprVisitor5, IExprVisitor4, IExprVisitor3, IExprVisitor2, IExprVisitor
	{
		private IPrecompileScope2 _precompileScope;

		private LList<AttributedString> _lstTokens;

		private readonly IScanner _scanner = APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.CreateScanner(string.Empty, bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);

		private static readonly LHashSet<Operator> s_hsBinaryPrefixOperators;

		internal static UserdefTypeCollector Instance { get; }

		private UserdefTypeCollector()
		{
		}

		internal LList<AttributedString> CollectUserdefTypes(IType type, IPrecompileScope2 precompileScope)
		{
			_precompileScope = precompileScope;
			_lstTokens = new LList<AttributedString>();
			Collect(type, null);
			return _lstTokens;
		}

		internal LList<AttributedString> CollectUserdefTypes(IExpression expr, IPrecompileScope2 precompileScope)
		{
			_precompileScope = precompileScope;
			_lstTokens = new LList<AttributedString>();
			expr.AcceptVisitor(this);
			return _lstTokens;
		}

		private void Collect(IType type, ISignature2 sign)
		{
			if (type is IGenericUserdefType genericUserdefType)
			{
				CollectGenericUserdef(sign, genericUserdefType);
			}
			else if (type is IUserdefType userdefType)
			{
				CollectUserdef(sign, userdefType);
			}
			else if (type is IPointerType pointerType)
			{
				CollectPointer(pointerType);
			}
			else if (type is IReferenceType referenceType)
			{
				CollectReference(referenceType);
			}
			else if (type is IArrayType arrayType)
			{
				CollectArray(arrayType);
			}
			else if (type is _IVariableLengthArrayType arrayType2)
			{
				CollectVarLenArray(arrayType2);
			}
			else if (type is ISubrangeType2 subrangeType)
			{
				CollectSubrange(subrangeType);
			}
			else
			{
				_lstTokens.Add((AttributedString)new PlainTextAttributedString(type.ToString()));
			}
		}

		private void CollectSubrange(ISubrangeType2 subrangeType)
		{
			_lstTokens.Add((AttributedString)new PlainTextAttributedString(subrangeType.BaseType.ToString()));
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("("));
			subrangeType.LowerBorder.AcceptVisitor(this);
			_lstTokens.Add((AttributedString)new PlainTextAttributedString(".."));
			subrangeType.UpperBorder.AcceptVisitor(this);
			_lstTokens.Add((AttributedString)new PlainTextAttributedString(")"));
		}

		private void CollectVarLenArray(_IVariableLengthArrayType arrayType)
		{
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("ARRAY ["));
			int dimensions = arrayType.Dimensions;
			for (int i = 0; i < dimensions; i++)
			{
				if (0 < i)
				{
					_lstTokens.Add((AttributedString)new PlainTextAttributedString(", "));
				}
				_lstTokens.Add((AttributedString)new PlainTextAttributedString("*"));
			}
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("] OF "));
			Collect(arrayType._Base, null);
		}

		private void CollectArray(IArrayType arrayType)
		{
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("ARRAY ["));
			IArrayDimension[] dimensions = arrayType.Dimensions;
			for (int i = 0; i < dimensions.Length; i++)
			{
				if (0 < i)
				{
					_lstTokens.Add((AttributedString)new PlainTextAttributedString(", "));
				}
				dimensions[i].LowerBorder.AcceptVisitor(this);
				_lstTokens.Add((AttributedString)new PlainTextAttributedString(".."));
				dimensions[i].UpperBorder.AcceptVisitor(this);
			}
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("] OF "));
			Collect(arrayType.Base, null);
		}

		private void CollectReference(IReferenceType referenceType)
		{
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("REFERENCE TO "));
			Collect(referenceType.Base, null);
		}

		private void CollectPointer(IPointerType pointerType)
		{
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("POINTER TO "));
			Collect(pointerType.Base, null);
		}

		private void CollectUserdef(ISignature2 sign, IUserdefType userdefType)
		{
			if (sign == null && -1 != userdefType.SignatureId)
			{
				sign = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetSignatureForPrecompileID(userdefType.SignatureId);
			}
			string text = userdefType.ToString();
			if (sign != null)
			{
				_lstTokens.Add((AttributedString)new UserdefTypeAttributedString(text, userdefType, bAvailable: true));
				return;
			}
			if ("VERSION".Equals(text, StringComparison.InvariantCultureIgnoreCase))
			{
				_lstTokens.Add((AttributedString)new PlainTextAttributedString(text));
				return;
			}
			bool bAvailable = _precompileScope.FindSignatureGlobal(((IUserdefType2)userdefType).NameExpression) != null;
			_lstTokens.Add((AttributedString)new UserdefTypeAttributedString(text, userdefType, bAvailable));
		}

		private void CollectGenericUserdef(ISignature2 sign, IGenericUserdefType genericUserdefType)
		{
			CollectUserdef(sign, genericUserdefType.GetNonGenericBaseType());
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("<"));
			bool flag = true;
			foreach (_IExpression genericConstantsInitialization in genericUserdefType.GenericConstantsInitializations)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					_lstTokens.Add((AttributedString)new PlainTextAttributedString(", "));
				}
				genericConstantsInitialization.AcceptVisitor(this);
			}
			_lstTokens.Add((AttributedString)new PlainTextAttributedString(">"));
		}

		private bool DetermineTypeOfExpression(IExpression expr)
		{
			if (!(_precompileScope.FindSignatureGlobal(expr) is ISignature2 signature))
			{
				return false;
			}
			ILMPreCompileSet precompileSetOfSignature = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetPrecompileSetOfSignature(signature);
			if (precompileSetOfSignature == null)
			{
				return false;
			}
			IExpressionInfo expressionInfo = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileSmartCodingService.GetExpressionInfo(precompileSetOfSignature, signature.ObjectGuid, signature.NameExpression.ToString());
			if (expressionInfo?.Type == null)
			{
				return false;
			}
			if (expr is ICompoAccessExpression && !string.IsNullOrEmpty(precompileSetOfSignature.Namespace))
			{
				_lstTokens.Add((AttributedString)new PlainTextAttributedString(precompileSetOfSignature.Namespace + "."));
			}
			Collect(expressionInfo.Type, signature);
			return true;
		}

		private bool AddNotAvailableUserdefTypeAttributedString(ICompoAccessExpression compo, ICompoAccessExpression namespaceAndTypeContainingExpression)
		{
			if (namespaceAndTypeContainingExpression == null)
			{
				return false;
			}
			_lstTokens.Add((AttributedString)new UserdefTypeAttributedString(namespaceAndTypeContainingExpression.ToString(), null, bAvailable: false));
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("."));
			compo.Right.AcceptVisitor(this);
			return true;
		}

		private bool DetermineUserdefTypeForSignature(ICompoAccessExpression compo, ICompoAccessExpression namespaceAndTypeContainingExpression, ISignature2 sign)
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Expected O, but got Unknown
			ILMPreCompileSet precompileSetOfSignature = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetPrecompileSetOfSignature(sign);
			if (precompileSetOfSignature == null)
			{
				return false;
			}
			if (APEnvironmentFacade.Instance.LMServiceProvider.PreCompileSmartCodingService.GetExpressionInfo(precompileSetOfSignature, sign.ObjectGuid, sign.NameExpression.ToString())?.Type is IUserdefType udt)
			{
				LStringBuilder val = new LStringBuilder();
				if (namespaceAndTypeContainingExpression != null)
				{
					val.Append(namespaceAndTypeContainingExpression.Left.ToString() + ".");
				}
				val.Append(sign.OrgName);
				_lstTokens.Add((AttributedString)new UserdefTypeAttributedString(((object)val).ToString(), udt, bAvailable: true));
				_lstTokens.Add((AttributedString)new PlainTextAttributedString("."));
				compo.Right.AcceptVisitor(this);
				return true;
			}
			return false;
		}

		private void AddPlainTextAttributedString(IExpression expr)
		{
			_lstTokens.Add((AttributedString)new PlainTextAttributedString(expr.ToString()));
		}

		private void VisitCommaSeparatedListOfExpressions(IEnumerable<IExpression> expressions)
		{
			bool flag = false;
			foreach (IExpression expression in expressions)
			{
				if (flag)
				{
					_lstTokens.Add((AttributedString)new PlainTextAttributedString(", "));
				}
				expression.AcceptVisitor(this);
				flag = true;
			}
		}

		public void visit(IWhileStatement whilst)
		{
		}

		public void visit(IRepeatStatement repeat)
		{
		}

		public void visit(IForStatement forloop)
		{
		}

		public void visit(IExitStatement exit)
		{
		}

		public void visit(IContinueStatement cont)
		{
		}

		public void visit(ISequenceStatement seq)
		{
		}

		public void visit(IIfStatement ifst)
		{
		}

		public void visit(IReturnStatement returnst)
		{
		}

		public void visit(IJumpStatement gotost)
		{
		}

		public void visit(ILabelStatement label)
		{
		}

		public void visit(ICommentStatement comment)
		{
		}

		public void visit(IPragmaStatement pragma)
		{
		}

		public void visit(IExpressionStatement expstat)
		{
		}

		public void visit(IEmptyStatement empty)
		{
		}

		public void visit(ICaseLabelStatement caselabel)
		{
		}

		public void visit(ICaseStatement casest)
		{
		}

		public void visit(IBreakPointStatement bpstate)
		{
		}

		public void visit(IPragmaIfStatement pifst)
		{
		}

		public void visit(IDefineStatement defstate)
		{
		}

		public void visit(IAddressExpression address)
		{
			AddPlainTextAttributedString(address);
		}

		public void visit(ICaseRangeExpression caserange)
		{
			AddPlainTextAttributedString(caserange);
		}

		public void visit(IDefineReference defref)
		{
			AddPlainTextAttributedString(defref);
		}

		public void visit(IVariableReference varref)
		{
			AddPlainTextAttributedString(varref);
		}

		public void visit(ITypeReference typeref)
		{
			AddPlainTextAttributedString(typeref);
		}

		public void visit(IPouReference pouref)
		{
			AddPlainTextAttributedString(pouref);
		}

		public void visit(IDefinedExpression defexp)
		{
			AddPlainTextAttributedString(defexp);
		}

		public void visit(IPragmaOperatorExpression popexp)
		{
			AddPlainTextAttributedString(popexp);
		}

		public void visit(IHasTypeExpression hastype)
		{
			AddPlainTextAttributedString(hastype);
		}

		public void visit(IHasAttributeExpression hasattribute)
		{
			AddPlainTextAttributedString(hasattribute);
		}

		public void visit(IHasValueExpression hasvalue)
		{
			AddPlainTextAttributedString(hasvalue);
		}

		public void visit(IPragmaAssertion assertion)
		{
		}

		public void visit(IThisExpression thisexp)
		{
			AddPlainTextAttributedString(thisexp);
		}

		public void visit(IBaseExpression baseexp)
		{
			AddPlainTextAttributedString(baseexp);
		}

		public void visit(IGlobalScopeExpression globexp)
		{
			AddPlainTextAttributedString(globexp);
		}

		public void visit(IDeRefAccessExpression deref)
		{
			deref.Base.AcceptVisitor(this);
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("^"));
		}

		public void visit(IAssignmentExpression assign)
		{
			if (!(assign.LValue is INullExpression))
			{
				assign.LValue.AcceptVisitor(this);
				_lstTokens.Add((AttributedString)new PlainTextAttributedString(" := "));
			}
			assign.RValue.AcceptVisitor(this);
		}

		public void visit(IIndexAccessExpression indexaccess)
		{
			indexaccess.Var.AcceptVisitor(this);
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("["));
			VisitCommaSeparatedListOfExpressions(indexaccess.Accesses);
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("]"));
		}

		public void visit(ICallExpression call)
		{
			AddPlainTextAttributedString(call.Callee);
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("("));
			VisitCommaSeparatedListOfExpressions(call.InputAssigns);
			_lstTokens.Add((AttributedString)new PlainTextAttributedString(")"));
		}

		public void visit(IConversionExpression conv)
		{
			_lstTokens.Add((AttributedString)new PlainTextAttributedString(conv.ToString()));
		}

		public void visit(IVariableExpression variable)
		{
			if (!DetermineTypeOfExpression(variable))
			{
				_lstTokens.Add((AttributedString)new PlainTextAttributedString(variable.ToString()));
			}
		}

		public void visit(ICompoAccessExpression compo)
		{
			ISignature2 signature = _precompileScope.FindSignatureGlobal(compo.Left) as ISignature2;
			ICompoAccessExpression compoAccessExpression = compo.Left as ICompoAccessExpression;
			if (signature == null && compoAccessExpression == null && compo.Left is IVariableExpression3 variableExpression && -1 != variableExpression.PrecompileSignatureId)
			{
				signature = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetSignatureForPrecompileID(variableExpression.PrecompileSignatureId);
			}
			bool flag = ((signature != null) ? DetermineUserdefTypeForSignature(compo, compoAccessExpression, signature) : AddNotAvailableUserdefTypeAttributedString(compo, compoAccessExpression));
			if (!flag && !DetermineTypeOfExpression(compo))
			{
				compo.Left.AcceptVisitor(this);
				_lstTokens.Add((AttributedString)new PlainTextAttributedString("."));
				compo.Right.AcceptVisitor(this);
			}
		}

		public void visit(IOperatorExpression op)
		{
			IExpression[] operands = op.Operands;
			bool flag = false;
			switch (operands.Length)
			{
			case 1:
				flag = true;
				break;
			case 2:
				if (s_hsBinaryPrefixOperators.Contains(op.Code))
				{
					flag = true;
					break;
				}
				_lstTokens.Add((AttributedString)new PlainTextAttributedString("("));
				operands[0].AcceptVisitor(this);
				_lstTokens.Add((AttributedString)new PlainTextAttributedString(_scanner.GetOperatorText(op.Code, bShort: false)));
				operands[1].AcceptVisitor(this);
				_lstTokens.Add((AttributedString)new PlainTextAttributedString(")"));
				break;
			default:
				flag = true;
				break;
			}
			if (!flag)
			{
				return;
			}
			_lstTokens.Add((AttributedString)new PlainTextAttributedString(_scanner.GetOperatorText(op.Code, bShort: false)));
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("("));
			for (int i = 0; i < operands.Length; i++)
			{
				if (0 < i)
				{
					_lstTokens.Add((AttributedString)new PlainTextAttributedString(", "));
				}
				operands[i].AcceptVisitor(this);
			}
			_lstTokens.Add((AttributedString)new PlainTextAttributedString(")"));
		}

		public void visit(ILiteralExpression literal)
		{
			_lstTokens.Add((AttributedString)new PlainTextAttributedString(literal.ToString()));
		}

		public void visit(IHasConstantValueExpression hasvalue)
		{
			AddPlainTextAttributedString(hasvalue);
		}

		public void visit(IStructureInitialization structInit)
		{
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("STRUCT("));
			VisitCommaSeparatedListOfExpressions(structInit.CompoInits);
			_lstTokens.Add((AttributedString)new PlainTextAttributedString(")"));
		}

		public void visit(IArrayInitialization arrayInit)
		{
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("["));
			VisitCommaSeparatedListOfExpressions(arrayInit.InitValues);
			_lstTokens.Add((AttributedString)new PlainTextAttributedString("]"));
		}

		public void visit(ICastExpression cast)
		{
			AddPlainTextAttributedString(cast);
		}

		public void visit(ICompilerVersionExpression compverExpr)
		{
		}

		public void visit(IRuntimeVersionExpression runverExpr)
		{
			AddPlainTextAttributedString(runverExpr);
		}

		public void visit(INamespaceAccessExpression namespaceaccess)
		{
			AddPlainTextAttributedString(namespaceaccess);
		}

		public void visit(IErrorExpression error)
		{
			AddPlainTextAttributedString(error);
		}

		public void visit(IErrorStatement error)
		{
		}

		public void visit(IHasConstantTypeExpression hasConstantTypeExpression)
		{
			AddPlainTextAttributedString(hasConstantTypeExpression);
		}

		public void visit(IPartialAccessExpression partialAccessExpression)
		{
			partialAccessExpression.Left.AcceptVisitor(this);
			_lstTokens.Add((AttributedString)new PlainTextAttributedString($".%{partialAccessExpression.PartSize}{partialAccessExpression.PartOffset}"));
		}

		static UserdefTypeCollector()
		{
			LHashSet<Operator> obj = new LHashSet<Operator>();
			obj.Add(Operator.Min);
			obj.Add(Operator.Max);
			obj.Add(Operator.Rol);
			obj.Add(Operator.Ror);
			obj.Add(Operator.Shl);
			obj.Add(Operator.Shr);
			s_hsBinaryPrefixOperators = obj;
			Instance = new UserdefTypeCollector();
		}
	}
}
