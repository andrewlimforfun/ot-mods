namespace DnDUtil.Core.Commands
{
    public interface IChatCommand
    {
        string Name { get; }
        string Description { get; }
    
        void Execute(string[] args);

        // tostring
        string ToString()
        {
            return Name;
        }
    }
}
