using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tpbc.Web.Application.DomainModel.Impl
{
    public class InMemoryMemberRepository : IMemberRepository
    {
        private static readonly IList<Member> Data = new List<Member>();

        public Member GetByUserName(string userName)
        {
            return this.Single(m => userName.Equals(m.UserName, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerator<Member> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) Data).GetEnumerator();
        }

        public void Add(Member item)
        {
            Data.Add(item);
        }

        public void Clear()
        {
            Data.Clear();
        }

        public bool Contains(Member item)
        {
            return Data.Contains(item);
        }

        public void CopyTo(Member[] array, int arrayIndex)
        {
            Data.CopyTo(array, arrayIndex);
        }

        public bool Remove(Member item)
        {
            return Data.Remove(item);
        }

        public int Count => Data.Count;

        public bool IsReadOnly => Data.IsReadOnly;
    }
}