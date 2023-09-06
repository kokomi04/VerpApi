using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Input;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Voucher;
using VErp.Services.Organization.Model.BusinessInfo;

namespace Services.Organization.Service.BusinessInfo.Implement
{
    public interface IObjectApprovalStepService
    {
        Task<IList<ObjectApprovalStepItemModel>> GetAllObjectApprovalStepItem();
        Task<IList<ObjectApprovalStepModel>> GetObjectApprovalStep(int objectTypeId, int objectId);
        Task<bool> UpdateObjectApprovalStep(ObjectApprovalStepModel model);
    }

    public class ObjectApprovalStepService : IObjectApprovalStepService
    {
        private readonly OrganizationDBContext _organizationContext;
        private readonly IMapper _mapper;
        private readonly IInputPrivateTypeHelperService _inputPrivateTypeHelperService;
        private readonly IInputPublicTypeHelperService _inputPublicTypeHelperService;
        private readonly IVoucherTypeHelperService _voucherTypeHelperService;


        public ObjectApprovalStepService(OrganizationDBContext organizationContext
            , IMapper mapper
            , IInputPrivateTypeHelperService inputPrivateTypeHelperService
            , IInputPublicTypeHelperService inputPublicTypeHelperService
            , IVoucherTypeHelperService voucherTypeHelperService)
        {
            _organizationContext = organizationContext;
            _mapper = mapper;
            _inputPrivateTypeHelperService = inputPrivateTypeHelperService;
            _inputPublicTypeHelperService = inputPublicTypeHelperService;
            _voucherTypeHelperService = voucherTypeHelperService;

            // _objectProcessActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ObjectProcessStep);
        }


        public async Task<IList<ObjectApprovalStepModel>> GetObjectApprovalStep(int objectTypeId, int objectId)
        {
            return await _organizationContext.ObjectApprovalStep.Where(x => x.ObjectTypeId == objectTypeId && x.ObjectId == objectId)
            .ProjectTo<ObjectApprovalStepModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
        }

        public async Task<bool> UpdateObjectApprovalStep(ObjectApprovalStepModel model)
        {
            if (!model.IsEnable)
                await ValidateObjectApprovalStep(model);

            var entity = await _organizationContext.ObjectApprovalStep.FirstOrDefaultAsync(x => x.ObjectTypeId == model.ObjectTypeId && x.ObjectId == model.ObjectId && x.ObjectApprovalStepTypeId == (int)model.ObjectApprovalStepTypeId);

            if (entity == null)
            {
                await CreateObjectApprovalStep(model);
            }
            else
            {
                entity.IsEnable = model.IsEnable;
                entity.ObjectFieldEnable = model.ObjectFieldEnable;
                await _organizationContext.SaveChangesAsync();
            }

            return true;
        }

        private async Task ValidateObjectApprovalStep(ObjectApprovalStepModel model)
        {
            switch (model.ObjectTypeId)
            {
                case (int)EnumObjectType.InputType:
                    await ValidateAccountancyPrivateBill(model.ObjectId, model.ObjectApprovalStepTypeId);
                    break;
                case (int)EnumObjectType.InputTypePublic:
                    await ValidateAccountancyPublicBill(model.ObjectId, model.ObjectApprovalStepTypeId);
                    break;
                case (int)EnumObjectType.VoucherType:
                    await ValidatePurchaseOrderBill(model.ObjectId, model.ObjectApprovalStepTypeId);
                    break;
            }
        }

        private async Task ValidateAccountancyPrivateBill(int inputTypeId, EnumObjectApprovalStepType type)
        {
            if (EnumObjectApprovalStepType.ApprovalStep == type)
            {
                var data = await _inputPrivateTypeHelperService.GetBillNotApprovedYet(inputTypeId);
                if (data.Count > 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không thể tắt bước duyệt chứng từ. Vẫn tồn tại chứng từ trên hệ thống chưa được duyệt");
            }
            else
            {
                var data = await _inputPrivateTypeHelperService.GetBillNotChekedYet(inputTypeId);
                if (data.Count > 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không thể tắt bước kiểm tra chứng từ. Vẫn tồn tại chứng từ trên hệ thống chưa được kiểm tra");
            }
        }

        private async Task ValidateAccountancyPublicBill(int inputTypeId, EnumObjectApprovalStepType type)
        {
            if (EnumObjectApprovalStepType.ApprovalStep == type)
            {
                var data = await _inputPublicTypeHelperService.GetBillNotApprovedYet(inputTypeId);
                if (data.Count > 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không thể tắt bước duyệt chứng từ. Vẫn tồn tại chứng từ trên hệ thống chưa được duyệt");
            }
            else
            {
                var data = await _inputPublicTypeHelperService.GetBillNotChekedYet(inputTypeId);
                if (data.Count > 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không thể tắt bước kiểm tra chứng từ. Vẫn tồn tại chứng từ trên hệ thống chưa được kiểm tra");
            }
        }

        private async Task ValidatePurchaseOrderBill(int voucherTypeId, EnumObjectApprovalStepType type)
        {
            if (EnumObjectApprovalStepType.ApprovalStep == type)
            {
                var data = await _voucherTypeHelperService.GetBillNotApprovedYet(voucherTypeId);
                if (data.Count > 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không thể tắt bước duyệt chứng từ. Vẫn tồn tại chứng từ trên hệ thống chưa được duyệt");
            }
            else
            {
                var data = await _voucherTypeHelperService.GetBillNotChekedYet(voucherTypeId);
                if (data.Count > 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không thể tắt bước kiểm tra chứng từ. Vẫn tồn tại chứng từ trên hệ thống chưa được kiểm tra");
            }
        }

        private async Task<int> CreateObjectApprovalStep(ObjectApprovalStepModel model)
        {
            var entity = _mapper.Map<ObjectApprovalStep>(model);
            await _organizationContext.ObjectApprovalStep.AddAsync(entity);
            await _organizationContext.SaveChangesAsync();
            return entity.ObjectApprovalStepId;
        }

        public async Task<IList<ObjectApprovalStepItemModel>> GetAllObjectApprovalStepItem()
        {
            var result = new List<ObjectApprovalStepItemModel>();

            var inputPrivateTask = InputPrivateMappingTypeModels();
            var inputPublicTask = InputPublicMappingTypeModels();
            var voucherTask = VoucherMappingTypeModels();

            result.AddRange(await inputPrivateTask);
            result.AddRange(await inputPublicTask);
            result.AddRange(await voucherTask);

            return result;
        }

        private async Task<IList<ObjectApprovalStepItemModel>> InputPrivateMappingTypeModels()
        {
            var inputTypes = await _inputPrivateTypeHelperService.GetInputTypeSimpleList();

            var result = new List<ObjectApprovalStepItemModel>();
            foreach (var inputType in inputTypes)
            {
                result.Add(new ObjectApprovalStepItemModel
                {
                    ModuleTypeId = EnumModuleType.Accountant,
                    ModuleTypeName = EnumModuleType.Accountant.GetEnumDescription(),
                    ObjectTypeId = EnumObjectType.InputType,
                    ObjectTypeName = EnumObjectType.InputType.GetEnumDescription(),
                    ObjectId = inputType.InputTypeId,
                    ObjectName = inputType.Title,
                    ObjectGroupId = inputType.InputTypeGroupId
                });
            }

            return await Task.FromResult(result);
        }

        private async Task<IList<ObjectApprovalStepItemModel>> InputPublicMappingTypeModels()
        {
            var inputTypes = await _inputPublicTypeHelperService.GetInputTypeSimpleList();

            var result = new List<ObjectApprovalStepItemModel>();
            foreach (var inputType in inputTypes)
            {
                result.Add(new ObjectApprovalStepItemModel
                {
                    ModuleTypeId = EnumModuleType.AccountantPublic,
                    ModuleTypeName = EnumModuleType.AccountantPublic.GetEnumDescription(),
                    ObjectTypeId = EnumObjectType.InputTypePublic,
                    ObjectTypeName = EnumObjectType.InputTypePublic.GetEnumDescription(),
                    ObjectId = inputType.InputTypeId,
                    ObjectName = inputType.Title,
                    ObjectGroupId = inputType.InputTypeGroupId
                });
            }

            return await Task.FromResult(result);
        }

        private async Task<IList<ObjectApprovalStepItemModel>> VoucherMappingTypeModels()
        {
            var voucherTypes = await _voucherTypeHelperService.GetVoucherTypeSimpleList();

            var result = new List<ObjectApprovalStepItemModel>();
            foreach (var voucherType in voucherTypes)
            {
                result.Add(new ObjectApprovalStepItemModel
                {
                    ModuleTypeId = EnumModuleType.PurchaseOrder,
                    ModuleTypeName = EnumModuleType.PurchaseOrder.GetEnumDescription(),
                    ObjectTypeId = EnumObjectType.VoucherType,
                    ObjectTypeName = EnumObjectType.VoucherType.GetEnumDescription(),
                    ObjectId = voucherType.VoucherTypeId,
                    ObjectName = voucherType.Title,
                    ObjectGroupId = voucherType.VoucherTypeGroupId
                });
            }

            return await Task.FromResult(result);
        }

    }
}
