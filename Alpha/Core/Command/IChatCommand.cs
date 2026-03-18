using System;

namespace Alpha.Core.Command
{
    public interface IChatCommand : IComparable<IChatCommand>
    {
        string Name { get; }
        string ShortName => "";
        string Description { get; }
        
        // for help text categorization, should be the mod name or feature name that owns this command    
        string Namespace { get; } 
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
