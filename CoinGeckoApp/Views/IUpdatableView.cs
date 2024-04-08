using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Views
{
    public interface IUpdatableView
    {
        /// <summary>
        /// Manual update of UI based on changes in a Model.
        /// <para>This method can be used as an alternative to Data Binding for rendering the UI.</para>
        /// </summary>
        Task UpdateUIAsync();

        /// <summary>
        /// Manually sets a new model in context inside a View (i.e. Page) or outside the View (e.g. Controller or ViewModel classes).
        /// </summary>
        /// <param name="model"></param>
        Task SetModelAsync(object model);
    }
}
