using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VErp.Commons.Enums.Organization;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Leave
{

    public class LeaveConfigListModel : IMapFrom<LeaveConfig>
    {
        public int? LeaveConfigId { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(128)]
        public string Title { get; set; }

        [MaxLength(1024)]
        public string Description { get; set; }
        public int AdvanceDays { get; set; }
        public decimal? MonthRate { get; set; }
        public int? MaxAyear { get; set; }
        public int? SeniorityMonthsStart { get; set; }
        public int? SeniorityOneYearRate { get; set; }
        public int? OldYearTransferMax { get; set; }
        public long? OldYearAppliedToDate { get; set; }
        public bool IsDefault { get; set; }

        //public void CreateMapping<T>(Profile profile) where T : LeaveConfigListModel =>
        //  profile.CreateMap<T, LeaveConfig>()
        //  .ForMember(d => d.OldYearAppliedToDate, s => s.MapFrom(f => f.OldYearAppliedToDate.UnixToDateTime()))
        //  .ReverseMap()
        //  .ForMember(d => d.OldYearAppliedToDate, s => s.MapFrom(f => f.OldYearAppliedToDate.GetUnix()));

        //public virtual void Mapping(Profile profile) => CreateMapping<LeaveConfigListModel>(profile);
    }

    public class LeaveConfigModel : LeaveConfigListModel, IMapFrom<LeaveConfig>
    {
        private IList<LeaveConfigRoleModel> _roles;

        public IList<LeaveConfigRoleModel> Roles
        {
            get
            {
                return _roles?.GroupBy(r => r.LeaveRoleTypeId)?.Select(g => g.First())?.ToList();

            }
            set
            {
                _roles = value;
            }
        }

        private IList<LeaveConfigSeniorityModel> _seniorities { get; set; }


        public IList<LeaveConfigSeniorityModel> Seniorities
        {
            get
            {
                return _seniorities?.GroupBy(s => s.Months)?.Select(g => g.First())?.ToList();

            }
            set
            {
                _seniorities = value;
            }
        }


        private IList<LeaveConfigValidationModel> _validations { get; set; }

        public IList<LeaveConfigValidationModel> Validations
        {
            get
            {
                return _validations?.GroupBy(s => s.TotalDays)?.Select(g => g.First())?.ToList();

            }
            set
            {
                _validations = value;
            }
        }

        //public override void Mapping(Profile profile) => CreateMapping<LeaveConfigModel>(profile);
    }

    public class LeaveConfigRoleUserModel : IMapFrom<LeaveConfigRole>
    {
        public int UserId { get; set; }
        public EnumLeaveRoleType LeaveRoleTypeId { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<LeaveConfigRoleUserModel, LeaveConfigRole>()
                .ForMember(d => d.LeaveRoleTypeId, s => s.MapFrom(m => (int)m.LeaveRoleTypeId))
                .ReverseMap()
                .ForMember(d => d.LeaveRoleTypeId, s => s.MapFrom(m => (EnumLeaveRoleType)m.LeaveRoleTypeId));
        }

    }

    public class LeaveConfigRoleModel
    {
        public IList<int> UserIds { get; set; }
        public EnumLeaveRoleType LeaveRoleTypeId { get; set; }

        public IList<LeaveConfigRoleUserModel> ToRoleUserModel()
        {
            return UserIds?.Distinct()?.Select(u => new LeaveConfigRoleUserModel
            {
                LeaveRoleTypeId = this.LeaveRoleTypeId,
                UserId = u
            }).ToList();
        }

    }


    public class LeaveConfigSeniorityModel : IMapFrom<LeaveConfigSeniority>
    {
        public int Months { get; set; }
        public int AdditionDays { get; set; }
    }

    public class LeaveConfigValidationModel : IMapFrom<LeaveConfigValidation>
    {
        public int TotalDays { get; set; }
        public int? MinDaysFromCreateToStart { get; set; }
        public bool IsWarning { get; set; }
    }
}
