/*
Luis Ricardo Gutierrez A01376121
Josep Romagosa A01374637
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace DeepLingo {

    class Parser {
        private ISet<TokenType> firstOfOperator = new HashSet<TokenType> ();
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
                TokenType.SUB,
                TokenType.IDENTIFIER,
                TokenType.VAR_INT,
                TokenType.VAR_CHAR,
                TokenType.VAR_STRING
            };

        static readonly ISet<TokenType> firstOfOperatorBool =
            new HashSet<TokenType> () {
                TokenType.OR,
                TokenType.AND
            };
        static readonly ISet<TokenType> firstOfOperatorComp =
            new HashSet<TokenType> () {
                TokenType.LT,
                TokenType.LOET,
                TokenType.GT,
                TokenType.GOET,
                TokenType.EQUALS,
                TokenType.NOT_EQUALS
            };
        static readonly ISet<TokenType> firstOfOperatorMath =
            new HashSet<TokenType> () {
                TokenType.SUM,
                TokenType.MUL,
                TokenType.SUB,
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
                TokenType.NOT
            };
        IEnumerator<Token> tokenStream;

        public Parser (IEnumerator<Token> tokenStream) {
            this.tokenStream = tokenStream;
            this.tokenStream.MoveNext ();
            firstOfOperator.UnionWith (firstOfOperatorComp);
            firstOfOperator.UnionWith (firstOfOperatorBool);
            firstOfOperator.UnionWith (firstOfOperatorMath);
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
        public Token ExpectSet (ISet<TokenType> category) {
            if (category.Contains (CurrentToken)) {
                Token current = tokenStream.Current;
                tokenStream.MoveNext ();
                return current;
            } else {
                throw new SyntaxError (category, tokenStream.Current);
            }
        }

        public Node Program () {
            Prog pro = new Prog();
            VarList varDef = new VarList();
            FunList funDef = new FunList();
            while (CurrentToken == TokenType.VAR) {
                varDef.Add(VarDef ());
            }

            while (CurrentToken == TokenType.IDENTIFIER) {
                funDef.Add(FunDef ());
                Expect (TokenType.BLOCK_END);
            }
            pro.Add(varDef);
            pro.Add(funDef);
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
            idList.Add(new Node(){AnchorToken = Expect (TokenType.IDENTIFIER)});
            while (CurrentToken == TokenType.LIST) {
                Expect (TokenType.LIST);
                idList.Add(new Node(){ AnchorToken = Expect (TokenType.IDENTIFIER)});
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
                    stmtCall = (Increment ());
                    stmtCall.Add(new Node(){AnchorToken = identifier});
                    break;
                case (TokenType.DECR):
                    stmtCall = (Decrement ());
                    stmtCall.Add(new Node() { AnchorToken = identifier });
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
            assign.AnchorToken = Expect (TokenType.ASSIGN);
            assign.Add(new Node() { AnchorToken = identifier });
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

        public Node Increment () {
            Increment incr = new Increment(){
                AnchorToken = Expect(TokenType.INCR)
            };
            Expect (TokenType.INSTRUCTION_END);
            return incr;
        }

        public Node Decrement () {
            Decrement decr = new Decrement(){
                AnchorToken =  Expect(TokenType.DECR)
            }; 
            Expect (TokenType.INSTRUCTION_END);
            return decr;
        }

        public Node FunCall () {
            Expect (TokenType.PARENTHESIS_OPEN);
            FunCall fnCall = new FunCall();
            while (firstOfSimpleExpression.Contains (CurrentToken)) {
                fnCall.Add(Expression ());
                while (CurrentToken == TokenType.LIST) {
                    Expect (TokenType.LIST);
                    fnCall.Add(Expression ());
                }
            }
            Expect (TokenType.PARENTHESIS_CLOSE);
            return fnCall;
        }

        public Node Expression () {
            var expr = (ExpressionUnary ());
            while (firstOfOperator.Contains (CurrentToken)) {
                var expr2 = (Operator ());
                expr2.Add(expr);
                expr2.Add(ExpressionUnary());
                expr =expr2;
            }
            return expr;

        }

        public Node ExpressionUnary () {
            dynamic resultado = null;
            if (FirstOfExprUnary.Contains (CurrentToken)) {
                  switch (CurrentToken) {
                    case TokenType.SUM:
                        resultado = (new Sum(){ AnchorToken = Expect(TokenType.SUM)});
                        break;
                    case TokenType.NOT:
                        //Expect (TokenType.NOT);
                        resultado = (new Not() { AnchorToken = Expect(TokenType.NOT) });
                        break;
                    case TokenType.SUB:
                        resultado = (new Sub() { AnchorToken = Expect(TokenType.SUB) });
                        break;
                    default:
                        break;

                }
            }
            dynamic resultado2 = null;
            switch (CurrentToken) {
                case TokenType.IDENTIFIER:
                    var identifier = Expect (TokenType.IDENTIFIER);
                    if (CurrentToken == TokenType.PARENTHESIS_OPEN) {
                        resultado2 = (FunCall ());
                        resultado2.AnchorToken = identifier;
                    }else{
                        resultado2 = (new Node(){AnchorToken = identifier});
                    }
                    break;
                case TokenType.ARR_BEGIN:
                    resultado2 = (Array());
                    break;
                case TokenType.VAR_CHAR:
                case TokenType.VAR_INT:
                case TokenType.VAR_STRING:
                    resultado2 = (Literal ());
                    break;
                case TokenType.TRUE:
                    resultado2 =  new OperatorBool();
                    resultado2.AnchorToken = Expect (TokenType.TRUE);
                    break;
                default:
                    throw new SyntaxError (firstOfSimpleExpression, tokenStream.Current);

            }
            if(resultado == null){
                return resultado2;
            }
            resultado.Add(resultado2);
            return resultado;
        }
        public Node Array () {
            // No estoy muy seguro como se hace este.
            // Ayura.
            Expect (TokenType.ARR_BEGIN);
            Array n1 = new Array();
            if (TokenType.ARR_END != CurrentToken) {
                n1.Add(Expression ());
                while (TokenType.LIST == CurrentToken) {
                    Expect (TokenType.LIST);
                    n1.Add(Expression ());
                }
            }
            Expect (TokenType.ARR_END);
            return n1;
        }
        public Node Literal () {
            switch (CurrentToken) {
                case TokenType.VAR_INT:
                    return new VarInt () { AnchorToken = Expect (TokenType.VAR_INT) };
                case TokenType.VAR_CHAR:
                    return new VarChar () { AnchorToken = Expect (TokenType.VAR_CHAR) };
                case TokenType.VAR_STRING:
                    return new VarString () { AnchorToken = Expect (TokenType.VAR_STRING) };
                default:
                    throw new SyntaxError (CurrentToken, tokenStream.Current);
            }
        }

        public Node Operator () {
            // Tenemos aqui que regresar algo con el operador de enmedio.
            dynamic oper = null;
            if (firstOfOperatorBool.Contains (CurrentToken)) {
                oper = OperatorBool ();
            } else if (firstOfOperatorComp.Contains (CurrentToken)) {
                oper = OperatorComp ();
            } else if (firstOfOperatorMath.Contains (CurrentToken)) {
                oper = OperatorMath ();
            }
            return oper;
        }
        public Node OperatorBool () {
            switch (CurrentToken) {
                case TokenType.OR:
                    return new Or () { AnchorToken = Expect (TokenType.OR) };
                case TokenType.AND:
                    return new And () { AnchorToken = Expect (TokenType.AND) };
                default:
                    throw new SyntaxError (CurrentToken, tokenStream.Current);
            }
        }
        public Node OperatorComp () {
            switch (CurrentToken) {
                case TokenType.GT:
                    return new Gt () { AnchorToken = Expect (TokenType.GT) };

                case TokenType.GOET:
                    return new Goet () { AnchorToken = Expect (TokenType.GOET) };

                case TokenType.LT:
                    return new Lt () { AnchorToken = Expect (TokenType.LT) };

                case TokenType.LOET:
                    return new Loet () { AnchorToken = Expect (TokenType.LOET) };

                case TokenType.EQUALS:
                    return new Equals () { AnchorToken = Expect (TokenType.EQUALS) };

                case TokenType.NOT_EQUALS:
                    return new Not_Equals () { AnchorToken = Expect (TokenType.NOT_EQUALS) };

                default:
                    throw new SyntaxError (CurrentToken, tokenStream.Current);
            }

        }
        public Node OperatorMath () {
            switch (CurrentToken) {
                case TokenType.SUM:
                    return new Sum () {
                        AnchorToken = Expect (TokenType.SUM)
                    };
                case TokenType.SUB:
                    return new Sub () {
                        AnchorToken = Expect (TokenType.SUB)
                    };
                case TokenType.DIV:
                    return new Div () {
                        AnchorToken = Expect (TokenType.DIV)
                    };
                case TokenType.MUL:
                    return new Mul () {
                        AnchorToken = Expect (TokenType.MUL)
                    };
                case TokenType.MOD:
                    return new Mod () {
                        AnchorToken = Expect (TokenType.MOD)
                    };
                default:
                    throw new SyntaxError (CurrentToken, tokenStream.Current);
            }

        }

    }
}