using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Step;

namespace VErp.Services.Manafacturing.Service.Step
{
    public interface IStepGroupService
    {
        //Step Group
        public Task<int> CreateStepGroup(StepGroupModel req);
        public Task<bool> UpdateStepGroup(int stepGroupId, StepGroupModel req);
        public Task<bool> DeleteStepGroup(int stepGroupId);
        public Task<PageData<StepGroupModel>> GetListStepGroup(string keyWord, int page, int size);
    }
}
