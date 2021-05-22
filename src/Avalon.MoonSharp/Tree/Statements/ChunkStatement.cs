using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class ChunkStatement : Statement, IClosureBuilder
    {
        private Statement _block;
        private SymbolRef _env;
        private RuntimeScopeFrame _stackFrame;
        private SymbolRef _varArgs;

        public ChunkStatement(ScriptLoadingContext lcontext) : base(lcontext)
        {
            lcontext.Scope.PushFunction(this, true);
            _env = lcontext.Scope.DefineLocal(WellKnownSymbols.ENV);
            _varArgs = lcontext.Scope.DefineLocal(WellKnownSymbols.VARARGS);
            _block = new CompositeStatement(lcontext);

            if (lcontext.Lexer.Current.Type != TokenType.Eof)
            {
                throw new SyntaxErrorException(lcontext.Lexer.Current, "<eof> expected near '{0}'",
                    lcontext.Lexer.Current.Text);
            }

            _stackFrame = lcontext.Scope.PopFunction();
        }

        public SymbolRef CreateUpvalue(BuildTimeScope scope, SymbolRef symbol)
        {
            return null;
        }


        public override void Compile(ByteCode bc)
        {
            var meta = bc.Emit_Meta("<chunk-root>", OpCodeMetadataType.ChunkEntrypoint);
            int metaip = bc.GetJumpPointForLastInstruction();

            bc.Emit_BeginFn(_stackFrame);
            bc.Emit_Args(_varArgs);

            bc.Emit_Load(SymbolRef.Upvalue(WellKnownSymbols.ENV, 0));
            bc.Emit_Store(_env, 0, 0);
            bc.Emit_Pop();

            _block.Compile(bc);
            bc.Emit_Ret(0);

            meta.NumVal = bc.GetJumpPointForLastInstruction() - metaip;
        }
    }
}