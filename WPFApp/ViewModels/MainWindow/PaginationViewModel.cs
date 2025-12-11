using System.Collections.ObjectModel;
using System.Windows.Input;


namespace ViewModels
{
    public class PaginationViewModel : ViewModelBase
    {
        // Propiedades de estado
        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    UpdatePageNumbers();
                    
                }
            }
        }

        private int _totalPages = 1;
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                if (SetProperty(ref _totalPages, value))
                {
                    UpdatePageNumbers();
                }
            }
        }

        public ObservableCollection<int> PageNumbers { get; set; } = new();

        // Comandos
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LoadPageCommand { get; }

        // Evento para notificar al padre sobre el cambio de página
        public event EventHandler<int>? PageChangeRequested;

        public PaginationViewModel()
        {
            LoadPageCommand = new RelayCommand(LoadPageFromParameter);
            PrevPageCommand = new RelayCommand(
                _ => PageChangeRequested?.Invoke(this, CurrentPage - 1),
                _ => CurrentPage > 1);
            NextPageCommand = new RelayCommand(
                _ => PageChangeRequested?.Invoke(this, CurrentPage + 1),
                _ => CurrentPage < TotalPages);

            UpdatePageNumbers();
        }

        private void LoadPageFromParameter(object? parameter)
        {
            if (parameter is int page && page != CurrentPage)
            {
                PageChangeRequested?.Invoke(this, page);
            }
        }

        // Su lógica original de cálculo de páginas
        private void UpdatePageNumbers()
        {
            PageNumbers.Clear();
            if (TotalPages <= 0) return;

            var pages = new List<int>();
            pages.Add(CurrentPage);

            for (int i = 1; i <= 4; i++)
            {
                var p = CurrentPage + i;
                if (p <= TotalPages) pages.Add(p);
            }

            if (TotalPages > CurrentPage && !pages.Contains(TotalPages))
                pages.Add(TotalPages);

            foreach (var p in pages.Distinct().OrderBy(x => x))
                PageNumbers.Add(p);
        }
    }
}