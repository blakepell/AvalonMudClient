using ModernWpf.Controls;
using System.Threading.Tasks;

namespace Avalon.Controls
{
    public interface INavMenuItem
    { 
        IconElement Icon { get; set; }

        string Title { get; set; }

        public string Argument { get; set; }

        Task ExecuteAsync();

        public INavMenuItem Reference { get; }

    }
}