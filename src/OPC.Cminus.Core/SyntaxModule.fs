namespace OPC.Cminus.Core

open OPC.Cminus.Core.Types

module SyntaxModule = 
    let isEndLine prev next = 
        match next with
        | [ token ] ->
            match token with
            | Tokens.Punctuation(str) when str.Equals(";") ->  true
            | _ -> false
        | head :: tail -> true
        | _ -> false

