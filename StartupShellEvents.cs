
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using ivNet.Club.Entities;
using ivNet.Club.Enums;
using ivNet.Club.Helpers;
using ivNet.Club.Models.DataLoad;
using ivNet.Club.Models.View;
using ivNet.Club.Models.View.Stats;
using Orchard.Environment;
using Orchard.Localization.Services;

namespace ivNet.Club
{
    public class StartupShellEvents : IOrchardShellEvents
    {
        public void Activated()
        {

            #region models->entities

            Mapper.CreateMap<MemberDetailModel, Member>()
                .ForMember(v => v.Address, e => e.Ignore())
                .ForMember(v => v.MemberKey,
                    m => m.MapFrom(e => CustomStringHelper.BuildKey(new[] { e.Lastname, e.Firstname })));

            Mapper.CreateMap<MemberDetailModel, Contact>()
                .ForMember(v => v.ContactKey,
                    m => m.MapFrom(e => CustomStringHelper.BuildKey(new[] { e.Email })));

            Mapper.CreateMap<MemberDetailModel, Address>()
                .ForMember(v => v.AddressKey,
                    m => m.MapFrom(e => CustomStringHelper.BuildKey(new[] { e.Address, e.Postcode })))
                .ForMember(v => v.AddressLine,
                    m => m.MapFrom(e => e.Address));

            #endregion

            #region entities->models

            Mapper.CreateMap<Player, PlayerDetail>()
                .ForMember(v => v.Name,
                    m => m.MapFrom(e => string.Format("{0}, {1}", e.Member.Lastname, e.Member.Firstname)))
                .ForMember(v => v.MemberId,
                    m => m.MapFrom(e => e.Member.Id))
                .ForMember(v => v.Dob,
                    m => m.MapFrom(e => e.Member.Dob))
                .ForMember(v => v.AgeGroup,
                    m => m.MapFrom(e => getAgeGroup(e.Member.Dob)));

            Mapper.CreateMap<BattingStat, Batting>()
                .ForMember(v => v.Name,
                    m => m.MapFrom(e => string.Format("{0}, {1}", e.Player.Member.Lastname, e.Player.Member.Firstname)))
                .ForMember(v => v.PlayerId,
                    m => m.MapFrom(e => e.Player.Id))
                .ForMember(v => v.HowOut,
                    m => m.MapFrom(e => e.HowOut == null ? string.Empty : e.HowOut.Name));

            Mapper.CreateMap<BowlingStat, Bowling>()
              .ForMember(v => v.Name,
                  m => m.MapFrom(e => string.Format("{0}, {1}", e.Player.Member.Lastname, e.Player.Member.Firstname)))
                   .ForMember(v => v.PlayerId,
                  m => m.MapFrom(e => e.Player.Id))
                   .ForMember(v => v.OversDec,
                  m => m.MapFrom(e => e.Overs));

            Mapper.CreateMap<FieldingStat, Fielding>()
             .ForMember(v => v.Name,
                 m => m.MapFrom(e => string.Format("{0}, {1}", e.Player.Member.Lastname, e.Player.Member.Firstname)))
                  .ForMember(v => v.PlayerId,
                 m => m.MapFrom(e => e.Player.Id));

            Mapper.CreateMap<Fixture, FixtureDetail>()
                .ForMember(v => v.FixtureId,
                    m => m.MapFrom(e => e.Id))
                .ForMember(v => v.HomeTeam,
                    m => m.MapFrom(e => e.HomeTeam == null ? "unknown" : e.HomeTeam.Name))
                .ForMember(v => v.ResultType,
                    m => m.MapFrom(e => e.ResultType == null ? "unknown" : e.ResultType.Name))
                .ForMember(v => v.Opposition,
                    m => m.MapFrom(e => e.Opposition == null ? "unknown" : e.Opposition.Name));

            Mapper.CreateMap<Member, ClubMemberModel>()
                .ForMember(v => v.MemberId,
                    m => m.MapFrom(e => e.Id))
                .ForMember(v => v.Email,
                    m => m.MapFrom(e => e.Contact.Email))
                .ForMember(v => v.Phone,
                    m => m.MapFrom(e => e.Contact.Phone))
                .ForMember(v => v.AddressLine,
                    m => m.MapFrom(e => e.Address.AddressLine))
                .ForMember(v => v.Postcode,
                    m => m.MapFrom(e => e.Address.Postcode));

            Mapper.CreateMap<Member, MemberModel>()
                .ForMember(v => v.MemberId,
                    m => m.MapFrom(e => e.Id))
                     .ForMember(v => v.Dob,
                 m => m.MapFrom(e => FormatDate(e.Dob)))
                .ForMember(v => v.Name,
                    m => m.MapFrom(e => string.Format("{0}, {1}", e.Lastname, e.Firstname)))
                     .ForMember(v => v.MemberTypeList,
                    m => m.MapFrom(e => GetMemberTypeList(e.Types)));

            Mapper.CreateMap<Member, MemberVettingModel>()
                .ForMember(v => v.MemberId,
                    m => m.MapFrom(e => e.Id))
                     .ForMember(v => v.CreateDate,
                 m => m.MapFrom(e => FormatDate(e.CreateDate)))
                .ForMember(v => v.Name,
                    m => m.MapFrom(e => string.Format("{0}, {1}", e.Lastname, e.Firstname)));            

            Mapper.CreateMap<Player, MemberModel>()
                .ForMember(v => v.MemberId,
                 m => m.MapFrom(e => e.Member.Id))
                  .ForMember(v => v.Dob,
                 m => m.MapFrom(e => FormatDate(e.Member.Dob)))
                .ForMember(v => v.Name,
                    m => m.MapFrom(e => string.Format("{0}, {1}", e.Member.Lastname, e.Member.Firstname)))
                .ForMember(v => v.PlayerType,
                    m => m.MapFrom(e => e.PlayerType == (int) PlayerType.Junior ? "J" : "S"));

            Mapper.CreateMap<Guardian, MemberModel>()
                .ForMember(v => v.MemberId,
                 m => m.MapFrom(e => e.Member.Id))
                .ForMember(v => v.Name,
                    m => m.MapFrom(e => string.Format("{0}, {1}", e.Member.Lastname, e.Member.Firstname)));

            Mapper.CreateMap<Player, Ward>()
                .ForMember(v => v.MemberId,
                    m => m.MapFrom(e => e.Member.Id))
                .ForMember(v => v.Dob,
                    m => m.MapFrom(e => e.Member.Dob))
                .ForMember(v => v.Nickname,
                    m => m.MapFrom(e => e.Nickname))
                .ForMember(v => v.Firstname,
                    m => m.MapFrom(e => e.Member.Firstname))
                .ForMember(v => v.Lastname,
                    m => m.MapFrom(e => e.Member.Lastname));

            #endregion

            #region models->models

            Mapper.CreateMap<Registration, ClubMemberModel>()
                .ForMember(v => v.AddressLine,
                    m => m.MapFrom(e => e.Address));
            
            #endregion

            #region data load models->entities

            Mapper.CreateMap<DLMember, Member>()
               .ForMember(v => v.MemberKey,
                   m => m.MapFrom(e => CustomStringHelper.BuildKey(new[] { e.Lastname, e.Firstname })));

            #endregion        
        }

        private string getAgeGroup(DateTime? dob)
        {
            if (dob == null) return string.Empty;

            // calculate the age on next 31st August 
            var days = 0; // number of days old on 31st August

            var today = DateTime.Now;
            // is 31st August in the next year
            var month = today.Month;

            if (month <= 8)
            {
                //31st August is this year
                days = (new DateTime(today.Year, 8, 31) - today).Days;
            }
            else
            {
                //31st August is next year
                days = (new DateTime(today.Year + 1, 8, 31) - today).Days;
            }
            var age = dob.GetValueOrDefault().AddDays(days);
            var ageGroup = today.Year - age.Year;
            
            if (ageGroup > 18) return string.Empty;
            
            return "U" + ageGroup.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
        }

        private string FormatDate(DateTime? dob)
        {
            if (dob == null) return "";
            return ((DateTime) dob).ToShortDateString();
        }

        private string GetMemberTypeList(IEnumerable<MemberType> types)
        {
            var memberTypeList = types.Select(memberType => memberType.Name).ToList();

            return string.Join(",", memberTypeList.OrderBy(m => m));
        }

        public void Terminating()
        {

        }
    }
}