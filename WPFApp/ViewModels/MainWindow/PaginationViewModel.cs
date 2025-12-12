using System.Collections.ObjectModel;
using System.Windows.Input;


namespace ViewModels
{
    public class PaginationViewModel : ViewModelBase
    {

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


        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LoadPageCommand { get; }

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
            var pagesSet = new HashSet<int>();

            pagesSet.Add(1);

            int start = CurrentPage - 2;
            int end = CurrentPage + 2;

            for (int i = start; i <= end; i++)
            {
                if (i > 1 && i < TotalPages)
                {
                    pagesSet.Add(i);
                }
            }

            if (TotalPages > 1)
            {
                pagesSet.Add(TotalPages);
            }

            foreach (var p in pagesSet.OrderBy(x => x))
            {
                PageNumbers.Add(p);
            }
        }
    }
}