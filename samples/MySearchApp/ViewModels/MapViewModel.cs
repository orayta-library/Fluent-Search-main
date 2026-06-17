using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySearchApp.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MySearchApp.ViewModels
{
    public partial class MapViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<MapLayer> layers = new();

        [ObservableProperty]
        private MapLayer? selectedLayer;

        [ObservableProperty]
        private int zoomLevel = 5;

        [ObservableProperty]
        private string? centerCoordinates = "0,0";

        public MapViewModel()
        {
            AddLayerCommand = new RelayCommand(AddLayer);
            ZoomInCommand = new RelayCommand(() => ZoomLevel++);
            ZoomOutCommand = new RelayCommand(() => { if (ZoomLevel>0) ZoomLevel--; });
            ResetViewCommand = new RelayCommand(ResetView);

            // sample layers
            layers.Add(new MapLayer { Name = "Base map", Visible = true });
            layers.Add(new MapLayer { Name = "Roads", Visible = true });
            layers.Add(new MapLayer { Name = "Points of interest", Visible = false });
            selectedLayer = layers.Count>0 ? layers[0] : null;
            OpenMapCommand = new RelayCommand(OpenMap);
        }

        public IRelayCommand AddLayerCommand { get; }
        public IRelayCommand ZoomInCommand { get; }
        public IRelayCommand ZoomOutCommand { get; }
        public IRelayCommand ResetViewCommand { get; }
        public IRelayCommand OpenMapCommand { get; }

        private void AddLayer()
        {
            layers.Add(new MapLayer { Name = $"Layer {layers.Count+1}", Visible = true });
        }

        private void ResetView()
        {
            ZoomLevel = 5;
            CenterCoordinates = "0,0";
        }

        private void OpenMap()
        {
            try
            {
                var url = "http://127.0.0.1:5000/";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch { }
        }
    }
}
