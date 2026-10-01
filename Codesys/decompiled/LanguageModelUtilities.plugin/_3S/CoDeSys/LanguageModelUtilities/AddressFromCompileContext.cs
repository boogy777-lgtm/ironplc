using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class AddressFromCompileContext : IExprVisitor
	{
		private int m_iArea = -1;

		private int m_iOffset;

		private Guid m_gdApplication;

		private IScope m_scope;

		private IScope Scope
		{
			get
			{
				if (m_scope == null)
				{
					m_scope = APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(m_gdApplication).CreateGlobalIScope();
				}
				return m_scope;
			}
		}

		public AddressFromCompileContext(Guid gdApplication)
		{
			m_gdApplication = gdApplication;
		}

		public void visit(ICompoAccessExpression compo)
		{
			IDataLocation dataLocation = compo.Right.DataLocation(Scope);
			if (dataLocation.IsRelativ)
			{
				m_iOffset += dataLocation.Offset;
				compo.Left.AcceptVisitor(this);
			}
			else
			{
				m_iOffset += dataLocation.Offset;
				m_iArea = dataLocation.Area;
			}
		}

		public void visit(IVariableExpression variable)
		{
			IDataLocation dataLocation = variable.DataLocation(Scope);
			if (dataLocation.IsRelativ)
			{
				m_iOffset += dataLocation.Offset;
				return;
			}
			m_iOffset += dataLocation.Offset;
			m_iArea = dataLocation.Area;
		}

		public void visit(IIndexAccessExpression indexaccess)
		{
			ICollection<IExpression> accesses = indexaccess.Accesses;
			ICompiledType deRefType = indexaccess.Var.Type.DeRefType;
			_ = indexaccess.Var.Type.BaseType;
			int num = 0;
			bool flag = false;
			if (deRefType.Class == TypeClass.Array)
			{
				if (deRefType is IArrayType arrayType && arrayType.Base != null)
				{
					IList<IArrayDimension> dimensions = arrayType.Dimensions;
					int num2 = deRefType.BaseType.Size(Scope);
					if (dimensions.Count == accesses.Count)
					{
						for (int num3 = dimensions.Count - 1; num3 >= 0; num3--)
						{
							IExpression obj = indexaccess.Accesses[num3];
							flag = false;
							bool bValid = false;
							int @int = obj.Literal(Scope).GetInt(out bValid);
							if (!bValid)
							{
								break;
							}
							int num4 = dimensions[num3].LowerBorderInt(out bValid, Scope);
							if (!bValid)
							{
								break;
							}
							int num5 = dimensions[num3].Range(out bValid, Scope);
							if (!bValid)
							{
								break;
							}
							num += (@int - num4) * num2;
							num2 *= num5;
							flag = true;
						}
						if (flag)
						{
							m_iOffset += num;
						}
					}
				}
				indexaccess.Var.AcceptVisitor(this);
				return;
			}
			throw new NotImplementedException("IndexAccessExpression");
		}

		public bool GetAddress(out int iArea, out int iOffset)
		{
			iArea = m_iArea;
			iOffset = m_iOffset;
			return m_iArea != -1;
		}

		public void visit(IForStatement forloop)
		{
		}

		public void visit(IContinueStatement cont)
		{
		}

		public void visit(IAssignmentExpression assign)
		{
		}

		public void visit(IReturnStatement returnst)
		{
		}

		public void visit(ILabelStatement label)
		{
		}

		public void visit(IPragmaStatement pragma)
		{
		}

		public void visit(ICallExpression call)
		{
		}

		public void visit(IConversionExpression conv)
		{
		}

		public void visit(IBaseExpression baseexp)
		{
		}

		public void visit(IAddressExpression address)
		{
		}

		public void visit(IDeRefAccessExpression deref)
		{
		}

		public void visit(IEmptyStatement empty)
		{
		}

		public void visit(ICaseLabelStatement caselabel)
		{
		}

		public void visit(IBreakPointStatement bpstate)
		{
		}

		public void visit(IVariableReference varref)
		{
		}

		public void visit(IPouReference pouref)
		{
		}

		public void visit(IPragmaOperatorExpression popexp)
		{
		}

		public void visit(IDefineStatement defstate)
		{
		}

		public void visit(IHasAttributeExpression hasattribute)
		{
		}

		public void visit(IPragmaAssertion assertion)
		{
		}

		public void visit(IHasValueExpression hasvalue)
		{
		}

		public void visit(IHasTypeExpression hastype)
		{
		}

		public void visit(IPragmaIfStatement pifst)
		{
		}

		public void visit(IDefinedExpression defexp)
		{
		}

		public void visit(ITypeReference typeref)
		{
		}

		public void visit(IDefineReference defref)
		{
		}

		public void visit(ICaseStatement casest)
		{
		}

		public void visit(ICaseRangeExpression caserange)
		{
		}

		public void visit(IGlobalScopeExpression globexp)
		{
		}

		public void visit(ILiteralExpression literal)
		{
		}

		public void visit(IThisExpression thisexp)
		{
		}

		public void visit(IOperatorExpression op)
		{
		}

		public void visit(IExpressionStatement expstat)
		{
		}

		public void visit(ICommentStatement comment)
		{
		}

		public void visit(IJumpStatement gotost)
		{
		}

		public void visit(IIfStatement ifst)
		{
		}

		public void visit(ISequenceStatement seq)
		{
		}

		public void visit(IExitStatement exit)
		{
		}

		public void visit(IRepeatStatement repeat)
		{
		}

		public void visit(IWhileStatement whilst)
		{
		}
	}
}
