using System.Collections.Generic;

namespace Tpbc.Web.Application.DomainModel
{
    public interface IMemberRepository : ICollection<Member>
    {
        Member GetByUserName(string userName);
    }
}