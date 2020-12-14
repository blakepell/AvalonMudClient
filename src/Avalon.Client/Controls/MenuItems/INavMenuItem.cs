using System.Threading.Tasks;
using MahApps.Metro.IconPacks;

namespace Avalon.Controls
{
    public interface INavMenuItem
    { 
        PackIconMaterialKind Icon { get; set; }

        string Title { get; set; }

        public string Argument { get; set; }

        Task ExecuteAsync();

        public INavMenuItem Reference { get; }

    }
}