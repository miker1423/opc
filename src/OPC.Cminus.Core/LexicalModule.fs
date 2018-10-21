namespace OPC.Cminus.Core

open OPC.Cminus.Core.Types

module LexicalModule =
    let isPunctuation str =
        match str with
        | "(" | ")" | " " | "\n" | "{" | "}" | "," | ";" | "\t" | "\r" -> Tokens.Punctuation(str)
        | _ -> Tokens.None

    let isOperator str =
        let operator = 
            match str with 
            | "+" | "-" -> Operators.Arimetic(AritmeticOps.Sum)
            | "/" | "*" -> Operators.Arimetic(AritmeticOps.Mult)
            | "&" | "|" | "!" -> Operators.Logic(str)
            | "<" | ">" | "==" -> Operators.Relational(str)
            | "=" -> Operators.Asignation(str)
            | _ -> Operators.None
        
        if(operator = Operators.None) then Tokens.None else Tokens.Operator(operator)

    let isKeyword str = 
        match str with
        | "while" | "else" | "return" 
        | "if" | "int" | "void" -> Tokens.Keyword(str)
        | _ -> Tokens.None

    let getToken str =
        let tryPunctuation = isPunctuation str
        match tryPunctuation with
        | Tokens.None -> 
            let isKeyword = isKeyword str
            match isKeyword with
            | Tokens.None -> 
                let isOp = isOperator str
                if(isOp = Tokens.None) then Tokens.Identifier(str) else isOp
            | _ -> isKeyword
        | _ -> tryPunctuation

    let rec processStringRec lexems list = 
        match lexems with
        | [lexem] -> (list @ [getToken lexem])
        | head::tail -> processStringRec tail (list @ [getToken head])
        | [] -> list


    let processString lexems =
        processStringRec lexems []
