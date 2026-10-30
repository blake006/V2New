using System.Windows.Controls;

namespace v2rayN.Base;

/// <summary>
/// 带列标识的 DataGrid 列，用于列的显示/隐藏与顺序持久化
/// </summary>
internal interface IDGColumn
{
    string ExName { get; set; }
}

internal class MyDGTextColumn : DataGridTextColumn, IDGColumn
{
    public string ExName { get; set; }
}

internal class MyDGTemplateColumn : DataGridTemplateColumn, IDGColumn
{
    public string ExName { get; set; }
}
