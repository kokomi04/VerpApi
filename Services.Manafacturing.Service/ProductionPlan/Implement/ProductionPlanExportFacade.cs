using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionPlan;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using StepEntity = VErp.Infrastructure.EF.ManufacturingDB.Step;

namespace VErp.Services.Manafacturing.Service.ProductionPlan.Implement
{


    public class ProductionPlanExportFacade
    {
        private IProductionPlanService _productionPlanService;
        private IProductHelperService _productHelperService;
        private IProductBomHelperService _productBomHelperService;
        private IProductCateHelperService _productCateHelperService;
        private IVoucherTypeHelperService _voucherTypeHelperService;
        private ICurrentContextService _currentContext;
        private IList<ProductionOrderListModel> productionPlanInfo = null;
        private IList<InternalProductCateOutput> productCates = null;
        private IDictionary<int, Dictionary<int, decimal>> productCateQuantity = new Dictionary<int, Dictionary<int, decimal>>();

        private IDictionary<long, Dictionary<int, WorkloadStepPlan>> workloadData = new Dictionary<long, Dictionary<int, WorkloadStepPlan>>();
        private IDictionary<long, PlanExtraInfo> extraPlanInfos = new Dictionary<long, PlanExtraInfo>();
        private decimal sumQuantity = 0;
        private decimal sumWorkload = 0;
        private IDictionary<int, WorkloadStepPlan> sumData = new Dictionary<int, WorkloadStepPlan>();
        private List<StepGroup> _groupSteps;
        private List<StepEntity> _allSteps;

        private ISheet sheet = null;
        private int currentRow = 0;
        private int maxColumnIndex = 0;
        private IList<VoucherOrderDetailSimpleModel> mapVoucherOrder;

        public ProductionPlanExportFacade SetProductionPlanService(IProductionPlanService productionPlanService)
        {
            _productionPlanService = productionPlanService;
            return this;
        }
        public ProductionPlanExportFacade SetProductHelperService(IProductHelperService productHelperService)
        {
            _productHelperService = productHelperService;
            return this;
        }
        public ProductionPlanExportFacade SetProductBomHelperService(IProductBomHelperService productBomHelperService)
        {
            _productBomHelperService = productBomHelperService;
            return this;
        }
        public ProductionPlanExportFacade SetProductCateHelperService(IProductCateHelperService productCateHelperService)
        {
            _productCateHelperService = productCateHelperService;
            return this;
        }
        public ProductionPlanExportFacade SetCurrentContextService(ICurrentContextService currentContext)
        {
            _currentContext = currentContext;
            return this;
        }
        public ProductionPlanExportFacade SetVoucherTypeHelperService(IVoucherTypeHelperService voucherTypeHelperService)
        {
            _voucherTypeHelperService = voucherTypeHelperService;
            return this;
        }
        public ProductionPlanExportFacade SetStepInfo(List<StepGroup> groupSteps, List<StepEntity> allSteps)
        {
            _groupSteps = groupSteps;
            _allSteps = allSteps;
            return this;
        }
        public async Task<(Stream stream, string fileName, string contentType)> Export(
            int? monthPlanId,
            int? factoryDepartmentId,
            long startDate,
            long endDate,
            ProductionPlanExportModel data,
            IList<string> mappingFunctionKeys = null)
        {
            maxColumnIndex = 23 + data.ProductCateIds.Length;
            productionPlanInfo = await _productionPlanService.GetProductionPlans(monthPlanId, factoryDepartmentId, startDate, endDate);
            productCates = (await _productCateHelperService.Search(null, string.Empty, -1, -1, string.Empty, true)).List.Where(pc => data.ProductCateIds.Contains(pc.ProductCateId)).ToList();

            var productIds = productionPlanInfo.Select(p => p.ProductId.Value).Distinct().ToList();
            var products = await _productHelperService.GetByIds(productIds);
            var productElements = await _productBomHelperService.GetElements(productIds.ToArray());

            var orderCodes = productionPlanInfo.Where(x => !string.IsNullOrWhiteSpace(x.OrderCode)).Select(x => x.OrderCode).ToList();
            mapVoucherOrder = await _voucherTypeHelperService.OrderByCodes(orderCodes);


            // map decimal place
            foreach (var plan in productionPlanInfo)
            {
                var product = products.FirstOrDefault(p => p.ProductId == plan.ProductId);
                plan.DecimalPlace = product != null ? product.DecimalPlace : 5;

            }

            // Xử lý tính toán số lượng chi tiết
            foreach (var productId in productIds)
            {
                var product = products.FirstOrDefault(p => p.ProductId == productId);
                productCateQuantity.Add(productId, new Dictionary<int, decimal>());

                foreach (var productCate in productCates)
                {
                    productCateQuantity[productId].Add(productCate.ProductCateId, 0);
                    // Nếu sản phẩm thuộc danh mục => thêm số lượng vào
                    if (product != null && product.ProductCateId == productCate.ProductCateId)
                    {
                        this.productCateQuantity[productId][productCate.ProductCateId] = 1;
                    }
                    else if (product != null && !productCates.Any(pc => pc.ProductCateId == product.ProductCateId))
                    {
                        var elementQuantity = productElements.Where(pe => pe.ParentProductId == product.ProductId && pe.ProductCateId == productCate.ProductCateId)
                        .Sum(pe => pe.Quantity.GetValueOrDefault() * pe.Wastage.GetValueOrDefault());

                        productCateQuantity[product.ProductId][productCate.ProductCateId] = elementQuantity;
                    }
                }
            }

            var xssfwb = new XSSFWorkbook();
            sheet = xssfwb.CreateSheet();


            await WriteTable();

            var currentRowTmp = currentRow;

            for (var i = 0; i <= maxColumnIndex; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            for (var i = 0; i <= maxColumnIndex; i++)
            {
                var c = sheet.GetColumnWidth(i);
                if (c < 2000)
                {
                    sheet.SetColumnWidth(i, 2000);
                }
            }

            WriteGeneralInfo(data.MonthPlanName);

            currentRow = currentRowTmp;
            WriteFooter(data.Note);

            var stream = new MemoryStream();
            xssfwb.Write(stream, true);
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"KHSX#{startDate.UnixToDateTime(_currentContext.TimeZoneOffset).ToString("dd_MM_yyyy")}#{endDate.UnixToDateTime(_currentContext.TimeZoneOffset).ToString("dd_MM_yyyy")}.xlsx";
            return (stream, fileName, contentType);
        }


        private void WriteGeneralInfo(string monthPlanName)
        {
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, maxColumnIndex));
            sheet.EnsureCell(0, 0).SetCellValue($"LỊCH SẢN XUẤT XƯỞNG THÁNG {monthPlanName}");
            sheet.GetRow(0).Height = 1500;
            sheet.SetCellStyle(0, 0, 20, true, false, VerticalAlignment.Center, HorizontalAlignment.Center, false);
        }


        private async Task WriteTable()
        {
            currentRow = 2;

            var fRow = currentRow;

            sheet.EnsureCell(fRow, 0).SetCellValue($"STT");
            sheet.EnsureCell(fRow, 1).SetCellValue($"Lệnh SX");
            sheet.EnsureCell(fRow, 2).SetCellValue($"Mã Đơn Hàng");
            sheet.EnsureCell(fRow, 3).SetCellValue($"S.lg Cont.");
            sheet.EnsureCell(fRow, 4).SetCellValue($"Số PO đối tác");
            sheet.EnsureCell(fRow, 5).SetCellValue($"Mã KH");
            sheet.EnsureCell(fRow, 6).SetCellValue($"Tên KH");
            sheet.EnsureCell(fRow, 7).SetCellValue($"Mã SP");
            sheet.EnsureCell(fRow, 8).SetCellValue($"Tên SP");
            sheet.EnsureCell(fRow, 9).SetCellValue($"ĐVT");
            sheet.EnsureCell(fRow, 10).SetCellValue($"S.lg đ.hàng");
            sheet.EnsureCell(fRow, 11).SetCellValue($"Vào lệnh");
            sheet.EnsureCell(fRow, 12).SetCellValue($"Bù hao");
            sheet.EnsureCell(fRow, 13).SetCellValue($"Tổng s.lg");
            sheet.EnsureCell(fRow, 14).SetCellValue($"Tổng KL tinh");
            sheet.EnsureCell(fRow, 15).SetCellValue($"Đơn giá");
            sheet.EnsureCell(fRow, 16).SetCellValue($"T.tiền n.tệ");
            sheet.EnsureCell(fRow, 17).SetCellValue($"T.tiền VNĐ");
            sheet.EnsureCell(fRow, 18).SetCellValue($"Ngày chứng từ");
            sheet.EnsureCell(fRow, 19).SetCellValue($"Bắt đầu");
            sheet.EnsureCell(fRow, 20).SetCellValue($"K.thúc h.trắng");
            sheet.EnsureCell(fRow, 21).SetCellValue($"K.thúc");
            sheet.EnsureCell(fRow, 22).SetCellValue($"Ngày giao");
            sheet.EnsureCell(fRow, 23).SetCellValue($"Ghi chú");
            int colIndx = 24;
            foreach (var productCate in productCates)
            {
                sheet.EnsureCell(fRow, colIndx).SetCellValue($"{productCate.ProductCateName}");
                colIndx++;
            }


            for (var j = 0; j <= maxColumnIndex; j++)
            {
                sheet.SetHeaderCellStyle(fRow, j);
            }

            currentRow = fRow + 1;

            WriteTableDetailData();

            await Task.CompletedTask;
        }


        private void WriteTableDetailData()
        {

            // var centerCell = sheet.GetCellStyle(hAlign: HorizontalAlignment.Center, isBorder: true);
            var productIds = productionPlanInfo.Select(p => p.ProductId.Value).Distinct().ToList();
            var products = _productHelperService.GetByIds(productIds).Result;
            var normalCell = sheet.GetCellStyle(isBorder: true);
            var numberCell = sheet.GetCellStyle(isBorder: true, dataFormat: "#,##0");
            var dateCell = sheet.GetCellStyle(isBorder: true, dataFormat: "dd/MM/yyyy");
            var productPurityCell = sheet.GetCellStyle(isBorder: true, dataFormat: "#,##0.00###");
            var stt = 1;
            foreach (var item in productionPlanInfo)
            {
                var product = products.FirstOrDefault(p => p.ProductId == item.ProductId);
                for (var i = 0; i <= maxColumnIndex; i++)
                {
                    var style = normalCell;
                    if (i == 14)
                    {
                        style = productPurityCell;
                    }
                    else if (i == 3 || (i >= 10 && i <= 17))
                    {
                        style = numberCell;
                    }
                    else if (i >= 18 && i <= 22)
                    {
                        style = dateCell;
                    }
                    sheet.EnsureCell(currentRow, i).CellStyle = style;
                }

                sheet.EnsureCell(currentRow, 0).SetCellValue(stt);
                sheet.EnsureCell(currentRow, 1).SetCellValue(item.ProductionOrderCode);
                sheet.EnsureCell(currentRow, 2).SetCellValue(item.OrderCode);
                var voucherOrder = mapVoucherOrder.FirstOrDefault(x => x.OrderCode == item.OrderCode && x.ProductId == item.ProductId);
                if (voucherOrder != null)
                {
                    sheet.EnsureCell(currentRow, 3).SetCellValue((double)(voucherOrder.ContainerQuantity));
                    sheet.EnsureCell(currentRow, 4).SetCellValue(voucherOrder.CustomerPO);
                    sheet.EnsureCell(currentRow, 5).SetCellValue(voucherOrder.PartnerCode);
                    sheet.EnsureCell(currentRow, 6).SetCellValue(voucherOrder.PartnerName != null ? voucherOrder.PartnerName : "N/A");
                    sheet.EnsureCell(currentRow, 10).SetCellValue((double)voucherOrder.Quantity);
                    if (voucherOrder.DeliveryDate > 0)
                    {
                        sheet.EnsureCell(currentRow, 22).SetCellValue(voucherOrder.DeliveryDate.UnixToDateTime(_currentContext.TimeZoneOffset));
                    }
                }
                sheet.EnsureCell(currentRow, 7).SetCellValue(item.ProductCode);
                sheet.EnsureCell(currentRow, 8).SetCellValue(item.ProductName);
                sheet.EnsureCell(currentRow, 9).SetCellValue(item.UnitName);
                sheet.EnsureCell(currentRow, 11).SetCellValue((double)item.Quantity.GetValueOrDefault());
                sheet.EnsureCell(currentRow, 12).SetCellValue((double)item.ReserveQuantity.GetValueOrDefault());
                var totalQuantity = item.Quantity.GetValueOrDefault() + item.ReserveQuantity.GetValueOrDefault();
                sheet.EnsureCell(currentRow, 13).SetCellValue((double)totalQuantity);
                if (product != null && product.ProductPurity.HasValue)
                {
                    sheet.EnsureCell(currentRow, 14).SetCellValue((double)(product.ProductPurity.Value * totalQuantity));
                }
                sheet.EnsureCell(currentRow, 15).SetCellValue((double)item.UnitPrice);
                sheet.EnsureCell(currentRow, 16).SetCellValue((double)(item.UnitPrice * totalQuantity));
                sheet.EnsureCell(currentRow, 17).SetCellValue((double)(item.UnitPrice * totalQuantity * (item.CurrencyRate.HasValue ? item.CurrencyRate : 1)));
                sheet.EnsureCell(currentRow, 18).SetCellValue(item.Date.UnixToDateTime(_currentContext.TimeZoneOffset));
                sheet.EnsureCell(currentRow, 19).SetCellValue(item.StartDate.UnixToDateTime(_currentContext.TimeZoneOffset));
                sheet.EnsureCell(currentRow, 20).SetCellValue(item.PlanEndDate.UnixToDateTime(_currentContext.TimeZoneOffset));
                sheet.EnsureCell(currentRow, 21).SetCellValue(item.EndDate.UnixToDateTime(_currentContext.TimeZoneOffset));
                sheet.EnsureCell(currentRow, 23).SetCellValue(item.Note);

                int colIndx = 24;
                foreach (var productCate in productCates)
                {
                    sheet.EnsureCell(currentRow, colIndx).SetCellValue((double)((item.Quantity.GetValueOrDefault() + item.ReserveQuantity.GetValueOrDefault()) * productCateQuantity[item.ProductId.Value][productCate.ProductCateId]));
                    colIndx++;
                }

                currentRow++;
                stt++;
            }
        }

        private void WriteFooter(string note)
        {
            var fRow = currentRow + 2;
            var sRow = currentRow + 2;

            sheet.EnsureCell(fRow, 0).SetCellValue($"Ghi chú");
            sheet.SetCellStyle(fRow, 0, 12, false, false, VerticalAlignment.Center, HorizontalAlignment.Center, true);

            sheet.AddMergedRegion(new CellRangeAddress(fRow, sRow, 1, maxColumnIndex));
            sheet.EnsureCell(fRow, 1).SetCellValue(note);

            var normalCell = sheet.GetCellStyle(isBorder: true);
            for (var j = 1; j <= maxColumnIndex; j++)
            {
                sheet.EnsureCell(fRow, j).CellStyle = normalCell;
            }

            sheet.SetCellStyle(fRow, 1, 12, false, false, VerticalAlignment.Center, HorizontalAlignment.Left, true, true);

            sheet.GetRow(fRow).Height = 1700;

            currentRow++;
        }




        public async Task<(Stream stream, string fileName, string contentType)> WorkloadExport(
          int? monthPlanId,
          int? factoryDepartmentId,
          long startDate,
          long endDate,
          string monthPlanName,
          List<ProductionPlanExtraInfoModel> extraInfos,
          IList<string> mappingFunctionKeys = null)
        {
            maxColumnIndex = 7 + _allSteps.Count * 2;

            productionPlanInfo = await _productionPlanService.GetProductionPlans(monthPlanId, factoryDepartmentId, startDate, endDate);

            var sortOrderMax = 0;
            foreach (var p in productionPlanInfo)
            {
                extraPlanInfos.Add(p.ProductionOrderDetailId.Value, new PlanExtraInfo()
                {
                    IsShow = true,
                    RowSpan = 1,
                    SortOrder = -1,
                    Workload = 0
                });
                // Sắp xếp, bổ sung thông tin mở rộng
                var extraInfo = extraInfos.Where(ei => ei.ProductionOrderDetailId == p.ProductionOrderDetailId).FirstOrDefault();
                if (extraInfo != null)
                {
                    extraPlanInfos[p.ProductionOrderDetailId.Value].SortOrder = extraInfo.SortOrder;
                }
                if (extraPlanInfos[p.ProductionOrderDetailId.Value].SortOrder > -1)
                {
                    sortOrderMax = sortOrderMax > extraPlanInfos[p.ProductionOrderDetailId.Value].SortOrder ? sortOrderMax : extraPlanInfos[p.ProductionOrderDetailId.Value].SortOrder;
                }

            }

            // Gán sortorder cho LSX còn lại
            foreach (var p in productionPlanInfo)
            {
                if (extraPlanInfos[p.ProductionOrderDetailId.Value].SortOrder == -1)
                {
                    var plan = productionPlanInfo.FirstOrDefault(pl => pl.ProductionOrderId == p.ProductionOrderId && extraPlanInfos[pl.ProductionOrderDetailId.Value].SortOrder > -1);
                    extraPlanInfos[p.ProductionOrderDetailId.Value].SortOrder = plan != null ? extraPlanInfos[plan.ProductionOrderDetailId.Value].SortOrder : ++sortOrderMax;
                }
            }


            // Sắp xếp
            productionPlanInfo = productionPlanInfo.OrderBy(p => extraPlanInfos[p.ProductionOrderDetailId.Value].SortOrder).ToList();

            var workloads = await _productionPlanService.GetWorkloadPlanByDate(monthPlanId, factoryDepartmentId, startDate, endDate);

            var productIds = new List<int>();
            var productSemiIds = new List<long>();
            foreach (var productionWorkload in workloads)
            {
                foreach (var workloadOutput in productionWorkload.Value.WorkloadOutput)
                {
                    foreach (var wo in workloadOutput.Value)
                    {
                        if (wo.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
                        {
                            productIds.Add((int)wo.ObjectId);
                        }
                        else
                        {
                            productSemiIds.Add(wo.ObjectId);
                        }
                    }
                }
            }
            productIds.AddRange(productionPlanInfo.Select(p => p.ProductId.Value).ToList());
            productSemiIds = productSemiIds.Distinct().ToList();
            var productSemis = _productionPlanService.GetProductSemis(productSemiIds);
            productIds.AddRange(productSemis.Where(ps => ps.RefProductId.HasValue).Select(ps => (int)ps.RefProductId.Value).ToList());
            productIds = productIds.Distinct().ToList();

            var products = await _productHelperService.GetByIds(productIds);

            // map decimal place
            foreach (var plan in productionPlanInfo)
            {
                var product = products.FirstOrDefault(p => p.ProductId == plan.ProductId);
                if (product != null && product.ProductPurity.HasValue)
                {
                    extraPlanInfos[plan.ProductionOrderDetailId.Value].Workload = product.ProductPurity.Value * (plan.Quantity.Value + plan.ReserveQuantity.Value);
                }
                plan.DecimalPlace = product != null ? product.DecimalPlace : 5;

            }

            // Xử lý tính toán số lượng, khối lượng tinh chi tiết
            var productionOrderIds = productionPlanInfo.Select(p => p.ProductionOrderId).Distinct().ToList();
            foreach (var productionOrderId in productionOrderIds)
            {
                workloadData.Add(productionOrderId, new Dictionary<int, WorkloadStepPlan>());
                foreach (var groupStep in _groupSteps)
                {
                    var steps = _allSteps.Where(s => s.StepGroupId == groupStep.StepGroupId).ToList();
                    if (steps.Count == 0) continue;
                    foreach (var step in steps)
                    {
                        workloadData[productionOrderId].Add(step.StepId, new WorkloadStepPlan()
                        {
                            Quantity = 0,
                            Workload = 0
                        });

                        if (workloads.ContainsKey(productionOrderId) && workloads[productionOrderId].WorkloadOutput.ContainsKey(step.StepId))
                        {
                            foreach (var wo in workloads[productionOrderId].WorkloadOutput[step.StepId])
                            {
                                workloadData[productionOrderId][step.StepId].Quantity += wo.Quantity;
                                if (wo.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
                                {
                                    var product = products.Where(p => p.ProductId == wo.ObjectId).FirstOrDefault();
                                    if (product != null && product.ProductPurity.HasValue)
                                    {
                                        workloadData[productionOrderId][step.StepId].Workload += wo.Quantity * product.ProductPurity.Value;
                                    }
                                }
                                else
                                {
                                    var productSemi = productSemis.Where(s => s.ProductSemiId == wo.ObjectId).FirstOrDefault();
                                    if (productSemi != null && productSemi.RefProductId.HasValue)
                                    {
                                        var product = products.FirstOrDefault(p => p.ProductId == productSemi.RefProductId);
                                        if (product != null && product.ProductPurity.HasValue)
                                        {
                                            workloadData[productionOrderId][step.StepId].Workload += wo.Quantity * product.ProductPurity.Value;
                                        }
                                    }
                                }
                            }
                        }
                    }


                }
            }

            // Nhóm dòng
            ProductionOrderListModel preItem = null;
            foreach (var item in productionPlanInfo)
            {
                var isDuplicate = preItem != null
                    && (preItem.ProductionOrderCode == item.ProductionOrderCode
                    || extraPlanInfos[preItem.ProductionOrderDetailId.Value].SortOrder == extraPlanInfos[item.ProductionOrderDetailId.Value].SortOrder);


                if (isDuplicate)
                {
                    extraPlanInfos[preItem.ProductionOrderDetailId.Value].RowSpan++;
                    extraPlanInfos[item.ProductionOrderDetailId.Value].IsShow = false;
                }
                else
                {
                    preItem = item;
                    extraPlanInfos[preItem.ProductionOrderDetailId.Value].RowSpan = 1;
                    extraPlanInfos[preItem.ProductionOrderDetailId.Value].IsShow = true;
                }
            }


            // Tổng
            foreach (var groupStep in _groupSteps)
            {
                var steps = _allSteps.Where(s => s.StepGroupId == groupStep.StepGroupId).ToList();
                if (steps.Count == 0) continue;
                foreach (var step in steps)
                {
                    sumData.Add(step.StepId, new WorkloadStepPlan
                    {
                        Quantity = 0,
                        Workload = 0
                    });
                }
            }

            foreach (var plan in productionPlanInfo)
            {
                sumQuantity += (plan.Quantity.Value + plan.ReserveQuantity.Value);
                sumWorkload += extraPlanInfos[plan.ProductionOrderDetailId.Value].Workload;
                if (extraPlanInfos[plan.ProductionOrderDetailId.Value].IsShow)
                {
                    foreach (var groupStep in _groupSteps)
                    {
                        var steps = _allSteps.Where(s => s.StepGroupId == groupStep.StepGroupId).ToList();
                        if (steps.Count == 0) continue;
                        foreach (var step in steps)
                        {
                            sumData[step.StepId].Quantity += workloadData[plan.ProductionOrderId][step.StepId].Quantity;
                            sumData[step.StepId].Workload += workloadData[plan.ProductionOrderId][step.StepId].Workload;
                        }
                    }
                }
            }


            var xssfwb = new XSSFWorkbook();
            sheet = xssfwb.CreateSheet();


            await WriteWorkloadTable();

            var currentRowTmp = currentRow;

            for (var i = 1; i <= maxColumnIndex; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            for (var i = 1; i <= maxColumnIndex; i++)
            {
                var c = sheet.GetColumnWidth(i);
                if (c < 2000)
                {
                    sheet.SetColumnWidth(i, 2000);
                }
            }

            WriteWorkloadGeneralInfo(monthPlanName);

            currentRow = currentRowTmp;

            var stream = new MemoryStream();
            xssfwb.Write(stream, true);
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"KHSXKL_{startDate}_{endDate}.xlsx";
            return (stream, fileName, contentType);
        }





        private void WriteWorkloadGeneralInfo(string monthPlanName)
        {
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, maxColumnIndex));
            sheet.EnsureCell(0, 0).SetCellValue($"KẾ HOẠCH SẢN XUẤT THEO SẢN LƯỢNG THÁNG {monthPlanName}");
            sheet.GetRow(0).Height = 1500;
            sheet.SetCellStyle(0, 0, 20, true, false, VerticalAlignment.Center, HorizontalAlignment.Left, false);
        }



        private async Task WriteWorkloadTable()
        {
            currentRow = 2;

            var fRow = currentRow;
            sheet.AddMergedRegion(new CellRangeAddress(fRow, currentRow + 2, 0, 0));
            sheet.EnsureCell(fRow, 0).SetCellValue($"Ngày");
            sheet.AddMergedRegion(new CellRangeAddress(fRow, currentRow + 2, 1, 1));
            sheet.EnsureCell(fRow, 1).SetCellValue($"Mã LSX");
            sheet.AddMergedRegion(new CellRangeAddress(fRow, currentRow + 2, 2, 2));
            sheet.EnsureCell(fRow, 2).SetCellValue($"Mã hàng");
            sheet.AddMergedRegion(new CellRangeAddress(fRow, currentRow + 2, 3, 3));
            sheet.EnsureCell(fRow, 3).SetCellValue($"Tên hàng");
            sheet.AddMergedRegion(new CellRangeAddress(fRow, currentRow + 2, 4, 4));
            sheet.EnsureCell(fRow, 4).SetCellValue($"Đơn vị");
            sheet.AddMergedRegion(new CellRangeAddress(fRow, currentRow + 2, 5, 5));
            sheet.EnsureCell(fRow, 5).SetCellValue($"Số lượng");
            sheet.AddMergedRegion(new CellRangeAddress(fRow, currentRow + 2, 6, 6));
            sheet.EnsureCell(fRow, 6).SetCellValue($"M3 tinh SP");


            // Dòng tổng
            sheet.AddMergedRegion(new CellRangeAddress(fRow + 3, fRow + 3, 0, 4));
            sheet.EnsureCell(fRow + 3, 0).SetCellValue($"Tổng");


            sheet.EnsureCell(fRow + 3, 5).SetCellValue((double)sumQuantity);
            sheet.EnsureCell(fRow + 3, 6).SetCellValue((double)sumWorkload);


            int colIndx = 7;

            foreach (var groupStep in _groupSteps)
            {
                var steps = _allSteps.Where(s => s.StepGroupId == groupStep.StepGroupId).ToList();
                if (steps.Count == 0) continue;
                sheet.AddMergedRegion(new CellRangeAddress(fRow, fRow, colIndx, colIndx + steps.Count * 2 - 1));
                sheet.EnsureCell(fRow, colIndx).SetCellValue($"{groupStep.StepGroupName}");
                var stepIndx = 0;
                foreach (var step in steps)
                {
                    sheet.AddMergedRegion(new CellRangeAddress(fRow + 1, fRow + 1, colIndx + stepIndx * 2, colIndx + stepIndx * 2 + 1));
                    sheet.EnsureCell(fRow + 1, colIndx + stepIndx * 2).SetCellValue($"{step.StepName}");
                    sheet.EnsureCell(fRow + 2, colIndx + stepIndx * 2).SetCellValue($"S.lg chi tiết");
                    sheet.EnsureCell(fRow + 2, colIndx + stepIndx * 2 + 1).SetCellValue($"Kh.lg");
                    sheet.EnsureCell(fRow + 3, colIndx + stepIndx * 2).SetCellValue((double)sumData[step.StepId].Quantity);
                    sheet.EnsureCell(fRow + 3, colIndx + stepIndx * 2 + 1).SetCellValue((double)sumData[step.StepId].Workload);
                    stepIndx++;
                }

                colIndx += steps.Count * 2;
            }

            var numberCell = sheet.GetCellStyle(isBorder: true, dataFormat: "#,##0");
            for (var j = 0; j <= maxColumnIndex; j++)
            {
                if (j >= 5)
                {
                    sheet.EnsureCell(currentRow + 3, j).CellStyle = numberCell;
                }
                sheet.SetHeaderCellStyle(fRow, j);
                sheet.SetHeaderCellStyle(fRow + 1, j);
                sheet.SetHeaderCellStyle(fRow + 2, j);
                sheet.SetHeaderCellStyle(fRow + 3, j);
            }

            currentRow = fRow + 4;

            WriteWorkloadTableDetailData();

            await Task.CompletedTask;
        }


        private void WriteWorkloadTableDetailData()
        {

            // var centerCell = sheet.GetCellStyle(hAlign: HorizontalAlignment.Center, isBorder: true);

            var normalCell = sheet.GetCellStyle(isBorder: true);

            var numberCell = sheet.GetCellStyle(isBorder: true, dataFormat: "#,##0");
            var dateCell = sheet.GetCellStyle(isBorder: true, dataFormat: "dd/MM/yyyy");

            foreach (var item in productionPlanInfo)
            {
                for (var i = 0; i <= maxColumnIndex; i++)
                {
                    var style = numberCell;
                    if (i == 1 || i == 2 || i == 3 || i == 4)
                    {
                        style = normalCell;
                    }
                    else if (i == 0)
                    {
                        style = dateCell;
                    }
                    sheet.EnsureCell(currentRow, i).CellStyle = style;
                }


                sheet.EnsureCell(currentRow, 0).SetCellValue((item.Date + _currentContext.TimeZoneOffset.GetValueOrDefault()).UnixToDateTime().Value);
                if (extraPlanInfos[item.ProductionOrderDetailId.Value].IsShow)
                {
                    if (extraPlanInfos[item.ProductionOrderDetailId.Value].RowSpan > 1)
                    {
                        sheet.AddMergedRegion(new CellRangeAddress(currentRow, currentRow + extraPlanInfos[item.ProductionOrderDetailId.Value].RowSpan - 1, 1, 1));
                    }

                    sheet.EnsureCell(currentRow, 1).SetCellValue(item.ProductionOrderCode);
                }

                sheet.EnsureCell(currentRow, 2).SetCellValue(item.ProductCode);
                sheet.EnsureCell(currentRow, 3).SetCellValue(item.ProductName);
                sheet.EnsureCell(currentRow, 4).SetCellValue(item.UnitName);
                sheet.EnsureCell(currentRow, 5).SetCellValue((double)(item.Quantity.GetValueOrDefault() + item.ReserveQuantity.GetValueOrDefault()));

                sheet.EnsureCell(currentRow, 6).SetCellValue((double)extraPlanInfos[item.ProductionOrderDetailId.Value].Workload);


                int colIndx = 7;

                foreach (var groupStep in _groupSteps)
                {
                    var steps = _allSteps.Where(s => s.StepGroupId == groupStep.StepGroupId).ToList();
                    if (steps.Count == 0) continue;

                    var stepIndx = 0;
                    foreach (var step in steps)
                    {
                        if (extraPlanInfos[item.ProductionOrderDetailId.Value].IsShow)
                        {
                            if (extraPlanInfos[item.ProductionOrderDetailId.Value].RowSpan > 1)
                            {
                                sheet.AddMergedRegion(new CellRangeAddress(currentRow, currentRow + extraPlanInfos[item.ProductionOrderDetailId.Value].RowSpan - 1, colIndx + stepIndx * 2, colIndx + stepIndx * 2));
                                sheet.AddMergedRegion(new CellRangeAddress(currentRow, currentRow + extraPlanInfos[item.ProductionOrderDetailId.Value].RowSpan - 1, colIndx + stepIndx * 2 + 1, colIndx + stepIndx * 2 + 1));
                            }

                            sheet.EnsureCell(currentRow, colIndx + stepIndx * 2).SetCellValue((double)workloadData[item.ProductionOrderId][step.StepId].Quantity);
                            sheet.EnsureCell(currentRow, colIndx + stepIndx * 2 + 1).SetCellValue((double)workloadData[item.ProductionOrderId][step.StepId].Workload);
                        }

                        stepIndx++;
                    }

                    colIndx += steps.Count * 2;
                }
                currentRow++;
            }
        }



        private class WorkloadStepPlan
        {
            public decimal Quantity { get; set; }
            public decimal Workload { get; set; }
        }

        private class PlanExtraInfo
        {
            public bool IsShow { get; set; }
            public int SortOrder { get; set; }
            public int RowSpan { get; set; }
            public decimal Workload { get; set; }
        }
    }
}
