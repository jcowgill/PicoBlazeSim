using JCowgill.PicoBlazeSim.Instructions;

namespace JCowgill.PicoBlazeSim
{
    /// <summary>
    /// Interface implemented by instruction visitors
    /// </summary>
    public interface IInstructionVisitor
    {
        void Visit(BinaryConstant instruction);
        void Visit(BinaryRegister instruction);
        void Visit(Shift instruction);
        void Visit(Return instruction);
        void Visit(ReturnInterrupt instruction);
        void Visit(SetInterruptFlag instruction);
        void Visit(SetRegisterBank instruction);
        void Visit(JumpCall instruction);
        void Visit(JumpCallIndirect instruction);
        void Visit(HwBuild instruction);
    }
}
