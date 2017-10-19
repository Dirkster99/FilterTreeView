namespace FilterTreeViewLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IHasDummyChild
    {

        /// <summary>
        /// Determines whether this item has a dummy child below or not.
        /// </summary>
        bool HasDummyChild { get; }
    }
}
