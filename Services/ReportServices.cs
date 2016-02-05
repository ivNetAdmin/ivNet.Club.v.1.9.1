
using System;
using System.Collections.Generic;
using System.Linq;
using ivNet.Club.Entities;
using ivNet.Club.Helpers;
using ivNet.Club.Models.View;
using Orchard;
using Orchard.Security;
using LogLevel = Orchard.Logging.LogLevel;

namespace ivNet.Club.Services
{
    public interface IReportServices : IDependency
    {
        ReportDetail GetReport(int id);
        void UpdateReport(int id, string reportHtml);
    }

    public class ReportServices : BaseService, IReportServices
    {
        private IConfigServices _configServices;

        public ReportServices(IAuthenticationService authenticationService, IConfigServices configServices)
            : base(authenticationService)
        {
            _configServices = configServices;
        }

        public ReportDetail GetReport(int id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {

                var fixture = session.CreateCriteria(typeof(Fixture))
                   .List<Fixture>()
                   .FirstOrDefault(x => x.Id.Equals(id));

                if (fixture == null) return null;

                var report = session.CreateCriteria(typeof (Report))
                    .List<Report>()
                    .FirstOrDefault(x => x.Fixture.Id.Equals(id));

                if (report == null)
                    return new ReportDetail
                    {
                        FixtureId = id,
                        DatePlayed = fixture.DatePlayed,
                        HomeTeam = fixture.HomeTeam == null ? "unknown" : fixture.HomeTeam.Name,
                        Opposition = fixture.Opposition == null ? "unknown" : fixture.Opposition.Name,
                        Result = fixture.Result,
                        ResultType = fixture.ResultType == null ? "unknown" : fixture.ResultType.Name,
                        ResultTypeList = _configServices.GetResultTypeList()
                    };

                var reportDetail = new ReportDetail
                {
                    FixtureId = id,
                    Html = report.Html,
                    DatePlayed = fixture.DatePlayed,
                    HomeTeam = fixture.HomeTeam == null ? "unknown" : fixture.HomeTeam.Name,
                    Opposition = fixture.Opposition == null ? "unknown" : fixture.Opposition.Name,
                    Result = fixture.Result,
                    ResultType = fixture.ResultType == null ? "unknown" : fixture.ResultType.Name,
                    ResultTypeList = _configServices.GetResultTypeList()
                };

                return reportDetail;
            }
        }

        public void UpdateReport(int id, string reportHtml)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var report = session.CreateCriteria(typeof (Report))
                            .List<Report>()
                            .FirstOrDefault(x => x.Fixture.Id.Equals(id)) ?? new Report();

                        if (report.Id==0)
                        {
                            report.Fixture = session.CreateCriteria(typeof(Fixture))
                                .List<Fixture>()
                                .FirstOrDefault(x => x.Id.Equals(id));
                        }

                        report.Html = reportHtml;

                        SetAudit(report);
                        session.SaveOrUpdate(report);

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
    }
}