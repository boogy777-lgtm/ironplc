using System.Collections;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;

namespace CODESYS.Parser35210.PragmaScanner
{
	internal class PragmaScanner : IPragmaScanner
	{
		private static readonly Hashtable s_htOperators_lockRequired;

		private readonly IScanner9 m_scanner;

		public int PositionOffset { get; private set; }

		public IScanner9 OrgScanner => m_scanner;

		internal PragmaScanner(IScanner9 scanner)
		{
			m_scanner = scanner;
		}

		internal PragmaScanner(IScanner9 scanner, IToken token)
			: this(scanner)
		{
			PositionOffset = token.PositionOffset;
		}

		public void Reset(string stPragma, IToken tokenPragma)
		{
			PositionOffset = tokenPragma.PositionOffset;
			m_scanner.Initialize(stPragma);
			m_scanner.AllowMultipleUnderlines = true;
		}

		static PragmaScanner()
		{
			s_htOperators_lockRequired = new Hashtable();
			s_htOperators_lockRequired["flow"] = PragmaOperator.flow;
			s_htOperators_lockRequired["noflow"] = PragmaOperator.noflow;
			s_htOperators_lockRequired["bp"] = PragmaOperator.bp;
			s_htOperators_lockRequired["nobp"] = PragmaOperator.nobp;
			s_htOperators_lockRequired["bp2"] = PragmaOperator.bp2;
			s_htOperators_lockRequired["nobp2"] = PragmaOperator.nobp2;
			s_htOperators_lockRequired["p"] = PragmaOperator.pos;
			s_htOperators_lockRequired["bpdef"] = PragmaOperator.bpdef;
			s_htOperators_lockRequired["succ"] = PragmaOperator.succ;
			s_htOperators_lockRequired["error"] = PragmaOperator.error;
			s_htOperators_lockRequired["fatalerror"] = PragmaOperator.fatalerror;
			s_htOperators_lockRequired["warning"] = PragmaOperator.warning;
			s_htOperators_lockRequired["info"] = PragmaOperator.info;
			s_htOperators_lockRequired["text"] = PragmaOperator.text;
			s_htOperators_lockRequired["attribute"] = PragmaOperator.attribute;
			s_htOperators_lockRequired["define"] = PragmaOperator.define;
			s_htOperators_lockRequired["undefine"] = PragmaOperator.undefine;
			s_htOperators_lockRequired["include"] = PragmaOperator.include;
			s_htOperators_lockRequired["defined"] = PragmaOperator.defined;
			s_htOperators_lockRequired["project_defined"] = PragmaOperator.project_defined;
			s_htOperators_lockRequired["variable"] = PragmaOperator.variable;
			s_htOperators_lockRequired["type"] = PragmaOperator.type;
			s_htOperators_lockRequired["task"] = PragmaOperator.task;
			s_htOperators_lockRequired["xref"] = PragmaOperator.xref;
			s_htOperators_lockRequired["resource"] = PragmaOperator.resource;
			s_htOperators_lockRequired["implicit"] = PragmaOperator.opimplicit;
			s_htOperators_lockRequired["allowpaths"] = PragmaOperator.allowpaths;
			s_htOperators_lockRequired["messageguid"] = PragmaOperator.messageguid;
			s_htOperators_lockRequired["on"] = PragmaOperator.on;
			s_htOperators_lockRequired["off"] = PragmaOperator.off;
			s_htOperators_lockRequired["from"] = PragmaOperator.from;
			s_htOperators_lockRequired["pou"] = PragmaOperator.pou;
			s_htOperators_lockRequired["hastype"] = PragmaOperator.hastype;
			s_htOperators_lockRequired["hasattribute"] = PragmaOperator.hasattribute;
			s_htOperators_lockRequired["assert"] = PragmaOperator.assert;
			s_htOperators_lockRequired["hasvalue"] = PragmaOperator.hasvalue;
			s_htOperators_lockRequired["hasconstantvalue"] = PragmaOperator.hasconstantvalue;
			s_htOperators_lockRequired["hasconstanttype"] = PragmaOperator.hasconstanttype;
			s_htOperators_lockRequired["OR"] = PragmaOperator.Or;
			s_htOperators_lockRequired["AND"] = PragmaOperator.And;
			s_htOperators_lockRequired["NOT"] = PragmaOperator.Not;
			s_htOperators_lockRequired["show_compile"] = PragmaOperator.show_compile;
			s_htOperators_lockRequired["show_precompile"] = PragmaOperator.show_precompile;
			s_htOperators_lockRequired["isenumtype"] = PragmaOperator.isenumtype;
			s_htOperators_lockRequired["disable"] = PragmaOperator.disable;
			s_htOperators_lockRequired["restore"] = PragmaOperator.restore;
			s_htOperators_lockRequired["__INTERNAL_POINTEROP"] = PragmaOperator.InternalPointerOp;
			s_htOperators_lockRequired["COMPILERVERSION"] = PragmaOperator.CompilerVersionPragma;
			s_htOperators_lockRequired["RUNTIMEVERSION"] = PragmaOperator.RuntimeVersionPragma;
		}

		public PragmaTokenType GetNext(out IPragmaToken token)
		{
			m_scanner.GetNext(out var token2);
			PragmaToken pragmaToken = (PragmaToken)(token = new PragmaToken(token2)
			{
				_type = PragmaTokenType.Error
			});
			switch (token2.Type)
			{
			case TokenType.Operator:
				switch (m_scanner.GetOperator(token2))
				{
				case Operator.If:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.If;
					break;
				case Operator.Else:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.Else;
					break;
				case Operator.Elsif:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.Elsif;
					break;
				case Operator.EndIf:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.EndIf;
					break;
				case Operator.LeftParenthesis:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.LeftParenthesis;
					break;
				case Operator.RightParenthesis:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.RightParenthesis;
					break;
				case Operator.Colon:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.Colon;
					break;
				case Operator.And:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.And;
					break;
				case Operator.Or:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.Or;
					break;
				case Operator.Not:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.Not;
					break;
				case Operator.Period:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.Period;
					break;
				case Operator.Comma:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.Comma;
					break;
				case Operator.Assign:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.assign;
					break;
				case Operator.From:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.from;
					break;
				case Operator.Type:
					pragmaToken._type = PragmaTokenType.Operator;
					pragmaToken._pop = PragmaOperator.type;
					break;
				}
				break;
			case TokenType.SingleByteString:
				pragmaToken._type = PragmaTokenType.SingleByteString;
				pragmaToken._stString = m_scanner.GetSingleByteString(token2);
				break;
			case TokenType.Integer:
			{
				pragmaToken._type = PragmaTokenType.Integer;
				m_scanner.GetInteger(token2, out var nValue, out var _, out var _, out var _);
				pragmaToken._lInteger = (long)nValue;
				break;
			}
			case TokenType.Boolean:
				pragmaToken._type = PragmaTokenType.Operator;
				if (m_scanner.GetBoolean(token2))
				{
					pragmaToken._pop = PragmaOperator.True;
				}
				else
				{
					pragmaToken._pop = PragmaOperator.False;
				}
				break;
			case TokenType.End:
				pragmaToken._type = PragmaTokenType.End;
				break;
			default:
			{
				string tokenText = m_scanner.GetTokenText(token2);
				lock (s_htOperators_lockRequired)
				{
					if (s_htOperators_lockRequired[tokenText] != null)
					{
						PragmaOperator pop = (PragmaOperator)s_htOperators_lockRequired[tokenText];
						pragmaToken._type = PragmaTokenType.Operator;
						pragmaToken._pop = pop;
					}
					else
					{
						pragmaToken._type = PragmaTokenType.Identifier;
						pragmaToken._stIdent = m_scanner.GetTokenText(token2);
					}
				}
				break;
			}
			}
			return pragmaToken._type;
		}

		public void SetPosition(IPragmaToken pt)
		{
			m_scanner.SetPosition(pt.OrgToken);
		}

		public string GetTokenText(IPragmaToken pt)
		{
			return m_scanner.GetTokenText(pt.OrgToken);
		}
	}
}
