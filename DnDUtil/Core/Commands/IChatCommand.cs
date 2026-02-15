using System;

namespace DnDUtil.Core.Commands
{
    public interface IChatCommand : IComparable<IChatCommand>
    {
        string Name { get; }
        string ShortName => "";
        string Description { get; }
    
        void Execute(string[] args);

        // tostring
        string ToString()
        {
            return Name;
        }

        int IComparable<IChatCommand>.CompareTo(IChatCommand? other)
        {
            if (other == null) return 1;
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }
    }
}
