using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TED;

namespace TotT.Simulog
{
    /// <summary>
    /// Interface for all Event predicates
    /// We have to do this as an interface because the real base class for Event predicates is TablePredicate
    /// and C# doesn't allow multiple inheritance
    /// </summary>
    public interface IEvent
    {
        public TablePredicate ChronicleUntyped { get; }
    }
}
