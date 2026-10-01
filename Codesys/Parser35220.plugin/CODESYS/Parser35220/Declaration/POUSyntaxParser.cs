using System;
using System.Collections.Generic;
using System.Linq;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x02000057 RID: 87
	internal class POUSyntaxParser
	{
		// Token: 0x17000157 RID: 343
		// (get) Token: 0x06000582 RID: 1410 RVA: 0x00017141 File Offset: 0x00015341
		private ParserContext Context { get; }

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x06000583 RID: 1411 RVA: 0x00017149 File Offset: 0x00015349
		private List<IPOUSyntax> POUSyntax { get; }

		// Token: 0x06000584 RID: 1412 RVA: 0x00017151 File Offset: 0x00015351
		private POUSyntaxParser(ParserContext context)
		{
			this.Context = context;
			this.POUSyntax = new List<IPOUSyntax>();
		}

		// Token: 0x06000585 RID: 1413 RVA: 0x0001716B File Offset: 0x0001536B
		public static IPOUSyntax[] ParsePOUSyntax(ParserContext context)
		{
			POUSyntaxParser pousyntaxParser = new POUSyntaxParser(context);
			pousyntaxParser._ParsePOUStatements();
			return pousyntaxParser.POUSyntax.ToArray();
		}

		// Token: 0x06000586 RID: 1414 RVA: 0x00017184 File Offset: 0x00015384
		private void _ParsePOUStatements()
		{
			IList<SyntaxElement> syntaxElements = SyntaxElementParser.ParseSyntaxElements(this.Context);
			int num = 0;
			bool flag;
			do
			{
				IPOUSyntax ipousyntax = this.NextTopLevelPOUSyntax(syntaxElements, ref num, out flag);
				POUSyntaxParser.FixPouSyntaxPosition(ipousyntax);
				if (ipousyntax == null || ipousyntax.Declaration == null)
				{
					break;
				}
				this.POUSyntax.Add(ipousyntax);
			}
			while (flag);
		}

		// Token: 0x06000587 RID: 1415 RVA: 0x000171CC File Offset: 0x000153CC
		private POUSyntax NextTopLevelPOUSyntax(IList<SyntaxElement> syntaxElements, ref int nIndex, out bool bEndOfPOUFound)
		{
			_ISequenceStatement isequenceStatement = this.Context.LMItemFactory.CreateSequenceStatement();
			POUSyntax pousyntax = new POUSyntax();
			Operator expectedEndOfPOU = 0;
			bEndOfPOUFound = false;
			bool flag = true;
			_IPOUDeclarationStatement parentPouDeclStatement = null;
			while (nIndex < syntaxElements.Count)
			{
				StatementElement statementElement = syntaxElements[nIndex] as StatementElement;
				if (statementElement != null)
				{
					if (this.TryProcessTypeDeclarationStatement(ref nIndex, ref bEndOfPOUFound, isequenceStatement, pousyntax, flag, statementElement))
					{
						return pousyntax;
					}
					_IPOUDeclarationStatement ipoudeclarationStatement = statementElement.Statement as _IPOUDeclarationStatement;
					if (ipoudeclarationStatement != null)
					{
						nIndex++;
						if (flag)
						{
							this.CheckInvalidAccessModifier(ipoudeclarationStatement);
							parentPouDeclStatement = ipoudeclarationStatement;
							isequenceStatement.AddStatement(statementElement.Statement);
							pousyntax.Declaration = isequenceStatement;
							isequenceStatement = this.Context.LMItemFactory.CreateSequenceStatement();
							pousyntax.Implementation = isequenceStatement;
							expectedEndOfPOU = this.GetExpectedEndOfOperator(ipoudeclarationStatement);
							short nMinus = POUSyntaxParser.MoveTrailingCommentsAndPragmasToImplementation(ipoudeclarationStatement, isequenceStatement);
							this.AdaptDeclarationPosition(ipoudeclarationStatement, nMinus);
							flag = false;
							continue;
						}
						this.ReadSubPou(pousyntax, ipoudeclarationStatement, isequenceStatement, syntaxElements, parentPouDeclStatement, ref nIndex);
						continue;
					}
					else
					{
						isequenceStatement.AddStatement(statementElement.Statement);
					}
				}
				else
				{
					EndOfPOUElement endOfPOUElement = syntaxElements[nIndex] as EndOfPOUElement;
					if (endOfPOUElement != null)
					{
						this.CheckMatchingEndOfPOUAndTryCompletePOUSyntax(pousyntax, expectedEndOfPOU, endOfPOUElement, isequenceStatement);
						bEndOfPOUFound = true;
						nIndex++;
						return pousyntax;
					}
				}
				nIndex++;
			}
			this.TryCompletePOUSyntax(pousyntax, isequenceStatement, 93);
			return pousyntax;
		}

		// Token: 0x06000588 RID: 1416 RVA: 0x00017300 File Offset: 0x00015500
		private void CheckInvalidAccessModifier(_IPOUDeclarationStatement pouDeclarationStatement)
		{
			if (pouDeclarationStatement.Class == 287)
			{
				this.CheckForUnexpectedAccessModifier(pouDeclarationStatement, 1099511627776L, 227);
				this.CheckForUnexpectedAccessModifier(pouDeclarationStatement, 2199023255552L, 228);
				this.CheckForUnexpectedAccessModifier(pouDeclarationStatement, 8796093022208L, 230);
				this.CheckForUnexpectedAccessModifier(pouDeclarationStatement, 140737488355328L, 224);
			}
		}

		// Token: 0x06000589 RID: 1417 RVA: 0x00017370 File Offset: 0x00015570
		private bool TryProcessTypeDeclarationStatement(ref int nIndex, ref bool bEndOfPOUFound, _ISequenceStatement sequenceStatement, POUSyntax pouSyntax, bool bFirstDeclaration, StatementElement se)
		{
			if (se.Statement is _ITypeDeclarationStatement)
			{
				sequenceStatement.AddStatement(se.Statement);
				if (bFirstDeclaration)
				{
					pouSyntax.Declaration = sequenceStatement;
				}
				else
				{
					this.Context.ErrorHandler.AddErrorST(se.Statement, 578, Array.Empty<object>());
				}
				bEndOfPOUFound = true;
				nIndex++;
				return true;
			}
			return false;
		}

		// Token: 0x0600058A RID: 1418 RVA: 0x000173D4 File Offset: 0x000155D4
		private static void FixPouSyntaxPosition(IPOUSyntax pouSyntax)
		{
			POUSyntaxParser.FixSequenceStatementPosition(pouSyntax.Declaration);
			POUSyntaxParser.FixSequenceStatementPosition(pouSyntax.Implementation);
			foreach (IPOUSyntax pouSyntax2 in pouSyntax.SubPOUs)
			{
				POUSyntaxParser.FixPouSyntaxPosition(pouSyntax2);
			}
		}

		// Token: 0x0600058B RID: 1419 RVA: 0x00017434 File Offset: 0x00015634
		private static void FixSequenceStatementPosition(_ISequenceStatement st)
		{
			if (st != null && st._StatementList.Count > 0)
			{
				st.PositionIntern = st._StatementList[0].PositionIntern;
			}
		}

		// Token: 0x0600058C RID: 1420 RVA: 0x00017460 File Offset: 0x00015660
		private static bool IsValidChildPouType(Operator eParentPouType, Operator eChildPouType)
		{
			HashSet<Operator> hashSet;
			return !POUSyntaxParser.s_htValidChildPouTypes.TryGetValue(eParentPouType, out hashSet) || hashSet.Contains(eChildPouType);
		}

		// Token: 0x0600058D RID: 1421 RVA: 0x00017488 File Offset: 0x00015688
		private void ReadSubPou(POUSyntax pouSyntax, _IPOUDeclarationStatement pouDeclarationStatement, _ISequenceStatement sequenceStatement, IList<SyntaxElement> syntaxElements, _IPOUDeclarationStatement parentPouDeclStatement, ref int nIndex)
		{
			Operator @class = pouDeclarationStatement.Class;
			if (!POUSyntaxParser.IsValidChildPouType(parentPouDeclStatement.Class, @class))
			{
				this.Context.ErrorHandler.AddErrorST(pouDeclarationStatement, 578, Array.Empty<object>());
				if (287 == @class)
				{
					this.Context.ErrorHandler.AddErrorST(parentPouDeclStatement, 9, new object[]
					{
						this.Context.Scanner.GetOperatorText(@class)
					});
				}
			}
			POUSyntax pousyntax = new POUSyntax
			{
				Declaration = this.Context.LMItemFactory.CreateSequenceStatement(),
				Implementation = this.Context.LMItemFactory.CreateSequenceStatement()
			};
			Operator expectedEndOfOperator = this.GetExpectedEndOfOperator(pouDeclarationStatement);
			POUSyntaxParser.MoveLastCommentsAndPragmas(sequenceStatement, pousyntax.Declaration);
			pousyntax.Declaration.AddStatement(pouDeclarationStatement);
			if (60 == @class || 290 == @class)
			{
				this.CheckDeclarationlessPou(pouDeclarationStatement);
			}
			if (290 == @class)
			{
				this.CheckTransition(pouDeclarationStatement);
			}
			short nMinus = POUSyntaxParser.MoveTrailingCommentsAndPragmasToImplementation(pouDeclarationStatement, pousyntax.Implementation);
			this.AdaptDeclarationPosition(pouDeclarationStatement, nMinus);
			while (nIndex < syntaxElements.Count)
			{
				SyntaxElement syntaxElement = syntaxElements[nIndex];
				StatementElement statementElement = syntaxElement as StatementElement;
				if (statementElement == null)
				{
					EndOfPOUElement endOfPOUElement = syntaxElement as EndOfPOUElement;
					if (endOfPOUElement != null)
					{
						this.CheckMatchingEndOfPOU(expectedEndOfOperator, endOfPOUElement, pousyntax);
						pouSyntax.AddSubpou(pousyntax);
						nIndex++;
						return;
					}
				}
				else
				{
					pousyntax.Implementation.AddStatement(statementElement.Statement);
				}
				nIndex++;
			}
		}

		// Token: 0x0600058E RID: 1422 RVA: 0x000175F4 File Offset: 0x000157F4
		private void CheckForUnexpectedAccessModifier(_IPOUDeclarationStatement pouDeclarationStatement, SignatureFlag eUnexpectedAccessModifierToCheck, Operator op)
		{
			if (eUnexpectedAccessModifierToCheck == pouDeclarationStatement.Access)
			{
				this.Context.ErrorHandler.AddErrorST(pouDeclarationStatement, 9, new object[]
				{
					this.Context.Scanner.GetOperatorText(op)
				});
			}
		}

		// Token: 0x0600058F RID: 1423 RVA: 0x0001762C File Offset: 0x0001582C
		private void CheckDeclarationlessPou(_IPOUDeclarationStatement pouDeclarationStatement)
		{
			_ISequenceStatement isequenceStatement = pouDeclarationStatement.Declarations as _ISequenceStatement;
			if (isequenceStatement != null)
			{
				if (Array.Exists<IStatement>(isequenceStatement.Statements, (IStatement stmt) => !(stmt is _ICommentStatement)))
				{
					this.Context.ErrorHandler.AddErrorST((_IExprement)isequenceStatement.Statements[0], 578, Array.Empty<object>());
				}
			}
			this.CheckForUnexpectedAccessModifier(pouDeclarationStatement, 4398046511104L, 229);
			if (pouDeclarationStatement.Extends.Any<_IExpression>())
			{
				this.Context.ErrorHandler.AddErrorST(pouDeclarationStatement, 9, new object[]
				{
					this.Context.Scanner.GetOperatorText(116)
				});
			}
		}

		// Token: 0x06000590 RID: 1424 RVA: 0x000176EC File Offset: 0x000158EC
		private void CheckTransition(_IPOUDeclarationStatement pouDeclarationStatement)
		{
			if (pouDeclarationStatement.Type != null)
			{
				this.Context.ErrorHandler.AddErrorST(pouDeclarationStatement, 182, Array.Empty<object>());
			}
			this.CheckForUnexpectedAccessModifier(pouDeclarationStatement, 1099511627776L, 227);
			this.CheckForUnexpectedAccessModifier(pouDeclarationStatement, 2199023255552L, 228);
			this.CheckForUnexpectedAccessModifier(pouDeclarationStatement, 8796093022208L, 230);
			this.CheckForUnexpectedAccessModifier(pouDeclarationStatement, 140737488355328L, 224);
		}

		// Token: 0x06000591 RID: 1425 RVA: 0x00017770 File Offset: 0x00015970
		private bool CheckMatchingEndOfPOU(Operator expectedEndOfPOU, EndOfPOUElement epe, POUSyntax pouSyntax)
		{
			if (expectedEndOfPOU != epe.EndOfPOUOperator)
			{
				_IStatement istatement = this.Context.LMItemFactory.CreateErrorStatement();
				istatement.PositionIntern = this.Context.LMItemFactory.CreateMinimalPosition(epe.Position, epe.PositionOffset);
				this.Context.ErrorHandler.AddErrorST(istatement, 578, Array.Empty<object>());
				_ISequenceStatement implementation = pouSyntax.Implementation;
				if (implementation != null)
				{
					implementation.AddStatement(istatement);
				}
			}
			return expectedEndOfPOU == epe.EndOfPOUOperator;
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x000177F0 File Offset: 0x000159F0
		private void CheckMatchingEndOfPOUAndTryCompletePOUSyntax(POUSyntax pouSyntax, Operator expectedEndOfPOU, EndOfPOUElement epe, _ISequenceStatement sequenceStatement)
		{
			if (this.CheckMatchingEndOfPOU(expectedEndOfPOU, epe, pouSyntax))
			{
				return;
			}
			Operator expectedStartOfOperator = POUSyntaxParser.GetExpectedStartOfOperator(epe.EndOfPOUOperator);
			if (expectedStartOfOperator == null)
			{
				this.TryCompletePOUSyntax(pouSyntax, sequenceStatement, 93);
				return;
			}
			this.TryCompletePOUSyntax(pouSyntax, sequenceStatement, expectedStartOfOperator);
		}

		// Token: 0x06000593 RID: 1427 RVA: 0x0001782E File Offset: 0x00015A2E
		private void AdaptDeclarationPosition(_IPOUDeclarationStatement pou, short nMinus)
		{
			if (pou == null)
			{
				return;
			}
			pou.LengthIntern -= nMinus;
		}

		// Token: 0x06000594 RID: 1428 RVA: 0x00017844 File Offset: 0x00015A44
		private static short MoveTrailingCommentsAndPragmasToImplementation(_IPOUDeclarationStatement declarationStatement, _ISequenceStatement sequenceStatement)
		{
			_ISequenceStatement isequenceStatement = declarationStatement.Declarations as _ISequenceStatement;
			if (isequenceStatement != null)
			{
				return POUSyntaxParser.MoveLastCommentsAndPragmas(isequenceStatement, sequenceStatement);
			}
			return 0;
		}

		// Token: 0x06000595 RID: 1429 RVA: 0x0001786C File Offset: 0x00015A6C
		private static short MoveLastCommentsAndPragmas(_ISequenceStatement seq, _ISequenceStatement sequenceStatement)
		{
			short num = 0;
			while (seq.Statements.Any<IStatement>())
			{
				_IStatement istatement = seq._StatementList.Last<_IStatement>();
				if (!(istatement is _ICommentStatement) && !(istatement is _IPragmaStatement) && !(istatement is _IDefineStatement) && !(istatement is _IEmptyStatement))
				{
					_IPragmaIfStatement ipragmaIfStatement = istatement as _IPragmaIfStatement;
					if (ipragmaIfStatement == null || POUSyntaxParser.ContainsDeclarationSyntax(ipragmaIfStatement))
					{
						break;
					}
				}
				seq._StatementList.Remove(istatement);
				sequenceStatement.InsertStatement(0, istatement);
				num += istatement.LengthIntern;
			}
			return num;
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x000178E8 File Offset: 0x00015AE8
		private static bool ContainsDeclarationSyntax(_IPragmaIfStatement ifStatement)
		{
			return DeclarationStatementDetector.CheckIfContainsDeclarationSyntax(ifStatement);
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x000178F0 File Offset: 0x00015AF0
		private Operator GetExpectedEndOfOperator(_IPOUDeclarationStatement pouDeclarationStatement)
		{
			if (pouDeclarationStatement == null)
			{
				return 0;
			}
			Operator @class = pouDeclarationStatement.Class;
			if (@class <= 88)
			{
				if (@class == 60)
				{
					return 70;
				}
				if (@class == 87)
				{
					return 73;
				}
				if (@class == 88)
				{
					return 74;
				}
			}
			else if (@class <= 118)
			{
				if (@class == 93)
				{
					return 76;
				}
				if (@class == 118)
				{
					return 281;
				}
			}
			else
			{
				if (@class == 119)
				{
					return 283;
				}
				switch (@class)
				{
				case 284:
				case 285:
					return 282;
				case 287:
					return 288;
				case 290:
					return 291;
				}
			}
			return 0;
		}

		// Token: 0x06000598 RID: 1432 RVA: 0x0001798C File Offset: 0x00015B8C
		private static Operator GetExpectedStartOfOperator(Operator eEndOfOperator)
		{
			if (eEndOfOperator <= 283)
			{
				switch (eEndOfOperator)
				{
				case 70:
					return 60;
				case 71:
				case 72:
				case 75:
					break;
				case 73:
					return 87;
				case 74:
					return 88;
				case 76:
					return 93;
				default:
					switch (eEndOfOperator)
					{
					case 281:
						return 118;
					case 282:
						return 285;
					case 283:
						return 119;
					}
					break;
				}
			}
			else
			{
				if (eEndOfOperator == 288)
				{
					return 287;
				}
				if (eEndOfOperator == 291)
				{
					return 290;
				}
			}
			return 0;
		}

		// Token: 0x06000599 RID: 1433 RVA: 0x00017A18 File Offset: 0x00015C18
		private void TryCompletePOUSyntax(POUSyntax pouSyntax, _ISequenceStatement sequenceStatement, Operator pouClassToUse = 93)
		{
			if (pouSyntax.Declaration != null)
			{
				return;
			}
			if (sequenceStatement.Statements.Length == 0)
			{
				return;
			}
			_IExpressionStatement[] array = sequenceStatement.Statements.OfType<_IExpressionStatement>().ToArray<_IExpressionStatement>();
			_IPOUDeclarationStatement ipoudeclarationStatement = this.Context.LMItemFactory.CreatePOUDeclarationStatement();
			if (1 < array.Length)
			{
				ipoudeclarationStatement.NameExpression = array[1]._Expr;
			}
			ipoudeclarationStatement.Declarations = sequenceStatement;
			ipoudeclarationStatement.Class = pouClassToUse;
			_ISequenceStatement isequenceStatement = this.Context.LMItemFactory.CreateSequenceStatement();
			isequenceStatement.Add(ipoudeclarationStatement);
			pouSyntax.Declaration = isequenceStatement;
			if (array.Length != 0)
			{
				string text = array[0]._Expr.ToString();
				this.Context.ErrorHandler.AddErrorST(array[0], 9, new object[]
				{
					text
				});
				return;
			}
			IStatement statement = Array.Find<IStatement>(sequenceStatement.Statements, (IStatement stmt) => !(stmt is IPragmaStatement) && !(stmt is ICommentStatement));
			if (statement != null)
			{
				this.Context.ErrorHandler.AddErrorST((_IExprement)statement, 578, new object[]
				{
					statement.ToString()
				});
			}
		}

		// Token: 0x040000CF RID: 207
		private static readonly Dictionary<Operator, HashSet<Operator>> s_htValidChildPouTypes = new Dictionary<Operator, HashSet<Operator>>
		{
			{
				287,
				new HashSet<Operator>
				{
					285,
					284
				}
			},
			{
				93,
				new HashSet<Operator>
				{
					60,
					118,
					285,
					284,
					290
				}
			},
			{
				88,
				new HashSet<Operator>
				{
					60,
					118,
					285,
					284,
					290
				}
			},
			{
				87,
				new HashSet<Operator>()
			},
			{
				60,
				new HashSet<Operator>()
			},
			{
				290,
				new HashSet<Operator>()
			},
			{
				119,
				new HashSet<Operator>
				{
					118,
					285,
					284
				}
			}
		};
	}
}
