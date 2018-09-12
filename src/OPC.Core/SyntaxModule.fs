namespace OPC.Core

open OPC.Core.Types

module SyntaxModule = 
    open System
    
    let rec IsEndLine tokens =
        match tokens with
        | [ token ] -> 
            match token with
            | Tokens.Punctuation str when str.Equals(";") -> true 
            | _ -> false
        | head :: tail -> 
            match head with
            | Tokens.Punctuation str when str.Equals(";") -> IsBlock tail
            | _ -> false
        | _ -> false

    and IsBlock tokens =
        match tokens with 
        | [ token ] -> 
            match token with 
            | Tokens.Punctuation str when str.Equals("}") -> true
            | _ -> false
        | head :: tail -> 
            match head with
            | Tokens.Punctuation str when str.Equals("{") 
                -> IsVariableDeclaration tail || IsAssigment tail || IsReturn tail || IsBlock tail
            | _ -> IsVariableDeclaration tokens || IsAssigment tokens || IsReturn tokens
        | _ -> false

    and IsIdentifier tokens = 
        match tokens with 
        | head :: tail ->
            match head with 
            | Tokens.Identifier str -> IsEndLine tail
            | _ -> false
        | _ -> false

    and IsVariableDeclaration tokens =
        match tokens with
        | head :: tail ->
            match head with
            | Tokens.Keyword (str, dt) -> 
                match dt with
                | DataTypes.Integer | DataTypes.Real | DataTypes.Logic -> IsIdentifier tail
                | _ -> false
            | _ -> false
        | _ -> false

    and IsReturn tokens =
        match tokens with 
        | head :: tail -> 
            match head with
            | Tokens.Keyword (str, dt) -> 
                match dt with
                | DataTypes.Return -> IsIdentifier tail
                | _ -> false
            | _ -> false
        | _ -> false

    and IsOperation tokens count = 
        if count < 0 then false
        else
        match tokens with
        | [ _ ] -> (IsEndLine tokens) && count = 0
        | head :: tail -> 
            match head with 
            | Tokens.Identifier _ -> IsOperation tail count
            | Tokens.Operator op -> 
                match op with 
                | Operators.Arimetic _ 
                | Operators.Relational _ 
                | Operators.Logic _ -> IsOperation tail count
                | _ -> false
            | Tokens.Constant cnst -> 
                match cnst with
                | Numbers.Integer _ | Numbers.Real _ ->  IsOperation tail count
                | _ -> false
            | Tokens.Punctuation str when str.Equals("(") -> IsOperation tail (count + 1)
            | Tokens.Punctuation str when str.Equals(")") -> IsOperation tail (count - 1)
            | _ -> IsEndLine tokens
        | _ -> false

    and IsAssigmentOp tokens =
        match tokens with
        | head :: tail -> 
            match head with 
            | Tokens.Operator op -> 
                match op with 
                | Operators.Asignation _ -> IsOperation tail 0
                | _ -> false
            | _ -> false
        | _ -> false

    and IsAssigment tokens =
        match tokens with
        | head :: tail -> 
            match head with 
            | Tokens.Identifier _ -> IsAssigmentOp tail
            | _ -> false
        | _ -> false

    and IsKeywordWithType token action = 
        match token with
        | Tokens.Keyword (_, dt) -> 
            match dt with 
            | DataTypes.Integer _ 
            | DataTypes.Logic _ 
            | DataTypes.Real _ -> action
            | _ ->  false
        | _ -> false
        
    and IsParam tokens =
        match tokens with
        | head :: tail ->
            match head with 
            | Tokens.Identifier id -> IsComma tail
            | Tokens.Punctuation str when str.Equals(")") -> true
            | _ -> IsKeywordWithType head (IsParam tail)
        | _ -> false

    and IsComma tokens =
        match tokens with
        | head :: tail -> 
            match head with 
            | Tokens.Punctuation str when str.Equals(",") -> IsParam tail
            | Tokens.Punctuation str when str.Equals(")") -> true
            | _ -> false
        | _ -> false

    and IsParams tokens =
        match tokens with 
        | [ token ] -> 
            match token with
            | Tokens.Punctuation str when str.Equals(")") -> true
            | _ -> false
        | head :: tail -> 
            match head with 
            | Tokens.Punctuation str when str.Equals(")") -> IsBlock tail
            | _ -> IsKeywordWithType head (IsParam tail)
        | _ -> false

    and IsFunction tokens =
        match tokens with
        | head :: tail -> 
            match head with
            | Tokens.Identifier str -> IsFunction tail
            | Tokens.Punctuation str when str.Equals("(") -> IsParams tail
            | _ -> IsKeywordWithType head (IsFunction tail)
        | _ -> false

    let transform(list:List<Tokens>) =
        list |> Seq.toList 

    let rec SyntaxAnalyze(tokenList:List<Tokens>) =
        let tokens = transform tokenList
        match tokens with 
        | head :: tail -> 
            match head with
            | Tokens.Keyword (_, dt) -> 
                match dt with
                | DataTypes.None | DataTypes.LogicConstant -> false
                | DataTypes.Main -> 
                    match tail with 
                    | head :: tail -> 
                        match head with 
                        | Tokens.Punctuation str when str.Equals("{") -> true
                        | _ -> false
                    | _ -> false
                | _ -> 
                    match tail with 
                    | head :: tail -> 
                        match head with
                        | Tokens.Punctuation str when str.Equals("(") -> true
                        | _ -> false
                    | _ -> false
            | _ -> false
        | _ -> false

        