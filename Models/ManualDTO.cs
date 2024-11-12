using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoClicker.ViewModels;

namespace AutoClicker.Models;

public class ManualDTO
{
    public ObservableCollection<ManualClickItem> Items { get; set; } = [];
    public int RepeatCount { get; set; } = 0;

    public static void Load(string path,ManualClickViewModel viewModel)
    {
        var json = File.ReadAllText(path);

        try
        {
            var dto = JsonSerializer.Deserialize<ManualDTO>(json);

            if (dto != null)
            {
                viewModel.ManualClickItems = dto.Items;
                viewModel.RepeatCount = dto.RepeatCount;
            }
        }
        catch
        {
        }

    }

    public static void Save(string path,ManualClickViewModel viewModel)
    {
        var dto  = new ManualDTO();
        dto.Items = viewModel.ManualClickItems;
        dto.RepeatCount = viewModel.RepeatCount;

        var json = JsonSerializer.Serialize(dto);
        File.WriteAllText(path, json);
    }
}
