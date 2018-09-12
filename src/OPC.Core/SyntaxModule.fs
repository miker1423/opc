namespace OPC.Core

open System.Collections.Generic

open OPC.Core.Types

module SyntaxModule = 

    let mutable symbolTable = new List<Symbol>()

    let findAndModifyTokenForFunc (token:Tokens) dt =
        let index = symbolTable.FindIndex(fun x -> x.Token.Equals(token))
        let symbol = symbolTable.[index]
        symbolTable.[index] <- { symbol with Type = dt; IdType = IdTypes.Function }

    let findAndModifyTokenForVar (token:Tokens) dt =
        let index = symbolTable.FindIndex(fun x -> x.Token.Equals(token))
        let symbol = symbolTable.[index]
        symbolTable.[index] <- { symbol with Type = dt; IdType = IdTypes.Variable }

    let rec IsEndLine tokens =
        match tokens with
        | [ token ] -> 
            match token with
            | Tokens.Punctuation (str, _, _) when str.Equals(";") -> true 
            | _ -> false
        | head :: tail -> 
            match head with
            | Tokens.Punctuation (str,_, _) when str.Equals(";") -> IsBlock tail
            | _ -> false
        | _ -> false

    and IsBlock tokens =
        match tokens with 
        | [ token ] -> 
            match token with 
            | Tokens.Punctuation (str,_,_) when str.Equals("}") -> true
            | _ -> false
        | head :: tail -> 
            match head with
            | Tokens.Punctuation (str,_,_) when str.Equals("{") 
                -> IsVariableDeclaration tail || 
                    IsAssigment tail || 
                    IsReturn tail || 
                    IsIf tail || 
                    IsWhile tail || 
                    IsBlock tail
            | Tokens.Punctuation (str,_,_) when str.Equals("}")
                -> IsBlock tail || IsFunction tail DataTypes.None || IsMain tail
            | _ -> IsVariableDeclaration tokens || 
                    IsAssigment tokens || 
                    IsReturn tokens || 
                    IsIf tokens || 
                    IsWhile tokens ||
                    IsFunction tail DataTypes.None
        | _ -> false

    and IsIdentifier tokens action = 
        match tokens with 
        | head :: tail ->
            match head with 
            | Tokens.Identifier (str,_,_) -> action tail
            | _ -> false
        | _ -> false
    and IsEndlineForDeclaration tokens =
        match tokens with
        | head :: tail -> 
            match head with 
            | Tokens.Punctuation (str,_,_) when str.Equals(";") -> (true, tail)
            | _ -> (false, tail)
        | _ -> (false, [])
    and IsIdentiferForDeclaration tokens =
        match tokens with 
        | head :: tail ->
            match head with 
            | Tokens.Identifier (_,_,_) -> (head, IsEndlineForDeclaration tail)
            | _ -> (Tokens.None , (false, tail))
        | _ -> (Tokens.None, (false, []))
    and IsVariableDeclaration tokens =
        match tokens with
        | head :: tail ->
            match head with
            | Tokens.Keyword (_, dt, _, _) -> 
                match dt with
                | DataTypes.Integer | DataTypes.Real | DataTypes.Logic ->
                    let (id , (isId,newTail)) = IsIdentiferForDeclaration tail
                    if isId then 
                        findAndModifyTokenForVar id dt
                        IsBlock newTail
                    else IsFunction tail dt
                | _ -> false
            | _ -> false
        | _ -> false

    and IsReturn tokens =
        match tokens with 
        | head :: tail -> 
            match head with
            | Tokens.Keyword (str, dt, _, _) -> 
                match dt with
                | DataTypes.Return -> IsIdentifier tail IsEndLine
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
            | Tokens.Operator (op,_,_) -> 
                match op with 
                | Operators.Arimetic _ 
                | Operators.Relational _ 
                | Operators.Logic _ -> IsOperation tail count
                | _ -> false
            | Tokens.Constant (cnst,_,_) -> 
                match cnst with
                | Numbers.Integer _ | Numbers.Real _ ->  IsOperation tail count
                | _ -> false
            | Tokens.Punctuation (str,_,_) when str.Equals("(") -> IsOperation tail (count + 1)
            | Tokens.Punctuation (str,_,_) when str.Equals(")") -> IsOperation tail (count - 1)
            | _ -> IsEndLine tokens
        | _ -> false

    and IsAssigmentOp tokens =
        match tokens with
        | head :: tail -> 
            match head with 
            | Tokens.Operator (op,_,_) -> 
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
        | Tokens.Keyword (_, dt, _, _) -> 
            match dt with 
            | DataTypes.Integer _ 
            | DataTypes.Logic _ 
            | DataTypes.Real _ -> action
            | _ ->  false
        | _ -> false
    and IsKeywordWithTypeFunc token action =
        match token with
        | Tokens.Keyword (_, dt, _, _) -> 
            match dt with 
            | DataTypes.Integer _ 
            | DataTypes.Logic _ 
            | DataTypes.Real _ -> action dt
            | _ ->  false
        | _ -> false
    and IsParam tokens =
        match tokens with
        | head :: tail ->
            match head with 
            | Tokens.Identifier (id,_,_) -> IsComma tail
            | Tokens.Punctuation (str,_,_) when str.Equals(")") -> IsBlock tail
            | _ -> IsKeywordWithType head (IsParam tail)
        | _ -> false

    and IsComma tokens =
        match tokens with
        | head :: tail -> 
            match head with 
            | Tokens.Punctuation (str,_,_) when str.Equals(",") -> IsParam tail
            | Tokens.Punctuation (str,_,_) when str.Equals(")") -> IsBlock tail
            | _ -> false
        | _ -> false

    and IsParams tokens =
        match tokens with 
        | [ token ] -> 
            match token with
            | Tokens.Punctuation (str,_,_) when str.Equals(")") -> true
            | _ -> false
        | head :: tail -> 
            match head with 
            | Tokens.Punctuation (str,_,_) when str.Equals(")") -> IsBlock tail
            | _ -> IsKeywordWithType head (IsParam tail)
        | _ -> false
            
    and IsFunction tokens dt =
        match tokens with
        | head :: tail -> 
            match head with
            | Tokens.Identifier (str,_,_) -> 
                findAndModifyTokenForFunc head dt
                IsFunction tail DataTypes.None
            | Tokens.Punctuation (str,_,_) when str.Equals("(") -> IsParams tail
            | _ -> IsKeywordWithTypeFunc head (IsFunction tail)
        | _ -> false

    and IsClosing tokens = 
        match tokens with
        | head :: tail -> 
            match head with 
            | Tokens.Punctuation (str,_,_) when str.Equals(")") -> IsBlock tail
            | _ -> false
        | _ -> false

    and IsOpening tokens =
        match tokens with 
        | head :: tail -> 
            match head with 
            | Tokens.Punctuation (str,_,_) when str.Equals("(") -> IsIdentifier tail IsClosing
            | _ -> false
        | _ -> false

    and IsIf tokens = 
        match tokens with 
        | head :: tail -> 
            match head with
            | Tokens.Keyword (str, dt,_,_) when str.Equals("si") -> IsOpening tail
            | _ -> false
        | _ -> false

    and IsWhile tokens =
        match tokens with
        | head :: tail -> 
            match head with
            | Tokens.Keyword (str, dt,_,_) when str.Equals("mientras") -> IsOpening tail
            | _ -> false
        | _ -> false

    and NoParams tokens =
        match tokens with
        | head :: tail -> 
            match head with
            | Tokens.Punctuation (str,_,_) when str.Equals("(") -> IsClosing tail
            |_ -> false
        | _ -> false

    and IsMain tokens =
        match tokens with
        | head :: tail -> 
            match head with
            | Tokens.Keyword (_, dt,_,_) -> 
                match dt with 
                | DataTypes.Main -> NoParams tail
                | _ -> false
            |_ -> false
        | _ -> false

    let transform(list:List<Tokens>) =
        list |> Seq.toList 

    let rec SyntaxAnalyze(tokenList:List<Tokens>, symbolList:List<Symbol>) =
        symbolTable <- symbolList
        let transformed = tokenList |> transform
        let result = IsFunction transformed DataTypes.None
        let symbols = symbolTable |> Seq.filter(fun x -> not (x.IdType.Equals(IdTypes.None)))
        (result, symbols)