using System;
using System.Collections.Generic;
using DnDUtil;

namespace DnDUtil.Core.Commands
{
    public class RollCommand : DndRollCommand
    {
        public override string Name => "roll";
        public override string ShortName => "r";
    }
}
