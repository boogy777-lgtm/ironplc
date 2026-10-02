//! Dialect gating as a parser input.
//!
//! [`ParseOptions`] is the set of dialect flags the parser consults. Each flag
//! has the name and meaning of the flag of the same name in the legacy
//! `CompilerOptions` (`ironplc-parser`); this crate does not depend on that
//! crate, so the flags are mirrored here and the tests convert between the two
//! by flag name ([`ParseOptions::FLAG_KEYS`], [`ParseOptions::set_flag_by_key`]).
//!
//! The legacy pipeline applies the flags by rewriting tokens before parsing
//! (design: parse-tree architecture, section 3.1). The parser here applies
//! them as decisions over the untouched tokens instead: [`keyword_enabled`]
//! is the single table that says which keyword is a keyword under which flag,
//! and the parser treats a disabled keyword as an ordinary name.
//!
//! [`keyword_enabled`]: ParseOptions::keyword_enabled

use crate::syntax_kind::SyntaxKind;

/// Declares [`ParseOptions`] and the by-name accessors from one flag list, so
/// a flag cannot exist without its key.
macro_rules! parse_options {
    ($($(#[$doc:meta])* $flag:ident),* $(,)?) => {
        /// The dialect flags the parser reads. All off (the default) is the
        /// strict IEC 61131-3 behavior.
        #[derive(Debug, Default, Clone, Copy, PartialEq, Eq)]
        pub struct ParseOptions {
            $($(#[$doc])* pub $flag: bool,)*
        }

        impl ParseOptions {
            /// The name of every flag, equal to the field name.
            pub const FLAG_KEYS: &'static [&'static str] = &[$(stringify!($flag)),*];

            /// Every flag on: all dialect extensions enabled.
            pub fn all() -> Self {
                ParseOptions { $($flag: true,)* }
            }

            /// Sets the flag called `key`; false when no flag has that name.
            pub fn set_flag_by_key(&mut self, key: &str, value: bool) -> bool {
                $(
                    if key == stringify!($flag) {
                        self.$flag = value;
                        return true;
                    }
                )*
                false
            }

            /// The value of the flag called `key`, if there is one.
            pub fn flag_by_key(&self, key: &str) -> Option<bool> {
                $(
                    if key == stringify!($flag) {
                        return Some(self.$flag);
                    }
                )*
                None
            }
        }
    };
}

parse_options! {
    /// `//` and `/* */` comments.
    allow_c_style_comments,
    /// Statement terminator optional after `END_IF` and the other `END_*` keywords.
    allow_missing_semicolon,
    /// `TIME` usable as a function name.
    allow_time_as_function_name,
    /// `LTIME`, `LDATE`, `LTOD` and `LDT` are keywords.
    allow_long_time_types,
    /// `REF_TO`, `REF` and `NULL` are keywords.
    allow_ref_to,
    /// `REFERENCE` is a keyword.
    allow_reference_to,
    /// `POINTER` is a keyword.
    allow_pointer_to,
    /// `PERSISTENT` is a keyword.
    allow_persistent_var,
    /// Partial-access selectors such as `.%X3`.
    allow_partial_access_syntax,
    /// Curly-brace pragmas.
    allow_pragmas,
    /// `{IF}`, `{ELSIF}`, `{ELSE}` and `{END_IF}` pragmas: the branches not
    /// taken are not parsed.
    allow_pragma_if,
    /// `AND_THEN` and `OR_ELSE` are keywords.
    allow_short_circuit_operators,
    /// `THIS`, `SUPER` and the object-oriented keywords.
    allow_fb_inheritance,
    /// `CONTINUE` is a keyword.
    allow_continue,
    /// `__TRY`, `__CATCH`, `__FINALLY`, `__ENDTRY` and `__THROW`.
    allow_try_catch,
    /// `JMP` and statement labels.
    allow_jump_statement,
    /// `CALC`.
    allow_calc_statement,
    /// `__WAIT`.
    allow_wait_statement,
    /// Nested `(* *)` comments.
    allow_nested_comments,
    /// `UNION` and `END_UNION` are keywords.
    allow_union_type,
    /// `VAR_STAT` is a keyword.
    allow_var_stat,
    /// `VAR_INST` is a keyword.
    allow_var_inst,
    /// `VAR_GENERIC` is a keyword.
    allow_var_generic,
    /// `NAMESPACE` and `END_NAMESPACE` are keywords.
    allow_namespace,
    /// `__BEGIN_IMPLEMENTATION` is a keyword.
    allow_begin_implementation,
    /// `BIT` is a keyword.
    allow_bit_type,
    /// `PARAMS` is a keyword.
    allow_params_of,
    /// Backtick-escaped identifiers.
    allow_escaped_identifiers,
    /// Identifiers with letters outside ASCII.
    allow_unicode_identifiers,
    /// Consecutive underscores inside an identifier.
    allow_multiple_underscores,
    /// `VAR END_VAR` with no declaration.
    allow_empty_var_blocks,
    /// `STRING(n)` and `WSTRING(n)`, a length in parentheses.
    allow_paren_string_length,
    /// `ARRAY[*] OF T`, the array whose bounds the caller supplies.
    allow_incomplete_array,
}

impl ParseOptions {
    /// True when `kind` is a keyword under these options, false when the
    /// dialect leaves the word available as an ordinary name.
    ///
    /// This is the one table that decides which keyword is gated by which
    /// flag. Kinds that are not keywords, and keywords no flag gates, are
    /// always enabled.
    pub fn keyword_enabled(&self, kind: SyntaxKind) -> bool {
        use SyntaxKind as K;
        match kind {
            K::Ltime | K::Ldate | K::Ltod | K::Ldt => self.allow_long_time_types,
            K::RefTo | K::Ref | K::Null => self.allow_ref_to,
            K::Reference => self.allow_reference_to,
            K::Pointer => self.allow_pointer_to,
            K::Extends
            | K::Implements
            | K::Interface
            | K::EndInterface
            | K::Abstract
            | K::Method
            | K::EndMethod
            | K::Property
            | K::EndProperty
            | K::EndGet
            | K::EndSet
            | K::This
            | K::Super => self.allow_fb_inheritance,
            K::AndThen | K::OrElse => self.allow_short_circuit_operators,
            K::Persistent => self.allow_persistent_var,
            K::Continue => self.allow_continue,
            K::Union | K::EndUnion => self.allow_union_type,
            K::VarStat => self.allow_var_stat,
            K::VarInst => self.allow_var_inst,
            K::VarGeneric => self.allow_var_generic,
            K::Namespace | K::EndNamespace => self.allow_namespace,
            K::BeginImplementation => self.allow_begin_implementation,
            K::Bit => self.allow_bit_type,
            K::Try | K::EndTry | K::Catch | K::Finally | K::Throw => self.allow_try_catch,
            K::Jmp => self.allow_jump_statement,
            K::Calc => self.allow_calc_statement,
            K::Wait => self.allow_wait_statement,
            K::Params => self.allow_params_of,
            _ => true,
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn default_when_constructed_then_every_flag_is_off() {
        let options = ParseOptions::default();
        for key in ParseOptions::FLAG_KEYS {
            assert_eq!(options.flag_by_key(key), Some(false), "{key}");
        }
    }

    #[test]
    fn all_when_constructed_then_every_flag_is_on() {
        let options = ParseOptions::all();
        for key in ParseOptions::FLAG_KEYS {
            assert_eq!(options.flag_by_key(key), Some(true), "{key}");
        }
    }

    #[test]
    fn set_flag_by_key_when_known_then_sets_and_when_unknown_then_false() {
        let mut options = ParseOptions::default();
        assert!(options.set_flag_by_key("allow_continue", true));
        assert!(options.allow_continue);
        assert!(!options.set_flag_by_key("allow_nothing", true));
        assert_eq!(options.flag_by_key("allow_nothing"), None);
    }

    #[test]
    fn keyword_enabled_when_gated_keyword_then_follows_its_flag() {
        let strict = ParseOptions::default();
        assert!(!strict.keyword_enabled(SyntaxKind::Continue));
        assert!(!strict.keyword_enabled(SyntaxKind::Ltime));
        assert!(strict.keyword_enabled(SyntaxKind::If));
        let all = ParseOptions::all();
        assert!(all.keyword_enabled(SyntaxKind::Continue));
        assert!(all.keyword_enabled(SyntaxKind::Ltime));
    }

    #[test]
    fn keyword_enabled_when_every_gated_keyword_then_some_flag_turns_it_on() {
        // A gated keyword that no flag could enable would be dead syntax.
        let strict = ParseOptions::default();
        let all = ParseOptions::all();
        for kind in SyntaxKind::ALL.iter().filter(|kind| kind.is_keyword()) {
            if !strict.keyword_enabled(*kind) {
                assert!(all.keyword_enabled(*kind), "{kind:?}");
            }
        }
    }
}
