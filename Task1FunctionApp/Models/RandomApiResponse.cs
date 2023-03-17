using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task1FunctionApp.Models
{
    public class RandomApiResponse
    {
        public Int32 Count { get; set; } = default!;
        public List<Entry> Entries { get; set; } = default!;
    }
}
