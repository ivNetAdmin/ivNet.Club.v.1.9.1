
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Remoting.Messaging;
using System.Text;
using AutoMapper;
using FluentNHibernate.Testing.Values;
using ivNet.Club.Controllers.api;
using ivNet.Club.Entities;
using ivNet.Club.Enums;
using ivNet.Club.Helpers;
using ivNet.Club.Models.View;
using ivNet.Mail.Services;
using NHibernate;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;
using System.Linq;
using LogLevel = Orchard.Logging.LogLevel;

namespace ivNet.Club.Services
{

    public interface IMemberServices : IDependency
    {
        int Register(MemberDetailModel registration);
        Contact GetContactByEmail(string email);
        void SetMemberType(MemberDetailModel model, string memberTypeName);
        void DataLoadAddGuardian(string juniorKey, string guardianKey);
        List<MemberModel> GetMembers(Dictionary<string, string> paramCollection);
        void Activate(int id, string type, bool activate);
        ClubMemberModel GetMember(int id);
        void RemoveWard(int guardianId, int wardId);
        Ward GetWard(string lastname, string firstname);
        int SaveMember(ClubMemberModel model);
        Member GetMemberByName(string firstname, string lastname);
        Member GetMemberByUserId(int id);
        List<MemberVettingModel> GetNewRegistrations();
        void CreateWebAccount(int id);
    }

    public class MemberServices : BaseService, IMemberServices
    {
        private readonly IRoleService _roleService;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        private readonly IMembershipService _membershipService;
        private readonly IEmailServices _emailServices;


        public MemberServices(IAuthenticationService authenticationService,
            IEmailServices emailServices,
            IMembershipService membershipService, 
            IRoleService roleService,
            IRepository<UserRolesPartRecord> userRolesRepository)
            : base(authenticationService)
        {
            _emailServices = emailServices;
            _roleService = roleService;
            _userRolesRepository = userRolesRepository;
            _membershipService = membershipService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public int Register(MemberDetailModel model)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        // check if member already exists
                        var member = CheckMemberExists(session, model);

                        var address = CheckAddressExists(session, model);
                        var contact = CheckContactExists(session, model);

                        if (contact != null)
                        {
                            SetAudit(contact);
                            session.SaveOrUpdate(contact);
                        }

                        if (address != null)
                        {
                            SetAudit(address);
                            session.SaveOrUpdate(address);
                        }

                        member.Contact = contact;
                        member.Address = address;

                        SetAudit(member);
                        session.SaveOrUpdate(member);

                        transaction.Commit();
                        return member.Id;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                        return 0;
                    }


                    // var registrationSubject = ConfigurationManager.AppSettings.Get("RegistrationSubject");

                    // _emailServices.SendActivationEmail(model.Email, model.Password, registrationSubject);

                }
            }
        }

        public Contact GetContactByEmail(string email)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var contactKey = CustomStringHelper.BuildKey(new[] {email});
                return session.CreateCriteria(typeof (Contact))
                    .List<Contact>().FirstOrDefault(x => x.ContactKey.Equals(contactKey));
            }
        }

        public Member GetMemberByUserId(int id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                
                var member = session.CreateCriteria(typeof(Member))
                    .List<Member>().FirstOrDefault(x => x.UserId.Equals(id));

                return member;
            }
        }

        public Ward GetWard(string lastname, string firstname)
        {
            var memberKey = CustomStringHelper.BuildKey(new[] {lastname, firstname});
            using (var session = NHibernateHelper.OpenSession())
            {
                var ward = session.CreateCriteria(typeof (Player))
                    .List<Player>().FirstOrDefault(x => x.Member.MemberKey.Equals(memberKey));

                return new Ward
                {
                    MemberId = ward == null ? 0 : ward.Member.Id,
                    Lastname = ward == null ? lastname : ward.Member.Lastname,
                    Firstname = ward == null ? firstname : ward.Member.Firstname,
                    Dob = ward == null ? (DateTime?) null : ward.Member.Dob,
                    Nickname = ward == null ? string.Empty : ward.Nickname,
                    GuardianList = ward == null ? string.Empty : GetGuardianList(ward)
                };
            }
        }

        public int SaveMember(ClubMemberModel model)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        // get member
                        var member = session.CreateCriteria(typeof (Member))
                            .List<Member>().FirstOrDefault(x => x.Id.Equals(model.MemberId)) ?? new Member();

                        SetName(session, member, model);

                        // deal accordingly with what sort (type) of member they are
                        foreach (var memberType in member.Types)
                        {
                            switch (memberType.Name)
                            {
                                case "Guardian":
                                    // update wards
                                    var guardian = session.CreateCriteria(typeof (Guardian))
                                        .List<Guardian>().FirstOrDefault(x => x.Member.Id.Equals(model.MemberId));
                                    
                                    // remove current ward list
                                    if (guardian != null)
                                    {
                                        SetAddress(session, member, model);
                                        SetContact(session, member, model);                                        
                                        SetWards(session, guardian, model.Wards);                                       

                                        SetAudit(guardian);
                                        session.SaveOrUpdate(guardian);
                                    }

                                    break;
                                case "Player":
                                    var player = session.CreateCriteria(typeof(Player))
                                           .List<Player>()
                                           .FirstOrDefault(x => x.Member.Id.Equals(member.Id));

                                    if (player != null)
                                    {
                                        // junior or senior
                                        if (player.PlayerType == (int) PlayerType.Senior)
                                        {
                                            SetAddress(session, member, model);
                                            SetContact(session, member, model);
                                        }

                                        // set nickname
                                        player.Nickname = model.Nickname;
                                        SetAudit(player);
                                        session.SaveOrUpdate(player);
                                    }
                                    break;
                                case "Official":
                                    break;
                            }
                        }
                        transaction.Commit();
                        return member.Id;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                        return 0;
                    }
                }
            }
        }

        public void SetMemberType(MemberDetailModel model, string memberTypeName)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var memberType = session.CreateCriteria(typeof (MemberType))
                            .List<MemberType>().FirstOrDefault(x => x.Name.Equals(memberTypeName));

                        var member = CheckMemberExists(session, model);

                        member.AddMemberType(memberType);

                        SetAudit(member);
                        session.SaveOrUpdate(member);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                    }
                }
            }
        }

        public void DataLoadAddGuardian(string juniorKey, string guardianKey)
        {

            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        //check if junior exists
                        var junior = CheckPlayerExists(session, juniorKey);
                        junior.PlayerType = (int) PlayerType.Junior;

                        if (junior.Id == 0)
                        {
                            SetAudit(junior);
                            session.SaveOrUpdate(junior);
                        }

                        //check if guardian exists
                        var guardian = CheckGuardianExists(session, guardianKey);
                        guardian.AddWard(junior);

                        SetAudit(guardian);
                        session.SaveOrUpdate(guardian);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();
                    }
                }
            }
        }

        public List<MemberModel> GetMembers(Dictionary<string, string> paramCollection)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var memberList = new List<MemberModel>();

                if (paramCollection.ContainsKey("type"))
                {
                    switch (paramCollection["type"])
                    {
                        case "Player":
                            IOrderedEnumerable<Player> players;
                            if (paramCollection["active"] == "active")
                            {
                                players = session.CreateCriteria(typeof (Player))
                                    .List<Player>().Where(x => x.IsActive.Equals(1)).OrderBy(x => x.Member.MemberKey);
                            }
                            else if (paramCollection["active"] == "inactive")
                            {
                                players = session.CreateCriteria(typeof (Player))
                                    .List<Player>().Where(x => x.IsActive.Equals(0)).OrderBy(x => x.Member.MemberKey);
                            }
                            else
                            {
                                players = session.CreateCriteria(typeof (Player))
                                    .List<Player>().OrderBy(x => x.Member.MemberKey);
                            }

                            memberList.AddRange(players.Select(player => Mapper.Map(player, new MemberModel())));
                            break;
                        case "Guardian":
                            IOrderedEnumerable<Guardian> guardians;
                            if (paramCollection["active"] == "active")
                            {
                                guardians = session.CreateCriteria(typeof (Guardian))
                                    .List<Guardian>().Where(x => x.IsActive.Equals(1)).OrderBy(x => x.Member.MemberKey);
                            }
                            else if (paramCollection["active"] == "inactive")
                            {
                                guardians = session.CreateCriteria(typeof (Guardian))
                                    .List<Guardian>().Where(x => x.IsActive.Equals(0)).OrderBy(x => x.Member.MemberKey);
                            }
                            else
                            {
                                guardians = session.CreateCriteria(typeof (Guardian))
                                    .List<Guardian>().OrderBy(x => x.Member.MemberKey);
                            }

                            foreach (var guardian in guardians)
                            {
                                memberList.Add(Mapper.Map(guardian, new MemberModel()));
                                var wardList = guardian.Wards.Select(
                                    player => string.Format("{0}, {1}",
                                        player.Member.Lastname, player.Member.Firstname)).ToList();
                                memberList.Last().WardList = string.Join("; ", wardList);
                            }

                            break;
                    }

                }
                else if (paramCollection.ContainsKey("member"))
                {
                    IOrderedEnumerable<Member> members;
                    if (paramCollection["active"] == "active")
                    {
                        members = session.CreateCriteria(typeof (Member))
                            .List<Member>().Where(x => x.IsActive.Equals(1)
                                                       && x.MemberKey.Contains(paramCollection["member"].ToLowerInvariant()))
                            .OrderBy(x => x.MemberKey);
                    }
                    else if (paramCollection["active"] == "inactive")
                    {
                        members = session.CreateCriteria(typeof (Member))
                            .List<Member>().Where(x => x.IsActive.Equals(0)
                                                       && x.MemberKey.Contains(paramCollection["member"].ToLowerInvariant()))
                            .OrderBy(x => x.MemberKey);
                    }
                    else
                    {
                        members = session.CreateCriteria(typeof (Member))
                            .List<Member>().Where(x => x.MemberKey.Contains(paramCollection["member"].ToLowerInvariant()))
                            .OrderBy(x => x.MemberKey);
                    }

                    memberList.AddRange(members.Select(member => Mapper.Map(member, new MemberModel())));
                }
                else
                {
                    IOrderedEnumerable<Member> members;
                    if (paramCollection["active"] == "active")
                    {
                        members = session.CreateCriteria(typeof (Member))
                            .List<Member>().Where(x => x.IsActive.Equals(1)).OrderBy(x => x.MemberKey);

                        //members = session.CreateCriteria(typeof(Member))
                        //  .List<Member>().Where(x => x.Types.Count == 0).OrderBy(x => x.MemberKey); 
                    }
                    else if (paramCollection["active"] == "inactive")
                    {
                        members = session.CreateCriteria(typeof (Member))
                            .List<Member>().Where(x => x.IsActive.Equals(0)).OrderBy(x => x.MemberKey);
                    }
                    else
                    {
                        members = session.CreateCriteria(typeof (Member))
                            .List<Member>().OrderBy(x => x.MemberKey);
                    }

                    memberList.AddRange(members.Select(member => Mapper.Map(member, new MemberModel())));

                }
                return memberList;
            }
        }

        public void Activate(int id, string type, bool activate)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    switch (type)
                    {
                        case "Player":

                            if (!ActivatePlayer(session, id, activate))
                            {
                                transaction.Rollback();
                                return;
                            }
                            break;

                        case "Guardian":

                            if (!ActivateGuardian(session, id, activate))
                            {
                                transaction.Rollback();
                                return;
                            }

                            break;
                        default:

                            var member = session.CreateCriteria(typeof (Member))
                                .List<Member>().FirstOrDefault(x => x.Id.Equals(id));

                            if (!ActivateMember(session, member, activate))
                            {
                                transaction.Rollback();
                                return;
                            }

                            foreach (var memberType in member.Types)
                            {
                                switch (memberType.Name)
                                {
                                    case "Player":
                                        if (!ActivatePlayer(session, member.Id, activate))
                                        {
                                            transaction.Rollback();
                                            return;
                                        }
                                        break;
                                    case "Guardian":
                                        if (!ActivateGuardian(session, member.Id, activate))
                                        {
                                            transaction.Rollback();
                                            return;
                                        }
                                        break;
                                    case "Official":
                                        break;
                                }
                            }
                            break;
                    }

                    transaction.Commit();
                }
            }
        }

        public ClubMemberModel GetMember(int id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                if (id == 0 && CurrentUser != null)
                {
                    // Is User logged in? If so get id          
                    var loggedInMember = session.CreateCriteria(typeof (Member))
                        .List<Member>().FirstOrDefault(x => x.UserId.Equals(CurrentUser.Id));
                    if (loggedInMember != null)
                        id = loggedInMember.Id;
                }

                var member = session.CreateCriteria(typeof (Member))
                        .List<Member>().FirstOrDefault(x => x.Id.Equals(id));
              
                var clubMemberModel = Mapper.Map(member, new ClubMemberModel());
                clubMemberModel.PlayerType = string.Empty;

                foreach (var memberType in member.Types)
                {
                    clubMemberModel.MemberTypList.Add(memberType.Name);
                    switch (memberType.Name)
                    {
                        case "Player":
                            var player = session.CreateCriteria(typeof (Player))
                                .List<Player>().FirstOrDefault(x => x.Member.Id.Equals(id));

                            clubMemberModel.Nickname = player.Nickname;
                            clubMemberModel.PlayerType = player.PlayerType == (int) PlayerType.Senior ? "S" : "J";

                            if (clubMemberModel.PlayerType == "J")
                            {
                                clubMemberModel.AddressLine = string.Empty;
                                clubMemberModel.Postcode = string.Empty;
                                clubMemberModel.Email = string.Empty;
                                clubMemberModel.Phone = string.Empty;
                            }

                            break;
                        case "Guardian":
                            var guardian = session.CreateCriteria(typeof (Guardian))
                                .List<Guardian>().FirstOrDefault(x => x.Member.Id.Equals(id));

                            foreach (var ward in guardian.Wards)
                            {
                                if (ward.IsActive == 1)
                                {
                                    clubMemberModel.Wards.Add(Mapper.Map(ward, new Ward()));
                                }
                            }
                            break;
                        case "Officer":
                            break;
                    }
                }
                
                return clubMemberModel;
            }
        }


        public Member GetMemberByName(string firstname, string lastname)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var memberKey = CustomStringHelper.BuildKey(new[] {lastname, firstname});
                return session.CreateCriteria(typeof (Member))
                    .List<Member>().FirstOrDefault(x => x.MemberKey.Equals(memberKey));
            }
        }

        public List<MemberVettingModel> GetNewRegistrations()
        {
            var memberList = new List<MemberVettingModel>();
            using (var session = NHibernateHelper.OpenSession())
            {
                var members = session.CreateCriteria(typeof (Member))
                    .List<Member>().Where(x => x.IsVetted.Equals(0));

                memberList.AddRange(members.Select(member => Mapper.Map(member, new MemberVettingModel())));
                return memberList;
            }
        }

        public void CreateWebAccount(int id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var member = session.CreateCriteria(typeof (Member))
                            .List<Member>().FirstOrDefault(x => x.Id.Equals(id));

                        if (member != null)
                        {
                            if (_membershipService.GetUser(member.Contact.Email) == null)
                            {

                                var pw = CustomStringHelper.GenerateInitialPassword(member.Firstname);

                                var user = _membershipService.CreateUser(new CreateUserParams(member.Contact.Email,
                                    pw, member.Contact.Email, null, null, false));

                                var roleRecord = _roleService.GetRoleByName("Member");

                                var existingAssociation =
                                    _userRolesRepository.Get(
                                        record => record.UserId == user.Id && record.Role.Id == roleRecord.Id);

                                if (existingAssociation == null)
                                {
                                    _userRolesRepository.Create(new UserRolesPartRecord
                                    {
                                        Role = roleRecord,
                                        UserId = user.Id
                                    });
                                }

                                member.UserId = user.Id;
                                member.IsVetted = 1;
                                member.IsActive = 1;
                                session.SaveOrUpdate(member);

                                var registrationSubject = ConfigurationManager.AppSettings.Get("RegistrationSubject");
                                _emailServices.SendActivationEmail(member.Contact.Email, pw, registrationSubject);

                                Activate(id, "member", true);
                            }
                        }

                        transaction.Commit();

                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();

                    }
                }

            }
        }

        public void RemoveWard(int guardianId, int wardId)
        {

            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var guardian = session.CreateCriteria(typeof (Guardian))
                            .List<Guardian>().FirstOrDefault(x => x.Member.Id.Equals(guardianId));
                        var ward = session.CreateCriteria(typeof (Player))
                            .List<Player>().FirstOrDefault(x => x.Member.Id.Equals(wardId));

                        if (guardian != null && ward != null)
                        {
                            guardian.RemoveWard(ward);
                            session.SaveOrUpdate(guardian);
                        }

                        transaction.Commit();

                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex, string.Empty, null);
                        transaction.Rollback();

                    }
                }
            }
        }

        private void SetName(ISession session, Member member, ClubMemberModel model)
        {
            var memberKey = CustomStringHelper.BuildKey(new[] { model.Lastname, model.Firstname });

            member.MemberKey = memberKey;
            member.Lastname = model.Lastname;
            member.Firstname = model.Firstname;

            SetAudit(member);
            session.SaveOrUpdate(member);
        }

        private void SetAddress(ISession session, Member member, ClubMemberModel model)
        {
            var addressKey = CustomStringHelper.BuildKey(new[] { model.AddressLine, model.Postcode });

            var address = session.CreateCriteria(typeof(Address))
                    .List<Address>()
                    .FirstOrDefault(x => x.AddressKey.Equals(addressKey));

            if (address == null)
            {
                address = new Address
                {
                    AddressKey = addressKey                   
                };              
            }

            address.AddressLine = model.AddressLine;
            address.Postcode = model.Postcode;
            SetAudit(address);
            session.SaveOrUpdate(address);

            member.Address = address;
            SetAudit(member);
            session.SaveOrUpdate(member);
        }

        private void SetContact(ISession session, Member member, ClubMemberModel model)
        {
            var contactKey = CustomStringHelper.BuildKey(new[] { model.Email });

            var contact = session.CreateCriteria(typeof(Contact))
                    .List<Contact>()
                    .FirstOrDefault(x => x.ContactKey.Equals(contactKey));

            if (contact == null)
            {
                contact = new Contact
                {
                    ContactKey = contactKey                   
                };              
            }

            contact.Email = model.Email;
            contact.Phone = model.Phone;

            SetAudit(contact);
            session.SaveOrUpdate(contact);

            member.Contact = contact;
            SetAudit(member);
            session.SaveOrUpdate(member);
        }

        private void SetWards(ISession session, Guardian guardian, List<Ward> wards)
        {
            guardian.Wards.Clear();

            //add new ward list
            foreach (var ward in wards)
            {

                var juniorPlayer = session.CreateCriteria(typeof(Player))
                    .List<Player>()
                    .FirstOrDefault(x => x.Member.Id.Equals(ward.MemberId));

                if (juniorPlayer != null)
                {
                    guardian.AddWard(juniorPlayer);
                }
                else
                {
                    juniorPlayer = new Player();
                    var juniorMember = session.CreateCriteria(typeof(Member))
                        .List<Member>().FirstOrDefault(x => x.Id.Equals(ward.MemberId));

                    if (juniorMember == null)
                    {
                        juniorMember = new Member
                        {
                            Lastname = ward.Lastname,
                            Firstname = ward.Firstname,
                            Types = new List<MemberType>(),
                            MemberKey =
                                CustomStringHelper.BuildKey(new[] { ward.Lastname, ward.Firstname })
                        };
                        var juniorType = session.CreateCriteria(typeof(MemberType))
                            .List<MemberType>().FirstOrDefault(x => x.Name.Equals("Player"));
                        juniorMember.AddMemberType(juniorType);
                    }

                    juniorMember.Dob = ward.Dob;
                    SetAudit(juniorMember);
                    session.SaveOrUpdate(juniorMember);

                    juniorPlayer.Member = juniorMember;
                    juniorPlayer.Nickname = ward.Nickname;
                    juniorPlayer.PlayerType = (int)PlayerType.Junior;
                    juniorPlayer.Guardians = new List<Guardian>();
                    SetAudit(juniorPlayer);
                    session.SaveOrUpdate(juniorPlayer);
                    guardian.AddWard(juniorPlayer);
                }
            }
        }

        private bool ActivateMember(ISession session, Member member, bool activate)
        {
            if (member == null) return false;

            member.IsActive = activate ? (byte) 1 : (byte) 0;
            if (member.IsActive == 1) member.IsVetted = 1;
            SetAudit(member);
            session.SaveOrUpdate(member);

            return true;
        }

        private bool ActivateGuardian(ISession session, int id, bool activate)
        {
            var guardian = session.CreateCriteria(typeof (Guardian))
                .List<Guardian>().FirstOrDefault(x => x.Member.Id.Equals(id));

            if (guardian == null) return false;

            guardian.IsActive = activate ? (byte) 1 : (byte) 0;
            if (guardian.IsActive == 1) guardian.IsVetted = 1;
            SetAudit(guardian);
            session.SaveOrUpdate(guardian);

            if (!activate) return true;

            var member = session.CreateCriteria(typeof (Member))
                .List<Member>().FirstOrDefault(x => x.Id.Equals(id));

            if (member == null) return false;

            if (member.IsActive == 0)
            {
                member.IsActive = 1;          
                SetAudit(member);
                session.SaveOrUpdate(member);
            }

            return true;
        }

        private bool ActivatePlayer(ISession session, int id, bool activate)
        {
            var player = session.CreateCriteria(typeof (Player))
                .List<Player>().FirstOrDefault(x => x.Member.Id.Equals(id));

            if (player == null) return false;

            player.IsActive = activate ? (byte) 1 : (byte) 0;
            if (player.IsActive==1) player.IsVetted = 1;

            SetAudit(player);
            session.SaveOrUpdate(player);

            if (!activate) return true;

            var member = session.CreateCriteria(typeof (Member))
                .List<Member>().FirstOrDefault(x => x.Id.Equals(id));

            if (member == null) return false;

            if (member.IsActive == 0)
            {
                member.IsActive = 1;              
                SetAudit(member);
                session.SaveOrUpdate(member);
            }
            return true;
        }

        private string GetGuardianList(Player ward)
        {
            var list = ward.Guardians.Select(
                guardian =>
                    string.Format("{0} {1}",
                        guardian.Member.Firstname,
                        guardian.Member.Lastname)).ToList();
            return string.Join(", ", list);
        }
    }
}