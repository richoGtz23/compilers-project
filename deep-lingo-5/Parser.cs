/*
Luis Ricardo Gutierrez A01376121
Josep Romagosa A01374637
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace DeepLingo {

    class Parser {
        static readonly ISet<TokenType> firstOfStatement =
            new HashSet<TokenType> () {
                TokenType.IDENTIFIER,
                TokenType.LOOP,
                TokenType.IF,
                TokenType.BREAK,
                TokenType.RETURN
            };

        static readonly ISet<TokenType> FirstOfExprUnary =
            new HashSet<TokenType> () {
                TokenType.NOT,
                TokenType.SUM,
                TokenType.SUB
            };
        static readonly ISet<TokenType> firstOfOperatorComp =
            new HashSet<TokenType> () {
                TokenType.LT,
                TokenType.LOET,
                TokenType.GT,
                TokenType.GOET
            };
        static readonly ISet<TokenType> firstOfOperatorMath =
            new HashSet<TokenType> () {
                TokenType.MUL,
                TokenType.DIV,
                TokenType.MOD
            };
        static readonly ISet<TokenType> firstOfSimpleExpression =
            new HashSet<TokenType> () {
                TokenType.IDENTIFIER,
                TokenType.VAR_INT,
                TokenType.VAR_CHAR,
                TokenType.VAR_STRING,
                TokenType.TRUE,
                TokenType.FALSE,
                TokenType.PARENTHESIS_OPEN,
                TokenType.NOT,
                TokenType.ARR_BEGIN,
                TokenType.SUM,
                TokenType.SUB,
                TokenType.MUL,
                TokenType.DIV,
                TokenType.MOD,
                TokenType.LT,
                TokenType.LOET,
                TokenType.GT,
                TokenType.GOET,
                TokenType.NOT_EQUALS,
                TokenType.EQUALS
            };

        static readonly ISet<TokenType> firstOfExprPrimary = new HashSet<TokenType>(){
            TokenType.IDENTIFIER,
            TokenType.PARENTHESIS_OPEN,
            TokenType.VAR_STRING,
            TokenType.VAR_CHAR,
            TokenType.VAR_INT,
            TokenType.ARR_BEGIN
        };
        IEnumerator<Token> tokenStream;

        public Parser (IEnumerator<Token> tokenStream) {
            this.tokenStream = tokenStream;
            this.tokenStream.MoveNext ();
        }

        public TokenType CurrentToken {
            get { return tokenStream.Current.Category; }
        }

        public Token Expect (TokenType category) {

            if (CurrentToken == category) {

                //Console.WriteLine ($"Success : Expected {category.ToString ()}, got {CurrentToken}, \n Token:{tokenStream.Current.Lexeme} ");
                Token current = tokenStream.Current;
                tokenStream.MoveNext ();
                return current;
            } else {
                throw new SyntaxError (category, tokenStream.Current);
            }

        }
        public Node Program () {
            Prog pro = new Prog();
            while (CurrentToken == TokenType.VAR || CurrentToken == TokenType.IDENTIFIER) {
                if(CurrentToken == TokenType.VAR){
                    pro.Add(VarDef());
                }else{
                    pro.Add(FunDef());
                    Expect (TokenType.BLOCK_END);
                }
            }
            Expect (TokenType.EOF);
            return pro;
        }

        public Node VarDef () {
            var varDef = new VarDef{
                AnchorToken = Expect(TokenType.VAR)
            };
            varDef.Add(IdList ());
            Expect (TokenType.INSTRUCTION_END);
            return varDef;
        }

        public Node FunDef () {
            FunDef funDef = new FunDef(){
                AnchorToken =  Expect(TokenType.IDENTIFIER)
            };
            Expect (TokenType.PARENTHESIS_OPEN);
            if (CurrentToken != TokenType.PARENTHESIS_CLOSE) {
                funDef.Add(IdList ());
            }
            Expect (TokenType.PARENTHESIS_CLOSE);
            Expect (TokenType.BLOCK_BEGIN);
            VarList vars = new VarList();
            StmtList stmts = new StmtList();
            while (CurrentToken == TokenType.VAR) {
                vars.Add(VarDef ());
            }
            while (firstOfStatement.Contains (CurrentToken)) {
                stmts.Add(Stmt ());
            }
            funDef.Add(vars);
            funDef.Add(stmts);
            return funDef;

        }

        public Node IdList () {
            IdList idList = new IdList();
            idList.Add(new Identifier(){AnchorToken = Expect (TokenType.IDENTIFIER)});
            while (CurrentToken == TokenType.LIST) {
                Expect (TokenType.LIST);
                idList.Add(new Identifier(){ AnchorToken = Expect (TokenType.IDENTIFIER)});
            }
            return idList;
        }

        public Node Stmt () {
            dynamic stmt = null;
            switch (CurrentToken) {
                case TokenType.IDENTIFIER:
                    if (CurrentToken == TokenType.PARENTHESIS_OPEN) {
                        stmt = (FunCall ());
                    } else {
                        stmt = (StmtCall ());
                    }
                    break;
                case TokenType.IF:
                    stmt=(If ());
                    break;
                case TokenType.LOOP:
                    stmt=(Loop ());
                    break;
                case TokenType.BREAK:
                    stmt=(Break ());
                    break;
                case TokenType.RETURN:
                    stmt=(Return ());
                    break;
                case TokenType.INSTRUCTION_END:
                    Expect (TokenType.INSTRUCTION_END);
                    break;
                default:
                    throw new SyntaxError (CurrentToken, tokenStream.Current);
            }
            return stmt;
        }

        public Node StmtCall () {
            var identifier = Expect (TokenType.IDENTIFIER);
            dynamic stmtCall = null;
            switch (CurrentToken) {
                case (TokenType.INCR):
                    Expect(TokenType.INCR);
                    stmtCall = (new Increment(){AnchorToken = identifier});
                    Expect(TokenType.INSTRUCTION_END);
                    break;
                case (TokenType.DECR):
                    Expect(TokenType.DECR);
                    stmtCall = (new Decrement() { AnchorToken = identifier });
                    Expect(TokenType.INSTRUCTION_END);
                    break;
                case (TokenType.ASSIGN):
                    stmtCall = (Assignment (identifier));
                    break;
                case (TokenType.PARENTHESIS_OPEN):
                    stmtCall = (FunCall ());
                    stmtCall.AnchorToken = identifier;
                    Expect (TokenType.INSTRUCTION_END);
                    break;
            }
            return stmtCall;

        }

        public Node If () {
            If ifNode = new If();
            ifNode.AnchorToken = Expect (TokenType.IF);
            Expect (TokenType.PARENTHESIS_OPEN);
            ifNode.Add(Expression ());
            Expect (TokenType.PARENTHESIS_CLOSE);
            Expect (TokenType.BLOCK_BEGIN);
            StmtList stmt =  new StmtList();
            while (firstOfStatement.Contains (CurrentToken)) {
                stmt.Add(Stmt ());
            }
            ifNode.Add(stmt);
            Expect (TokenType.BLOCK_END);
            var elseIfFlag = 0;
            ElseIfList elseIfList = null;
            while (CurrentToken == TokenType.ELSEIF) {
                if(elseIfFlag == 0){
                    elseIfList = new ElseIfList();
                    elseIfFlag = 1;
                }
                ElseIf elseif = new ElseIf();
                elseif.AnchorToken = Expect (TokenType.ELSEIF);
                Expect (TokenType.PARENTHESIS_OPEN);
                elseif.Add(Expression ());
                Expect (TokenType.PARENTHESIS_CLOSE);
                Expect (TokenType.BLOCK_BEGIN);
                Stmt stmtEI = new Stmt();
                while (firstOfStatement.Contains (CurrentToken)) {
                    stmtEI.Add(Stmt ());
                }
                elseif.Add(stmtEI);
                Expect (TokenType.BLOCK_END);
                elseIfList.Add(elseif);
            }
            if(elseIfFlag ==1){
                ifNode.Add(elseIfList);
            }
            if (CurrentToken == TokenType.ELSE) {
                Else elseNode = new Else();
                elseNode.AnchorToken = Expect (TokenType.ELSE);
                Expect (TokenType.BLOCK_BEGIN);
                StmtList stmtE = new StmtList();
                while (firstOfStatement.Contains (CurrentToken)) {
                    stmtE.Add(Stmt ());
                }
                elseNode.Add(stmtE);
                Expect (TokenType.BLOCK_END);
                ifNode.Add(elseNode);
            }
            return ifNode;
        }

        public Node Loop () {
            Loop loop = new Loop(){
                AnchorToken = Expect(TokenType.LOOP)
            };
            Expect (TokenType.BLOCK_BEGIN);
            while (firstOfStatement.Contains (CurrentToken)) {
                loop.Add(Stmt ());
            }
            Expect (TokenType.BLOCK_END);
            return loop;
        }

        public Node Break () {
            Break br = new Break(){
                AnchorToken = Expect(TokenType.BREAK)
            };
            Expect (TokenType.INSTRUCTION_END);
            return br;
        }

        public Node Assignment (Token identifier) {
            //Expect (TokenType.IDENTIFIER);
            Assignment assign = new Assignment();
            Expect (TokenType.ASSIGN);
            assign.AnchorToken = identifier;
            assign.Add(Expression ());
            if (CurrentToken == TokenType.INSTRUCTION_END) {
                Expect (TokenType.INSTRUCTION_END);
            }
            return assign;

        }

        public Node Return () {
            Return ret = new Return(){
                AnchorToken = Expect(TokenType.RETURN)
            };
            ret.Add(Expression ());
            Expect (TokenType.INSTRUCTION_END);
            return ret;
        }
        public Node FunCall () {
            Expect (TokenType.PARENTHESIS_OPEN);
            FunCall fnCall = new FunCall();
            fnCall.Add(ExprList());
            while (firstOfSimpleExpression.Contains (CurrentToken)) {
                
                while (CurrentToken == TokenType.LIST) {
                    Expect (TokenType.LIST);
                    fnCall.Add(ExprList ());
                }
            }
            Expect (TokenType.PARENTHESIS_CLOSE);
            return fnCall;
        }
        public Node Expression(){
            var expr = ExpressionAnd();
            while(CurrentToken == TokenType.OR){
                var expr2 = new Or(){
                    AnchorToken = Expect(TokenType.OR)
                };
                expr2.Add(expr);
                expr2.Add(ExpressionAnd());
                expr = expr2;
            }
            return expr;
        }
        public Node ExpressionAnd(){
            var expr = ExporEq();
            while(CurrentToken == TokenType.AND){
                 var expr2 = new And(){
                    AnchorToken = Expect(TokenType.AND)
                };
                expr2.Add(expr);
                expr2.Add(ExporEq());
                expr = expr2;
            }
            return expr;
            
        }
        public Node ExporEq(){
            var expr = ExprComp();
            //.WriteLine(tokenStream.Current);
            while(CurrentToken == TokenType.EQUALS || CurrentToken == TokenType.NOT_EQUALS){
                
               var expr2 = new ExpressionEquality();
               if(CurrentToken == TokenType.EQUALS){
                   expr2.AnchorToken = Expect(TokenType.EQUALS);
               }else{
                   expr2.AnchorToken = Expect(TokenType.NOT_EQUALS);
               }
               expr2.Add(expr);
               expr2.Add(ExprComp());
               expr = expr2;
            }
            return expr;
        }
        
        public Node ExprComp(){
            var expr = ExprAdd();
           
            while (firstOfOperatorComp.Contains(CurrentToken))
            {
                var expr2 = new ExpressionComparison();
                switch (CurrentToken)
                {
                    case TokenType.LT:
                        expr2.AnchorToken = Expect(TokenType.LT);
                        break;

                    case TokenType.LOET:
                        expr2.AnchorToken = Expect(TokenType.LOET);
                        break;
                        
                    case TokenType.GT:
                         expr2.AnchorToken = Expect(TokenType.GT);
                        break;
                    case TokenType.GOET:
                         expr2.AnchorToken = Expect(TokenType.GOET);
                        break;
                        
                    default:
                        throw new SyntaxError(firstOfOperatorComp,
                                              tokenStream.Current);
                }
               expr2.Add(expr);
               expr2.Add(ExprAdd());
               expr=expr2;
                
            }
            return expr;
        }
        
        public Node ExprAdd(){
            var expr = ExprMul();
            while (CurrentToken == TokenType.SUM || CurrentToken == TokenType.SUB){
                var expr2 = new ExpressionAdd();
                if(CurrentToken == TokenType.SUB){
                    expr2.AnchorToken = Expect(TokenType.SUB);    
                }else{
                    expr2.AnchorToken = Expect(TokenType.SUM);
                }
                expr2.Add(expr);
                expr2.Add(ExprMul());
                expr = expr2;
            }
            return expr;     
        }
        
        public Node ExprMul(){
            var expr = ExprUnary();
            
            while (firstOfOperatorMath.Contains(CurrentToken))
            {
                var expr2 = new ExpressionMul();
                switch (CurrentToken)
                {
                    case TokenType.MUL:
                        expr2.AnchorToken=Expect(TokenType.MUL);
                        break;

                    case TokenType.MOD:
                        expr2.AnchorToken=Expect(TokenType.MOD);
                        break;

                    case TokenType.DIV:
                        expr2.AnchorToken=Expect(TokenType.DIV);
                        break;

                    default:
                        throw new SyntaxError(firstOfOperatorMath,tokenStream.Current);
                }
                expr2.Add(expr);
                expr2.Add(ExprUnary());
                expr = expr2;
            }
            return expr;
        }
        
        public Node ExprUnary(){
            var expr = new ExpressionUnary();
            var temp = expr;
             if (FirstOfExprUnary.Contains(CurrentToken))
            {
                
                while (FirstOfExprUnary.Contains(CurrentToken))
                {
                    switch (CurrentToken)
                    {
                        case TokenType.SUM:
                            temp.AnchorToken=Expect(TokenType.SUM);
                            break;

                        case TokenType.SUB:
                            temp.AnchorToken=Expect(TokenType.SUB);
                            break;

                        case TokenType.NOT:
                            temp.AnchorToken=Expect(TokenType.NOT);
                            break;

                        default:
                            throw new SyntaxError(FirstOfExprUnary,
                                                  tokenStream.Current);
                    }
                    if(!FirstOfExprUnary.Contains(CurrentToken)){
                        temp.Add(ExprPrimary());
                    }else{
                        var newNode = new ExpressionUnary(); 
                        temp.Add(newNode);
                        temp = newNode;
                    }
                }
            }
            else
            {
                return ExprPrimary();
            }
            return expr;
        }
        
        public Node ExprPrimary(){
            switch (CurrentToken)
            {
                case TokenType.IDENTIFIER:
                    var token = Expect(TokenType.IDENTIFIER);
                    if (CurrentToken == TokenType.PARENTHESIS_OPEN)
                    {
                        Expect(TokenType.PARENTHESIS_OPEN);
                        var func = new FunCall(){
                            AnchorToken = token
                        };
                        func.Add(ExprList());
                        Expect(TokenType.PARENTHESIS_CLOSE);
                        return func;
                    }else{
                        var id = new Identifier(){
                            AnchorToken=token
                        };
                        return id;
                    }
                case TokenType.ARR_BEGIN:
                    var arra = new Array();
                    Expect(TokenType.ARR_BEGIN);
                    arra.Add(ExprList());
                    Expect(TokenType.ARR_END);
                    return arra;

                case TokenType.VAR_STRING:
                    var stri = new VarString();
                    stri.AnchorToken = Expect(TokenType.VAR_STRING);
                    return stri;

                case TokenType.VAR_CHAR:
                    var cha = new VarChar();
                    cha.AnchorToken = Expect(TokenType.VAR_CHAR);
                    return cha;

                case TokenType.VAR_INT:
                    var inti = new VarInt();
                    inti.AnchorToken = Expect(TokenType.VAR_INT);
                    return inti;

                case TokenType.PARENTHESIS_OPEN:
                    Expect(TokenType.PARENTHESIS_OPEN);
                    var expr = Expression();
                    Expect(TokenType.PARENTHESIS_CLOSE);
                    return expr;

                default:
                    throw new SyntaxError(firstOfExprPrimary,
                                            tokenStream.Current);
            }
           
        }
        public Node ExprList(){
            var ExpressionList = new ExpressionList();
            if(CurrentToken == TokenType.PARENTHESIS_OPEN || CurrentToken == TokenType.ARR_END){
                return ExpressionList;
            }
            if (firstOfSimpleExpression.Contains(CurrentToken))
            {
                ExpressionList.Add(Expression());
            }
                while (CurrentToken == TokenType.LIST)
                {   
                    Expect(TokenType.LIST);
                    ExpressionList.Add(Expression());
                }
            
            return ExpressionList;
        }
    }
}