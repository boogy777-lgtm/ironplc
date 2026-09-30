using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal static class TOU
	{
		private class DerefVisitor : IExprVisitor
		{
			private List<IDeRefAccessExpression> m_derefs;

			public bool ContainsDeref => m_derefs.Count != 0;

			public DerefVisitor()
			{
				m_derefs = new List<IDeRefAccessExpression>();
			}

			public void visit(IAssignmentExpression assign)
			{
				assign.LValue.AcceptVisitor(this);
				assign.RValue.AcceptVisitor(this);
			}

			public void visit(IExpressionStatement expstat)
			{
				expstat.Expr.AcceptVisitor(this);
			}

			public void visit(IOperatorExpression op)
			{
				IExpression[] operands = op.Operands;
				for (int i = 0; i < operands.Length; i++)
				{
					operands[i].AcceptVisitor(this);
				}
			}

			public void visit(IConversionExpression conv)
			{
				conv.Exp.AcceptVisitor(this);
			}

			public void visit(IIndexAccessExpression indexaccess)
			{
				indexaccess.Var.AcceptVisitor(this);
				IExpression[] accesses = indexaccess.Accesses;
				for (int i = 0; i < accesses.Length; i++)
				{
					accesses[i].AcceptVisitor(this);
				}
			}

			public void visit(ICompoAccessExpression compo)
			{
				compo.Left.AcceptVisitor(this);
				compo.Right.AcceptVisitor(this);
			}

			public void visit(ICallExpression call)
			{
				call.Callee.AcceptVisitor(this);
				IAssignmentExpression[] inputAssigns = call.InputAssigns;
				for (int i = 0; i < inputAssigns.Length; i++)
				{
					inputAssigns[i].AcceptVisitor(this);
				}
				inputAssigns = call.OutputAssigns;
				for (int i = 0; i < inputAssigns.Length; i++)
				{
					inputAssigns[i].AcceptVisitor(this);
				}
			}

			public void visit(IGlobalScopeExpression globexp)
			{
				globexp.Base.AcceptVisitor(this);
			}

			public void visit(IDeRefAccessExpression deref)
			{
				deref.Base.AcceptVisitor(this);
				m_derefs.Add(deref);
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

			public void visit(IThisExpression thisexp)
			{
			}

			public void visit(IBaseExpression baseexp)
			{
			}

			public void visit(ILiteralExpression literal)
			{
			}

			public void visit(IAddressExpression address)
			{
			}

			public void visit(IVariableExpression variable)
			{
			}

			public void visit(IEmptyStatement empty)
			{
			}

			public void visit(ICaseRangeExpression caserange)
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

			public void visit(IDefineReference defref)
			{
			}

			public void visit(IVariableReference varref)
			{
			}

			public void visit(ITypeReference typeref)
			{
			}

			public void visit(IPouReference pouref)
			{
			}

			public void visit(IDefinedExpression defexp)
			{
			}

			public void visit(IPragmaOperatorExpression popexp)
			{
			}

			public void visit(IPragmaIfStatement pifst)
			{
			}

			public void visit(IDefineStatement defstate)
			{
			}

			public void visit(IHasTypeExpression hastype)
			{
			}

			public void visit(IHasAttributeExpression hasattribute)
			{
			}

			public void visit(IHasValueExpression hasvalue)
			{
			}

			public void visit(IPragmaAssertion assertion)
			{
			}
		}

		public static IECExprInfo GetIECExprInfo(string stExpression, Guid gdApp)
		{
			if (stExpression.Length == 0)
			{
				return null;
			}
			IPreCompileContext precompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(gdApp);
			if (precompileContext == null)
			{
				return null;
			}
			IExpressionInfo expressionInfo = precompileContext.GetExpressionInfo(Guid.Empty, stExpression);
			if (expressionInfo == null)
			{
				return null;
			}
			bool num = expressionInfo.Expression is IVariableExpression || expressionInfo.Expression is ICompoAccessExpression;
			bool flag = expressionInfo.Expression is IDeRefAccessExpression;
			bool flag2 = expressionInfo.Expression is IIndexAccessExpression;
			bool flag3 = expressionInfo.Expression is ILiteralExpression;
			if (!num && !flag && !flag2)
			{
				IType baseType = GetBaseType(null, expressionInfo.Type, bIsIndexAccess: false);
				if (baseType == null)
				{
					return null;
				}
				if (flag3)
				{
					return new IECExprInfo(GetSmallestTypeOfLiteral((ILiteralExpression)expressionInfo.Expression), null, bBit: false, bProperty: false, "");
				}
				return new IECExprInfo(baseType, null, bBit: false, bProperty: false, "");
			}
			ISignature signature = null;
			IType type = null;
			IVariable variable = null;
			if (flag)
			{
				IDeRefAccessExpression deRefAccessExpression = (IDeRefAccessExpression)expressionInfo.Expression;
				IIdentifierInfo identifierInfo = ProcessLValueExp(precompileContext, deRefAccessExpression.Base);
				if (identifierInfo == null || identifierInfo.Variable == null || identifierInfo.Variable.Type == null)
				{
					return null;
				}
				signature = identifierInfo.Signature;
				type = GetBaseType(signature, identifierInfo.Variable.Type, bIsIndexAccess: false);
				variable = identifierInfo.Variable;
			}
			else if (expressionInfo.Expression is ICompoAccessExpression && ((ICompoAccessExpression)expressionInfo.Expression).Right is ILiteralExpression)
			{
				ICompoAccessExpression compoAccessExpression = (ICompoAccessExpression)expressionInfo.Expression;
				IIdentifierInfo identifierInfo2 = ProcessLValueExp(precompileContext, compoAccessExpression.Left);
				if (identifierInfo2 == null || identifierInfo2.Variable == null || identifierInfo2.Variable.Type == null)
				{
					return null;
				}
				signature = identifierInfo2.Signature;
				type = GetBaseType(signature, identifierInfo2.Variable.Type, bIsIndexAccess: false);
				variable = identifierInfo2.Variable;
			}
			else
			{
				IIdentifierInfo identifierInfo3 = ProcessLValueExp(precompileContext, expressionInfo.Expression);
				if (identifierInfo3 == null || identifierInfo3.Variable == null || identifierInfo3.Variable.Type == null)
				{
					return null;
				}
				signature = identifierInfo3.Signature;
				type = GetBaseType(signature, identifierInfo3.Variable.Type, bIsIndexAccess: true);
				variable = identifierInfo3.Variable;
			}
			if (type == null)
			{
				return null;
			}
			IDirectVariable directVariable = variable?.Address;
			bool bBit = false;
			if (type.Class == TypeClass.Bit || (directVariable != null && directVariable.Size == DirectVariableSize.X))
			{
				bBit = true;
			}
			bool bProperty = false;
			string stPropertyMonitor = string.Empty;
			if (variable != null && variable.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
			{
				bProperty = true;
				if (variable.HasAttribute(CompileAttributes.ATTRIBUTE_MONITORING) && variable.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) == CompileAttributes.ATTRIBUTEVALUE_VARIABLE)
				{
					ISignature signature2 = signature;
					if (signature2 != null)
					{
						IVariable[] all = signature2.All;
						foreach (IVariable variable2 in all)
						{
							if (variable2 == variable || !variable2.HasAttribute(CompileAttributes.ATTRIBUTE_USELOCATION))
							{
								continue;
							}
							string attributeValue = variable2.GetAttributeValue(CompileAttributes.ATTRIBUTE_USELOCATION);
							if (StrEqI(attributeValue, variable.Name))
							{
								int num2 = stExpression.LastIndexOf(attributeValue, StringComparison.InvariantCultureIgnoreCase);
								if (num2 >= 0)
								{
									stPropertyMonitor = stExpression.Remove(num2, attributeValue.Length);
									stPropertyMonitor += variable2.Name;
								}
							}
						}
					}
				}
			}
			return new IECExprInfo(type, variable, bBit, bProperty, stPropertyMonitor);
		}

		private static IType GetSmallestTypeOfLiteral(ILiteralExpression eil)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetTypeOfLiteral(eil, bLRealSupported: true, bTreatLRealAsReal: false, bInt64Supported: true, bTreatInt64AsInt32: false);
		}

		private static IIdentifierInfo ProcessLValueExp(IPreCompileContext pcc, IExpression exp)
		{
			ICompoAccessExpression obj = exp as ICompoAccessExpression;
			IIndexAccessExpression indexAccessExpression = exp as IIndexAccessExpression;
			IVariableExpression variableExpression = exp as IVariableExpression;
			if (obj != null || variableExpression != null)
			{
				IIdentifierInfo[] identifierInfo = pcc.GetIdentifierInfo(Guid.Empty, exp.ToString());
				if (identifierInfo == null || identifierInfo.Length != 1)
				{
					return null;
				}
				return identifierInfo[0];
			}
			if (indexAccessExpression != null)
			{
				return ProcessLValueExp(pcc, indexAccessExpression.Var);
			}
			return null;
		}

		private static IType GetBaseType(ISignature sig, IType t, bool bIsIndexAccess)
		{
			bool flag = false;
			IType type = t;
			bool flag2 = bIsIndexAccess;
			while (!flag && type != null)
			{
				switch (type.Class)
				{
				case TypeClass.Userdef:
					type = GetUserDefBaseType(type as IUserdefType2, sig);
					break;
				case TypeClass.Array:
					if (flag2)
					{
						type = ((IArrayType)type).Base;
						flag2 = false;
					}
					else
					{
						type = null;
					}
					break;
				case TypeClass.String:
				case TypeClass.WString:
					type = null;
					break;
				default:
					flag = true;
					break;
				}
			}
			return type;
		}

		private static bool StrEqI(string stA, string stB)
		{
			return string.Compare(stA, stB, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		private static IType GetUserDefBaseType(IUserdefType2 udt, ISignature sigType)
		{
			if (udt == null || udt.NameExpression == null || sigType == null)
			{
				return null;
			}
			IPreCompileContext2 precompileContextOfSignature = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(sigType);
			if (precompileContextOfSignature == null)
			{
				return null;
			}
			if (!(precompileContextOfSignature.CreatePrecompileScope(Guid.Empty) is IPrecompileScope2 precompileScope))
			{
				return null;
			}
			ISignature signature = precompileScope.FindSignatureGlobal(udt.NameExpression);
			if (signature == null)
			{
				return null;
			}
			IType type = null;
			if (signature.GetFlag(SignatureFlag.Enum) && signature.Constant != null && signature.Constant[0] != null && signature.Constant[0].OriginalType != null)
			{
				type = signature.Constant[0].OriginalType.BaseType;
			}
			else if (signature.GetFlag(SignatureFlag.Alias) && signature.All != null && signature.All.Length == 1)
			{
				type = signature.All[0].Type;
				if (type != null && type.Class == TypeClass.Userdef)
				{
					return GetUserDefBaseType(type as IUserdefType2, signature);
				}
			}
			return type;
		}

		internal static bool IsPointerDeref(string stExp, out string stPointerExp)
		{
			IExpression expression = ParseExpression(stExp);
			if (expression is IDeRefAccessExpression)
			{
				stPointerExp = ((IDeRefAccessExpression)expression).Base.ToString();
				return true;
			}
			stPointerExp = "";
			return false;
		}

		internal static bool ContainsPointerDeref(string stExp)
		{
			DerefVisitor derefVisitor = new DerefVisitor();
			IExpression expression = ParseExpression(stExp);
			if (expression == null)
			{
				return false;
			}
			expression.AcceptVisitor(derefVisitor);
			return derefVisitor.ContainsDeref;
		}

		public static IExpression ParseExpression(string stExp)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(stExp, bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);
			return (APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner) as IParser2).ParseExpression();
		}
	}
}
