using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monster_Traiding_Cards.Security;

namespace Monster_Traiding_Cards.Base
{
    /// <summary>Persistent objects implement this interface.</summary>
    /// <remarks>This interface provides internal information about an object.</remarks>
    public interface __IAtom
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets or sets the internal ID of an object.</summary>
        public object? __InternalID { get; set; }

        /// <summary>Gets the editing session for this object.</summary>
        public Session? __EditingSession { get; }
    }
}
