//! The expansion of `#[derive(Located)]`: which field gives the `span` of a
//! node, and the implementation of `Located` that returns it.
use proc_macro::TokenStream;
use quote::quote;
use syn::spanned::Spanned;
use syn::DeriveInput;
use syn::Error;
use syn::Field;
use syn::FieldsNamed;
use syn::Ident;
use syn::Result;

/// Selects how a field contributes to the derived `span` implementation.
enum LocatedKind {
    /// The field is a `SourceSpan`; clone it.
    Position,
    /// The field is a sub-node implementing `Located`; delegate to it.
    Delegate,
}

/// Returns a stream of tokens that implement `Located` for the given type.
pub(crate) fn expand_located(ast: &DeriveInput) -> Result<TokenStream> {
    let name = &ast.ident;

    let fields = match &ast.data {
        syn::Data::Struct(data_struct) => match &data_struct.fields {
            syn::Fields::Named(named_fields) => named_fields,
            _ => {
                return Err(Error::new(
                    ast.span(),
                    "#[derive(Located)] is only supported for structs with named fields",
                ))
            }
        },
        _ => {
            return Err(Error::new(
                ast.span(),
                "#[derive(Located)] is only supported for structs with named fields",
            ))
        }
    };

    let body = located_body(fields)?;

    let (impl_generics, ty_generics, where_clause) = ast.generics.split_for_impl();
    let gen = quote! {
        impl #impl_generics crate::core::Located for #name #ty_generics #where_clause {
            fn span(&self) -> crate::core::SourceSpan {
                #body
            }
        }
    };

    Ok(gen.into())
}

/// Returns the body of the derived `span` method for the given fields.
fn located_body(fields: &FieldsNamed) -> Result<proc_macro2::TokenStream> {
    // Collect every field that carries a `#[located(...)]` attribute.
    let mut annotated: Vec<(&Ident, LocatedKind)> = Vec::new();
    for field in &fields.named {
        if let Some(kind) = located_field_kind(field)? {
            let ident = field
                .ident
                .as_ref()
                .expect("named field always has an identifier");
            annotated.push((ident, kind));
        }
    }

    match annotated.as_slice() {
        // No attribute: fall back to a field named `position`.
        [] => {
            let has_position = fields
                .named
                .iter()
                .any(|f| f.ident.as_ref().is_some_and(|i| i == "position"));
            if has_position {
                Ok(quote! { self.position.clone() })
            } else {
                Err(Error::new(
                    fields.span(),
                    "#[derive(Located)] requires a field named `position` or a field annotated \
                     with #[located(position)] or #[located(delegate)]",
                ))
            }
        }
        [(ident, kind)] => match kind {
            LocatedKind::Position => Ok(quote! { self.#ident.clone() }),
            LocatedKind::Delegate => Ok(quote! { self.#ident.span() }),
        },
        _ => Err(Error::new(
            fields.span(),
            "#[derive(Located)] permits at most one #[located(...)] field attribute",
        )),
    }
}

/// Returns the `Located` kind requested by a field's `#[located(...)]`
/// attribute, or `None` when the field carries no such attribute.
fn located_field_kind(field: &Field) -> Result<Option<LocatedKind>> {
    let mut kind = None;
    for attr in &field.attrs {
        if attr.path().is_ident("located") {
            attr.parse_nested_meta(|meta| {
                if meta.path.is_ident("position") {
                    kind = Some(LocatedKind::Position);
                    return Ok(());
                }
                if meta.path.is_ident("delegate") {
                    kind = Some(LocatedKind::Delegate);
                    return Ok(());
                }
                Err(meta.error("unrecognized value in located; expected `position` or `delegate`"))
            })?;
        }
    }
    Ok(kind)
}
