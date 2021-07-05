using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Step;

namespace VErp.Services.Manafacturing.Service.Step
{
    public interface IStepService
    {
        //Step
        public Task<int> CreateStep(StepModel req);
        public Task<bool> UpdateStep(int stepId, StepModel req);
        public Task<bool> DeleteStep(int stepId);
        public Task<StepModel> GetStep(int stepId);
        public Task<PageData<StepModel>> GetListStep(string keyword, int page, int size);

        public Task<IList<StepModel>> GetStepByArrayId(int[] arrayId);
    }
}
