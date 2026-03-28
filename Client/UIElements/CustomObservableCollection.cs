
using System.Collections.ObjectModel;
using System.Collections.Specialized;


namespace Client.UIElements
{
    /// <summary>
    /// Имеет оптимизированный метод вставки по индексу
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CustomObservableCollection<T> : ObservableCollection<T>
    {
        public void InsertRange(int startIndex, IEnumerable<T> items)
        {
            if (items == null || !items.Any()) return;

            // 1. Блокируем стандартные уведомления (проверка на рекурсию)
            this.CheckReentrancy();

            int currentIndex = startIndex;
            foreach (var item in items)
                this.Items.Insert(currentIndex++, item);

            // 3. Генерируем ОДНО событие для всего списка добавленных элементов
            // Это заставит CollectionView перерисоваться всего 1 раз
            this.OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    items is System.Collections.IList list ? list : items.ToList(),
                    startIndex));

            // Уведомляем об изменении количества (Count)
            this.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(Count)));
        }
    }
}
