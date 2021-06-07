using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Verp.Services.ReportConfig.Model;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;

namespace Verp.Services.ReportConfig.Service.Implement
{
    public class RepeatColumnUtils
    {
        /*
        public static IList<ReportColumnModel> RepeatColumnProcess(IList<ReportColumnModel> columns, IList<NonCamelCaseDictionary> dataTable)
        {
            // Xử lý nếu có repeat columns
            if (dataTable.Count > 0)
            {
                var includeColumns = new List<ReportColumnModel>();
                var removeColumns = new List<ReportColumnModel>();
                foreach (var (key, value) in dataTable[0])
                {
                    foreach (var column in columns)
                    {
                        var hasInclude = false;
                        if (!column.IsRepeat.HasValue || !column.IsRepeat.Value) continue;
                        var pattern = $"{column.Alias}(?<suffix>\\w+)";
                        Regex rx = new Regex(pattern);
                        MatchCollection match = rx.Matches(key);
                        if (match.Count > 0)
                        {
                            var suffixKey = match[0].Groups["suffix"].Value;

                            var includeColumn = Utils.DeepClone(column);
                            includeColumn.Alias = $"{column.Alias}{suffixKey}";
                            includeColumn.Value = $"{column.Value}{suffixKey}";
                            includeColumn.SuffixKey = $"{suffixKey}";
                            includeColumn.OriginValue = column.Value;
                            includeColumns.Add(includeColumn);
                            hasInclude = true;
                        }
                        if (hasInclude && !removeColumns.Any(c => c.Value == column.Value)) removeColumns.Add(column);
                    }
                }
                if (includeColumns.Count > 0)
                {
                    // Lấy ra các nhóm cột lặp liền kề
                    removeColumns = removeColumns.OrderBy(c => c.SortOrder).ToList();
                    var removeColumnGroup = removeColumns.Aggregate(new List<List<ReportColumnModel>>(), (group, item) =>
                    {
                        var length = group.Count;
                        if (length == 0)
                        {
                            group.Add(new List<ReportColumnModel>() { item });
                        }
                        else
                        {
                            var lastColumn = group[length - 1][group[length - 1].Count - 1];
                            // Kiểm tra tồn tại cột xen giữa
                            var hasMiddle = columns.Any(c => c.SortOrder > lastColumn.SortOrder && c.SortOrder < item.SortOrder);
                            if (hasMiddle)
                            {
                                group.Add(new List<ReportColumnModel>() { item });
                            }
                            else
                            {
                                group[length - 1].Add(item);
                            }
                        }
                        return group;
                    });

                    foreach (var group in removeColumnGroup)
                    {
                        // Chèn danh sách data lặp có trong nhóm
                        var startIndx = columns.IndexOf(group[0]);
                        var insertColumns = includeColumns.Where(c => group.Any(oc => oc.Value == c.OriginValue)).ToList();
                        for (var indx = 0; indx < insertColumns.Count; indx++)
                        {
                            columns.Insert(startIndx + indx, includeColumns[indx]);
                        }
                        // Gỡ hết cột lặp mẫu
                        foreach (var removeColumn in group)
                        {
                            columns.Remove(removeColumn);
                        }
                    }
                }
            }
            return columns;
        }*/

        public static IList<ReportColumnModel> RepeatColumnAndSortProcess(IList<ReportColumnModel> columns, IList<NonCamelCaseDictionary> dataTable)
        {
            if (dataTable == null || dataTable.Count == 0)
                return columns;

            columns = columns.OrderBy(c => c.SortOrder).ToList();

            //grant groupId for all column
            var groupIndex = 0;
            var currentGroupIndex = 0;
            foreach (var c in columns)
            {
                if (currentGroupIndex != c.ColGroupId || c.ColGroupId == 0)
                {
                    currentGroupIndex = c.ColGroupId;
                    groupIndex++;
                    c.ColGroupId = groupIndex;
                }
                else
                {
                    c.ColGroupId = groupIndex;
                }
            };


            var firstRow = new NonCamelCaseDictionary();
            foreach (var row in dataTable)
            {
                foreach (var (k, v) in row)
                {
                    if (!firstRow.ContainsKey(k))
                    {
                        firstRow.Add(k, v);
                    }
                    else if (firstRow[k].IsNullObject())
                    {
                        firstRow[k] = v;
                    }

                }
            }


            var dynamicColumns = new List<ReportColumnModel>();
            foreach (var (key, _) in firstRow)
            {
                foreach (var column in columns)
                {
                    if (column.IsRepeat != true) continue;
                    var pattern = $"{column.Alias}(?<suffix>\\w+)";
                    Regex rx = new Regex(pattern);
                    MatchCollection match = rx.Matches(key);
                    if (match.Count > 0)
                    {
                        var suffixKey = match[0].Groups["suffix"].Value;

                        var newColumn = Utils.DeepClone(column);

                        var nameColumn = $"{column.ColGroupName}{suffixKey}";

                        if (firstRow.ContainsKey(nameColumn))
                        {
                            newColumn.ColGroupName = firstRow[nameColumn]?.ToString();
                        }

                        if (string.IsNullOrWhiteSpace(newColumn.ColGroupName) && firstRow.ContainsKey(column.ColGroupName))
                        {
                            newColumn.ColGroupName = firstRow[column.ColGroupName]?.ToString();
                        }

                        newColumn.Alias = $"{column.Alias}{suffixKey}";
                        newColumn.Value = $"{column.Value}{suffixKey}";
                        newColumn.SuffixKey = $"{suffixKey}";
                        newColumn.OriginValue = column.Value;
                        dynamicColumns.Add(newColumn);
                    }
                };
            };

            //remove repeat column
            for (var i = columns.Count - 1; i >= 0; i--)
            {
                var c = columns[i];
                if (c.IsRepeat == true)
                {
                    columns.RemoveAt(i);
                }
            }

            //insert repeat columns
            foreach (var newColumn in dynamicColumns)
            {
                columns.Add(newColumn);
            }

            //sort column by groupId, suffixkey, then by sortOrder
            return columns.OrderBy(c => c.ColGroupId)
                        .ThenBy(c => c.SuffixKey)
                        .ThenBy(c => c.SortOrder)
                        .ToList();
        }
    }
}
