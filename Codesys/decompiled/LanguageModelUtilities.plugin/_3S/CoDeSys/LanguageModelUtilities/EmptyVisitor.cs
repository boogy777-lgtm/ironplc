using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal abstract class EmptyVisitor : IExprementVisitorNoTraversion
	{
		private IStandardTraverser m_traverser;

		public IStandardTraverser Traverser
		{
			get
			{
				return m_traverser;
			}
			set
			{
				m_traverser = value;
			}
		}

		public virtual void visit(IVariableExpression variable, AccessFlag access, IPrecompileScope5 scope)
		{
		}

		public virtual void visit(ICompiledPOU cpou)
		{
		}

		public virtual void visit(IWhileStatement whilst)
		{
		}

		public virtual void visit(IRepeatStatement repeat)
		{
		}

		public virtual void visit(IForStatement forloop)
		{
		}

		public virtual void visit(IExitStatement exit)
		{
		}

		public virtual void visit(IContinueStatement cont)
		{
		}

		public virtual void visit(ISequenceStatement seq)
		{
		}

		public virtual void visit(IAssignmentExpression assign)
		{
		}

		public virtual void visit(IIfStatement ifst)
		{
		}

		public virtual void visit(IReturnStatement returnst)
		{
		}

		public virtual void visit(IJumpStatement gotost)
		{
		}

		public virtual void visit(ILabelStatement label)
		{
		}

		public virtual void visit(ICommentStatement comment)
		{
		}

		public virtual void visit(IPragmaStatement pragma)
		{
		}

		public virtual void visit(IExpressionStatement expstat)
		{
		}

		public virtual void visit(ICallExpression call)
		{
		}

		public virtual void visit(IOperatorExpression op)
		{
		}

		public virtual void visit(ICastExpression cast)
		{
		}

		public virtual void visit(INewExpression newexp)
		{
		}

		public virtual void visit(ITypeExpression typeexp)
		{
		}

		public virtual void visit(IConversionExpression conv)
		{
		}

		public virtual void visit(IThisExpression thisexp)
		{
		}

		public virtual void visit(IBaseExpression baseexp)
		{
		}

		public virtual void visit(ILiteralExpression literal)
		{
		}

		public virtual void visit(IAddressExpression address)
		{
		}

		public virtual void visit(IIndexAccessExpression indexaccess)
		{
		}

		public virtual void visit(ICompoAccessExpression compo, AccessFlag access, IPrecompileScope5 scope)
		{
		}

		public virtual void visit(IDeRefAccessExpression deref)
		{
		}

		public virtual void visit(IGlobalScopeExpression globexp)
		{
		}

		public virtual void visit(ISystemScopeExpression systemscope)
		{
		}

		public virtual void visit(IEmptyStatement empty)
		{
		}

		public virtual void visit(ICaseRangeExpression caserange)
		{
		}

		public virtual void visit(ICaseLabelStatement caselabel)
		{
		}

		public virtual void visit(ICaseStatement casest)
		{
		}

		public virtual void visit(IErrorExpression errorexp)
		{
		}

		public virtual void visit(INullExpression errorexp)
		{
		}

		public virtual void visit(IQualifiedNameExpression qne)
		{
		}

		public virtual void visit(IVariableDeclarationStatement vds)
		{
		}

		public virtual void visit(IVariableDeclarationListStatement vdls)
		{
		}

		public virtual void visit(IPOUDeclarationStatement pds)
		{
		}

		public virtual void visit(ITypeDeclarationStatement tds)
		{
		}

		public virtual void visit(IEnumDeclarationStatement eds)
		{
		}

		public virtual void visit(IEnumDeclarationListStatement eds)
		{
		}

		public virtual void visit(IDefineReference defref)
		{
		}

		public virtual void visit(IVariableReference varref)
		{
		}

		public virtual void visit(ITypeReference typeref)
		{
		}

		public virtual void visit(IPouReference pouref)
		{
		}

		public virtual void visit(IDefinedExpression defexp)
		{
		}

		public virtual void visit(IPragmaOperatorExpression popexp)
		{
		}

		public virtual void visit(IPragmaIfStatement pifst)
		{
		}

		public virtual void visit(IBreakPointStatement bpstate)
		{
		}

		public virtual void visit(IDefineStatement defstate)
		{
		}

		public virtual void visit(IHasTypeExpression hastype)
		{
		}

		public virtual void visit(IIsEnumTypeExpression isenumtype)
		{
		}

		public virtual void visit(IHasAttributeExpression hasattribute)
		{
		}

		public virtual void visit(IHasValueExpression hasvalue)
		{
		}

		public virtual void visit(IHasConstantValueExpression hasvalue)
		{
		}

		public virtual void visit(IPragmaAssertion assertion)
		{
		}

		public virtual void visit(ICompilerVersionExpression compversion)
		{
		}
	}
}
