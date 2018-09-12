namespace OPC.Core

open OPC.Core.Types

module SyntaxModule = 

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

        