using System.Runtime.Caching;

namespace PokerOdds.Web.OWIN.Interfaces
{
    interface IHasCache
    {
        ObjectCache Cache { get; set; }
    }
}
