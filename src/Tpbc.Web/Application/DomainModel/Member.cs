namespace Tpbc.Web.Application.DomainModel
{
    public class Member
    {
        public string UserName { get; }
        public string FullName { get; set; }

        public Member(string userName,string fullName)
        {
            UserName = userName;
            FullName = fullName;
        }
    }
}