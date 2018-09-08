namespace OPC.Core

open System
open System.Text
open System.Collections.Generic

module LexicalModule =

    type LastAction = 
        | Added
        | Again
        | None

    type Operators =
        | Arimetic of string
        | Logic of string
        | Relational of string
        | Asignation of string
        | CheckAgain
        | None

    type Tokens = 
        | Keyword of string
        | Operator of Operators
        | Identifier of string
        | Punctuation of string
        | Constant of string
        | None
        
    let keyworkds = [ "si"; "principal"; "entero"; "real"; "logico"; "mientras"; "regresa"; "verdadero"; "falso" ]

    let isPunctuation str = 
        match str with
        | "(" | ")" | " " | "\n" | "{" | "}" | "," | ";" -> Tokens.Punctuation(str)
        | _ -> Tokens.None
        
    let isOperator str checkEquals =
        let operator = 
            match str with
            | "+" | "-" | "*" | "/" | "^" -> Operators.Arimetic(str)
            | "&" | "|" | "!" -> Operators.Logic(str)
            | "<" | ">" -> Operators.Relational(str)
            | "==" when checkEquals -> Operators.Relational(str)
            | "=" when checkEquals -> Operators.Asignation(str)
            | "=" when (not checkEquals) -> Operators.CheckAgain
            | _ -> Operators.None

        match operator with
        | Operators.None -> Tokens.None
        | _ -> Tokens.Operator(operator)

    let isKeyword str =
        let exits = List.exists ((=) str) keyworkds
        if(exits) then
            Tokens.Keyword(str)
        else
            Tokens.None

    let addAndReset(buffer:StringBuilder, tokenList:List<Tokens>, token:Tokens) = 
        buffer.Clear() |> ignore
        tokenList.Add(token)
        LastAction.Added

    let checkKeyword(buffer:StringBuilder, tokenList:List<Tokens>, str:string) =
        let keyword = isKeyword str
        match keyword with
        | Tokens.Keyword _ -> addAndReset(buffer, tokenList, keyword) 
        | Tokens.None -> LastAction.None
        | _ -> LastAction.None

    let checkOperator(buffer:StringBuilder, tokenList:List<Tokens>, str:string, checkAgain:bool) =
        let operator = isOperator str checkAgain
        match operator with 
        | Tokens.Operator op ->
            match op with
            | Operators.CheckAgain -> LastAction.Again
            | _ -> addAndReset(buffer, tokenList, operator)
        | Tokens.None -> LastAction.None
        | _ -> LastAction.None
        
    let checkPunctuation(buffer:StringBuilder, tokenList:List<Tokens>, str) =
        let puntuation = isPunctuation str
        match puntuation with
        | Tokens.Punctuation _ -> addAndReset(buffer, tokenList, puntuation)
        | Tokens.None -> LastAction.None
        | _ -> LastAction.None

    let checkFullOperator(buffer:StringBuilder, tokenList:List<Tokens>, i:byref<int>, text:ReadOnlySpan<char>) =
        let shouldCheck = false
        let operator = checkOperator(buffer, tokenList, buffer.ToString(), shouldCheck)
        if(operator = LastAction.Again) then
            let shouldAppend = isPunctuation(text.[i+1].ToString())
            if(shouldAppend = Tokens.None) then
                buffer.Append(text.[i+1]) |> ignore
                i <- i + 1
                checkOperator(buffer, tokenList, buffer.ToString(), not shouldCheck)
            else 
                checkOperator(buffer, tokenList, buffer.ToString(), not shouldCheck)
        else
            operator

    let getFullStr(buffer:StringBuilder, i:byref<int>, text:ReadOnlySpan<char>) = 
        let mutable punctuation = isPunctuation(text.[i].ToString())
        buffer.Clear() |> ignore
        while ((punctuation = Tokens.None) && (i < text.Length - 1)) do
            buffer.Append(text.[i]) |> ignore
            i <- i + 1
            punctuation <- isPunctuation(text.[i].ToString())

        buffer.ToString()


    let processSpan(text:ReadOnlySpan<char>, buffer:StringBuilder, tokenList:List<Tokens>) =
        let mutable i = 0
        while i < text.Length - 1 do
            buffer.Append(text.[i]) |> ignore
            let punctuation = checkPunctuation(buffer, tokenList, buffer.ToString())
            if(punctuation <> LastAction.Added) then
                let result = checkFullOperator(buffer, tokenList, &i, text)
                if(result <> LastAction.Added) then
                    let id = getFullStr(buffer, &i, text)
                    let keyword = checkKeyword(buffer, tokenList, id)
                    if(keyword = LastAction.Added) then i <- i + 1
                else 
                    i <- i + 1
            else
                i <- i + 1
        tokenList

        
    let getTokens(text:ReadOnlySpan<char>) =
        let buffer = StringBuilder()
        let tokenList = List<Tokens>()
        processSpan(text, buffer, tokenList)


