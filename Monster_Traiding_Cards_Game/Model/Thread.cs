using System;
using System.Security;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Monster_Traiding_Cards.Base;
using Monster_Traiding_Cards.Repositories;
using Monster_Traiding_Cards.Security;  

namespace Monster_Traiding_Cards.Model
{
    /// <summary>This class represents a thread.</summary>
    public sealed class Thread : Atom, IAtom, __IAtom
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private static members                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>User repository.</summary>
        private static ThreadRepository _Repository = new();



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                                     //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new instance of this class.</summary>
        public Thread()
        { }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the thread ID.</summary>
        public int ID
        {
            get { return (int?)_InternalID ?? -1; }
        }


        /// <summary>Gets or sets the thread title.</summary>
        public string Title
        {
            get; set;
        } = string.Empty;


        /// <summary>Gets or sets the thread time.</summary>
        public DateTime Time
        {
            get; internal set;
        } = DateTime.Now;


        /// <summary>Gets the thread owner user name.</summary>
        public string Owner
        {
            get; internal set;
        } = string.Empty;


        /// <summary>Gets the entries for this thread.</summary>
        public IEnumerable<Entry> Entries
        {
            get { return Entry.For(this); }
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public static methods                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets a thread by its ID.</summary>
        /// <param name="id">ID.</param>
        /// <returns>Returns the thread with the given ID.</returns>
        public static Thread ByID(int id)
        {
            return _Repository.Get(id);
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // [override] Atom                                                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Deletes the object.</summary>
        public override void Delete()
        {
            if (!(_EditingSession?.Valid ?? false)) { throw new SecurityException("Required authentication."); }
            if (!(_EditingSession.Is(Owner) || _EditingSession.IsAdmin))
            {
                throw new SecurityException("Only owner or admins can delete thread.");
            }

            _Repository.Delete(this);
        }


        /// <summary>Saves the object.</summary>
        public override void Save()
        {
            if (!(_EditingSession?.Valid ?? false)) { throw new SecurityException("Required authentication."); }

            if (_InternalID is null)
            {
                Owner = _EditingSession!.User!.UserName;
            }
            else if (!(_EditingSession.Is(Owner) || _EditingSession.IsAdmin))
            {
                throw new SecurityException("Only owner or admins can edit thread.");
            }

            _Repository.Save(this);
        }


        /// <summary>Refrehes the object.</summary>
        public override void Refresh()
        {
            _Repository.Refresh(this);
        }
    }
}
