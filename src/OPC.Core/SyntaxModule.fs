namespace OPC.Core

open OPC.Core.Types

module SyntaxModule = 
    
    let transform(list:List<Tokens>) =
        list |> Seq.toList 

    let syntaxAnalyze(tokenList:List<Tokens>) =
        let tokens = transform tokenList
        match tokens with 
        | head :: tail -> 
            match head with 
            | Tokens.Keyword (_, dt) -> 
                match dt with 
                | DataTypes.None | DataTypes.LogicConstant -> false
                | _ -> true
            | _ -> false
        | _ -> false