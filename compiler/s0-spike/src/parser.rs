//! Scoped recursive-descent + Pratt parser for the S0 spike.
//!
//! The parser is deliberately split from the tree: it records an event stream
//! (`Start`/`Token`/`Finish`) which [`crate::cst`] replays into a rowan green
//! node. That is the same event/sink shape as rust-analyzer and truST, and it
//! is what makes left-associative Pratt expressions expressible in a
//! single-pass builder: a binary node that starts before already-emitted
//! operands is hoisted with a forward-parent link.
//!
//! It parses exactly one slice: a POU declaration (`PROGRAM` /
//! `FUNCTION_BLOCK` / `FUNCTION`) with variable blocks, plus assignment, call,
//! `IF`, and empty statements over Pratt expressions. Errors produce
//! `ErrorNode`s and diagnostics; the parser always makes progress and always
//! covers every input token.

use crate::lexer::{lex, LexDiagnostic, Token, TokenKind};
use crate::syntax::SyntaxKind;
use rowan::{TextRange, TextSize};

/// Maximum recursion depth before recovery kicks in (expression/statement
/// nesting). Bounds stack use on pathological inputs.
pub const MAX_DEPTH: usize = 96;

/// A syntax problem with the byte range it was detected at.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct ParseError {
    pub message: String,
    pub range: TextRange,
}

/// Result of [`parse`]: green tree plus diagnostics, in source order.
#[derive(Debug)]
pub struct Parse {
    pub green: rowan::GreenNode,
    pub errors: Vec<ParseError>,
}

/// Parses `source` completely. Every token — trivia included — appears in the
/// event stream exactly once, in source order.
pub fn parse(source: &str) -> Parse {
    let (tokens, lex_diagnostics) = lex(source);
    let significant: Vec<u32> = tokens
        .iter()
        .enumerate()
        .filter(|(_, token)| !token.kind.is_trivia())
        .map(|(index, _)| u32::try_from(index).unwrap_or(u32::MAX))
        .collect();
    let source_len = u32::try_from(source.len()).unwrap_or(u32::MAX);

    let mut parser = Parser {
        tokens: &tokens,
        significant,
        pos: 0,
        events: Vec::new(),
        errors: lex_diagnostics
            .into_iter()
            .map(ParseError::from_lex)
            .collect(),
        depth: 0,
        source_len,
    };
    parser.source_file();

    let green = crate::cst::Sink::build(&tokens, parser.events);
    Parse {
        green,
        errors: parser.errors,
    }
}

impl ParseError {
    fn from_lex(diagnostic: LexDiagnostic) -> Self {
        ParseError {
            message: diagnostic.message.to_string(),
            range: diagnostic.range,
        }
    }
}

/// Node events replayed by the sink.
#[derive(Debug, Clone, PartialEq, Eq)]
pub(crate) enum Event {
    /// Start a node. `forward_parent`, when set, names a later `Start` whose
    /// node must wrap this one (left-recursion hoisting for Pratt parsing).
    Start {
        kind: SyntaxKind,
        forward_parent: Option<u32>,
    },
    /// Emit one raw token (index into the token vector).
    Token { index: u32 },
    /// Finish the current node.
    Finish,
    /// Slot for a marker that has not been completed yet.
    Placeholder,
}

/// Points at an `Event::Start` slot that [`Marker::complete`] will fill in.
struct Marker {
    pos: usize,
}

impl Marker {
    fn complete(self, parser: &mut Parser<'_, '_>, kind: SyntaxKind) -> CompletedMarker {
        match parser.events.get_mut(self.pos) {
            Some(slot @ Event::Placeholder) => {
                *slot = Event::Start {
                    kind,
                    forward_parent: None,
                };
            }
            Some(Event::Start {
                kind: existing_kind,
                ..
            }) => *existing_kind = kind,
            _ => {}
        }
        parser.events.push(Event::Finish);
        CompletedMarker { pos: self.pos }
    }
}

/// A completed node that can be wrapped by a later (left-recursive) start.
#[derive(Clone, Copy)]
struct CompletedMarker {
    pos: usize,
}

impl CompletedMarker {
    /// Starts a node that wraps everything from this node onwards.
    fn precede(self, parser: &mut Parser<'_, '_>) -> Marker {
        let new_pos = parser.events.len();
        parser.events.push(Event::Placeholder);
        set_forward_parent(&mut parser.events, self.pos, new_pos);
        Marker { pos: new_pos }
    }
}

fn set_forward_parent(events: &mut [Event], from: usize, to: usize) {
    let mut current = from;
    loop {
        match events.get_mut(current) {
            Some(Event::Start {
                forward_parent: Some(next),
                ..
            }) => current += *next as usize,
            Some(Event::Start { forward_parent, .. }) => {
                *forward_parent = Some(u32::try_from(to - current).unwrap_or(0));
                break;
            }
            _ => break,
        }
    }
}

/// Words that end a statement list; recovery stops there.
const STATEMENT_END_KEYWORDS: &[&str] = &[
    "END_IF",
    "ELSE",
    "ELSIF",
    "END_PROGRAM",
    "END_FUNCTION_BLOCK",
    "END_FUNCTION",
    "END_VAR",
    "END_CASE",
    "END_WHILE",
    "END_FOR",
    "END_REPEAT",
    "UNTIL",
    "THEN",
    "DO",
];

/// Keywords that open a variable block.
const VAR_KEYWORDS: &[&str] = &[
    "VAR",
    "VAR_INPUT",
    "VAR_OUTPUT",
    "VAR_IN_OUT",
    "VAR_GLOBAL",
    "VAR_TEMP",
];

struct Parser<'t, 'src> {
    tokens: &'t [Token<'src>],
    significant: Vec<u32>,
    pos: usize,
    events: Vec<Event>,
    errors: Vec<ParseError>,
    depth: usize,
    source_len: u32,
}

impl<'t, 'src> Parser<'t, 'src> {
    // ----- token access -------------------------------------------------

    fn current(&self) -> Option<&Token<'src>> {
        self.significant
            .get(self.pos)
            .and_then(|index| self.tokens.get(*index as usize))
    }

    fn current_kind(&self) -> Option<TokenKind> {
        self.current().map(|token| token.kind)
    }

    fn current_text(&self) -> Option<&'src str> {
        self.current().map(|token| token.text)
    }

    fn eof(&self) -> bool {
        self.current().is_none()
    }

    fn at_kind(&self, kind: TokenKind) -> bool {
        self.current_kind() == Some(kind)
    }

    fn at_kw(&self, keyword: &str) -> bool {
        self.current_kind() == Some(TokenKind::Ident)
            && self
                .current_text()
                .is_some_and(|text| text.eq_ignore_ascii_case(keyword))
    }

    fn at_any_kw(&self, keywords: &[&str]) -> bool {
        keywords.iter().any(|keyword| self.at_kw(keyword))
    }

    fn at_op(&self, operator: &str) -> bool {
        self.current_kind() == Some(TokenKind::Operator) && self.current_text() == Some(operator)
    }

    fn current_range(&self) -> TextRange {
        self.current().map_or_else(
            || TextRange::empty(TextSize::from(self.source_len)),
            |token| token.range,
        )
    }

    // ----- event emission -----------------------------------------------

    fn bump(&mut self) {
        if let Some(index) = self.significant.get(self.pos) {
            self.events.push(Event::Token { index: *index });
            self.pos += 1;
        }
    }

    fn start(&mut self) -> Marker {
        let pos = self.events.len();
        self.events.push(Event::Placeholder);
        Marker { pos }
    }

    fn start_node(&mut self, kind: SyntaxKind) {
        self.events.push(Event::Start {
            kind,
            forward_parent: None,
        });
    }

    fn finish_node(&mut self) {
        self.events.push(Event::Finish);
    }

    fn eat_kind(&mut self, kind: TokenKind) -> bool {
        if self.at_kind(kind) {
            self.bump();
            true
        } else {
            false
        }
    }

    fn eat_kw(&mut self, keyword: &str) -> bool {
        if self.at_kw(keyword) {
            self.bump();
            true
        } else {
            false
        }
    }

    fn eat_op(&mut self, operator: &str) -> bool {
        if self.at_op(operator) {
            self.bump();
            true
        } else {
            false
        }
    }

    fn expect_ident(&mut self) {
        if !self.eat_kind(TokenKind::Ident) {
            self.zero_width_error("expected identifier");
        }
    }

    fn expect_kw(&mut self, keyword: &str) {
        if !self.eat_kw(keyword) {
            self.zero_width_error(&format!("expected `{keyword}`"));
        }
    }

    fn expect_op(&mut self, operator: &str) {
        if !self.eat_op(operator) {
            self.zero_width_error(&format!("expected `{operator}`"));
        }
    }

    // ----- diagnostics and recovery -------------------------------------

    fn error(&mut self, message: &str, range: TextRange) {
        self.errors.push(ParseError {
            message: message.to_string(),
            range,
        });
    }

    /// Records an error without consuming input (for missing tokens).
    fn zero_width_error(&mut self, message: &str) {
        let range = self.current_range();
        self.error(message, range);
        self.start_node(SyntaxKind::ErrorNode);
        self.finish_node();
    }

    /// Records an error, then consumes at least one token (unless at EOF or a
    /// stop keyword) into an `ErrorNode` so parsing can resume.
    fn error_and_recover(&mut self, message: &str, stop_keywords: &[&str]) {
        let range = self.current_range();
        self.error(message, range);
        self.start_node(SyntaxKind::ErrorNode);
        if !self.eof() && !self.at_any_kw(stop_keywords) {
            self.bump();
            while !self.eof()
                && !self.at_any_kw(stop_keywords)
                && !self.at_op(";")
                && !self.at_any_kw(STATEMENT_END_KEYWORDS)
            {
                self.bump();
            }
        }
        self.finish_node();
    }

    /// Consumes tokens until an expression boundary; used by expression
    /// recovery so the enclosing construct can continue.
    fn consume_expression_sync(&mut self) {
        while !self.eof()
            && !self.at_op(";")
            && !self.at_op(",")
            && !self.at_op(")")
            && !self.at_op("]")
            && !self.at_any_kw(STATEMENT_END_KEYWORDS)
        {
            self.bump();
        }
    }

    // ----- grammar ------------------------------------------------------

    fn source_file(&mut self) {
        self.start_node(SyntaxKind::SourceFile);
        while !self.eof() {
            if self.at_kw("PROGRAM") || self.at_kw("FUNCTION_BLOCK") || self.at_kw("FUNCTION") {
                self.pou_decl();
            } else {
                self.error_and_recover(
                    "expected a POU declaration (PROGRAM, FUNCTION_BLOCK, FUNCTION)",
                    &["PROGRAM", "FUNCTION_BLOCK", "FUNCTION"],
                );
            }
        }
        self.finish_node();
    }

    fn pou_decl(&mut self) {
        self.start_node(SyntaxKind::ProgramDecl);
        let end_keyword = if self.at_kw("FUNCTION_BLOCK") {
            "END_FUNCTION_BLOCK"
        } else if self.at_kw("FUNCTION") {
            "END_FUNCTION"
        } else {
            "END_PROGRAM"
        };
        self.bump(); // POU keyword
        self.expect_ident();
        if self.eat_op(":") {
            // FUNCTION return type.
            self.type_ref();
        }
        while self.at_any_kw(VAR_KEYWORDS) {
            self.var_block();
        }
        self.statement_list();
        self.expect_kw(end_keyword);
        self.finish_node();
    }

    fn var_block(&mut self) {
        self.start_node(SyntaxKind::VarBlock);
        self.bump(); // VAR keyword
        while !self.at_kw("END_VAR") && !self.eof() {
            // Constant/retain/persistent qualifiers apply to the block.
            if self.at_any_kw(&["CONSTANT", "RETAIN", "NON_RETAIN", "PERSISTENT"]) {
                self.bump();
                continue;
            }
            let before = self.pos;
            if self.at_kind(TokenKind::Ident) {
                self.var_decl();
            }
            if self.pos == before {
                if self.at_kw("END_VAR") || self.eof() {
                    break;
                }
                self.error_and_recover("expected variable declaration", &["END_VAR"]);
            }
        }
        self.expect_kw("END_VAR");
        self.finish_node();
    }

    fn var_decl(&mut self) {
        self.start_node(SyntaxKind::VarDecl);
        self.expect_ident();
        if self.eat_kw("AT") {
            self.direct_var();
        }
        self.expect_op(":");
        self.type_ref();
        if self.eat_op(":=") {
            self.expr_bp(0);
        }
        self.expect_op(";");
        self.finish_node();
    }

    fn direct_var(&mut self) {
        self.start_node(SyntaxKind::DirectVar);
        self.eat_op("%");
        if self.at_kind(TokenKind::Ident) || self.at_kind(TokenKind::IntegerLit) {
            self.bump();
        } else {
            self.zero_width_error("expected direct-variable location");
        }
        while self.at_op(".") {
            self.bump();
            if self.at_kind(TokenKind::Ident) || self.at_kind(TokenKind::IntegerLit) {
                self.bump();
            } else {
                self.zero_width_error("expected direct-variable address part");
                break;
            }
        }
        self.eat_op("*");
        self.finish_node();
    }

    fn type_ref(&mut self) {
        self.start_node(SyntaxKind::TypeRef);
        if self.at_kw("ARRAY") {
            self.bump();
            self.expect_op("[");
            self.expr_bp(0);
            self.expect_op("..");
            self.expr_bp(0);
            self.expect_op("]");
            self.expect_kw("OF");
            self.type_ref();
        } else {
            self.expect_ident();
            while self.at_op(".") {
                self.bump();
                self.expect_ident();
            }
            if self.eat_op("(") {
                self.expr_bp(0);
                self.expect_op(")");
            }
            if self.eat_op("[") {
                self.expr_bp(0);
                self.expect_op("]");
            }
        }
        self.finish_node();
    }

    fn statement_list(&mut self) {
        self.start_node(SyntaxKind::StatementList);
        loop {
            if self.eof() || self.at_any_kw(STATEMENT_END_KEYWORDS) {
                break;
            }
            let before = self.pos;
            self.statement();
            if self.pos == before {
                self.error_and_recover(
                    "unexpected token in statement list",
                    STATEMENT_END_KEYWORDS,
                );
            }
        }
        self.finish_node();
    }

    fn statement(&mut self) {
        if self.at_kw("IF") {
            self.if_stmt();
        } else if self.at_op(";") {
            let marker = self.start();
            self.bump();
            marker.complete(self, SyntaxKind::EmptyStmt);
        } else if self.at_kind(TokenKind::Ident) {
            self.assignment_or_call();
        } else {
            self.error_and_recover("expected a statement", STATEMENT_END_KEYWORDS);
        }
    }

    fn assignment_or_call(&mut self) {
        let marker = self.start();
        self.name_path();
        if self.at_op(":=") {
            self.bump();
            self.expr_bp(0);
            self.expect_op(";");
            marker.complete(self, SyntaxKind::AssignStmt);
        } else if self.at_op("(") {
            self.bump();
            self.arg_list();
            self.expect_op(")");
            self.expect_op(";");
            marker.complete(self, SyntaxKind::CallStmt);
        } else {
            self.error(
                "expected `:=` or `(` after statement target",
                self.current_range(),
            );
            self.consume_expression_sync();
            marker.complete(self, SyntaxKind::ErrorNode);
        }
    }

    fn name_path(&mut self) {
        self.expect_ident();
        while self.at_op(".") {
            self.bump();
            self.expect_ident();
        }
        while self.at_op("[") {
            self.bump();
            self.expr_bp(0);
            self.expect_op("]");
        }
        while self.eat_op("^") {}
    }

    fn if_stmt(&mut self) {
        self.start_node(SyntaxKind::IfStmt);
        self.expect_kw("IF");
        self.expr_bp(0);
        self.expect_kw("THEN");
        self.statement_list();
        while self.at_kw("ELSIF") {
            self.bump();
            self.expr_bp(0);
            self.expect_kw("THEN");
            self.statement_list();
        }
        if self.eat_kw("ELSE") {
            self.statement_list();
        }
        self.expect_kw("END_IF");
        // CODESYS-compatible dialects allow `END_IF` without `;`.
        self.eat_op(";");
        self.finish_node();
    }

    fn arg_list(&mut self) {
        self.start_node(SyntaxKind::ArgList);
        if !self.at_op(")") && !self.eof() {
            self.expr_bp(0);
            while self.eat_op(",") {
                self.expr_bp(0);
            }
        }
        self.finish_node();
    }

    // ----- expressions (Pratt) ------------------------------------------

    /// Binding powers: (left, right) per binary level.
    fn peek_binop(&self) -> Option<(u8, u8)> {
        match self.current_kind()? {
            TokenKind::Ident => {
                let text = self.current_text()?;
                if text.eq_ignore_ascii_case("OR") {
                    Some((1, 2))
                } else if text.eq_ignore_ascii_case("XOR") {
                    Some((3, 4))
                } else if text.eq_ignore_ascii_case("AND") {
                    Some((5, 6))
                } else if text.eq_ignore_ascii_case("MOD") {
                    Some((11, 12))
                } else {
                    None
                }
            }
            TokenKind::Operator => {
                let text = self.current_text()?;
                match text {
                    "=" | "<>" | "<" | ">" | "<=" | ">=" => Some((7, 8)),
                    "+" | "-" => Some((9, 10)),
                    "*" | "/" => Some((11, 12)),
                    _ => None,
                }
            }
            _ => None,
        }
    }

    fn expr_bp(&mut self, min_bp: u8) -> CompletedMarker {
        let marker = self.start();
        if self.depth >= MAX_DEPTH {
            let range = self.current_range();
            self.error("expression nesting limit exceeded", range);
            self.consume_expression_sync();
            return marker.complete(self, SyntaxKind::ErrorNode);
        }
        self.depth += 1;
        let mut lhs = self.primary(marker);
        while let Some((left_bp, right_bp)) = self.peek_binop() {
            if left_bp < min_bp {
                break;
            }
            let wrapping = lhs.precede(self);
            self.bump(); // operator
            self.expr_bp(right_bp);
            lhs = wrapping.complete(self, SyntaxKind::BinaryExpr);
        }
        self.depth -= 1;
        lhs
    }

    fn primary(&mut self, marker: Marker) -> CompletedMarker {
        if self.at_kw("NOT") || self.at_op("-") || self.at_op("+") {
            self.bump();
            self.expr_bp(13);
            return marker.complete(self, SyntaxKind::UnaryExpr);
        }
        if self.at_kind(TokenKind::IntegerLit)
            || self.at_kind(TokenKind::RealLit)
            || self.at_kind(TokenKind::StringLit)
            || self.at_kw("TRUE")
            || self.at_kw("FALSE")
        {
            self.bump();
            return marker.complete(self, SyntaxKind::LiteralExpr);
        }
        if self.at_op("(") {
            self.bump();
            self.expr_bp(0);
            self.expect_op(")");
            return marker.complete(self, SyntaxKind::ParenExpr);
        }
        if self.at_kind(TokenKind::Ident) {
            self.bump();
            while self.at_op(".") {
                self.bump();
                self.expect_ident();
            }
            if self.at_op("(") {
                self.bump();
                self.arg_list();
                self.expect_op(")");
                return marker.complete(self, SyntaxKind::CallExpr);
            }
            while self.at_op("[") {
                self.bump();
                self.expr_bp(0);
                self.expect_op("]");
            }
            while self.eat_op("^") {}
            return marker.complete(self, SyntaxKind::NameRef);
        }
        let range = self.current_range();
        self.error("expected expression", range);
        self.consume_expression_sync();
        marker.complete(self, SyntaxKind::ErrorNode)
    }
}
