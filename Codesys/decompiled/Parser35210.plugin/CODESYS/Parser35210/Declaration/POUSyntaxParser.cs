using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Declaration
{
	internal class POUSyntaxParser
	{
		private ParserContext Context { get; }

		private List<IPOUSyntax> POUSyntax { get; }

		private POUSyntaxParser(ParserContext context)
		{
			Context = context;
			POUSyntax = new List<IPOUSyntax>();
		}

		public static IPOUSyntax[] ParsePOUSyntax(ParserContext context)
		{
			POUSyntaxParser pOUSyntaxParser = new POUSyntaxParser(context);
			pOUSyntaxParser._ParsePOUStatements();
			return pOUSyntaxParser.POUSyntax.ToArray();
		}

		private void _ParsePOUStatements()
		{
			IList<SyntaxElement> syntaxElements = SyntaxElementParser.ParseSyntaxElements(Context);
			int nIndex = 0;
			bool bEndOfPOUFound;
			do
			{
				IPOUSyntax iPOUSyntax = NextTopLevelPOUSyntax(syntaxElements, ref nIndex, out bEndOfPOUFound);
				if (iPOUSyntax != null && iPOUSyntax.Declaration != null)
				{
					POUSyntax.Add(iPOUSyntax);
					continue;
				}
				break;
			}
			while (bEndOfPOUFound);
		}

		private POUSyntax NextTopLevelPOUSyntax(IList<SyntaxElement> syntaxElements, ref int nIndex, out bool bEndOfPOUFound)
		{
			_ISequenceStatement iSequenceStatement = Context.LMItemFactory.CreateSequenceStatement();
			POUSyntax pOUSyntax = new POUSyntax();
			Operator expectedEndOfPOU = Operator.None;
			bEndOfPOUFound = false;
			bool flag = true;
			while (nIndex < syntaxElements.Count)
			{
				if (syntaxElements[nIndex] is StatementElement statementElement)
				{
					if (statementElement.Statement is _IPOUDeclarationStatement iPOUDeclarationStatement)
					{
						nIndex++;
						if (flag)
						{
							iSequenceStatement.AddStatement(statementElement.Statement);
							pOUSyntax.Declaration = iSequenceStatement;
							iSequenceStatement = (pOUSyntax.Implementation = Context.LMItemFactory.CreateSequenceStatement());
							expectedEndOfPOU = GetExpectedEndOfOperator(iPOUDeclarationStatement);
							short nMinus = MoveTrailingCommentsAndPragmasToImplementation(iPOUDeclarationStatement, iSequenceStatement);
							AdaptDeclarationPosition(iPOUDeclarationStatement, nMinus);
							flag = false;
						}
						else
						{
							ReadSubPou(pOUSyntax, iPOUDeclarationStatement, iSequenceStatement, syntaxElements, ref nIndex);
						}
						continue;
					}
					iSequenceStatement.AddStatement(statementElement.Statement);
				}
				else if (syntaxElements[nIndex] is EndOfPOUElement epe)
				{
					CheckMatchingEndOfPOU(expectedEndOfPOU, epe, pOUSyntax);
					bEndOfPOUFound = true;
					nIndex++;
					FixPouSyntaxPosition(pOUSyntax);
					return pOUSyntax;
				}
				nIndex++;
			}
			return pOUSyntax;
		}

		private static void FixPouSyntaxPosition(IPOUSyntax pouSyntax)
		{
			FixSequenceStatementPosition(pouSyntax.Declaration);
			FixSequenceStatementPosition(pouSyntax.Implementation);
			foreach (IPOUSyntax subPOU in pouSyntax.SubPOUs)
			{
				FixPouSyntaxPosition(subPOU);
			}
		}

		private static void FixSequenceStatementPosition(_ISequenceStatement st)
		{
			if (st != null && st._StatementList.Count > 0)
			{
				st.PositionIntern = st._StatementList[0].PositionIntern;
			}
		}

		private void ReadSubPou(POUSyntax pouSyntax, _IPOUDeclarationStatement pouDeclarationStatement, _ISequenceStatement sequenceStatement, IList<SyntaxElement> syntaxElements, ref int nIndex)
		{
			POUSyntax pOUSyntax = new POUSyntax
			{
				Declaration = Context.LMItemFactory.CreateSequenceStatement(),
				Implementation = Context.LMItemFactory.CreateSequenceStatement()
			};
			Operator expectedEndOfOperator = GetExpectedEndOfOperator(pouDeclarationStatement);
			MoveLastCommentsAndPragmas(sequenceStatement, pOUSyntax.Declaration);
			pOUSyntax.Declaration.AddStatement(pouDeclarationStatement);
			short nMinus = MoveTrailingCommentsAndPragmasToImplementation(pouDeclarationStatement, pOUSyntax.Implementation);
			AdaptDeclarationPosition(pouDeclarationStatement, nMinus);
			while (nIndex < syntaxElements.Count)
			{
				SyntaxElement syntaxElement = syntaxElements[nIndex];
				if (!(syntaxElement is StatementElement statementElement))
				{
					if (syntaxElement is EndOfPOUElement epe)
					{
						CheckMatchingEndOfPOU(expectedEndOfOperator, epe, pOUSyntax);
						pouSyntax.AddSubpou(pOUSyntax);
						nIndex++;
						break;
					}
				}
				else
				{
					pOUSyntax.Implementation.AddStatement(statementElement.Statement);
				}
				nIndex++;
			}
		}

		private void CheckMatchingEndOfPOU(Operator expectedEndOfPOU, EndOfPOUElement epe, POUSyntax pouSyntax)
		{
			if (expectedEndOfPOU != epe.EndOfPOUOperator)
			{
				_IStatement iStatement = Context.LMItemFactory.CreateErrorStatement();
				iStatement.PositionIntern = Context.LMItemFactory.CreateMinimalPosition(epe.Position, epe.PositionOffset);
				Context.ErrorHandler.AddErrorST(iStatement, MessageId.Err_UnexpectedStatement);
				pouSyntax.Implementation.AddStatement(iStatement);
			}
		}

		private void AdaptDeclarationPosition(_IPOUDeclarationStatement pou, short nMinus)
		{
			if (pou != null)
			{
				pou.LengthIntern -= nMinus;
			}
		}

		private static short MoveTrailingCommentsAndPragmasToImplementation(_IPOUDeclarationStatement declarationStatement, _ISequenceStatement sequenceStatement)
		{
			if (declarationStatement.Declarations is _ISequenceStatement seq)
			{
				return MoveLastCommentsAndPragmas(seq, sequenceStatement);
			}
			return 0;
		}

		private static short MoveLastCommentsAndPragmas(_ISequenceStatement seq, _ISequenceStatement sequenceStatement)
		{
			short num = 0;
			while (seq.Statements.Any())
			{
				_IStatement iStatement = seq._StatementList.Last();
				if (!(iStatement is _ICommentStatement) && !(iStatement is _IPragmaStatement) && !(iStatement is _IDefineStatement) && !(iStatement is _IEmptyStatement) && (!(iStatement is _IPragmaIfStatement ifStatement) || ContainsDeclarationSyntax(ifStatement)))
				{
					break;
				}
				seq._StatementList.Remove(iStatement);
				sequenceStatement.InsertStatement(0, iStatement);
				num = (short)(num + iStatement.LengthIntern);
			}
			return num;
		}

		private static bool ContainsDeclarationSyntax(_IPragmaIfStatement ifStatement)
		{
			return DeclarationStatementDetector.CheckIfContainsDeclarationSyntax(ifStatement);
		}

		private Operator GetExpectedEndOfOperator(_IPOUDeclarationStatement pouDeclarationStatement)
		{
			if (pouDeclarationStatement == null)
			{
				return Operator.None;
			}
			switch (pouDeclarationStatement.Class)
			{
			case Operator.Function:
				return Operator.EndFunction;
			case Operator.FunctionBlock:
				return Operator.EndFunctionBlock;
			case Operator.Action:
				return Operator.EndAction;
			case Operator.Program:
				return Operator.EndProgram;
			case Operator.Method:
				return Operator.EndMethod;
			case Operator.Interface:
				return Operator.EndInterface;
			default:
				return Operator.None;
			}
		}
	}
}
