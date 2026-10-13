//! The expansion of `#[derive(Attributed)]`: the implementation of
//! `crate::attribute::Attributed` for a node with a field `attributes`.
use quote::quote;
use syn::spanned::Spanned;
use syn::DeriveInput;
use syn::Error;
use syn::Result;

/// Returns a stream of tokens that implement `Attributed` for the given type.
///
/// A node without the field `attributes` is refused here; a node that has the
/// field and does not derive this is refused by the match over the
/// declaration kinds in `attribute.rs`.
pub(crate) fn expand_attributed(ast: &DeriveInput) -> Result<proc_macro2::TokenStream> {
    let name = &ast.ident;

    let has_field = match &ast.data {
        syn::Data::Struct(data_struct) => match &data_struct.fields {
            syn::Fields::Named(named_fields) => named_fields.named.iter().any(|field| {
                field
                    .ident
                    .as_ref()
                    .is_some_and(|ident| ident == "attributes")
            }),
            _ => false,
        },
        _ => false,
    };
    if !has_field {
        return Err(Error::new(
            ast.span(),
            "#[derive(Attributed)] is only supported for structs with a named field `attributes`",
        ));
    }

    let (impl_generics, ty_generics, where_clause) = ast.generics.split_for_impl();
    Ok(quote! {
        impl #impl_generics crate::attribute::Attributed for #name #ty_generics #where_clause {
            fn attributes(&self) -> &crate::attribute::Attributes {
                &self.attributes
            }
        }
    })
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn expand_attributed_when_struct_has_attributes_field_then_ok() {
        let ast: DeriveInput = syn::parse_str("struct Thing { pub attributes: Attributes }")
            .expect("test source must parse");
        assert!(expand_attributed(&ast).is_ok());
    }

    #[test]
    fn expand_attributed_when_struct_has_no_attributes_field_then_error() {
        let ast: DeriveInput =
            syn::parse_str("struct Thing { pub name: Id }").expect("test source must parse");
        assert!(expand_attributed(&ast).is_err());
    }

    #[test]
    fn expand_attributed_when_enum_then_error() {
        let ast: DeriveInput = syn::parse_str("enum Thing { A }").expect("test source must parse");
        assert!(expand_attributed(&ast).is_err());
    }
}
