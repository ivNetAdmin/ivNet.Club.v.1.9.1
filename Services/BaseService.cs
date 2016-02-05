using System;
using System.Linq;
using AutoMapper;
using ivNet.Club.Entities;
using ivNet.Club.Enums;
using ivNet.Club.Models.View;
using NHibernate;
using Orchard.Logging;
using Orchard.Security;

namespace ivNet.Club.Services
{
    public class BaseService
    {
        protected readonly IUser CurrentUser;

        public BaseService(IAuthenticationService authenticationService)
        {
            CurrentUser = authenticationService.GetAuthenticatedUser();
            Logger = NullLogger.Instance;
        }

        protected ILogger Logger { get; set; }

        protected Member CheckMemberExists(ISession session, MemberDetailModel model)
        {
            var member = Mapper.Map(model, new Member());
            var existingMember = session.CreateCriteria(typeof(Member))
                .List<Member>().FirstOrDefault(x => x.MemberKey.Equals(member.MemberKey));

            // you cannot change your name
            if (existingMember != null)
            {
                if (existingMember.LegacyId == model.LegacyId) return existingMember;

                //get last cher of firstname check if numerical if so increment it if not add 1
                var duplicateCounterCheck = model.Firstname.Substring(model.Firstname.Length - 1);
                var duplicateCounter = 0;
                int.TryParse(duplicateCounterCheck, out duplicateCounter);
                duplicateCounter = duplicateCounter + 1;
                model.Firstname = string.Format("{0}{1}",
                    model.Firstname.Substring(0, model.Firstname.Length - (duplicateCounter == 1 ? 0 : 1)),
                    duplicateCounter);
                member = CheckMemberExists(session, model);
            }

            return member;
            //return existingMember != null ? Mapper.Map(model, existingMember) : member;
        }

        protected Player CheckPlayerExists(ISession session, string playerKey)
        {
            Player player = null;
            try
            {
                player = session.CreateCriteria(typeof(Player))
                    .List<Player>().FirstOrDefault(x => x.Member.MemberKey.Equals(playerKey));
            }
            catch (NullReferenceException) { }

            if (player != null) return player;

            var member = session.CreateCriteria(typeof(Member))
                .List<Member>().FirstOrDefault(x => x.MemberKey.Equals(playerKey));

            if (member == null) return null;

            player = new Player {Member = member};
            player.Init();

            return player;
        }

        protected Guardian CheckGuardianExists(ISession session, string guardianKey)
        {
            Guardian guardian = null;
            try { 
            guardian = session.CreateCriteria(typeof(Guardian))
               .List<Guardian>().FirstOrDefault(x => x.Member.MemberKey.Equals(guardianKey));

            }
            catch (NullReferenceException) { }

            if (guardian != null) return guardian;

            var member = session.CreateCriteria(typeof(Member))
                .List<Member>().FirstOrDefault(x => x.MemberKey.Equals(guardianKey));

            if (member == null) return null;

            guardian = new Guardian {Member = member};
            guardian.Init();

            return guardian;
        }

        protected Contact CheckContactExists(ISession session, MemberDetailModel model)
        {
            var contact = Mapper.Map(model, new Contact());

            if (string.IsNullOrEmpty(contact.ContactKey)) return null;

            var existingContact = session.CreateCriteria(typeof(Contact))
                .List<Contact>().FirstOrDefault(x => x.ContactKey.Equals(contact.ContactKey));

            return existingContact != null ? Mapper.Map(model, existingContact) : contact;
        }

        protected Address CheckAddressExists(ISession session, MemberDetailModel model)
        {
            var address = Mapper.Map(model, new Address());

            if (string.IsNullOrEmpty(address.AddressKey)) return null;

            var existingAddress = session.CreateCriteria(typeof(Address))
                .List<Address>().FirstOrDefault(x => x.AddressKey.Equals(address.AddressKey));

            return existingAddress != null ? Mapper.Map(model, existingAddress) : address;
        }

        protected void SetAudit(BaseEntity entity, string merge = null)
        {
            var currentUser = CurrentUser != null ? CurrentUser.UserName : "Non-Authenticated";

            entity.ModifiedBy = currentUser;
            entity.ModifiedDate = DateTime.Now;

            if (entity.Id != 0) return;

            entity.IsActive = 1;
            entity.CreatedBy = currentUser;
            entity.CreateDate = DateTime.Now;
        }
    }
}